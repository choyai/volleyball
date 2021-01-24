using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;

public class TrainingCamera : MonoBehaviour
{
    public GameObject[] areas;
    int idx = 0;
    Vector3 offset = new Vector3( 20, 20, -15 );

    void Awake()
    {
        areas = GameObject.FindGameObjectsWithTag("TrainingArea");   
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            transform.position = areas[idx].transform.position + offset;
            idx++;
            if(idx >= areas.Length)
            {
                idx = 0;
            }
        }
    }
}
