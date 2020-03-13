using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public GameObject Object;
    public Vector3 offset = new Vector3(0,0,-10);
    public bool follow = true;

    void Start() {
        
    }

    void LateUpdate() {
        if(Object != null && follow)
            transform.position = Object.transform.position + offset;
    }
}
