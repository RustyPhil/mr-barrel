using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Основной контроллер аватара игрока.<br/>
/// Служит для передачи сигналов от устройств ввода и других игровых систем.
/// </summary>
public class AvatarController : MonoBehaviour
{
    #region Parameters

    /// <summary>
    /// Методы управления аватаром. Можно использовать более одного устройства ввода.<br/>
    /// Чотбы избежать наложения сигналов, пока активно одно устройство ввода, остальные отключаются.
    /// </summary>
    [Header("Player controls")]
    [SerializeField] private MovementController[] input;
    
    /// <summary>
    /// Для огибания препятствий используется NavMeshAgent. Также он хранит линейную и угловую скорость.<br/>
    /// Поиск пути для аватара не используется.<br/>
    /// Линейное движение применяется к NavMeshAgent
    /// </summary>
    [Header("Navigation")]
    [SerializeField] private NavMeshAgent movementNode;
    
    /// <summary>
    /// Объект, к которому применяется вращение. Может быть вложен в объект, к которому применяется движение.<br/>
    /// Разделение сделано для того, чтобы зафиксировать поворот игровой камеры.
    /// </summary>
    [SerializeField] private Transform rotationNode;
    
    /// <summary>
    /// Отображение аватара. Управляет анимациями всех входящих в состав аватара объектов.
    /// </summary>
    [Header("Linked components")]
    [SerializeField] private VisualStateControl view;
    
    /// <summary>
    /// Компонент, управляющий коллизиями.<br/>
    /// Передает сигналы о столкновениях объектам взаимодействия, обнаружения и получения урона.
    /// </summary>
    [SerializeField] private CollisionProvider hitbox;

    /// <summary>
    /// Время нужное на остановку, в случае если от устройства ввода поступают данные о движении, но скорость равна нулю.
    /// </summary>
    [Header("Cover settings")]
    [SerializeField] private float stopTime = 0.15f;
    
    /// <summary>
    /// Время нужное на переход в состояние маскировки.
    /// </summary>
    [SerializeField] private float hideTime = 0.75f;
    
    /// <summary>
    /// Надевать ли на аватар бочку в начале уровня.
    /// </summary>
    [SerializeField] private bool startCovered;

    /// <summary>
    /// Активное устройство ввода, управляющее движением аватара.
    /// </summary>
    private MovementController activeInput = null;
    
    /// <summary>
    /// Флаг состояния движения с нулевой скоростью.<br/>
    /// Обычно возникает на экранном джойстике.
    /// </summary>
    private bool isIdle = true;
    
    /// <summary>
    /// Время в состоянии движения с нулевой скоростью.<br/>
    /// Как только это время превысит <paramref name="stopTime"/>, аватар перейдет в состояние остановки.<br/>
    /// Как только время превысит <paramref name="hideTime"/>, аватар перейдет в состояние маскировки.
    /// </summary>
    private float idleTime = 0f;
    
    /// <summary>
    /// Процесс ожидания перехода в состояние остановки и маскировки.
    /// </summary>
    private Coroutine CoverTimeout = null;
    
    #endregion
    
    #region Base methods

    void OnEnable()
    {
        // Включение всех устройств ввода, чтобы игрок мог начать исполдьзовать любое.
        foreach (var inputSource in input)
        {
            inputSource.OnStartMove += OnStartInput;
            inputSource.SetActiveInput(true);
        }

        hitbox.OnTakeDamage += TakeDamage;
        GameState.OnGameEnd += FullStop;

        // Переход в скрытое состояние, если уровень это предполагает.
        hitbox.SetHidden(startCovered);
        view.Cover(startCovered);
    }

    void OnDisable()
    {
        hitbox.OnTakeDamage -= TakeDamage;
        GameState.OnGameEnd -= FullStop;
        
        foreach (var inputSource in input)
            inputSource.OnStartMove -= OnStartInput;

        if(activeInput == null)
            return;
        
        activeInput.OnDirectionUpdate -= MoveAvatar;
        activeInput.OnEndMove -= StopMoving;
        activeInput = null;
    }

    #endregion
    
    #region Movement
    
