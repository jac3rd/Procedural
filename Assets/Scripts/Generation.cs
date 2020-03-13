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
    public float settleStableTime = 1;
    private float settleStableTimeHelper = 0;
    private int prevOverlaps = -1;
    public float cull = 0.0f;
    private LinkedList<Vector3[]> halls = new LinkedList<Vector3[]>();
    private Dictionary<GameObject,LinkedList<GameObject>> adjList = new Dictionary<GameObject, LinkedList<GameObject>>();
    public Tilemap walls, backgrounds;
    public TileBase tileBase;
    private Vector3Int minCell;
    private Vector3Int maxCell;
    public LevelInit levelInit;

    void Start() {
        Application.targetFrameRate = -1;
        Random.InitState(System.Environment.TickCount);
        GenerateRooms();
    }

    void GenerateRooms() {
        for(int i = 0; i < numRooms; i++) {
            Vector3 position = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),0);
            while(Vector3.Distance(position,Vector3.zero) > 1)
                position = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),0);
            GameObject room = GameObject.Instantiate(Room,position,new Quaternion());
            room.GetComponent<BoxCollider2D>().size = new Vector2(Random.Range(minSize+1,maxSize+2), Random.Range(minSize+1,maxSize+2));
            room.transform.parent = transform;
        }
        waitRoomsSettle = true;
    }

    void CullRooms() {
        for(int i = transform.childCount-1; i >= 0; i--) {
            if(Random.Range(0,1f) < cull)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else {
                BoxCollider2D collider2D = transform.GetChild(i).GetComponent<BoxCollider2D>();
                collider2D.attachedRigidbody.bodyType = RigidbodyType2D.Static;
                collider2D.isTrigger = true;
                collider2D.size += new Vector2(-1,-1);
                float xCenter = (Mathf.Floor(collider2D.bounds.max.x) + Mathf.Floor(collider2D.bounds.min.x))/2;
                float yCenter = (Mathf.Floor(collider2D.bounds.max.y) + Mathf.Floor(collider2D.bounds.min.y))/2;
                transform.GetChild(i).position = new Vector3(xCenter,yCenter,0);
            }
        }
        FillWorld();
        GetAdjancencies();
        //GenerateHalls();
        DrawRooms();
        //DrawHalls();
        levelInit.CreateSpawn();
        levelInit.SpawnPlayer();
        /*
        for(int i = transform.childCount-1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        */
    }

    void FillWorld() {
        walls.gameObject.GetComponent<TilemapCollider2D>().enabled = false;
        Vector3 min = new Vector3(float.PositiveInfinity,float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity,float.NegativeInfinity);
        for(int i = 0; i < transform.childCount; i++) {
            Bounds bounds = transform.GetChild(i).GetComponent<Collider2D>().bounds;
            min = new Vector3(Mathf.Min(min.x,bounds.min.x),Mathf.Min(min.y,bounds.min.y),0);
            max = new Vector3(Mathf.Max(max.x,bounds.max.x),Mathf.Max(max.y,bounds.max.y),0);
        }
        minCell = Vector3Int.FloorToInt(min);
        maxCell = Vector3Int.FloorToInt(max);
        for(int x = minCell.x-1; x <= maxCell.x; x++)
            for(int y = minCell.y-1; y <= maxCell.y; y++) {
                Vector3Int cellPos = new Vector3Int(x,y,0);
                walls.SetTile(cellPos, tileBase);
                walls.SetTileFlags(cellPos, TileFlags.None);
                walls.SetColor(cellPos,Color.grey);
            }
    }

    void GetAdjancencies() {
        Vector3 cellSize = GameObject.Find("Grid").GetComponent<Grid>().cellSize;
        for(int i = 0; i < transform.childCount; i++) {
            GameObject room = transform.GetChild(i).gameObject;
            adjList.Add(room, new LinkedList<GameObject>());
            BoxCollider2D roomCol = room.GetComponent<BoxCollider2D>();
            for(float x = roomCol.bounds.min.x + cellSize.x/2; x < roomCol.bounds.max.x; x += cellSize.x) {
                Collider2D down = Physics2D.Raycast(new Vector2(x,roomCol.bounds.min.y - cellSize.y/2), Vector2.down, cellSize.y).collider;
                Collider2D up = Physics2D.Raycast(new Vector2(x,roomCol.bounds.max.y + cellSize.y/2), Vector2.up, cellSize.y).collider;
                if(down != null && !adjList[room].Contains(down.gameObject))
                    adjList[room].AddLast(down.gameObject);
                if(up != null && !adjList[room].Contains(up.gameObject))
                    adjList[room].AddLast(up.gameObject);
            }   
            for(float y = roomCol.bounds.min.y + cellSize.y/2; y < roomCol.bounds.max.y; y += cellSize.y) {
                Collider2D left = Physics2D.Raycast(new Vector2(roomCol.bounds.min.x - cellSize.x/2,y), Vector2.left, cellSize.x).collider;
                Collider2D right = Physics2D.Raycast(new Vector2(roomCol.bounds.max.x + cellSize.x/2,y), Vector2.right, cellSize.x).collider;
                if(left != null && !adjList[room].Contains(left.gameObject))
                    adjList[room].AddLast(left.gameObject);
                if(right != null && !adjList[room].Contains(right.gameObject))
                    adjList[room].AddLast(right.gameObject);
            }
        }
        walls.gameObject.GetComponent<TilemapCollider2D>().enabled = true;
    }

    void GenerateHallsPrim() {
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
    }

    void DrawRooms() {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = Color.red;
        colorKeys[0].time = 0f;
        colorKeys[1].color = Color.blue;
        colorKeys[1].time = 1f;
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 0.25f;
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 0.25f;
        alphaKeys[1].time = 1f;
        gradient.SetKeys(colorKeys, alphaKeys);
        for(int i = 0; i < transform.childCount; i++) {
            Bounds bounds = transform.GetChild(i).GetComponent<Collider2D>().bounds;
            Vector3Int min = walls.WorldToCell(bounds.min);
            Vector3Int max = walls.WorldToCell(bounds.max);
            float heat = adjList[transform.GetChild(i).gameObject].Count/4f;
            for(int x = min.x; x < max.x; x++)
                for(int y = min.y; y < max.y; y++) {
                    Vector3Int cellPos = new Vector3Int(x,y,0);
                    backgrounds.SetTile(cellPos, tileBase);
                    walls.SetTile(cellPos, null);
                    backgrounds.SetTileFlags(cellPos, TileFlags.None);
                    backgrounds.SetColor(cellPos, gradient.Evaluate(heat));
                }
        }
    }

    void DrawHalls() {
        foreach(Vector3[] hall in halls) {
            Vector3Int room1 = Vector3Int.RoundToInt(hall[0]);
            Vector3Int room2 = Vector3Int.RoundToInt(hall[1]);
            if(Random.value < 0.5) {
                for(int x = room1.x; x != room2.x+(int)Mathf.Sign(room2.x-room1.x); x += (int)Mathf.Sign(room2.x-room1.x)) {
                    backgrounds.SetTile(new Vector3Int(x,room1.y,0), null);
                    backgrounds.SetTile(new Vector3Int(x,room1.y-1,0), null);
                }
                for(int y = room1.y; y != room2.y+(int)Mathf.Sign(room2.y-room1.y); y += (int)Mathf.Sign(room2.y-room1.y)) {
                    backgrounds.SetTile(new Vector3Int(room2.x,y,0), null);
                    backgrounds.SetTile(new Vector3Int(room2.x-1,y,0), null);
                }
            } else {
                for(int y = room1.y; y != room2.y+(int)Mathf.Sign(room2.y-room1.y); y += (int)Mathf.Sign(room2.y-room1.y)) {
                    backgrounds.SetTile(new Vector3Int(room1.x,y,0), null);
                    backgrounds.SetTile(new Vector3Int(room1.x-1,y,0), null);
                }
                for(int x = room1.x; x != room2.x+(int)Mathf.Sign(room2.x-room1.x); x += (int)Mathf.Sign(room2.x-room1.x)) {
                    backgrounds.SetTile(new Vector3Int(x,room2.y,0), null);
                    backgrounds.SetTile(new Vector3Int(x,room2.y-1,0), null);
                }
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
