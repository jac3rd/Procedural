using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 5f;
    public Rigidbody2D rb2d;
    public Teleport teleport;
    public Vector3 facing = Vector3.up;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"),0).normalized;
        if(direction.magnitude != 0) {
            facing = direction;
            Transform spotLight = transform.GetChild(2);
            spotLight.forward = direction;
            spotLight.GetComponent<FollowParent>().offset = -3*direction + new Vector3(0,0,spotLight.GetComponent<FollowParent>().offset.z);
        }
        rb2d.velocity = speed*direction;
        if(Input.GetButtonDown("Jump") && direction.magnitude != 0 && teleport.CheckDirection(direction))
            teleport.TeleportEntity(direction);
    }
}
