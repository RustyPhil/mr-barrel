using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Компонент, анимированно обновляющий значение числа в текстовой строке.
/// </summary>
[RequireComponent(typeof(Text))]
public class NumberIndicator : MonoBehaviour
{
    #region Parameters

    /// <summary>
    /// Время проигрывания анимации.
    /// </summary>
    [SerializeField] private float animationTime = 1f;
    
    /// <summary>
    /// Ссылка на текстовое поле. Заполняется автоматически.
    /// </summary>
    private Text text;
    
    /// <summary>
    /// Текущее значение.
    /// </summary>
    private int currentNumber = 0;
    
    /// <summary>
    /// Целевое значение.
    /// </summary>
    private int targetNumber = 0;

    /// <summary>
    /// Процесс проигрывания анимации.
    /// </summary>
    private Coroutine countRoutine;
    
    #endregion

    #region Base methods

    private void Awake() => text = GetComponent<Text>();
    
    private void OnEnable()
    {
        // За время отключение компонента данные, скорее всего, устарели. Нет нужды в анимации.
        currentNumber = targetNumber;
        text.text = currentNumber.ToString();
    }

    private void OnDisable()
    {
        // Остановка анимации.
        if (countRoutine == null)
            return;
        
        StopCoroutine(countRoutine);
        countRoutine = null;
    }
    
    #endregion

    #region Indicator

    /// <summary>
    /// Обновить значение индикатора
    /// </summary>
    /// <param name="newNumber">Новое числовое значение</param>
    /// <param name="instant">Флаг пропуска анимации</param>
    public void UpdateText(int newNumber, bool instant = false)
    {
        targetNumber = newNumber;
        if (countRoutine != null)
        {
            StopCoroutine(countRoutine);
            countRoutine = null;
        }

        if (instant || !gameObject.activeInHierarchy)
            currentNumber = newNumber;
        else
            countRoutine = StartCoroutine(CountRoutine());
    }

    /// <summary>
    /// Процесс обновления значения.
    /// </summary>
    private IEnumerator CountRoutine()
    {
        int startingNumber = currentNumber;
        float timePassed = 0f;

        while (targetNumber != currentNumber)
        {
            yield return null;
            // Поскольку значение целочисленное а таймер - с плавающей точкой, нужно считать не само число а время.
            // В противном случае округление может съесть весь прогресс и создать бесконечный цикл.
            timePassed += Time.deltaTime;
            if (timePassed >= animationTime)
                currentNumber = targetNumber;
            else
                currentNumber = (int)Mathf.Lerp(startingNumber, targetNumber, timePassed / animationTime);

            text.text = currentNumber.ToString();
        }

        countRoutine = null;
    }
    
    #endregion
}
