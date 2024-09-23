using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Компонент поведения противника, ответственный за патрулирование.
/// </summary>
public class PatrolBehaviour : PartialBehaviour
{
    #region Parameters
    
    /// <summary>
    /// Ссылки на объекты, определяющие точки пути объекта.<br/>
    /// При инициализации помимо них используется положение объекта на момент инициализации.
    /// </summary>
    [SerializeField] private Transform[] patrolMarkers;
    
    /// <summary>
    /// Пауза в конце маршрута, перед началом обратного пути.
    /// </summary>
    [SerializeField] private float pause = 0f;

    /// <summary>
    /// Точки пути. Заполняются при инициализации.
    /// </summary>
    private List<Vector3> waypoints = new List<Vector3>();
    
    /// <summary>
    /// Следующая точка, к которой движетя объект.
    /// </summary>
    private int nextPoint = 0;
    
    /// <summary>
    /// Процесс движения по карте.
    /// </summary>
    private Coroutine movement;

    /// <summary>
    /// Хэш ключа анимации ходьбы.
    /// </summary>
    private static int WALK = Animator.StringToHash("walk");

    #endregion

    #region PartialBehaviour methods

    public override void Init(Animator anim, NavMeshAgent nav)
    {
        base.Init(anim, nav);

        waypoints.Add(transform.position);
        foreach (Transform t in patrolMarkers)
            waypoints.Add(t.position);
    }

    public override bool GiveControl()
    {
        inControl = true;
        movement = StartCoroutine(MovementRoutine());
        return true;
    }

    public override bool RemoveControl(bool forced)
    {
        inControl = false;
        StopCoroutine(movement);
        navigation.isStopped = true;
        view.SetBool(WALK, false);
        return true;
    }

    #endregion

    #region Patrol

    /// <summary>
    /// Процесс движения по точкам.
    /// </summary>
    private IEnumerator MovementRoutine()
    {
        // Если компонент не котролирует объект, то он отключает навигацию.
        navigation.isStopped = !inControl;
        
        while (inControl)
        {
            MoveToNext();
            
            do
                yield return null;
            while (navigation.pathPending || navigation.remainingDistance > navigation.stoppingDistance);

            nextPoint++;
            if (nextPoint >= waypoints.Count && pause > 0f)
            {
                // Достигнута конечная точка. Отключение анимации ходьбы и пауза.
                view.SetBool(WALK, false);
                yield return new WaitForSeconds(pause);
            }

        }
    }

    /// <summary>
    /// Создать маршрут до следующей точки.<br/>
    /// Если достигнута конечная точка, то список точек меняет последовательность на обратную и объект начинает движение в обратную сторону.
    /// </summary>
    private void MoveToNext()
    {
        if (nextPoint >= waypoints.Count)
        {
            nextPoint = 0;
            waypoints.Reverse();
        }

        NavMeshPath path = new NavMeshPath();
        navigation.CalculatePath(waypoints[nextPoint], path);
        navigation.SetPath(path);
        view.SetBool(WALK, true);
    }
    
    #endregion
}
