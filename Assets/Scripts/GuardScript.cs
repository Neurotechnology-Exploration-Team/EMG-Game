using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GuardScript : MonoBehaviour
{
    //fields
    public GameObject Player;
    public float visionRadius;
    private Vector3 patrolStartPoint;
    public Vector3 patrolEndPoint;
    private Vector3 nextPoint;
    public Vector3 stop1;
    public Vector3 stop2;
    public Vector3 stop3;
    private List<Vector3> stopList = new List<Vector3>();

    private bool aware = false;
    private float detectionTimer = 1;
    private bool detected;

    public float speed;
    private Vector3 direction;
    private float waitTimer = 1;
    private bool stopped = false;
    bool goingBackWard = false;

    public bool canMove = true;
    private float stunTimer = 1;

    private float detectionEndTimer = 2;
    bool speedChanged = false;
    public GameObject questionMark;

    private float distance;

    // Start is called before the first frame update
    void Start()
    {


        questionMark.SetActive(false);

        //patrol start point is the position of the gaurd object at the start of play
        patrolStartPoint = transform.position;

        //stopList[0] is always the start point
        stopList.Add(patrolStartPoint);
        //checks how many stops along the patrol have been created and adds them to the list
        if (stop1 != Vector3.zero)
        {
            stopList.Add(stop1);

            if (stop2 != Vector3.zero)
            {
                stopList.Add(stop2);

                if (stop3 != Vector3.zero)
                {
                    stopList.Add(stop3);
                }
            }

        }
        //adds the endpoint after any previosu stop points
        stopList.Add(patrolEndPoint);

        GetNextPatrolPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            //updates positon
            if (stopped == false)
            {
                transform.position += direction * speed * Time.deltaTime;
            }



            distance = Mathf.Sqrt(Mathf.Pow((nextPoint.x - transform.position.x), 2) + Mathf.Pow((nextPoint.y - transform.position.y), 2));
            //if the guard very close to the next point sets the position to the exact point, this is to avoid the GetNextPatrolPoint method not working if the transform.postion isn't perfectly equal to the patrol point
            if (distance < 0.5)
            {
                stopped = true;
                transform.position = nextPoint;

                //stops at each point for 1 second
                if (waitTimer <= 0)
                {
                    GetNextPatrolPoint();
                    stopped = false;
                    waitTimer = 1;
                }

                waitTimer -= Time.deltaTime;

            }

            //checks if the player is positioned within detection range
            if (!aware)
            {
                aware = PlayerDetection();
            }


            if (aware)
            {

                if (!speedChanged)
                {
                    speed = speed / 2;
                    speedChanged = true;
                }

                questionMark.SetActive(true);

                DetectedPathSet();

                detectionTimer -= Time.deltaTime;
                if (detectionTimer <= 0)
                {
                    SceneManager.LoadScene("LossScene");
                }

                //since I'm reusing the method this should be false if the player is out the detection radius
                bool playerOutOfAwareness = PlayerDetection();

                if (!playerOutOfAwareness)
                {
                    detectionEndTimer -= Time.deltaTime;

                    if (detectionEndTimer < 0)
                    {
                        speed *= 2;
                        aware = false;
                        direction = nextPoint - transform.position;
                        direction.Normalize();
                        questionMark.SetActive(false);
                    }
                }
            }
        }
        else
        {
            if (stunTimer < 0)
            {
                canMove = true;
                stunTimer = 1;
            }

            stunTimer -= Time.deltaTime;
        }

    }

    private void GetNextPatrolPoint()
    {


        if (goingBackWard == false)
        {
            for (int i = 0; i < stopList.Count; i++)
            {
                if (transform.position == stopList[i])
                {
                    //if the guard is at the last point of the patrol reverses the order and sets nextPoint to the previous point in the list
                    if (i == stopList.Count - 1)
                    {
                        goingBackWard = true;
                        nextPoint = stopList[i - 1];
                    }
                    else
                    {
                        nextPoint = stopList[i + 1];
                        //ends loop once correct point has been found
                        i = stopList.Count;
                    }

                }
            }
        }
        //reversing patrol
        else
        {
            for (int i = 0; i < stopList.Count; i++)
            {
                if (transform.position == stopList[i])
                {
                    if (i == 0)
                    {
                        goingBackWard = false;
                        nextPoint = stopList[i + 1];
                    }
                    else
                    {
                        nextPoint = stopList[i - 1];
                    }
                }
            }
        }

        direction = nextPoint - transform.position;
        direction.Normalize();
    }

    private bool PlayerDetection()
    {
        //distance formula d=sqrt((x_2-x_1)�+(y_2-y_1)�)
        float distance = Mathf.Pow((Player.transform.position.x - transform.position.x), 2) + Mathf.Pow((Player.transform.position.y - transform.position.y), 2);
        distance = Mathf.Sqrt(distance);
        if (distance < visionRadius)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void DetectedPathSet()
    {
        direction = Player.transform.position - transform.position;
        direction.Normalize();
    }


}
