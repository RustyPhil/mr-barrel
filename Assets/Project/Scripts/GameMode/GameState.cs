using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Статический контроллер игрового процесса.<br/>
/// Управляет такими глобальными параметрами как подсчет очков, победа и поражение.
/// </summary>
public static class GameState
{
    #region Parameters
    
    /// <summary>
    /// Флаг паузы игры. Обычно пауза наступает после победы или поражения.
    /// </summary>
    private static bool paused = false;
    
    /// <summary>
    /// Количество очков, набранных в этом раунде.
    /// </summary>
    private static int score = 0;
    
    /// <summary>
    /// Лучший результат игрока за все время.
    /// </summary>
    private static int bestScore = 0;

    /// <summary>
    /// Ключ хранения лучшего результата игрока в данных приложения.
    /// </summary>
    private const string BEST = "BestScore";
    
    /// <summary>
    /// Очки набранные игроком, разбитые на категории по источнику получения.
    /// </summary>
    // TODO: заменить использование длинных строк локализации на более эффективные ключи.
    private static readonly Dictionary<string, int> scoreByTypes = new Dictionary<string, int>();
    
    #endregion

    #region Getters

    /// <summary>
    /// Флаг паузы игры. Обычно пауза наступает после победы или поражения.
    /// </summary>
    public static bool IsPaused => paused;
    
    /// <summary>
    /// Количество очков, набранных в этом раунде.
    /// </summary>
    public static int CurrentScore => score;
    
    /// <summary>
    /// Лучший результат игрока за все время.
    /// </summary>
    public static int BestScore => bestScore;

    /// <summary>
    /// Очки набранные игроком, разбитые на категории по источнику получения.
    /// </summary>
    public static Dictionary<string, int> ScoreByTypes => scoreByTypes;
    
    #endregion

    #region Events

    /// <summary>
    /// Вызывается при обновлении набранных игроком очков.
    /// </summary>
    public static Action<int> OnScoreUpdate;
    
    /// <summary>
    /// Вызывается при досижении контрольной точки на карте. 
    /// </summary>
    public static Action<int> OnPointReached;

    /// <summary>
    /// Отправляет true при победе игрока и false при поражении.
    /// </summary>
    public static Action<bool> OnGameEnd;

    #endregion
    
    #region Methods

    /// <summary>
    /// Сброс состоянния партии. Вызывается при загрузке сцены.
    /// </summary>
    private static void ResetGame()
    {
        score = 0;
        paused = false;
        scoreByTypes.Clear();

        if (PlayerPrefs.HasKey(BEST))
            bestScore = PlayerPrefs.GetInt(BEST);
        else
            bestScore = 0;

        OnScoreUpdate?.Invoke(score);
    }

    /// <summary>
    /// Добавление очков игроку.
    /// </summary>
    public static void AddScore(int add, string type)
    {
        score += add;
        scoreByTypes[type] = scoreByTypes.GetValueOrDefault(type, 0) + add;
        OnScoreUpdate?.Invoke(score);
    }

    /// <summary>
    /// Достижение ключевой точки на карте.
    /// </summary>
    public static void ReachPoint(int pointId) => OnPointReached?.Invoke(pointId);

    /// <summary>
    /// Завершение игры
    /// </summary>
    /// <param name="win">true - победа в игре, false - поражение</param>
    public static void EndGame(bool win)
    {
        if(paused)
            return;
        
        paused = true;
        if(score > bestScore)
            PlayerPrefs.SetInt(BEST, score);
        
        OnGameEnd?.Invoke(win);
    }

    /// <summary>
    /// Перезапуск уровня. Вызывает перезагрузку сцены.
    /// </summary>
    public static void Restart()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        ResetGame();
    }
    
    #endregion
}
