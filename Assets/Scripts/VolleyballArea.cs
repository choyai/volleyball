using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Assertions;

namespace Volleyball
{

    //  VolleyballState contains information about each agent to
    //  be used by VolleyballArea
    [System.Serializable]
    public class VolleyballState
    {
        public int playerIndex;
        public Rigidbody agentRb;
        public Vector3 startingPos;
        public VolleyballAgent agentScript;
        public float ballPosReward;
        public bool isServing = false;
    }

    //  VolleyballArea Class
    public class VolleyballArea : MonoBehaviour
    {
        // The Game Phase
        public enum GamePhase
        {
            Start = 0,
            Play = 1,
            End = 2
        }

        public GameObject ball;
        public GamePhase phase;
        
        [FormerlySerializedAs("ballRB")]
        [HideInInspector]
        public Rigidbody ballRb;

        //  ball controller script    
        VolleyballController m_BallController;
        public List<VolleyballState> playerStates = new List<VolleyballState>();
        [HideInInspector]
        public Vector3 ballStartingPos;
        public GameObject goalTextUI;
        [HideInInspector]
        public bool canResetBall;
        public int bluePlayerTurn;
        public int yellowPlayerTurn;
        public Vector3 ballVelocity;

        float serviceCooldown = 0.7f;
        bool recentlyServed = false;

        System.Random randall = new System.Random();

        VolleyballAgent.VolleyballTeam prevScoredTeam;
        public int sameTeamBallTouchedCount = 0;

        public VolleyballAgent.VolleyballTeam prevTouchedTeam;


        public int prevTouchedAgentId;

        EnvironmentParameters m_ResetParams;

        public int blueScore, yellowScore;

        public float motionTimer = 0f;
        public bool timerRunning = false;

        void Update()
        {
            if( timerRunning )
            {

                motionTimer += Time.deltaTime;

                if( ballRb.velocity.magnitude > Mathf.Epsilon )
                {
                    motionTimer = 0f;
                    timerRunning = false;
                }

                if( motionTimer > 5f )
                {
                    Scored(VolleyballAgent.GetOppositeTeam(prevTouchedTeam));
                    motionTimer = 0f;
                    timerRunning = false;
                }

            }
            else
            {
                timerRunning = true;
            }
            foreach (var ps in playerStates)
            {
                Vector3 direction = ballRb.transform.position - ps.agentRb.transform.position;
                direction.Normalize();
                Vector3 contactPoint = 4f * direction + ps.agentRb.transform.position;
                ps.agentScript.contactPoint.transform.position = contactPoint;

                Vector3 landingPoint = ComputeLandingPoint( ps.agentScript, contactPoint );

                ps.agentScript.projectedTarget.transform.position = landingPoint;

            }
        }

        void Awake()
        {
            canResetBall = true;

            if (goalTextUI) { goalTextUI.SetActive(false); }
            
            //  set ball attrs
            ballRb = ball.GetComponent<Rigidbody>();
            //  set the ball's area attr as this VolleyballArea
            m_BallController = ball.GetComponent<VolleyballController>();
            m_BallController.area = this;
            
            ballStartingPos = ball.transform.position;

            System.Random rnd = new System.Random();
            int rndIndex = rnd.Next(2);
            // First team to serve is randomly selected
            prevScoredTeam = (VolleyballAgent.VolleyballTeam) rndIndex;
            sameTeamBallTouchedCount = 0;
            prevTouchedTeam = prevScoredTeam;
            bluePlayerTurn = 0;
            yellowPlayerTurn = 0;
            blueScore = 0;
            yellowScore = 0;
            m_ResetParams = Academy.Instance.EnvironmentParameters;

            ballRb.velocity = new Vector3(0f, 0f, 0f);
            ballRb.useGravity = false;

            // The ball should be parented to a random agent of the team that scored until
            // it is served.

            var playerCount = 0;

            // Run some setup for each agent.

            foreach (var ps in playerStates)
            {
                if(ps.agentScript.team == prevScoredTeam)
                {
                    ref int playerTurn = ref this.GetPlayerTurnByTeam( ps.agentScript.team );
                    if(playerCount == playerTurn)
                    {
                        ball.transform.SetParent(ps.agentScript.gameObject.transform);
                        ball.transform.localPosition = new Vector3(1f, 0f, 0f);
                        ps.agentScript.isServing = true;
                        playerTurn = (playerTurn==0) ? 1 : 0;
                        ps.playerIndex = 0;
                    }
                    else
                    {
                        ps.agentScript.isServing = false;
                        playerCount++;
                        ps.playerIndex = 1;
                    }
                }
                else
                {
                    ps.agentScript.isServing = false;
                }
            }

            ballRb.angularVelocity = Vector3.zero;

        }

