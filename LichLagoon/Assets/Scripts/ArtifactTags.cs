using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtifactTags : MonoBehaviour
{
    public List<GameObject> tagged = new List<GameObject>();
    public Vector3 pos1, pos2, pos3, iPos1, iPos2, iPos3;
    public bool display = false;

    public float oneForMod, twoForMod, twoSideMod, thrForMod, thrSideMod;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(transform.parent.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        iPos1 = transform.localPosition + (transform.forward * oneForMod);
        iPos2 = transform.localPosition + (transform.forward * twoForMod) + (-transform.right * twoSideMod);
        iPos3 = transform.localPosition + (transform.forward * thrForMod) + (transform.right * thrSideMod);


        if (!(SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Rilee Home")))
        {
            for(int i = 0; i < tagged.Count; i++)
            {
                tagged[i].SetActive(true);
                if (i == 0)
                    tagged[i].transform.position = pos1;
                if (i == 1)
                    tagged[i].transform.position = pos2;
                if (i == 2)
                    tagged[i].transform.position = pos3;
            }
        }
        if ((SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Rilee Home")))
        {
            if (display)
            {
                for (int i = 0; i < tagged.Count; i++)
                {
                    Rigidbody rb = tagged[i].GetComponent<Rigidbody>();
                    tagged[i].SetActive(true);
                    tagged[i].GetComponent<rotater>().activeRotate = true;
                    tagged[i].transform.localScale = tagged[i].GetComponent<grabbable>().grabS;
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        //rb.useGravity = false;
                    }
                    if (i == 0)
                        tagged[i].transform.position = iPos1;
                    if (i == 1)
                        tagged[i].transform.position = iPos2;
                    if (i == 2)
                        tagged[i].transform.position = iPos3;
                }
            }
            else
            {
                for (int i = 0; i < tagged.Count; i++)
                {
                    tagged[i].SetActive(false);
                }
            } 
        }
    }

    public void addTag(GameObject obj)
    {
        if (tagged.Count < 3)
        {
            tagged.Add(obj);
            obj.GetComponent<grabbable>().inInventory = true;
            obj.GetComponent<grabbable>().tagged = true;
            obj.SetActive(false);  
        }
    }
}
