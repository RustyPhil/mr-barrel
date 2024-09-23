using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Экранный джойстик. Запускается по нажатию на подложку интерфейса, движение джойстика также ограничено этой подложкой.
/// </summary>
public class ScreenJoystick : MovementController, IPointerDownHandler
{
    #region Parameters
    /// <summary>
    /// Внутренний круг джойстика, соответствующий текущему нажатию
    /// </summary>
    [Header("Elements")]
    [SerializeField] private RectTransform mainCircle;
    
    /// <summary>
    /// Внешний круг джойстика. Сдвиг внутреннего круга относительно него определяет вектор сигнала этого устройства ввода.
    /// </summary>
    [SerializeField] private RectTransform outerCircle;

    /// <summary>
    /// Максимальное расстояние между внешним и внутренним кругом.
    /// </summary>
    [Header("Sensitivity")]
    [SerializeField] private float followRange = 1.0f;

    /// <summary>
    /// Слепая зона, в пределах которой вектор скорости равен нулю.
    /// </summary>
    [SerializeField] private float deadzoneRange = 0.1f;
    
    /// <summary>
    /// Ограничения на передвижение джойстика по экрану.
    /// </summary>
    [Header("Padding")]
    [SerializeField] private float borders = 25f;
    
    
    /// <summary>
    /// Позиция главного круга на экране.
    /// </summary>
    private Vector2 lastMousePosition;
    
    /// <summary>
    /// Позиция подложки на экране.
    /// </summary>
    private Vector2 lastOuterPosition;

    /// <summary>
    /// Соотношение физического размера экрана и размера интерфейса. Используется для корвертации положения указателя в точки интерфейса.
    /// </summary>
    private float scale = 1f;
    
    /// <summary>
    /// Прямоугольник подложки джойстика.<br/>
    /// Этот прямоугольник и <paramref name="borders"/> задают конечную область.
    /// </summary>
    private Rect bounds = Rect.zero;
    
    /// <summary>
    /// RectTransform подложки джойстика.
    /// </summary>
    private RectTransform myRectTransform;

    /// <summary>
    /// Процесс управление джойстиком.
    /// </summary>
    private Coroutine joystickControl;
    
    #endregion

    #region Base methods

    private void Awake() => myRectTransform = transform as RectTransform;

    private void OnEnable()
    {
        // Обновление масштаба и отключение отображения джойстика. Отображение будет включено по нажатию на подложку.
        UpdateScale();
        mainCircle.gameObject.SetActive(false);
        outerCircle.gameObject.SetActive(false);
    }

    private void OnRectTransformDimensionsChange() => UpdateScale();

    public override void SetActiveInput(bool enable)
    {
        // При отключении этого источника ввода он выключается вместе со своим гейм объектом.
        base.SetActiveInput(enable);
        gameObject.SetActive(enable);
    }

    /// <summary>
    /// Реализация метода интерфейса IPointerDownHandler. Прослушивает нажатия на подложку джойстика.<br/>
    /// Нажатия вне подложки не активируют джойстик, однако после запуска джойстика игрок может выводить указатель за подложку.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isActive && joystickControl == null)
            joystickControl = StartCoroutine(JoystickPlay());
    }

    #endregion

    #region MovementController methods

    protected override void StartMove()
    {
        lastOuterPosition = ClampRectWithBorders(Input.mousePosition * scale, bounds, borders);
        mainCircle.gameObject.SetActive(true);
        outerCircle.gameObject.SetActive(true);
        base.StartMove();
    }

    protected override void EndMove()
    {
        mainCircle.gameObject.SetActive(false);
        outerCircle.gameObject.SetActive(false);
        base.EndMove();
    }

    #endregion
    
    #region Movement

    /// <summary>
    /// Обновление соотношения размера интерфейса и камеры.<br/>
    /// Актуально в случае использования на устройствах с возможностью менять размер окна.
    /// </summary>
    private void UpdateScale()
    {
        if (myRectTransform != null && Camera.main != null)
        {
            scale = myRectTransform.rect.width / Camera.main.pixelWidth;
            bounds = new Rect(Vector2.zero, myRectTransform.rect.size);
        }
    } 

    /// <summary>
    /// Процесс, отслеживающий перемещения указателя по экрану.
    /// </summary>
    private IEnumerator JoystickPlay()
    {
        StartMove();
        
        while(isActive && Input.GetMouseButton(0))
        {
            // Получение ближайшей к указателю точки внутри подложки. Эта точка считается положением внутреннего круга джойстика.
            lastMousePosition = ClampRectWithBorders(Input.mousePosition * scale, bounds, borders);

            Vector2 offset = lastMousePosition - lastOuterPosition;
            Vector2 direction = Vector2.ClampMagnitude(offset, followRange);
            
            // Внешний круг джойстика отстоит от границы дополнительно на followRange.
            lastOuterPosition = ClampRectWithBorders(lastOuterPosition - direction + offset, bounds, borders + followRange);
            
            mainCircle.anchoredPosition = lastMousePosition;
            outerCircle.anchoredPosition = lastOuterPosition;

            // Если расстояние между внешним и внутренним кругами меньше чем размер слепой зоны, сигнал обнуляется.
            if (direction.magnitude > deadzoneRange)
                DirectionUpdate(direction.normalized);
            else
                DirectionUpdate(Vector2.zero);

            yield return null;
        }
        
        EndMove();
        joystickControl = null;
    }

    /// <summary>
    /// Получение ближайшей к <paramref name="input"/> точки внутри прямоугольника, пределяемого <paramref name="limit"/> и <paramref name="border"/>.
    /// </summary>
    private Vector2 ClampRectWithBorders(Vector2 input, Rect limit, float border)
    {
        return new Vector2(
            Mathf.Clamp(input.x, limit.xMin + border, limit.xMax - border), 
            Mathf.Clamp(input.y, limit.yMin + border, limit.yMax - border)
            );
    }

    #endregion Movement
}