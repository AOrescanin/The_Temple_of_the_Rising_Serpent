using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float bounceForce = 18f;
    [SerializeField] AudioSource bounceSound;
    private Animator jumpPadAnimator;

    void Awake() 
    {
        jumpPadAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
            jumpPadAnimator.SetTrigger("bounce");
            bounceSound.Play();
        }
    }
}
