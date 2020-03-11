using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Generation : MonoBehaviour
{
    public int numRooms = 50;
    public int minSize = 4;
    public int maxSize = 10;
    public GameObject Room;
    private bool waitRoomsSettle = false;
    public float settleStableTime = 2;
    private float settleStableTimeHelper = 0;
    private int prevOverlaps = -1;
    public float cull = 0.5f;
    private LinkedList<Vector3[]> halls = new LinkedList<Vector3[]>();
    public Tilemap tilemap;
    public TileBase tileBase;
    private Vector3Int minCell;
    private Vector3Int maxCell;

    void Start() {
        Random.InitState(System.Environment.TickCount);
        GenerateRooms();
    }

    void GenerateRooms() {
        for(int i = 0; i < numRooms; i++) {
            Vector3 position = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),0);
            while(Vector3.Distance(position,Vector3.zero) > 1)
                position = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),0);
            GameObject room = GameObject.Instantiate(Room,position,new Quaternion());
            room.GetComponent<BoxCollider2D>().size = new Vector2(Random.Range(minSize,maxSize+2), Random.Range(minSize,maxSize));
            room.transform.parent = transform;
        }
        waitRoomsSettle = true;
    }

    void CullRooms() {
        for(int i = transform.childCount-1; i >= 0; i--) {
            if(Random.Range(0,1f) <= cull)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
                transform.GetChild(i).GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
        FillWorld();
        GenerateHalls();
        DrawRooms();
        DrawHalls();
        for(int i = transform.childCount-1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    void FillWorld() {
        Vector3 min = new Vector3(float.PositiveInfinity,float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity,float.NegativeInfinity);
        for(int i = 0; i < transform.childCount; i++) {
            Bounds bounds = transform.GetChild(i).GetComponent<Collider2D>().bounds;
            min = new Vector3(Mathf.Min(min.x,bounds.min.x),Mathf.Min(min.y,bounds.min.y),0);
            max = new Vector3(Mathf.Max(max.x,bounds.max.x),Mathf.Max(max.y,bounds.max.y),0);
        }
        minCell = Vector3Int.FloorToInt(min);
        maxCell = Vector3Int.FloorToInt(max);
        for(int x = minCell.x; x <= maxCell.x; x++)
            for(int y = minCell.y; y <= maxCell.y; y++)
                tilemap.SetTile(new Vector3Int(x,y,0), tileBase);
    }

    void GenerateHalls() {
        LinkedList<Vector3> positions = new LinkedList<Vector3>();
        for(int i = 0; i < transform.childCount; i++)
            positions.AddLast(transform.GetChild(i).position);
        LinkedList<Vector3> mstNodes = new LinkedList<Vector3>();
        mstNodes.AddLast(positions.First.Value);
        positions.RemoveFirst();
        while(positions.Count > 0) {
            float minDist = float.PositiveInfinity;
            Vector3 minPos = new Vector3();
            Vector3 pos = new Vector3();
            foreach(Vector3 p1 in mstNodes)
                foreach(Vector3 p2 in positions) {
                    float dist = Vector3.Distance(p1,p2);
                    if(dist < minDist) {
                        minDist = dist;
                        minPos = p2;
                        pos = p1;
                    }
                }
            mstNodes.AddLast(minPos);
            halls.AddLast(new Vector3[2]);
            halls.Last.Value[0] = pos;
            halls.Last.Value[1] = minPos;
            positions.Remove(minPos);
        }
        DrawRooms();
    }

    void DrawRooms() {
        for(int i = 0; i < transform.childCount; i++) {
            Bounds bounds = transform.GetChild(i).GetComponent<Collider2D>().bounds;
            Vector3Int min = tilemap.WorldToCell(bounds.min);
            Vector3Int max = tilemap.WorldToCell(bounds.max);
            for(int x = min.x+1; x < max.x; x++)
                for(int y = min.y+1; y < max.y; y++)
                    tilemap.SetTile(new Vector3Int(x,y,0), null);
        }
    }

    void DrawHalls() {
        foreach(Vector3[] hall in halls) {
            Vector3Int room1 = Vector3Int.RoundToInt(hall[0]);
            Vector3Int room2 = Vector3Int.RoundToInt(hall[1]);
            if(Random.Range(0,2) == 0) {
                for(int x = room1.x; x != room2.x; x += (int)Mathf.Sign(room2.x-room1.x))
                    tilemap.SetTile(new Vector3Int(x,room1.y,0), null);
                for(int y = room1.y; y != room2.y; y += (int)Mathf.Sign(room2.y-room1.y))
                    tilemap.SetTile(new Vector3Int(room2.x,y,0), null);
            } else {
                for(int y = room1.y; y != room2.y; y += (int)Mathf.Sign(room2.y-room1.y))
                    tilemap.SetTile(new Vector3Int(room1.x,y,0), null);
                for(int x = room1.x; x != room2.x; x += (int)Mathf.Sign(room2.x-room1.x))
                    tilemap.SetTile(new Vector3Int(x,room2.y,0), null);
            }
        }
    }

    void FixedUpdate() {
        if(waitRoomsSettle) {
            int overlaps = 0;
            ContactFilter2D contactFilter2D = new ContactFilter2D();
            for(int i = 0; i < transform.childCount; i++) {
                Collider2D[] results = new Collider2D[4];
                overlaps += transform.GetChild(i).GetComponent<Rigidbody2D>().OverlapCollider(contactFilter2D,results);
            }
            if(overlaps == prevOverlaps) {
                settleStableTimeHelper += Time.fixedDeltaTime;
                if(settleStableTimeHelper >= settleStableTime) {
                    waitRoomsSettle = false;
                    CullRooms();
                }
            }
            else
                settleStableTimeHelper = 0;
            prevOverlaps = overlaps;
        }
    }
}
