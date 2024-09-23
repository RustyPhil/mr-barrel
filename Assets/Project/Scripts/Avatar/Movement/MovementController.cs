using System;
using UnityEngine;

/// <summary>
/// Абстрактный класс контроллера.<br/>
/// Реализация нескольких наследников позволяет поддерживать несколько методов ввода.
/// </summary>
public abstract class MovementController : MonoBehaviour
{

    #region Parameters

    /// <summary>
    /// Флаг активности источника ввода. Активный источник готов принимать ввод от игрока.
    /// </summary>
    protected bool isActive;

    #endregion Parameters
    
    #region Events

    /// <summary>
    /// Вызывается при активации источника ввода.
    /// </summary>
    public event Action<MovementController> OnStartMove;
    
    /// <summary>
    /// Вызывается при деактивации источника ввода.
    /// </summary>
    public event Action OnEndMove;
    
    /// <summary>
    /// Вызывается каждый кадр пока источник ввода активен.<br/>
    /// Передает нормализованный вектор направления.
    /// </summary>
    public event Action<Vector2> OnDirectionUpdate;

    #endregion

    #region Methods

    /// <summary>
    /// Установить флаг активности источника ввода.
    /// </summary>
    public virtual void SetActiveInput(bool enable) => isActive = enable;
    
    /// <summary>
    /// Начать управлять аватаром с помощью данного устройства ввода. 
    /// </summary>
    protected virtual void StartMove() => OnStartMove?.Invoke(this);

    /// <summary>
    /// Прекратить управлять аватаром с помощью данного устройства ввода.
    /// </summary>
    protected virtual void EndMove() => OnEndMove?.Invoke();
    
    /// <summary>
    /// Двигать аватар. Метод должен вызываться один раз в кадр, до тех пор пока устройство ввода не прекратит управлять аватаром.
    /// </summary>
    protected void DirectionUpdate(Vector2 data) => OnDirectionUpdate?.Invoke(data);

    #endregion
}
