using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtifactTags : MonoBehaviour
{
    public List<GameObject> tagged = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0))
        {
            for(int i = 0; i < tagged.Count; i++)
            {
                tagged[i].SetActive(true);
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            for (int i = 0; i < tagged.Count; i++)
            {
                tagged[i].SetActive(false);
            }
        }
    }

    public void addTag(GameObject obj)
    {
        if (tagged.Count < 3)
        {
            tagged.Add(obj);
            obj.GetComponent<grabbable>().tagged = true;
            obj.SetActive(false);  
        }
    }
}
