using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapShipScript : MonoBehaviour
{
    public Vector3 startPos, target;
    public GameObject targetObj;

    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        startPos = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        target = targetObj.transform.localPosition;
        this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, target, Time.fixedDeltaTime * speed);
    }
}
