using UnityEngine;
using System; // For EventHandler

public class InputManager : MonoBehaviour
{
    public static Action<Vector3> OnDragged;
    public static Action OnDragEnded;
    public static Action<Vector2> OnDoubleTap;


    public static Vector3 InputPosition;

    private bool isEditor;
    private Vector2 startDragPosition;
    private bool isDragging;
    private const float MINIMUM_DRAG_DISTANCE = 50f;
    private float lastTapTime = 0;
    private const float DOUBLE_TAP_THRESHOLD = 0.3f;

    private void Awake()
    {
        isEditor = Application.isEditor;
    }

    private void Update()
    {
        if (isEditor)
        {
            HandleEditorInput();
        }
        else
        {
            HandleMobileInput();
        }
    }

    private void HandleEditorInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startDragPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            OnDragEnded?.Invoke();
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 _currentDragPosition = Input.mousePosition;
            float _dragDistance = Vector2.Distance(startDragPosition, _currentDragPosition);
            if (_dragDistance >= MINIMUM_DRAG_DISTANCE)
            {
                Vector2 _dragDirection = (_currentDragPosition - startDragPosition).normalized;
                OnDragged?.Invoke(_dragDirection);
                startDragPosition = _currentDragPosition; // Update start position to the current position for continuous dragging
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            float _timeSinceLastTap = Time.time - lastTapTime;

            if (_timeSinceLastTap <= DOUBLE_TAP_THRESHOLD)
            {
                OnDoubleTap?.Invoke(Input.mousePosition);
            }

            lastTapTime = Time.time;
        }
    }
    
    private void HandleMobileInput()
    {
        if (Input.touchCount > 0)
        {
            Touch _touch = Input.GetTouch(0);

            switch (_touch.phase)
            {
                case TouchPhase.Began:
                    if (isDragging == false)
                    {
                        float _timeSinceLastTap = Time.time - lastTapTime;

                        if (_timeSinceLastTap <= DOUBLE_TAP_THRESHOLD)
                        {
                            OnDoubleTap?.Invoke(_touch.position);
                        }

                        lastTapTime = Time.time;
                    }

                    startDragPosition = _touch.position;
                    isDragging = true;
                    break;
                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        float _dragDistance = Vector2.Distance(startDragPosition, _touch.position);
                        if (_dragDistance >= MINIMUM_DRAG_DISTANCE)
                        {
                            Vector2 _dragDirection = (_touch.position - startDragPosition).normalized;
                            OnDragged?.Invoke(_dragDirection);
                            startDragPosition = _touch.position; // Update for continuous dragging
                        }
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isDragging)
                    {
                        OnDragEnded?.Invoke();
                        isDragging = false;
                    }
                    break;
            }
        }
    }
}