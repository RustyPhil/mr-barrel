using UnityEngine;

/// <summary>
/// Пуля, наносящая урон игроку.<br/>
/// Адаптирована для взаимодействия с пулом объектов.
/// </summary>
[RequireComponent(typeof(CollisionListener))]
public class Bullet : MonoBehaviour
{
    #region Parameters
    
    /// <summary>
    /// Скорость пули. Пуля игнорирует навигацию и движется по прямой через все препятствия.
    /// </summary>
    [SerializeField] private float speed;
    
    /// <summary>
    /// Компонент, получающий коллизии с аватаром.
    /// </summary>
    private CollisionListener hitDetector;
    
    /// <summary>
    /// Хэш префаба эффекта попадания пули в игрока.
    /// </summary>
    private static int BOOM = Animator.StringToHash("Boom");
    
    #endregion

    #region Base methods
    
    private void OnEnable()
    {
        if (hitDetector == null)
            hitDetector = GetComponent<CollisionListener>();
        hitDetector.OnEnter += Kill;
    }

    private void OnDisable() => hitDetector.OnEnter -= Kill;
    
    private void Update() => transform.position += transform.forward * speed * Time.deltaTime;
    
    #endregion

    #region Collision
    
    /// <summary>
    /// Вызывается при коллизии.
    /// </summary>
    private void Kill(GameObject target, CollisionListener emitter)
    {
        PrefabPool.GetFromPool(BOOM, transform.position);

        // Если попадание в игрока, то игрок получает урон.
        CollisionProvider reciever = target.GetComponent<CollisionProvider>();
        if(reciever != null)
            reciever.TakeDamage();
        
        // Если пуля настроена как предмет хранящийся в пуле, то она возвращается в пул сразу после попадания.
        // В противном случае пуля уничтожается.
        PoolableItem poolable = GetComponent<PoolableItem>();
        if(poolable != null)
            PrefabPool.ReturnToPool(poolable);
        else
            Destroy(gameObject);
    }
    
    #endregion
}
