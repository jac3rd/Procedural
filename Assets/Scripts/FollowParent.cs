using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowParent : MonoBehaviour
{
    public Vector3 offset = new Vector3(0,0,-10);
    public bool follow = true;
    // Start is called before the first frame update
    void Start() {
        
    }

    void LateUpdate() {
        if(transform.parent != null && follow)
            transform.position = transform.parent.position + offset;
    }
}
