using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Volleyball
{
    public class VolleyballController: MonoBehaviour
    {
        // [HideInInspector]
        public VolleyballArea area;

        // update function makes sure balls con't stray too far from court
        void Update()
        {
            // Check the position for out of bounds
            if ( Mathf.Abs(transform.localPosition.x) > 15 ||
                Mathf.Abs(transform.localPosition.z) > 20 )
            {
                area.OutOfBounds();
            }
        }

        // technically OnCollisionEnter should handle most collisions
        // add more collision methods if this fails
        void OnCollisionEnter(Collision col)
        {
            // pass the gameObject since it might be an agent
            area.BallCollision_cb(col.gameObject);
        }


    }
}