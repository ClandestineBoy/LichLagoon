using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManger : MonoBehaviour
{
    public bool nextScene;

    // Start is called before the first frame update
    void Start()
    {
    }

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
                SceneManager.LoadScene("Intro");
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Intro"))
        {
            if (nextScene == true)
            {
                nextScene = false;
                SceneManager.LoadScene("Night2");
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Night2"))
        {
            if (nextScene == true)
            {
                nextScene = false;
                SceneManager.LoadScene("Rilee Home");
            }
        }
    }

}