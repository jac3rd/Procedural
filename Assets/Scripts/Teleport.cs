using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public float distance = 1.5f;
    // Start is called before the first frame update
    void Start() {
        
    }

    public bool CheckDirection(Vector2 direction) {
        Vector3 newPosition = transform.position + (Vector3)(distance*direction.normalized);
        GameObject teleportChecker = transform.GetChild(1).gameObject;
        teleportChecker.GetComponent<FollowParent>().follow = false;
        teleportChecker.transform.position = newPosition;
        ContactPoint2D[] contactPoints = new ContactPoint2D[1];
        int numContacts = teleportChecker.GetComponent<Collider2D>().GetContacts(contactPoints);
        teleportChecker.GetComponent<FollowParent>().follow = true;
        if(numContacts != 0)
            return false;
        return true;
    }

    public void TeleportEntity(Vector2 direction) {
        transform.position = transform.position + (Vector3)(distance*direction.normalized);
    }
}
