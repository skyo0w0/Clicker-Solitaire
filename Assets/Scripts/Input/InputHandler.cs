using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // События для передачи данных о положении мыши и кликах
    public event Action<Vector2> OnMouseMove;
    public event Action<Vector2> OnMouseClick;
    public event Action OnClickCanceled; 
    // Новое событие для отмены клика
    private Vector2 _pointerWorldPosition;
    private InputSystem_Actions _inputSystem;

    private void Awake()
    {
        _inputSystem = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputSystem.Enable();

        // Подписка на события ввода
        _inputSystem.Player.Point.performed += OnPointPerformed;
        _inputSystem.Player.Click.performed += OnClickPerformed;
        _inputSystem.Player.Click.canceled += OnClickCanceledHandler; // Подписка на canceled
    }

    private void OnDisable()
    {
        // Отписка от событий ввода
        _inputSystem.Player.Point.performed -= OnPointPerformed;
        _inputSystem.Player.Click.performed -= OnClickPerformed;
        _inputSystem.Player.Click.canceled -= OnClickCanceledHandler;

        _inputSystem.Disable();
    }

    // Обработчик движения мыши
    private void OnPointPerformed(InputAction.CallbackContext context)
    {
        Vector2 pointerPosition = context.ReadValue<Vector2>();
        _pointerWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(pointerPosition.x, pointerPosition.y, 0));
        OnMouseMove?.Invoke(_pointerWorldPosition); 
    }

    // Обработчик клика мыши
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        OnMouseClick?.Invoke(_pointerWorldPosition);
    }

    // Обработчик отмены клика
    private void OnClickCanceledHandler(InputAction.CallbackContext context)
    {
        OnClickCanceled?.Invoke();
        Debug.Log("Click canceled");
    }
}
