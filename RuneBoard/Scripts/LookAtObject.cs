using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtObject : MonoBehaviour
{
    [SerializeField] GameObject targetObject;

    private void Start()
    {
        if (targetObject == null)
        {
            targetObject = Camera.main.gameObject;
        } 
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion r1 = Quaternion.LookRotation(transform.position - targetObject.transform.position);
        transform.rotation = Quaternion.Euler(r1.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
