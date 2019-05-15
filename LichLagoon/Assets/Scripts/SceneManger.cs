using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManger : MonoBehaviour
{
    public bool nextScene = false, finalScene = false, fading = false;
    public string nextName = "";
    public float delay, fadeSpeed;

    public Image fadeOutRect;

    private void Start()
    {
        nextName = " "; finalScene = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(finalScene == true)
        {
            finalScene = false;
            nextName = "End";
            StartCoroutine(fadeDelay());
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("StartScreen"))
        {
            if (Input.anyKeyDown)
            {
                nextScene = true;
            }
            if (nextScene == true)
            {
                nextScene = false;
                nextName = "Intro";
                StartCoroutine(fadeDelay());
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Intro"))
        {
            if (nextScene == true)
            {
                nextScene = false;
                nextName = "Night2";
                StartCoroutine(fadeDelay());
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Night2"))
        {
            if (nextScene == true)
            {
                nextScene = false;
                nextName = "Rilee Home";
                StartCoroutine(fadeDelay());
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Rilee Home"))
        {
            if (nextScene == true)
            {
                nextScene = false;
                nextName = "Night3";
                StartCoroutine(fadeDelay());
            }
        }


        if (fading)
        {
            fadeOut();
        }
    }

    void fadeOut()
    {
        fadeOutRect.color = Vector4.MoveTowards(fadeOutRect.color, Color.black, Time.fixedDeltaTime * fadeSpeed);

        if (fadeOutRect.color == Color.black)
        {
            SceneManager.LoadScene(nextName);
        }
    }

    IEnumerator fadeDelay()
    {
        yield return new WaitForSeconds(delay);
        fading = true;
    }
}