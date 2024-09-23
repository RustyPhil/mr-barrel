using UnityEngine;

/// <summary>
/// Индикатор набранных очков
/// </summary>
public class ScoreIndicator : MonoBehaviour
{
    /// <summary>
    /// Ссылка на поле, анимированно заполняющее очки.
    /// </summary>
    [SerializeField] private NumberIndicator indicator;
    
    private void Start()
    {
        indicator.UpdateText(GameState.CurrentScore);
        GameState.OnScoreUpdate += UpdateScore;
        GameState.OnGameEnd += Hide;
    }

    private void OnDestroy()
    {
        GameState.OnScoreUpdate -= UpdateScore;
        GameState.OnGameEnd -= Hide;
    }

    /// <summary>
    /// Вызывается при изменении количества очков у игрока.
    /// </summary>
    void UpdateScore(int newScore) => indicator.UpdateText(newScore);

    /// <summary>
    /// При завершении игры поле скрывается.
    /// </summary>
    // TODO: при расширении функционала главного интерфейса игры, этот функционал стоит вынести из отдельных полей в центральный менеджер.
    private void Hide(bool win) => gameObject.SetActive(false);
}
