using UnityEngine;

/// <summary>
/// Класс управления с помощью базовых устройств ввода (клавиатура, геймпад).
/// </summary>
public class DeviceInput : MovementController
{

    #region Parameters
    /// <summary>
    /// Название горизонтальной оси устройства.
    /// </summary>
    [Tooltip("Название горизонтальной оси в настройках ввода")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    
    /// <summary>
    /// Название вертикальной оси устройства.
    /// </summary>
    [Tooltip("Название вертикальной оси в настройках ввода")]
    [SerializeField] private string verticalAxis = "Vertical";
    
    /// <summary>
    /// Размер невосприимчивой для управления зоны. Сигнал с магнитудой меньше этого значения будет передавать скорость равную нулю.
    /// </summary>
    [Tooltip("Невосприимчивая для управления зона, направление движения в которой равно нулю")] 
    [SerializeField] private float deadzoneRange = 0.1f;

    /// <summary>
    /// Флаг нахождения в слепой зоне более одного кадра.
    /// </summary>
    private bool isIdle = true;
    
    #endregion Parameters

    private void OnEnable() => isIdle = true;
    
    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis));
        if(input.magnitude < deadzoneRange && isIdle)
            return;

        // Обновление флагов, начало или завершение движения. Передача контроля над аватаром.
        if (input.magnitude < deadzoneRange)
        {
            isIdle = true;
            EndMove();
            return;
        }

        if (isIdle)
        {
            StartMove();
            isIdle = false;
        }
        
        DirectionUpdate(input.normalized);
    }
    
    // Если контроллер не собирается принимать сигнал с этого устройства ввода, то компонент полностью выключается.
    public override void SetActiveInput(bool enable)
    {
        base.SetActiveInput(enable);
        enabled = enable;
    }
}
