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
            room.GetComponent<BoxCollider2D>().size = new Vector2(Random.Range(minSize,maxSize+1), Random.Range(minSize,maxSize));
            room.transform.parent = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
