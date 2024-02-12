using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RecognitionAudioTrigger : MonoBehaviour
{
    [FormerlySerializedAs("audioSource")] 
    public AudioSource recognitionAudioSource;

    void Start()
    {
        // Get the AudioSource component attached to this GameObject
        recognitionAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // // Check if a specific key is pressed, e.g., the spacebar
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     // Play the audio if the spacebar is pressed
        //     recognitionAudioSource.Play();
        // }
    }
}
