using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolleyballController: MonoBehaviour
{
    // Uncomment these when implementing functionality
    // [HideInInspector]
    // public VolleyballArea area;
    // public string yellowGoalTag;
    // public string blueGoalTag;

    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }

    void OnCollisionEnter(Collision col)
    {
        string collidedTag = GetCollidedTag(col);
    }

    // Gets the tag of the collided object to be passed onto rectArea for scoring
    string GetCollidedTag(Collision col)
    {
        return col.gameObject.tag;
    }


}
