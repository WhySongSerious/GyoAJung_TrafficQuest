using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs;

    private void Start()
    {
        Instantiate(SelectACarPrefab(), transform);
    }

    private GameObject SelectACarPrefab()
    {
        var randomIndex = Random.Range(0, carPrefabs.Length);
        return carPrefabs[randomIndex];
    }
}
