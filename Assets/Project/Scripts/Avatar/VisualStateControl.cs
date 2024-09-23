using UnityEngine;

/// <summary>
/// Отображание аватара игрока. Управляет всеми аниматорами, входящими в стостав аватара.
/// </summary>
public class VisualStateControl : MonoBehaviour
{
    #region Parameters

    /// <summary>
    /// Основной аватар игрока. Ходит, приседает, танцует.
    /// </summary>
    [SerializeField] protected Animator character;
    /// <summary>
    /// Основной аниматор бочки. Синхронизируется с основным аватаром.
    /// </summary>
    [SerializeField] protected Animator barrelMovement;
    /// <summary>
    /// Дополнительный аниматор бочки. Используется только для проигрывания анимации получения урона.
    /// </summary>
    [SerializeField] protected Animator barrelDestruction;

    #endregion Parameters

    #region Animation IDs
    
    // Хеши параметров аниматоров аватара и бочки.

    private static readonly int COVERED =  Animator.StringToHash("covered");
    private static readonly int HIDDEN = Animator.StringToHash("hidden");
    private static readonly int MOVE = Animator.StringToHash("move");
    private static readonly int STOP = Animator.StringToHash("stop");
    
    private static readonly int VICTORY = Animator.StringToHash("victory");
    private static readonly int DEFEAT = Animator.StringToHash("defeat");
    private static readonly int REMOVE = Animator.StringToHash("remove");

    #endregion

    #region Animation control

    /// <summary>
    /// Надевает и снимает бочку с аватара.
    /// </summary>
    public void Cover(bool input) => character.SetBool(COVERED, input);

    /// <summary>
    /// Включает и выключает режим маскировки.
    /// </summary>
    public void Hide(bool input) => character.SetBool(HIDDEN, input);

    /// <summary>
    /// Переводит аватар в режим движения.
    /// </summary>
    public void StartMoving()
    {
        character.SetTrigger(MOVE);
        barrelMovement.SetTrigger(MOVE);
        
        character.ResetTrigger(STOP);
        barrelMovement.ResetTrigger(STOP);
    }

    /// <summary>
    /// Переводит аватар в режим покоя.
    /// </summary>
    public void SetIdle()
    {
        character.SetTrigger(STOP);
        barrelMovement.SetTrigger(STOP);
        
        character.ResetTrigger(MOVE);
        barrelMovement.ResetTrigger(MOVE);
    }

    /// <summary>
    /// Проигрывает анимацию победы.
    /// </summary>
    public void SetVictory()
    {
        character.SetTrigger(VICTORY);
        barrelMovement.SetTrigger(REMOVE);
    }
    
    /// <summary>
    /// Проигрывает анимацию поражения.
    /// </summary>
    public void SetDefeat()
    {
        character.SetTrigger(DEFEAT);
        barrelMovement.SetTrigger(DEFEAT);
        barrelDestruction.enabled = true;
    }

    #endregion Animation control
}
