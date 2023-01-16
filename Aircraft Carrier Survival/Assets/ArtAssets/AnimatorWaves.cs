using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorWaves : MonoBehaviour
{

    //bool loop = false;
    // Start is called before the first frame update
    void Start()
    {

    }
    public float speed=1;
    public float base2 = -2;
    public float bass3 = 5;

    public float bass4 = 5;
    float baseLerp = 0;
    public bool inverted = false;
    // Update is called once per frame
    void Update()
    {
        if (inverted)
        {
            if (this.transform.localScale.x <= bass3)
            {
                baseLerp = 0;
            }

            baseLerp += Time.deltaTime / speed;
            this.transform.localScale = new Vector3(Mathf.Lerp(base2, bass3, baseLerp), Mathf.Lerp(bass4, 0, baseLerp), this.transform.localScale.z);
        }
        else
        {
            if (this.transform.localScale.x >= bass3)
            {
                baseLerp = 0;
            }

            baseLerp += Time.deltaTime / speed;
            this.transform.localScale = new Vector3(Mathf.Lerp(base2, bass3, baseLerp), Mathf.Lerp(bass4, 0, baseLerp), this.transform.localScale.z);
        }
    }
}
