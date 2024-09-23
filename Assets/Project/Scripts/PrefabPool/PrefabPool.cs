using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Пул префабов, пригодных для многократного использования в рамках одного прохождения уровня.
/// </summary>
public class PrefabPool : MonoBehaviour
{
    #region Type
    
    /// <summary>
    /// Тип данных, используемый для хранения настроек отдельного префаба.
    /// </summary>
    [Serializable]
    private class PoolType
    {
        /// <summary>
        /// Имя пула. Его хэш используется как идентификатор типа объектов.
        /// </summary>
        [SerializeField] private string name;
        
        /// <summary>
        /// Префаб объекта, хранится в ассетах.
        /// </summary>
        public PoolableItem prefab;
        
        /// <summary>
        /// Количество копий объекта, которое нужно положить в пул при запуске игры.
        /// </summary>
        public int initOnLoad = 0;

        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        private int id = 0;
        
        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public int ID => id;

        /// <summary>
        /// Генерация идентификатора.
        /// </summary>
        public void Init() => id = Animator.StringToHash(name);
    }
    
    #endregion

    #region Parameters

    /// <summary>
    /// Настройки префабов и их пулов.
    /// </summary>
    [SerializeField] private PoolType[] poolableItems;
    
    /// <summary>
    /// Реализация синглтона
    /// </summary>
    // TODO: избавиться от синглтона.
    private static PrefabPool instance;

    /// <summary>
    /// Хранилище объектов.
    /// </summary>
    private Dictionary<int, Stack<PoolableItem>> pool = new Dictionary<int, Stack<PoolableItem>>();
    
    #endregion

    #region Base methods
    
    /// <summary>
    /// Инициализация пула. Создания хранилища, заполнение начальными объектами.<br/>
    /// Процесс заполения пуля растянут на несколько кадров.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Start()
    {
        instance = this;
        
        // Создание хранилища. Хэши имен используются как ключи словаря.
        foreach (PoolType entry in poolableItems)
        {
            entry.Init();
            pool[entry.ID] = new Stack<PoolableItem>();
        }

        yield return null;
        
        // Заполнение пула, по объекту за кадр.
        foreach (PoolType entry in poolableItems)
        {
            for(int i = 0; i < entry.initOnLoad; i++)
            {
                SpawnItem(entry);
                yield return null;
            }
        }
    }

    private void OnDestroy() => instance = null;
    
    #endregion

    #region Pool

    /// <summary>
    /// Размещение нового объекта в пуле.
    /// </summary>
    /// <param name="itemID">Хэш названия объекта.</param>
    private void SpawnItem(int itemID)
    {
        foreach (PoolType entry in poolableItems)
        {
            if(entry.ID != itemID)
                continue;
            
            SpawnItem(entry);
            return;
        }
    }

    
    /// <summary>
    /// Размещение нового объекта в пуле.
    /// </summary>
    /// <param name="entry">Полные данные об объекте.</param>
    private void SpawnItem(PoolType entry)
    {
        GameObject newItem = Instantiate(entry.prefab.gameObject);
        PoolableItem itemData = newItem.GetComponent<PoolableItem>();
        itemData.Init(entry.ID);
        pool[entry.ID].Push(itemData);
        
        ParkItem(newItem);
    }

    /// <summary>
    /// Разместить объект в пуле.<br/>
    /// Объект становится наследником пула, его локальная позиция и вращение обнуляются. Сам объект выключается.
    /// </summary>
    private void ParkItem(GameObject item)
    {
        item.SetActive(false);
        Transform newTransform = item.transform;
        newTransform.SetParent(transform);
        newTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// Получить объект из пула.<br/>
    /// Если в пуле нет достаточного количества объектов, создается новый.
    /// </summary>
    public static GameObject GetFromPool(int itemID, Vector3 position)
    {
        if (instance == null || !instance.pool.ContainsKey(itemID))
            return null;
        
        if(instance.pool[itemID].Count == 0)
            instance.SpawnItem(itemID);

        PoolableItem result = instance.pool[itemID].Pop();
        result.transform.SetParent(null);
        result.transform.position = position;
        result.gameObject.SetActive(true);
        result.StartPlay();
        
        return result.gameObject;
    }
    
    /// <summary>
    /// Вернуть объект в пул.<br/>
    /// Если объект не может храниться в пуле, то он уничтожается.
    /// </summary>
    public static void ReturnToPool(PoolableItem item)
    {
        if (instance == null || !instance.pool.ContainsKey(item.ID))
            Destroy(item.gameObject);
        else
        {
            instance.pool[item.ID].Push(item);
            instance.ParkItem(item.gameObject);
        }
    }
    
    #endregion
}
