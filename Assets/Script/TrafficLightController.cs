using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [SerializeField] private Renderer TrafficLightRender;
    [SerializeField] private Material red, green;
    private Material[] mats;
    public bool redLight;
    private float waitTime = 10f;

    void Start()
    {
        mats = TrafficLightRender.sharedMaterials;
        Debug.Log(mats + " " + mats.Length);
        if (mats.Length > 2)
            StartCoroutine(ChangeToRed());
        else
            StartCoroutine(ChangeRed());
    }
    IEnumerator ChangeRed()
    {
        redLight = true;
        mats[1] = red;
        TrafficLightRender.sharedMaterials = mats;
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(ChangeGreen());
    }

    IEnumerator ChangeGreen()
    {
        redLight = false;
        mats[1] = green;
        TrafficLightRender.sharedMaterials = mats;
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(ChangeRed());
    }
    IEnumerator ChangeToRed()
    {
        redLight = true;
        mats[1] = red;
        mats[2] = green;
        TrafficLightRender.sharedMaterials = mats;
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(ChangeToGreen());
    }

    IEnumerator ChangeToGreen()
    {
        redLight = false;
        mats[1] = green;
        mats[2] = red;
        TrafficLightRender.sharedMaterials = mats;
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(ChangeToRed());
    }
}
