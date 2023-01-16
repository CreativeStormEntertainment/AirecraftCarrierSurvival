using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField]
    private Transform follower = null;

    private void OnEnable()
    {
        follower.gameObject.SetActive(true);
    }

    private void Update()
    {
        follower.position = transform.position;
    }

    private void OnDisable()
    {
        follower.gameObject.SetActive(false);
    }
}
