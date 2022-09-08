using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehaviorScript : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 10f;
    private int waypointIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        iTween.MoveTo(gameObject, iTween.Hash(
            "position", waypoints[waypointIndex],
            "oncomplete", "nextWaypoint",
            "easetype", iTween.EaseType.linear,
            "speed", moveSpeed
            ));
    }

    private void nextWaypoint()
    {
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        iTween.MoveTo(gameObject, iTween.Hash(
            "position", waypoints[waypointIndex],
            "oncomplete", "nextWaypoint",
            "easetype", iTween.EaseType.linear,
            "speed", moveSpeed
            ));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