        public IEnumerator serviceCooldownTimer()
        {
            recentlyServed = true;
            yield return new WaitForSeconds(serviceCooldown);
            recentlyServed = false;
        }

        IEnumerator ShowGoalUI()
        {
            if (goalTextUI) goalTextUI.SetActive(true);
            yield return new WaitForSeconds(.25f);
            if (goalTextUI) goalTextUI.SetActive(false);
        }

        //  handles scoring
        public void Scored(VolleyballAgent.VolleyballTeam goalTeam)
        {
            VolleyballAgent.VolleyballTeam lastTouched = getAgentByID( prevTouchedAgentId ).team;
            VolleyballAgent.VolleyballTeam scoredTeam;
            scoredTeam = ( lastTouched == goalTeam ) ? VolleyballAgent.GetOppositeTeam( lastTouched ) : lastTouched;
            if( scoredTeam == VolleyballAgent.VolleyballTeam.Blue )
            {
                blueScore++;
            }
            else
            {
                yellowScore++;
            }
            Debug.Log( "Scored: " + scoredTeam.ToString() );
            GiveReward(scoredTeam);
        }

        public void OutOfBounds()
        {
            OutOfBounds( prevTouchedAgentId );
        }
        
        public void OutOfBounds( int id )
        {
            OutOfBounds( getAgentByID( id ) );
        }

        public void OutOfBounds( VolleyballAgent agent )
        {
            Debug.Log( "OOBs by " + agent.gameObject.name );
            OutOfBounds( agent.team );
        }

        // calculate who fouled
        public void OutOfBounds(VolleyballAgent.VolleyballTeam fouledTeam)
        {
            if( fouledTeam == VolleyballAgent.VolleyballTeam.Blue)
            {
                yellowScore++;
                GiveReward(VolleyballAgent.VolleyballTeam.Yellow);
            }
            else
            {
                blueScore++;
                GiveReward(VolleyballAgent.VolleyballTeam.Blue);
            }
        }

        public void GiveReward(VolleyballAgent.VolleyballTeam scoredTeam)
        {
            foreach (var ps in playerStates)
            {
                var reward = 0f;
                if (ps.agentScript.team == scoredTeam)
                {
                    reward = 1 + ps.agentScript.timePenalty;
                    reward += ps.agentScript.totalHitReward;
                }
                else
                {
                    reward = Mathf.Clamp(-0.5f + ps.agentScript.timePenalty, -1, 0);
                    reward += ps.agentScript.totalHitReward;
                }
                ps.agentScript.totalHitReward = 0f;
                ps.agentScript.AddReward(reward);
                // Debug.Log("Reward = " + reward.ToString());
                ps.agentScript.EndEpisode();  //all agents need to be reset
            }
            // set prevScoredTeam, prevTouchedTeam
            prevScoredTeam = scoredTeam;
            prevTouchedTeam = scoredTeam;
        }

