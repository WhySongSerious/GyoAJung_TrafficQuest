using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    [SerializeField] GameObject CalculateSpeed;


    private void OnTriggeredStay(Collider col)
    {
        if (col.CompareTag("TrafficSignArea"))
        {
            Debug.Log("In TrafficSignArea");
        }
        if (col.CompareTag("LimitSpeedArea"))
        {
            Debug.Log("In LimitSpeedArea");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Building"))
        {
            Debug.Log("Bumped Building");
        }
    }
}
