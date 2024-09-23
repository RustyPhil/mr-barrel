using System;
using UnityEngine;

/// <summary>
/// Компонент, позволяющий подписаться на события анимации, без прямого указания вызываемого метода внутри самой анимации.
/// </summary>
public class AnimationEventListener : MonoBehaviour
{
    /// <summary>
    /// Вызывается при получении события анимации.
    /// </summary>
    public Action OnAnimationEvent;

    /// <summary>
    /// Этот метод необходимо вызывать из анимации.
    /// </summary>
    public void CallAnimationEvent() => OnAnimationEvent?.Invoke();
}