        public void BallCollision_cb( GameObject collidedObject )
        {
            string colliderTag = collidedObject.tag;
            VolleyballAgent agent;
            switch( colliderTag )
            {
                case "Blue": 
                    agent = collidedObject.GetComponent<VolleyballAgent>();
                    playerTouchedBall( agent );
                    break;
                case "Yellow":
                    agent = collidedObject.GetComponent<VolleyballAgent>();
                    playerTouchedBall( agent );
                    break;
                case "blueGoal":
                    Scored(VolleyballAgent.VolleyballTeam.Blue);
                    break;
                case "yellowGoal":
                    Scored(VolleyballAgent.VolleyballTeam.Yellow);
                    break;
                case "Wall":
                    OutOfBounds(prevTouchedTeam);
                    break;
                case "Net":
                    OutOfBounds(prevTouchedTeam);
                    break;
                default:
                    Debug.LogError("This tag is not covered!");
                    break;
            }
        }

        void playerTouchedBall( VolleyballAgent agent )
        {
            if( agent.recentlyHit || recentlyServed )
            {
                return;
            }
            //  foul ball
            if( getAgentID(agent) == prevTouchedAgentId )
            {
                OutOfBounds( agent.team);
                return;
            }
            prevTouchedAgentId = getAgentID(agent);

            // count the team touches
            if( prevTouchedTeam == agent.team )
            {
                sameTeamBallTouchedCount++;
            }
            else
            {
                // reset counter
                sameTeamBallTouchedCount = 0;
            }
            prevTouchedTeam = agent.team;
            
            if( sameTeamBallTouchedCount > 3 )
            {
                OutOfBounds( agent.team );
            }
            
        }

        public void ResetBall()
        {
            // // print scores
            // Debug.Log("Blue: " + blueScore.ToString());
            // Debug.Log("Yellow: " + yellowScore.ToString());
            
            ballRb.velocity = new Vector3(0f, 0f, 0f);
            ballRb.useGravity = false;
            // The ball should be parented to a random agent of the team that scored until
            // it is served.

            // The ball resets towards whichever team scored
            // ballRb.velocity = new Vector3( 0f, 0f, 1f * (1f - (float)scoredTeam) - 1f * (float)scoredTeam );


            // currently it is random who gets to serve the code below doesn't work.
            int turn = randall.Next(4);
            // Debug.Log(turn);
            // for( int count = 0; count < 4; count++ )
            // {
            //     var ps = playerStates[count];
            //     if( count == turn )
            //     {
            //         ball.transform.SetParent(ps.agentScript.gameObject.transform);
            //         ball.transform.localPosition = new Vector3(1f, 0.0f, 0f);
            //         ps.agentScript.isServing = true;
            //     }
            //     else
            //     {
            //         ps.isServing = false;
            //         count++;
            //     }
            // }
            foreach (var ps in playerStates)
            {
                if(ps.agentScript.team == prevScoredTeam)
                {
                    ref int playerTurn = ref this.GetPlayerTurnByTeam( ps.agentScript.team );
                    if( ps.playerIndex == playerTurn)
                    {
                        //Debug.Log("player name :" + ps.agentScript.gameObject.name );
                        //Debug.Log("player index :" + ps.playerIndex );
                        //Debug.Log("player turn: "  + playerTurn );
                        ball.transform.SetParent(ps.agentScript.gameObject.transform);
                        ball.transform.localPosition = new Vector3(1f, 0.0f, 0f);
                        ps.agentScript.isServing = true;
                        
                    }
                    else
                    {
                        ps.isServing = false;
                    }
                }
                else
                {
                    ps.isServing = false;
                }
            }
            //  if the ball was never reset
            //  and there was no valid agent serving
            if( ball.transform.parent == this.transform )
            {
                // the serving agent is a the first agent in the scoring team
                foreach ( var ps in playerStates )
                {
                    if( ps.agentScript.team == prevScoredTeam )
                    {
                        ball.transform.SetParent(ps.agentScript.gameObject.transform);
                        ball.transform.localPosition = new Vector3(1f, 0.0f, 0f);
                        ps.agentScript.isServing = true;
                        break;
                    }
                }
                // Debug.Log("fuck");
            }
            ballRb.angularVelocity = Vector3.zero;
            ballRb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            this.phase = GamePhase.Start;

        }

