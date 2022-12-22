using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] float loadDelay = 3f;
    [SerializeField] Animator transition;

    public void PlayGame()
    {
        StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
