using UnityEngine;
using System.Collections;

/// <summary>
/// Объект, который может храниться в пуле. Применяется для многократно используемых в игровом процессе эффектов и префабов.
/// </summary>
public class PoolableItem : MonoBehaviour
{
    #region Parameters
    
    /// <summary>
    /// Убирать ли объект обратно в пул по времени.
    /// </summary>
    [SerializeField] private bool autoDespawn;
    
    /// <summary>
    /// Через какое время убирать предмет обрато в пул. 
    /// </summary>
    [SerializeField] private float despawnTime = 2f;

    /// <summary>
    /// Процесс ожидания возвращения объекта в пул.
    /// </summary>
    private Coroutine DespawnRoutine;

    /// <summary>
    /// Хаэ имени префаба.
    /// </summary>
    private int id;
    
    /// <summary>
    /// Хаэ имени префаба.
    /// </summary>
    public int ID => id;
    
    #endregion

    #region Base methods
    
    private void OnDisable()
    {
        if (DespawnRoutine != null)
        {
            StopCoroutine(DespawnRoutine);
            DespawnRoutine = null;
        }
    }
    
    #endregion

    #region Main methods

    /// <summary>
    /// При инициализации объекту задается его хэш.
    /// </summary>
    public void Init(int myId) => id = myId;

    /// <summary>
    /// Запуск ожидания автоматического возвращения в пул объектов.
    /// </summary>
    public void StartPlay()
    {
        if (autoDespawn && despawnTime > 0f)
            DespawnRoutine = StartCoroutine(WaitAndDespawn());
    }

    /// <summary>
    /// Ожидание автоматического возвращения в пул объектов.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitAndDespawn()
    {
        yield return new WaitForSeconds(despawnTime);
        PrefabPool.ReturnToPool(this);
    }
    
    #endregion
}