    /// <summary>
    /// Метод передает устройство ввода эксклюзивное управление аватаром.
    /// </summary>
    /// <param name="emitter">Устройство ввода, с которого пришел сигнал.</param>
    void OnStartInput(MovementController emitter)
    {
        if (activeInput != null)
        {
            activeInput.OnDirectionUpdate -= MoveAvatar;
            activeInput.OnEndMove -= StopMoving;
        }

        activeInput = emitter;
        activeInput.OnDirectionUpdate += MoveAvatar;
        activeInput.OnEndMove += StopMoving;

        // Все устройства ввода кроме активного отключаются, чтобы избежать наложения сигналов.
        foreach (var inputSource in input)
        {
            if(inputSource != emitter)
                inputSource.SetActiveInput(false);
        }
    }
    
    /// <summary>
    /// Метод управления движанием и вращением аватара.<br/>
    /// Пока устройство ввода контролирует аватар, оно вызывает этот метод раз в кадр.
    /// </summary>
    /// <param name="direction">Нормализованный вектор. Направление движение, считанное устройством ввода.</param>
    private void MoveAvatar(Vector2 direction)
    {
        if (direction.magnitude >= 0.1f)
        {
            // Аватар получил сигнал двигаться.
            // Прерывание процесса маскировки аватара и отмена действующей маскировки.
            if(CoverTimeout != null)
                StopCoroutine(CoverTimeout);
            hitbox.SetHidden(false);
            idleTime = 0f;
            if (isIdle)
            {
                view.StartMoving();
                view.Hide(false);
                isIdle = false;
            }
            
            // Вычисление направления движения аватара, с учетом ограниченности угловой скорости.
            // Разворот аватара по направлению движения.
            Vector3 direction3D = new Vector3(direction.x, 0f, direction.y);
            Quaternion targetRotation = Quaternion.LookRotation(direction3D);
            
            rotationNode.rotation = Quaternion.RotateTowards(rotationNode.rotation, targetRotation, movementNode.angularSpeed * Time.deltaTime);
            Vector3 currentRotation = rotationNode.forward;
            
            Vector3 speedScale = Vector3.Project(direction3D, currentRotation);
            movementNode.Move(currentRotation * speedScale.magnitude * Time.deltaTime * movementNode.speed);
        }
        else
        {
            // Аватар находится под котролем устройства ввода, но устройство ввода передает нулевую скорость.
            // Отсчет времени до остановки анимации движения и включения маскировки.
            
            idleTime += Time.deltaTime;
            if (!isIdle && idleTime >= stopTime)
            {
                isIdle = true;
                view.SetIdle();
            }

            if (idleTime >= hideTime)
            {
                view.Hide(true);
                hitbox.SetHidden(true);
            }
        }
    }

    /// <summary>
    /// Корутина, ожидающая завршения таймера маскировки.<br/>
    /// Используется если все устройства ввода отключились от управления аватаром.
    /// </summary>
    private IEnumerator DelayAndHide()
    {
        yield return new WaitForSeconds(hideTime - idleTime);
        
        view.Hide(true);
        hitbox.SetHidden(true);
        CoverTimeout = null;
    }

    /// <summary>
    /// Остановка движения и анимации ходьбы аватара. Запуск ожидания перехода в состояние маскировки.
    /// </summary>
    private void StopMoving()
    {
        if (!isIdle)
        {
            view.SetIdle();
            isIdle = true;
        }

        if (hideTime <= idleTime)
        {
            view.Hide(true);
            hitbox.SetHidden(true);
        }
        else if (CoverTimeout == null)
            CoverTimeout = StartCoroutine(DelayAndHide());
        
        if (activeInput != null)
        {
            activeInput.OnDirectionUpdate -= MoveAvatar;
            activeInput.OnEndMove -= StopMoving;
            activeInput = null;
        }

        // Все устройства ввода включаются, любое из них может взять управление.
        foreach (var inputSource in input)
            inputSource.SetActiveInput(true);
    }

    #endregion Movement

    #region Interactions

    /// <summary>
    /// Получение урона. На данный момент любой урон приводит к поражению.
    /// </summary>
    private void TakeDamage(float damage) => GameState.EndGame(false);

    /// <summary>
    /// Постановка аватара на паузу, остановка всех устройств ввода, запуск финальной анимации.<br/>
    /// Используется при победе и поражении на уровне.
    /// </summary>
    private void FullStop(bool win)
    {
        if (CoverTimeout == null)
            CoverTimeout = StartCoroutine(DelayAndHide());
        
        foreach (var inputSource in input)
            inputSource.SetActiveInput(false);
        
        if(win)
            view.SetVictory();
        else
            view.SetDefeat();
    }

    #endregion
}
