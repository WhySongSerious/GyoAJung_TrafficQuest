using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCam : MonoBehaviour
{
    [SerializeField]
    private GameObject go_Target;

    [SerializeField]
    private float speed;
    private Vector3 StartPosition;

    private Vector3 difValue;
    void Start()
    {
        StartPosition = go_Target.transform.position;// new Vector3(go_Target.transform.position.x, go_Target.transform.position.y, go_Target.transform.position.z);
        difValue = transform.position - go_Target.transform.position;
        difValue = new Vector3(Mathf.Abs(difValue.x), Mathf.Abs(difValue.y), Mathf.Abs(difValue.z));
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, go_Target.transform.position + difValue, speed);
    }
}
