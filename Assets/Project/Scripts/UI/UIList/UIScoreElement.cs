using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Элемент интерфейсного списка количества очков, полученных из разных источников.
/// </summary>
public class UIScoreElement : UIListElement
{
    /// <summary>
    /// Текстовое поле, отображающее название источника.
    /// </summary>
    [SerializeField] private Text title;
    
    /// <summary>
    /// Количество очков. Заплняется с анимацией.
    /// </summary>
    [SerializeField] private NumberIndicator count;
    
    public void Init(string myTitle, int myNumber)
    {
        title.text = myTitle;
        count.UpdateText(0, true);
        count.UpdateText(myNumber);
    }

}

