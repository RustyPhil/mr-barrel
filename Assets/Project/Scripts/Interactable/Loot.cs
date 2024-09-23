using UnityEngine;

/// <summary>
/// Объект, дающий игроку очки при коллизии.
/// </summary>
[RequireComponent(typeof(CollisionListener))]
public class Loot : MonoBehaviour
{
    /// <summary>
    /// Количество очков, которые получит игрок.
    /// </summary>
    [SerializeField] private int scoreValue;
    
    /// <summary>
    /// Категория, к которой относятся очки от этого объекта.
    /// </summary>
    [SerializeField] private string scoreType = "Money collected";
    
    /// <summary>
    /// Компонент, получающий коллизии с аватаром.
    /// </summary>
    private CollisionListener hitDetector;

    /// <summary>
    /// Хэш айди эффекта, отображаемого при сборе объекта.
    /// </summary>
    private static int REWARD = Animator.StringToHash("Reward");
    
    private void OnEnable()
    {
        if (hitDetector == null)
            hitDetector = GetComponent<CollisionListener>();
        hitDetector.OnEnter += Collect;
    }

    private void OnDisable() => hitDetector.OnEnter -= Collect;
    
    /// <summary>
    /// Сбор и уничтожение объекта. Показ эффекта.
    /// </summary>
    private void Collect(GameObject target, CollisionListener emitter)
    {
        hitDetector.OnEnter -= Collect;
        GameState.AddScore(scoreValue, scoreType);
        PrefabPool.GetFromPool(REWARD, transform.position);
        
        Destroy(gameObject);
    }
}
