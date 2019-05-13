using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class spriteAlternator : MonoBehaviour
{

    public bool changed = false;
    public float delay = 0f;

    public Sprite sp0, sp1;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(alternate());
    }

    IEnumerator alternate()
    {
        yield return new WaitForSeconds(delay);

        if (!changed)
        {
            this.GetComponent<Image>().sprite = sp1;
            changed = true;
        }
        else
        {
            this.GetComponent<Image>().sprite = sp0;
            changed = false;
        }
        StartCoroutine(alternate());
    }
}
