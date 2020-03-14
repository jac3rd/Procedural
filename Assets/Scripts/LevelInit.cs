using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelInit : MonoBehaviour
{
    public GameObject Player;
    public GameObject Exit;
    public Vector3 spawn;
    public Vector3 exit;
    public Camera mainCamera;

    public void CreateSpawn() {
        spawn = transform.GetChild(0).position;
        for(int i = 1; i < transform.childCount; i++) {
            Vector3 position = transform.GetChild(i).position;
            if(position.y < spawn.y)
                spawn = position;
        }
    }

    public void CreateExit() {
        exit = transform.GetChild(0).position;
        for(int i = 1; i < transform.childCount; i++) {
            Vector3 position = transform.GetChild(i).position;
            if(position.y > exit.y)
                exit = position;
        }
        Grid grid = GameObject.Find("Grid").GetComponent<Grid>();
        exit = grid.GetCellCenterWorld(grid.WorldToCell(exit));
    }

    public void SpawnPlayer() {
        GameObject newPlayer = GameObject.Instantiate(Player, spawn, new Quaternion());
        mainCamera.GetComponent<FollowObject>().Object = newPlayer;
    }

    public void SpawnExit() {
        GameObject newExit = GameObject.Instantiate(Exit, exit, new Quaternion());
    }
}
