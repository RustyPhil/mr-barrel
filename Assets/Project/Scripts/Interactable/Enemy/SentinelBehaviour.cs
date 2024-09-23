using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Компонент поведения противника, ответственный за обнаружение игрока и атаку.
/// </summary>
public class SentinelBehaviour : PartialBehaviour
{
    #region Parameters
    
    /// <summary>
    /// Список детекторов. Всвязи со сложной геометрией зоны видимости, используется сразу несколько детекторов.
    /// </summary>
    [Header("Detection")]
    [SerializeField] private CollisionListener[] detectors;
    
    /// <summary>
    /// Слои, объекты в которых перекрывают обзор и позволяют игроку скрыться.
    /// </summary>
    [SerializeField] private LayerMask obstacles;
    
    /// <summary>
    /// Индикатор тревоги. Отображается когда противник видит игрока.
    /// </summary>
    // TODO: заменить на более универсальный вариант управления.
    [SerializeField] private GameObject alarm;
    
    /// <summary>
    /// Погрешность наведения противника на игрока.
    /// </summary>
    [Header("Aiming")]
    [SerializeField] private float shootingAngle = 3f;
    
    /// <summary>
    /// Позиция ствола оружия внутри модели объекта. Пуля начинает полет в этой точке.
    /// </summary>
    [Header("Shooting")]
    [SerializeField] private Transform muzzle;
    
    /// <summary>
    /// Задержка между выстрелами.
    /// </summary>
    [SerializeField] private float pauseAfterShot = 0f;

    /// <summary>
    /// Количество детекторов, которые заметили игрока.
    /// </summary>
    private int alarmLevel = 0;
    
    /// <summary>
    /// Компонент, ожидающий событие анимации.
    /// </summary>
    private AnimationEventListener animationListener;
    
    /// <summary>
    /// Флаг, определяющий то видит противник игрока или нет.
    /// </summary>
    private bool alarmed;
    
    /// <summary>
    /// Кэшированное состояние показа индикатора тревоги противника.
    /// </summary>
    private bool alarmVisibe;
    
    /// <summary>
    /// Время последнего рейкаста в игрока. Используется для избегания лишних вычислений.
    /// </summary>
    private float lastTargetCheck = 0f;
    
    /// <summary>
    /// Последняя цель, обнаруженная детектором.
    /// </summary>
    private GameObject lastTarget;

    // Хэши управления анимациями объекта.
    private static int SHOOT = Animator.StringToHash("shoot");
    private static int READY = Animator.StringToHash("ready");
    
    // Хэш названия пула, в котором хранятся пули.
    private static int BULLET = Animator.StringToHash("Bullet");

    /// <summary>
    /// Процесс выстрела.
    /// </summary>
    private Coroutine shootRoutine;
    
    #endregion

    #region Base methods

    private void OnDestroy()
    {
        if(animationListener != null)
            animationListener.OnAnimationEvent -= ShootTarget;
        
        foreach (CollisionListener detector in detectors)
        {
            detector.OnEnter -= UpAlarmLevel;
            detector.OnExit -= DownAlarmLevel;
        }
    }
    
    private void Update()
    {
        CheckTargetVisible();
        
        if(inControl && !alarmed)
            ReturnControl();
        else if (!inControl && alarmed)
        {
            CollisionProvider targetCollision = lastTarget.GetComponent<CollisionProvider>();
            if(targetCollision != null && !targetCollision.IsHidden)
                TryTakeControl();
        }

        ShowAlarm(inControl && alarmed);
    }
    
    #endregion

    #region PartialBehaviour methods

    public override void Init(Animator anim, NavMeshAgent nav)
    {
        base.Init(anim, nav);
        
        alarmVisibe = alarm != null && alarm.activeSelf;

        // Отслеживание событий анимации.
        animationListener = view.GetComponent<AnimationEventListener>();
        if(animationListener)
            animationListener.OnAnimationEvent += ShootTarget;
        
        // Отслеживание коллизий
        foreach (CollisionListener detector in detectors)
        {
            detector.OnEnter += UpAlarmLevel;
            detector.OnExit += DownAlarmLevel;
        }
    }

