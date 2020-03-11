using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    public int numRooms = 50;
    public int minSize = 4;
    public int maxSize = 10;
    public GameObject Room;
    // Start is called before the first frame update
    void Start() {
        Random.InitState(System.Environment.TickCount);
        GenerateRooms();
    }

    void GenerateRooms() {
        for(int i = 0; i < numRooms; i++) {
            GameObject room = GameObject.Instantiate(Room);
            BoxCollider2D boxCollider2D = room.GetComponent<BoxCollider2D>();
            boxCollider2D.size = new Vector2(Random.Range(minSize,maxSize+1), Random.Range(minSize,maxSize));
            room.GetComponent<SpriteRenderer>().size = boxCollider2D.size;
            room.transform.parent = transform;
        }
    }

    // Update is called once per frame
    void Update() {
        int overlaps = 0;
        ContactFilter2D contactFilter2D = new ContactFilter2D();
        for(int i = 0; i < transform.childCount; i++) {
            Collider2D[] results = new Collider2D[4];
            overlaps += transform.GetChild(i).GetComponent<Rigidbody2D>().OverlapCollider(contactFilter2D,results);
        }
        Debug.Log(overlaps);
    }
}
