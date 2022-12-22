using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] public Transform pos1, pos2, startPos;
    [SerializeField] private float speed;
    Vector3 nextPos;

    void Start()
    {
        nextPos = startPos.position;
    }

    void FixedUpdate()
    {
        if(transform.position == pos1.position)
        {
            nextPos = pos2.position;
        }

        if(transform.position == pos2.position)
        {
            nextPos = pos1.position;
        }

        transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(pos1.position, pos2.position);
    }
}
