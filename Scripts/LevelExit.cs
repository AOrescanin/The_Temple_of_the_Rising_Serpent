using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] float levelLoadDelay = 2.1f;
    [SerializeField] AudioSource lvlUpSound;
    [SerializeField] Animator transition;

    void OnTriggerEnter2D(Collider2D other) 
    {
        lvlUpSound.Play();
        StartCoroutine(loadNextLevel());
    }

    //loads the next level after a delay if there is another level
    IEnumerator loadNextLevel()
    {
        transition.SetTrigger("Fade");

        yield return new WaitForSecondsRealtime(levelLoadDelay);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if(nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }
}
