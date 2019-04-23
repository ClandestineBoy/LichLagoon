using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPoint : MonoBehaviour
{

    public GameObject targetObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = targetObj.transform.position;
    }
}
