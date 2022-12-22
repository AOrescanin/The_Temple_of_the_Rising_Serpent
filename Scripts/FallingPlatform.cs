using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] float fallDelay = .72f;
    [SerializeField] float destroyDelay = 3.6f;
    [SerializeField] float respawnDelay = 3.0f;
    [SerializeField] GameObject prefab;
    [SerializeField] Vector3 pos;
    [SerializeField] AudioSource fallSound;
    [SerializeField] AudioSource respawnSound;
    private bool hasTouched = false;

    void Start() 
    {
        //get a copy of the starting position for respawning
        pos = transform.position;
    }

    void OnCollisionEnter2D(Collision2D other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            fallSound.Play();
            StartCoroutine(Fall());
        }
    }

    private void OnCollisionExit2D(Collision2D other) 
    {
        if(other.gameObject.CompareTag("Player") && !hasTouched)
        {
            StartCoroutine(Respawn());
            hasTouched = true;
        }
    }

    //platform falls after a delay
    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallDelay);
        rb.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject, destroyDelay);
    }

    //platform respawns after a delay
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        rb.bodyType = RigidbodyType2D.Kinematic;
        Instantiate(prefab, pos, prefab.transform.rotation);
        respawnSound.Play();
        hasTouched = false;
    }
}
