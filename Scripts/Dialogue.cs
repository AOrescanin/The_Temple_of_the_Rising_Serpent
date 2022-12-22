using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    [SerializeField] GameObject text;
    [SerializeField] AudioSource textSound;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        text.SetActive(true);
        textSound.Play();
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        text.SetActive(false);
    }
}
