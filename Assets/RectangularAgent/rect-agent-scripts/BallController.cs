using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [HideInInspector]
    public RectArea area;
    public string yellowGoalTag; //will be used to check if collided with yellow goal
    public string blueGoalTag; //will be used to check if collided with blue goal
    public bool staying = false;



    void Update()
    {

        // Check the position in case the collision bugs out
        if (transform.position.y < (transform.localScale.y / 2f)  + 0.1f)
        {
            // TODO: make this not ugly
            //  case that the x is in court
            if( Mathf.Abs(transform.localPosition.x) < 5f)
            {
                // check whether we are on the yellow side or blue side
                if (transform.localPosition.z > 0f && transform.localPosition.z < 10f )
                {
                    // yellow scores
                        area.GoalTouched(RectAgent.RectTeam.Yellow);
                }
                else if (transform.localPosition.z < 0f && transform.localPosition.z > -10f)
                {
                    // blue scores
                    area.GoalTouched(RectAgent.RectTeam.Blue);
                }
                else
                {
                    //oob
                    area.OutOfBounds();
                }
                
            }
            else
            {
                // oob
                area.OutOfBounds();
            }
        }

        // Check the position for out of bounds
        if ( Mathf.Abs(transform.localPosition.x) > 15 ||
             Mathf.Abs(transform.localPosition.z) > 20 )
        {
            area.OutOfBounds();
        }
    }
    void OnCollisionEnter(Collision col)
    {
        //Debug.Log("entrer");
        
        if (col.gameObject.CompareTag(yellowGoalTag)) //ball touched yellow goal
        {
            area.GoalTouched(RectAgent.RectTeam.Blue);
        }
        else if (col.gameObject.CompareTag(blueGoalTag)) //ball touched blue goal
        {
            area.GoalTouched(RectAgent.RectTeam.Yellow);
        }
        else if (col.gameObject.CompareTag("Blue"))
        {
            
            area.prevTouchedTeam = RectAgent.RectTeam.Blue;
        }
        else if (col.gameObject.CompareTag("Yellow"))
        {
            area.prevTouchedTeam = RectAgent.RectTeam.Yellow;
        }
        else if (col.gameObject.CompareTag("Wall") || col.gameObject.CompareTag("Net"))
        {
            //Debug.Log("wall");
            area.OutOfBounds();
            
        }
    }
    void OnCollisionStay(Collision col)
    {
        // Check if the goaltouched funciton has already been called.
        // If so we wait for the episode to end( do nothing )
        if( !this.staying )
        {
            if (col.gameObject.CompareTag(yellowGoalTag)) //ball touched yellow goal
            {
                //Debug.Log("blue stay");
                area.GoalTouched(RectAgent.RectTeam.Blue);
            }
            else if (col.gameObject.CompareTag(blueGoalTag)) //ball touched blue goal
            {
                //Debug.Log("yellow stay");
                area.GoalTouched(RectAgent.RectTeam.Yellow);
            }
            else if (col.gameObject.CompareTag("Blue"))
            {
                area.prevTouchedTeam = RectAgent.RectTeam.Blue;
            }
            else if (col.gameObject.CompareTag("Yellow"))
            {
                area.prevTouchedTeam = RectAgent.RectTeam.Yellow;
            }
        }
        this.staying = true;


    }
    void OnCollisiontExit(Collision col)
    {

        if (col.gameObject.CompareTag(yellowGoalTag)) //ball touched yellow goal
        {
            area.GoalTouched(RectAgent.RectTeam.Blue);
        }
        else if (col.gameObject.CompareTag(blueGoalTag)) //ball touched blue goal
        {
            area.GoalTouched(RectAgent.RectTeam.Yellow);
        }
        else if (col.gameObject.CompareTag("Blue"))
        {
            area.prevTouchedTeam = RectAgent.RectTeam.Blue;
        }
        else if (col.gameObject.CompareTag("Yellow"))
        {
            area.prevTouchedTeam = RectAgent.RectTeam.Yellow;
        }
        else if( col.gameObject.CompareTag("Wall") || col.gameObject.CompareTag("Net"))
        {
            //Debug.Log("wallex");
            area.OutOfBounds();
        }
    }
}
