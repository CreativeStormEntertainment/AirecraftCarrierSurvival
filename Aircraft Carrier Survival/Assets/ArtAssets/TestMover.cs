using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMover : MonoBehaviour
{
    public Transform test;
    public float speed = 0.1f;
    public float interval=1.0f;
    public float testTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (testTime+ interval < Time.timeSinceLevelLoad)
        {
            testTime = Time.timeSinceLevelLoad;
            test.gameObject.SetActive(!test.gameObject.activeSelf);
        }
        this.transform.position += transform.forward * speed * Time.deltaTime;
    }
}
