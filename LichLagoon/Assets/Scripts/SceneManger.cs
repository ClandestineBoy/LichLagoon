using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManger : MonoBehaviour
{
    public bool nextScene;
    public string nextName;
    public float delay;

    public Image fadeOutRect;

    // Update is called once per frame
    void Update()
    {
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
                fadeOut();
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Intro"))
        {
            if (nextScene == true)
            {
                nextScene = false;
                nextName = "Night2";
                fadeOut();
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Night2"))
        {
            if (nextScene == true)
            {
                nextScene = false;
                nextName = "Rilee Home";
                fadeOut();
            }
        }
    }

    void fadeOut()
    {
        fadeOutRect.color = Vector4.MoveTowards(fadeOutRect.color, Color.black, Time.fixedDeltaTime * delay);

        if (fadeOutRect.color == Color.black)
        {
            SceneManager.LoadScene(nextName);
        }
    }
}