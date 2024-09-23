using System;
using UnityEngine;

/// <summary>
/// Компонент, ожидающий коллизии с аватаром игрока.
/// </summary>
public class CollisionListener : MonoBehaviour
{
    /// <summary>
    /// Вызывается при входе аватара в коллайдер объекта.
    /// </summary>
    public Action<GameObject, CollisionListener> OnEnter;
    
    /// <summary>
    /// Вызывается при выходе аватара из коллайдера.
    /// </summary>
    public Action<GameObject, CollisionListener> OnExit;
    
    /// <summary>
    /// Начало коллизии
    /// </summary>
    /// <param name="target">Объект, вызвавший коллизию.</param>
    public void StartCollision(GameObject target) => OnEnter?.Invoke(target, this);

    /// <summary>
    /// Завершение коллизии
    /// </summary>
    /// <param name="target">Объект, вызвавший коллизию.</param>
    public void EndCollision(GameObject target) => OnExit?.Invoke(target, this);
}
