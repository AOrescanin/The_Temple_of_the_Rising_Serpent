using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingPlatform : MonoBehaviour
{
    [SerializeField] public Transform pos1, pos2, startPos;
    [SerializeField] private float speed;
    [SerializeField] AudioSource sinkingSound;
    Vector3 nextPos;

    void Start()
    {
        nextPos = startPos.position;
    }

    void OnCollisionEnter2D(Collision2D other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            nextPos = pos2.position;
            sinkingSound.enabled = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            nextPos = pos1.position;
            sinkingSound.enabled = false;
        }
    }
    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(pos1.position, pos2.position);
    }
}
