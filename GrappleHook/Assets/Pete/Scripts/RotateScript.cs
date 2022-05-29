using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour
{
    public float speed = 1;

    private void Update()
    {
        transform.eulerAngles += Vector3.up * Time.deltaTime * speed;
    }
}