        public ref int GetPlayerTurnByTeam( VolleyballAgent.VolleyballTeam team )
        {
            
            switch ( team )
            {
                case VolleyballAgent.VolleyballTeam.Blue:
                    return ref this.bluePlayerTurn;
                case VolleyballAgent.VolleyballTeam.Yellow:
                    return ref this.yellowPlayerTurn;
                default:
                    // I can't think of a default value ref
                    return ref this.bluePlayerTurn;
            }
        }

        public void Service( VolleyballAgent agent )
        {
            ref int playerTurn = ref this.GetPlayerTurnByTeam( agent.team );
            if( this.phase == GamePhase.Start )
            {
                if( ball.transform.localPosition != new Vector3( 1f, 0f, 0f ))
                {
                    ball.transform.localPosition = new Vector3( 1f, 0f, 0f );
                }
                ballRb.WakeUp();
                ballRb.constraints = RigidbodyConstraints.None;
                ballRb.useGravity = true;

                //  may need to determine the pitch angle of the ball via input,
                //  as gameplay is too hard otherwise...

                Vector3 localVelocity = new Vector3(16f, 13f, 0f);
                //Debug.Log("team = " + agent.team.ToString() );
                //Debug.Log("localVelocity = " + localVelocity.ToString() );
                Vector3 worldVelocity = agent.transform.TransformVector(localVelocity);
                //Debug.Log(" world velocity " + worldVelocity.ToString() );
                ballRb.velocity = worldVelocity;
                
                ball.transform.SetParent( this.transform, true );
                playerTurn = ( playerTurn == 0) ? 1 : 0;
                agent.isServing = false;
                this.phase = GamePhase.Play;
                playerTouchedBall( agent );
                StartCoroutine( serviceCooldownTimer() );
            }
        }

        // An agent takes a hit action if the ball is within range
        public void Hit( VolleyballAgent agent )
        {
            // if the ball is within range
            Vector3 direction = ball.transform.position - agent.transform.position;
            var distance = direction.magnitude;
            // Debug.Log(distance);
            if( distance < 4f && !recentlyServed )
            {
                // Debug.Log("hitting");
                // calculate the hit direction
                direction.Normalize();
                ballRb.velocity = agent.hitPower * direction;
                agent.totalHitReward += agent.m_HitReward;

                playerTouchedBall( agent );

            }
            StartCoroutine( agent.HitCooldownTimer() );
        }

        // Computes where to place the aim assist tool
        Vector3 ComputeLandingPoint( VolleyballAgent agent, Vector3 contactPoint )
        {
            if( (ballRb.position - agent.transform.position).magnitude < 2f)
            {
                return agent.transform.position;
            }

            Vector3 initialVelocity = contactPoint - agent.transform.position;
            
            initialVelocity.Normalize();

            initialVelocity *= agent.hitPower;

            // TODO: Check for net collision
            // Basic ballistics
            float timeToFloor;
            timeToFloor = initialVelocity.y / (float) Physics.gravity.magnitude + Mathf.Sqrt(initialVelocity.y * initialVelocity.y / (Physics.gravity.magnitude * Physics.gravity.magnitude) - 4f * ballRb.position.y / Physics.gravity.magnitude );
            //float timeToNet 
            // Debug.Log("timeToFloor = " + timeToFloor.ToString());
            Vector3 landingPoint = timeToFloor * initialVelocity;

            // Assert.AreApproximatelyEqual( 0.0f, landingPoint.y);
            
            return agent.transform.position;
        }

        //  utility
        int getAgentID( VolleyballAgent agent )
        {
            VolleyballState ps = playerStates.Find( p => p.agentScript == agent );
            return ps.playerIndex;
        }

        VolleyballAgent getAgentByID( int id )
        {
            VolleyballState ps =  playerStates.Find( p => p.playerIndex == id );
            return ps.agentScript;
        }

    }
}
