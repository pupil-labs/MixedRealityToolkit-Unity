using UnityEngine;

public class MovingStim : MonoBehaviour
{
    [SerializeField]
    private Transform stim;
    [SerializeField]
    private Transform[] waypoints;
    [SerializeField]
    private float speed = 1f;
    [SerializeField]
    private float tolerance = 0.001f;
    [SerializeField]
    private GazeDataLogging logging;

    private bool isMoving = false;
    private int waypointIndex = 0;

    void Start()
    {

    }

    void Update()
    {
        if (isMoving)
        {
            Transform currentWaypoint = waypoints[waypointIndex];
            stim.localPosition = Vector3.MoveTowards(stim.localPosition, currentWaypoint.localPosition, Time.deltaTime * speed);
            if (Vector3.Distance(stim.localPosition, currentWaypoint.localPosition) < tolerance)
            {
                stim.localPosition = currentWaypoint.localPosition;
                if (++waypointIndex == waypoints.Length)
                {
                    waypointIndex = 0;
                    isMoving = false;
                    logging.Log(4, 0, 0, 0, 0);
                }
            }
        }
    }

    public void StartMoving()
    {
        isMoving = true;
        logging.Log(3, 0, 0, 0, 0);
    }
}
