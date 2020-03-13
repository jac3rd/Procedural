using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelInit : MonoBehaviour
{
    public GameObject Player;
    public Vector3 spawn;
    public Camera mainCamera;

    public void CreateSpawn() {
        spawn = transform.GetChild(0).position;
        for(int i = 1; i < transform.childCount; i++) {
            Vector3 position = transform.GetChild(i).position;
            if(position.y < spawn.y)
                spawn = position;
        }
    }

    public void SpawnPlayer() {
        GameObject newPlayer = GameObject.Instantiate(Player, spawn, new Quaternion());
        mainCamera.GetComponent<FollowObject>().Object = newPlayer;
    }
}
