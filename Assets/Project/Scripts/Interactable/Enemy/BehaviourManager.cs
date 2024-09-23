using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Менеджер поведений ИИ-противников.<br/>
/// Предает управление различным компонентам противника, в соответствии с приоритетами.
/// </summary>
public class BehaviourManager : MonoBehaviour
{
    #region Parameters
    
    /// <summary>
    /// Аниматор, используемый прочими компонентами.
    /// </summary>
    [Header("Components")]
    [SerializeField] protected Animator view;
    
    /// <summary>
    /// NavMeshAgent, определяющий движение и вращение противника.
    /// </summary>
    [SerializeField] protected NavMeshAgent navigation;
    
    /// <summary>
    /// Список поведений, который могут брать контроль над объектом, в порядке убывания приоритета. 
    /// </summary>
    [Header("Behaviour parts")]
    [Tooltip("Порядок расположения компонентов в списке определяет их приоритет.")]
    [SerializeField] private PartialBehaviour[] behaviours;
    
    #endregion

    #region Base methods
    
    private void Start()
    {
        foreach (PartialBehaviour behaviour in behaviours)
        {
            behaviour.Init(view, navigation);
            
            behaviour.OnNeedControl += TryTakeControl;
            behaviour.OnReturnControl += ReturnControl;
        }

        GameState.OnGameEnd += FullStop;

        ActivateBehaviour();
    }
    
    private void OnDestroy()
    {
        foreach (PartialBehaviour behaviour in behaviours)
            behaviour.OnNeedControl -= TryTakeControl;
        
        GameState.OnGameEnd -= FullStop;
    }
    
    #endregion

    #region Object control
    
    /// <summary>
    /// Передать контроль над объектом определенному поведению.
    /// </summary>
    private void TryTakeControl(PartialBehaviour behaviour)
    {
        int senderIndex = Array.IndexOf(behaviours, behaviour);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if(i == senderIndex)
                continue;
            
            if(!behaviours[i].RemoveControl(i>senderIndex))
                return;
        }

        behaviour.GiveControl();
    }

    /// <summary>
    /// Освободить объект от контроля активного поведения.
    /// </summary>
    private void ReturnControl(PartialBehaviour behaviour) => ActivateBehaviour();

    /// <summary>
    /// Передать контроль самому приоритетному из готовых забрать контроль поведений.
    /// </summary>
    private void ActivateBehaviour()
    {
        foreach (PartialBehaviour behaviour in behaviours)
        {
            if(behaviour.GiveControl())
                break;
        }
    }

    /// <summary>
    /// Отключение всех поведений объекта. Вызывается при установке игры на паузу.
    /// </summary>
    private void FullStop(bool win)
    {
        foreach (PartialBehaviour behaviour in behaviours)
            behaviour.RemoveControl(true);
    }
    
    #endregion
}
