using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Окно завернеия игры
/// </summary>
// TODO: добавить настраиваемые на компонентах конфигурации интерфейсов и убрать весь этот страшный хардкод.
public class EndGame : MonoBehaviour
{
    #region Parameters
    
    /// <summary>
    /// Контейнер, содержащий в себе все прочие части интерфейса.
    /// </summary>
    [SerializeField] private GameObject mainContainer;
    
    /// <summary>
    /// Заголовок окна, используемый при победе.
    /// </summary>
    [Header("Title")]
    [SerializeField] private GameObject titleVictory;
    
    /// <summary>
    /// Заголовок окна, используемый при поражении.
    /// </summary>
    [SerializeField] private GameObject titleDefeat;

    /// <summary>
    /// Подробная информация об источниках получения очков.
    /// </summary>
    [Header("Score list")]
    [SerializeField] private UIList scoreList;
    
    /// <summary>
    /// Контейнер общего количества очков.
    /// </summary>
    [Header("Total score")]
    [SerializeField] private GameObject totalScoreContainer;
    
    /// <summary>
    /// Индикатор общего количества очков.
    /// </summary>
    [SerializeField] private NumberIndicator totalScore;

    /// <summary>
    /// Объект, отображаемый если игрок побил свой прежний рекорд.
    /// </summary>
    [Header("Best score")]
    [SerializeField] private GameObject newBest;
    
    /// <summary>
    /// Объект, отображаемый если игрок не смог побить свой рекорд.
    /// </summary>
    [SerializeField] private GameObject bestScoreContainer;
    
    /// <summary>
    /// Значение актуального рекорда.
    /// </summary>
    [SerializeField] private NumberIndicator bestScore;

    // Пауза жду показом элементов списка.
    [Header("Animation")]
    [SerializeField] private float pauseOnElement = 0.25f;

    
    /// <summary>
    /// Процесс заполнения окна.
    /// </summary>
    private Coroutine loadingRoutine;
    
    #endregion

    #region Base mathods
    
    private void Awake() => GameState.OnGameEnd += Init;


    private void OnDestroy() => GameState.OnGameEnd -= Init;
    
    #endregion

    #region UI
    
    /// <summary>
    /// Заполнение инерфейса. Вызывается в момент завешения игры.
    /// </summary>
    /// <param name="win"></param>
    private void Init(bool win)
    {
        // Включение самого окна.
        mainContainer.SetActive(true);
        
        // Выбор заголовка.
        titleVictory.SetActive(win);
        titleDefeat.SetActive(!win);
        
        // Скрытие элементов, отображаемых в анимации.
        totalScoreContainer.SetActive(false);
        bestScoreContainer.SetActive(false);
        newBest.SetActive(false);
        scoreList.HideElements();

        // Запуск анимации
        loadingRoutine = StartCoroutine(ScoreRoutine());
    }
    
    /// <summary>
    /// Анимированное заполнение интерфейсов.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScoreRoutine()
    {
        yield return null;
        
        var wait =  new WaitForSeconds(pauseOnElement);
        
        // Постепенное заполнение списка источников очков, полученных игроком.
        foreach (KeyValuePair<string, int> pair in GameState.ScoreByTypes)
        {
            yield return wait;
            UIScoreElement element = scoreList.AddNew() as UIScoreElement;
            if (element == null)
                continue;
            
            element.Init(pair.Key, pair.Value);
            
        }
        
        yield return wait;
        
        // Показ общего количества набранных очков.
        totalScoreContainer.SetActive(true);
        totalScoreContainer.transform.SetAsLastSibling();
        
        totalScore.gameObject.SetActive(true);
        totalScore.UpdateText(0, true);
        totalScore.UpdateText(GameState.CurrentScore);
        
        yield return wait;

        // Показ информации о рекордах.
        if (GameState.CurrentScore <= GameState.BestScore)
        {
            // Случай когда рекорд не побит.
            bestScoreContainer.SetActive(true);
            bestScoreContainer.transform.SetAsLastSibling();
            
            bestScore.gameObject.SetActive(true);
            bestScore.UpdateText(0, true);
            bestScore.UpdateText(GameState.CurrentScore);
        }
        else
        {
            // Повый рекорд.
            newBest.SetActive(true);
            newBest.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Метод вызывается нажатием кнопки в интерфейсе.
    /// </summary>
    public void RestartGame() => GameState.Restart();
    
    #endregion
}
