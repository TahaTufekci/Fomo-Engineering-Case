using States;
using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        public Vector2 SwipeDirection { get; private set; }
        public bool SwipeDetected { get; private set; }

        private Vector2 _startPosition;
        private Vector2 _currentPosition;
        private bool _isSwiping = false;
        private Camera _mainCamera;
        private RaycastHit[] _hit;
        private int _hitCount;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _hit = new RaycastHit[1];
        }

        private void Update()
        {
            if (GameManager.Instance.currentGameState != GameState.WaitingInput) return;
            SwipeDetector();
            GenerateInput();
        }

        private void SwipeDetector()
        {
            SwipeDetected = false;
            if (Input.GetMouseButtonDown(0))
            {
                _startPosition = Input.mousePosition;
                _isSwiping = true;
            }

            if (Input.GetMouseButton(0) && _isSwiping)
            {
                _currentPosition = Input.mousePosition;
                Vector2 swipe = _currentPosition - _startPosition;

                if (swipe.magnitude >= 50) // Threshold for swipe length
                {
                    swipe.Normalize();
                    if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                    {
                        SwipeDirection = swipe.x > 0 ? Vector2.right : Vector2.left;
                    }
                    else
                    {
                        SwipeDirection = swipe.y > 0 ? Vector2.up : Vector2.down;
                    }

                    SwipeDetected = true;
                    _isSwiping = false; // Stop further swipes until the button is pressed again
                }
            }
        }

        private void GenerateInput()
        {
            if (SwipeDetected)
            {
                // Check for hit object
                var hitObject = GetHitObject(_startPosition);
                if (hitObject == null)
                {
                    Debug.Log("No object hit.");
                    return;
                }

                // Handle swipe on BlockCell
                if (hitObject.TryGetComponent(out BlockCell block))
                {
                    GameManager.Instance.OnBlockSwiped?.Invoke(block, SwipeDirection);
                    SwipeDetected = false; // Reset swipe detection after handling the swipe
                }
            }
        }

        private GameObject GetHitObject(Vector3 position)
        {
            _hitCount = Physics.RaycastNonAlloc(_mainCamera.ScreenPointToRay(position), _hit);
            if (_hitCount > 0 && _hit[0].collider != null)
            {
                return _hit[0].collider.gameObject;
            }
            return null;
        }

    }
}
