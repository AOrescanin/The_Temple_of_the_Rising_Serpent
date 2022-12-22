using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    float timerValue;
    bool isDashing;
    float timerCooldown;
    public float fillFraction;
    [SerializeField] Image timeImage;

    private void Start() 
    {
        // links timer to dash
        timerCooldown = FindObjectOfType<PlayerMovement>().dashingCooldown; 
    }
    void Update()
    {
        isDashing = FindObjectOfType<PlayerMovement>().isDashing;
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (isDashing)
        {
            timerValue = 0;
        }

        if(timerValue <= timerCooldown)
        {
            timerValue += Time.deltaTime;
            fillFraction = timerValue / timerCooldown;
        }

        timeImage.fillAmount = fillFraction;
    }
}
