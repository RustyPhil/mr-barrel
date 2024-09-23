using System;
using UnityEngine;

/// <summary>
/// Компонент, отслеживающий столкновение аватара с интерактивными объектами.
/// </summary>
public class CollisionProvider : MonoBehaviour
{
    /// <summary>
    /// Флаг маскировки объекта.
    /// </summary>
    // TODO: заменить флаг внутри компонента на изменение настройки обнаружения по слоям.
    private bool isHidden = false; 

    /// <summary>
    /// Вызывается при получении урона.
    /// </summary>
    public Action<float> OnTakeDamage;

    /// <summary>
    /// Флаг маскировки объекта.
    /// </summary>
    public bool IsHidden => isHidden;

    /// <summary>
    /// Нанести урон объекту.
    /// </summary>
    public void TakeDamage(float damage = 1f) => OnTakeDamage?.Invoke(damage);

    /// <summary>
    /// Изменить статус маскировки объекта.
    /// </summary>
    public void SetHidden(bool hidden) => isHidden = hidden;

    /// <summary>
    /// Вызывается при коллизии с интерактивным объектом. Передает этому объекту информацию о себе.
    /// </summary>
    private void OnTriggerEnter(Collider collide)
    {
        CollisionListener agent = collide.GetComponent<CollisionListener>();
        if(agent == null)
            return;
        
        agent.StartCollision(gameObject);
    }

    /// <summary>
    /// Вызывается при завершении коллизии с интерактивным объектом. Передает этому объекту информацию о себе.
    /// </summary>
    private void OnTriggerExit(Collider collide)
    {
        CollisionListener agent = collide.GetComponent<CollisionListener>();
        if(agent == null)
            return;
        
        agent.EndCollision(gameObject);
    }
}
