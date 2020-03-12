using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Vector3 offset = new Vector3(0,0,-10);
    // Start is called before the first frame update
    void Start() {
        
    }

    void LateUpdate() {
        if(transform.parent != null)
            transform.position = transform.parent.position + offset;
    }
}
