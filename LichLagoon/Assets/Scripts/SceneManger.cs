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
        DontDestroyOnLoad(gameObject);   
    }

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene() == SceneManager.GetSceneByName("StartScreen"))
        {
            if (Input.anyKeyDown)
            {
                nextScene = true;
            }
            if(nextScene == true)
            {
                nextScene = false;
                SceneManager.LoadScene("Intro");
            }
        }
    }
}
