using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Заполняемый список для интерфейсов. Предназначен для использования с Layout.
/// </summary>
// TODO: при массовом использовании стоит создать отдельные наследники под все типы элементов и избегать приведения типов.
public class UIList : MonoBehaviour
{
    #region Parameters
    
    /// <summary>
    /// Префаб, которым будет заполняться данный список.
    /// </summary>
    [SerializeField] private UIListElement prefab;
    
    /// <summary>
    /// Активные элементы, отображаемые в интерфейсе в настоящий момент. 
    /// </summary>
    private List<UIListElement> activeElements = new List<UIListElement>();
    
    /// <summary>
    /// Пул элементов, скрытых из интерфейса.
    /// </summary>
    private Stack<UIListElement> inactiveElements = new Stack<UIListElement>();
    
    #endregion

    #region Methods
    
    /// <summary>
    /// Получение элемента списка. Элемент либо получается из пула, либо создается.<br/>
    /// Большие списки стоит заполнять в несколько кадров.
    /// </summary>
    public UIListElement AddNew()
    {
        UIListElement element;
        // Получение объекта из пула.
        if (inactiveElements.Count > 0)
            element = inactiveElements.Pop();
        // Создание нового объекта.
        else
        {
            GameObject newElement = Instantiate(prefab.gameObject);
            element = newElement.GetComponent<UIListElement>();
            
            element.transform.SetParent(transform);
            element.transform.localScale = Vector3.one;
            element.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        element.transform.SetAsLastSibling();
        element.gameObject.SetActive(true);
        
        return element;
    }

    // Перемещение всех объектов в пул.
    public void HideElements()
    {
        foreach (UIListElement element in activeElements)
        {
            element.gameObject.SetActive(false);
            inactiveElements.Push(element);
        }
        
        activeElements.Clear();
    }
    
    #endregion
}