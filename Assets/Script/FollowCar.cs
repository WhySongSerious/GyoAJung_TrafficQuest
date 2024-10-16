using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCar : MonoBehaviour
{
    GameObject obj_player;
    public Transform player; // Reference to the player's transform
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
       obj_player = GetComponent<GameObject>();
       offset = transform.position - player.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + offset;
    }
}
