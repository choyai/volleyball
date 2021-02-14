using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Volleyball;
// using Unity.MLAgents;

namespace Tests
{
    //	Technically, this is more of an integration test, but
    //	MLAgents necessitates that anyways
    public class VolleyballTest
    {
        VolleyballArea area;
        [SetUp]
        public void Init()
        {
            Physics.gravity = Vector3.zero;
            GameObject areaObject = MonoBehaviour
            .Instantiate(Resources.Load<GameObject>("Prefabs/VollleyballTraining")); 
            area = areaObject.GetComponent<VolleyballArea>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(area.gameObject);
        }

        [Test]
        public void VolleyballAreaIsLoaded()
        {
            Assert.IsNotNull(area.gameObject);
        }

        // A Test behaves as an ordinary method
        [Test]
        public void VolleyballHitTestSimplePasses()
        {

            // Use the Assert class to test conditions
            Assert.Pass();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator VolleyballHitTestWithEnumeratorPasses()
        {
            // Check the phase after the ball hit     
            area.gameObject.transform.position = Vector3.zero;
            area.phase = VolleyballArea.GamePhase.Play;
            Rigidbody ball = area.ballRb;
            // Collide with wall
            ball.position = new Vector3(0f, 2.5f, 0f );
            yield return new WaitForSeconds( 0.1f );
            Assert.False( area.phase == VolleyballArea.GamePhase.Play );
        }

        [UnityTest]
        public IEnumerator VolleyballHitWallEndsPhase()
        {
            // Check the phase after the ball hit     
            area.gameObject.transform.position = Vector3.zero;
            area.phase = VolleyballArea.GamePhase.Play;
            Rigidbody ball = area.ballRb;
            // Collide with net
            ball.position = new Vector3( 15f, 1f, 0f );
            yield return new WaitForSeconds( 0.1f );
            Assert.False( area.phase == VolleyballArea.GamePhase.Play );
        }

        [UnityTest]
        public IEnumerator VolleyballOutOfBoundsEndsPhase()
        {
             // Check the phase after the ball hit     
            area.gameObject.transform.position = Vector3.zero;
            area.phase = VolleyballArea.GamePhase.Play;
            Rigidbody ball = area.ballRb;
            // Collide with net
            ball.position = new Vector3( 15f, 1f, 20f );
            yield return new WaitForSeconds( 0.1f );
            Assert.False( area.phase == VolleyballArea.GamePhase.Play );
        }

        [UnityTest]
        public IEnumerator AllAgentsSpawn()
        {
            area.gameObject.transform.position = Vector3.zero;
            yield return new WaitForSeconds( 0.1f );

            // All agents should be in agent list
            var ps = area.playerStates;
            // Debug.Assert( ps.Count == 4 );
            Assert.True( ps.Count == 4 );
            // Somehow the names aren't correct, we'll just verify number of agents
        }
        /*
        The next section goes into the detailed game state changes that should happen.
        We are checking the state changes when certain actions happen in
        TODO: make agent selection not random??
        */
        [UnityTest]
        public IEnumerator GamePhaseChangesWhenBallIsServed()
        {
            area.gameObject.transform.position = Vector3.zero;
            yield return new WaitForSeconds( 2f );

            var ps = area.playerStates;
            // find the agent that has the ball
            List<VolleyballAgent> serving = new List<VolleyballAgent>();
            foreach( VolleyballState p in ps )
            {
                
                if ( p.agentScript.isServing )
                {
                    serving.Add(p.agentScript);
                }
            
            }
            Assert.True( serving.Count != 0);
            // call the service function
            Debug.Log( serving[0].gameObject.name );
            area.Service( serving[0] );

            yield return new WaitForSeconds( 0.2f );
            Assert.True( area.phase == VolleyballArea.GamePhase.Play );
        }

        [UnityTest]
        public IEnumerator prevTouchedAgentIdChangesWhenServed()
        {
            area.gameObject.transform.position = Vector3.zero;
            yield return new WaitForSeconds( 2f );

            var ps = area.playerStates;
            // find the agent that has the ball
            List<VolleyballAgent> serving = new List<VolleyballAgent>();
            foreach( VolleyballState p in ps )
            {
                
                if ( p.agentScript.isServing )
                {
                    serving.Add(p.agentScript);
                }
            
            }
            Assert.True( serving.Count != 0);
            // call the service function
            Debug.Log( serving[0].gameObject.name );
            area.Service( serving[0] );

            yield return new WaitForSeconds( 0.2f );
            Assert.True( area.getAgentID(serving[0]) == area.prevTouchedAgentId );
        }
    }
}
