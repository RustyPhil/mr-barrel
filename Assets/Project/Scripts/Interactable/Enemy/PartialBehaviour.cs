using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Абстрактный класс компонента поведения противника.
/// </summary>
public abstract class PartialBehaviour : MonoBehaviour
{

    #region Parameters

    /// <summary>
    /// Аниматор, используемый прочими компонентами.
    /// </summary>
    protected Animator view;
    
    /// <summary>
    /// NavMeshAgent, определяющий движение и вращение противника.
    /// </summary>
    protected NavMeshAgent navigation;
    
    /// <summary>
    /// Флаг, показывающий что компонент сейчас контролирует объект.
    /// </summary>
    protected bool inControl = false;
    
    /// <summary>
    /// Флаг, показывающий что компонент готов к работе.
    /// </summary>
    protected bool isInit = false;
    
    #endregion

    #region Events

    /// <summary>
    /// Вызывается, когда компонент хочет получить контроль над объектом.
    /// </summary>
    public Action<PartialBehaviour> OnNeedControl;
    
    /// <summary>
    /// Вызывается, когда компонент хочет отдать контроль над объектом.
    /// </summary>
    public Action<PartialBehaviour> OnReturnControl;
    
    #endregion

    #region Methods

    /// <summary>
    /// Получение ссылок на компоненты. Вызывается менеджером поведений.
    /// </summary>
    public virtual void Init(Animator anim, NavMeshAgent nav)
    {
        view = anim;
        navigation = nav;
        isInit = true;
    }

    /// <summary>
    /// Передача контроля компоненту.
    /// </summary>
    public virtual bool GiveControl() => false;

    /// <summary>
    /// Отстранение компонента от управления объектом.
    /// </summary>
    public virtual bool RemoveControl(bool forced)
    {
        inControl = false;
        return true;
    }

    /// <summary>
    /// Компонент вызывает этот метод, когда хочет получить управление.
    /// </summary>
    protected void TryTakeControl() => OnNeedControl?.Invoke(this);
    
    /// <summary>
    /// Компонент вызывает этот метод, когда хочет отдать управление.
    /// </summary>
    protected virtual void ReturnControl()
    {
        inControl = false;
        OnReturnControl?.Invoke(this);
    }
    
    #endregion
}
