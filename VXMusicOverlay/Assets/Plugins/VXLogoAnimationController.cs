using UnityEngine;

//using Valve.VR;

namespace Plugins
{
    public class VXLogoAnimationController : MonoBehaviour
    {
        private Animator _animator;

        private VXMusicOverlay _vxmOverlay;

        private string _animationName = "VXMRecognition";
        private bool _isInProcessingState = false;
        
        private float _animationTimer = 0f;  // Timer to track animation duration
        private float _timeoutThreshold = 25f;  // 10 seconds timeout threshold

        // Start is called before the first frame update
        void Start()
        {
            GameObject _vxmOverlayGameObject = GameObject.Find("OverlayScriptWrapper");
        
            if (_vxmOverlayGameObject != null)
            {
                _vxmOverlay = _vxmOverlayGameObject.GetComponent<VXMusicOverlay>();

                if (_vxmOverlay != null)
                {
                    _vxmOverlay.OnRecognitionStateBegin += HandleRecognitionStateBegin;
                    _vxmOverlay.OnRecognitionStateEnded += HandleRecognitionStateEnd;
                }
            }
        
            _animator = GetComponent<Animator>();
            
            // Make sure it doesn't run at the beginning
            _animator.enabled = false;
            
            if (_animator == null)
            {
                Debug.LogError("Animator component not found on this GameObject.");
            }
        }
    
        // Event handler method for value change
        private void HandleRecognitionStateBegin()
        {
            Debug.Log("Begin Animation Controller Recognition State");

            _isInProcessingState = true;
            
            _animator.enabled = true;
            _animator.Play(_animationName, -1, 0f);
            _animator.speed = 1;
            
            _animationTimer = 0f;  // Reset and start the timer
        }

        private void HandleRecognitionStateEnd()
        {
            Debug.Log("End Animation Controller Recognition State");

            _isInProcessingState = false;
            
            _animator.bodyPosition = _animator.rootPosition;
            
            _animationTimer = 0f;  // Reset and start the timer
        }
        
        private void OnDestroy()
        {
            if (_vxmOverlay != null)
            {
                _vxmOverlay.OnRecognitionStateBegin -= HandleRecognitionStateBegin;
                _vxmOverlay.OnRecognitionStateEnded -= HandleRecognitionStateEnd;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //if (_isInProcessingState)
            //{
                _animationTimer += Time.deltaTime;

                if (_animator.enabled && _animationTimer >= _timeoutThreshold)
                {
                    Debug.LogWarning("Animation timeout exceeded. Resetting animation and controller.");
                    HandleRecognitionStateEnd();
                    
                    // Optionally, force animation to the last frame if not exactly finishing at the end.
                    _animator.Play(_animationName, -1, 0f);
                    _animator.speed = 0; // Stop the animation
                    
                    _animator.enabled = false; // Disable the animator until the next run 
                }
                
                if (!_isInProcessingState && _animator.enabled && _animator.GetCurrentAnimatorStateInfo(0).IsName(_animationName))
                {
                    float progress = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
                    if (progress >= 0.98f)
                    {
                        // Optionally, force animation to the last frame if not exactly finishing at the end.
                        _animator.Play(_animationName, -1, 0f);
                        _animator.speed = 0; // Stop the animation
                    
                        _animator.enabled = false; // Disable the animator until the next run 
                    }
                }
            //}
        }
    }
}
