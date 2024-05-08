using System.Collections;
using System.Collections.Generic;
using Plugins;
using UnityEngine;
using UnityEngine.Serialization;

public class OverlayAudioHandler : MonoBehaviour
{
    [FormerlySerializedAs("RecognitionAudioSource")] [FormerlySerializedAs("recognitionAudioSource")] [FormerlySerializedAs("audioSource")] 
    public AudioSource OverlayAudioSource;

    private VXMusicOverlay _vxmOverlay;

    
    void Start()
    {
        // Get the AudioSource component attached to this GameObject
        OverlayAudioSource = GetComponent<AudioSource>();
        
        GameObject _vxmOverlayGameObject = GameObject.Find("OverlayScriptWrapper");
        
        if (_vxmOverlayGameObject != null)
        {
            _vxmOverlay = _vxmOverlayGameObject.GetComponent<VXMusicOverlay>();

            if (_vxmOverlay != null)
            {
                _vxmOverlay.OnRecognitionStateBegin += HandleRecognitionStateBegin;
            }
        }
    }
    
    private void HandleRecognitionStateBegin()
    {
        Debug.Log("Begin RecognitionAudioTrigger Recognition State");
            
        OverlayAudioSource.Play();
    }
}
