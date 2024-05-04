using UnityEngine;

//using Valve.VR;

namespace Plugins
{
    public class VXLogoAnimationController : MonoBehaviour
    {
        private Animator _animator;

        private VXMusicOverlay _vxmOverlay;

        private string _animationName = "VXMRecognition";

        private bool _isVxInProcessingState = false;
    
        // Start is called before the first frame update
        void Start()
        {
            GameObject _vxmOverlayGameObject = GameObject.Find("OverlayScriptWrapper");
        
            if (_vxmOverlayGameObject != null)
            {
                _vxmOverlay = _vxmOverlayGameObject.GetComponent<VXMusicOverlay>();

                if (_vxmOverlay != null)
                {
                    // Subscribe to the event in ScriptA
                    _vxmOverlay.OnRecognitionStateTriggered += HandlePublicValueChange;
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
        private void HandlePublicValueChange(bool newValue)
        {
            Debug.Log("Value from EasyOpenVROverlayForUnity changed to: " + newValue);

            if (newValue)
            {
                //MainThreadDispatcher.ExecuteOnMainThread(() =>
                //{
                    _isVxInProcessingState = true;
                    
                    _animator.enabled = true;
                    _animator.Play(_animationName, -1, 0f);
                    _animator.speed = 1;
                
                //});
            }
            else
            {
                var resetState = _animator.rootPosition;

                _animator.bodyPosition = resetState;
                
                _isVxInProcessingState = false;
                
            }
        }

        // Remember to unsubscribe from the event when ScriptB is destroyed
        private void OnDestroy()
        {
            if (_vxmOverlay != null)
            {
                _vxmOverlay.OnRecognitionStateTriggered -= HandlePublicValueChange;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(_animationName) && !_isVxInProcessingState )
            {
                float progress = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
                if (progress >= 0.98f)
                {
                    // Optionally, force animation to the last frame if not exactly finishing at the end.
                    _animator.Play(_animationName, -1, 0f);
                    _animator.speed = 0; // Stop the animation
                }
            }
        }
    }
}
