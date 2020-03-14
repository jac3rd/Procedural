using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{

    public Collider2D col2D;

    void OnTriggerStay2D(Collider2D info) {
        Debug.Log(info.gameObject.name);
        if(info.gameObject.CompareTag("Player") && col2D.bounds.Contains(info.transform.position)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