    public override bool GiveControl()
    {
        if (CheckTargetVisible())
        {
            inControl = true;
            ShowAlarm(true);
            shootRoutine = StartCoroutine(ShootRoutine());
            return true;
        }

        return false;
    }

    public override bool RemoveControl(bool forced)
    {
        if (!forced && CheckTargetVisible())
            return false;
        
        ShowAlarm(false);
        inControl = false;
        if(shootRoutine != null)
            StopCoroutine(shootRoutine);
        view.SetBool(SHOOT, false);

        return true;
    }

    protected override void ReturnControl()
    {
        StopCoroutine(shootRoutine);
        view.SetBool(SHOOT, false);
        base.ReturnControl();
    }

    #endregion

    #region Sentinel

    /// <summary>
    /// Процесс выстрела. При повторных выстрелах пауза может быть больше нуля.
    /// </summary>
    private IEnumerator ShootRoutine(float waitForShot = 0f)
    {
        // Ожидание перезарядки.
        view.SetBool(SHOOT, false);
        if (waitForShot > 0f)
            yield return new WaitForSeconds(waitForShot);
        
        // Наведение на цель.
        bool rotating = true;
        view.SetBool(SHOOT, true);
        while(rotating && lastTarget != null)
        {
            yield return null;
            
            Vector3 targetVector = lastTarget.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetVector);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                navigation.angularSpeed * Time.deltaTime);

            rotating = Vector3.Angle(transform.forward, targetVector) > shootingAngle;
        }
        
        yield return null;
        
        // Выстрел. Если нет компонента, получающего события анимации, то выстрел производится мгновеенно.
        view.SetTrigger(READY);
        if(animationListener == null)
            ShootTarget();
    }

    /// <summary>
    /// Произвести выстрел.
    /// </summary>
    private void ShootTarget()
    {
        GameObject bullet = PrefabPool.GetFromPool(BULLET, muzzle.position);
        bullet.transform.rotation = transform.rotation;
        
        if(CheckTargetVisible())
            shootRoutine = StartCoroutine(ShootRoutine(pauseAfterShot));
    }

    /// <summary>
    /// Проверка видимости последнец цели, попавшей в коллайдеры детекторов.
    /// </summary>
    private bool CheckTargetVisible()
    {
        // Если коллайдер покинул детекторы, то тревога снимается.
        if (alarmLevel <= 0)
            alarmed = false;
        else if (lastTargetCheck < Time.time)
        {
            // Рейкаст от объекта к цели.
            // Если между целью и объектом обнаружено непрозрачное препятствие, то тревога снимается.
            lastTargetCheck = Time.time;
            Vector3 myPosition = transform.position;
            Vector3 targetPosition = lastTarget.transform.position;
            alarmed = !Physics.Raycast(new Ray(myPosition, targetPosition - myPosition), 
                Vector3.Distance(myPosition, targetPosition), obstacles);
        }

        return alarmed;
    }

    /// <summary>
    /// Переключение индикатора тревоги.
    /// </summary>
    private void ShowAlarm(bool set)
    {
        if(set == alarmVisibe)
            return;

        alarm?.SetActive(set);
        alarmVisibe = set;
    }

    /// <summary>
    /// Повысить уровень тревоги. Вызывается при попадании аватара в детектор.
    /// </summary>
    private void UpAlarmLevel(GameObject target, CollisionListener emitter)
    {
        alarmLevel++;
        lastTarget = target;
    }

    /// <summary>
    /// Понизить уровень тревоги. Вызывается при выходе аватара из детектора.
    /// </summary>
    private void DownAlarmLevel(GameObject target, CollisionListener emitter)
    {
        alarmLevel--;
        lastTarget = target;
    }
    
    #endregion
}
