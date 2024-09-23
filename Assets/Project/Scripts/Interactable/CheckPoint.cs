using UnityEngine;

/// <summary>
/// Точка на карте, достижение которой дает игроку награду.
/// </summary>
// TODO: В настоящий момент избыточна, можно было бы ограничиться только сбором лута и объединить весь функционал в нем.
[RequireComponent(typeof(CollisionListener))]
public class CheckPoint : MonoBehaviour
{
    /// <summary>
    /// Название точки.
    /// </summary>
    [SerializeField] private string pointName;
    
    /// <summary>
    /// Хэш названия точки.
    /// </summary>
    private int pointId;
    
    /// <summary>
    /// Компонент, получающий коллизии с аватаром игрока.
    /// </summary>
    private CollisionListener hitDetector;

    /// <summary>
    /// Хэш эффекта, проигрываемого при достижении точки.
    /// </summary>
    private static int REWARD = Animator.StringToHash("Reward");
    
    private void OnEnable()
    {
        if (hitDetector == null)
        {
            hitDetector = GetComponent<CollisionListener>();
            pointId = Animator.StringToHash(pointName);
        }

        hitDetector.OnEnter += Reached;
    }

    private void OnDisable() => hitDetector.OnEnter -= Reached;

    /// <summary>
    /// Метод, вызываемый при достижении точки.
    /// </summary>
    private void Reached(GameObject target, CollisionListener emitter)
    {
        PrefabPool.GetFromPool(REWARD, target.transform.position);
        GameState.ReachPoint(pointId);
    }
}
