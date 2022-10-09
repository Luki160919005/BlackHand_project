using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public float speed = 5;
	public float waitTime = .3f;
	public float turnSpeed = 90;

	public Light spotlight;
	public float viewDistance;
	public LayerMask viewMask;
	float viewAngle;
    bool stillMoving = false;

	public Transform pathHolder;
	public Transform player;
	Color originalSpotlightColour;

 
	Vector3[] pathChaser;
	int targetIndex;


    void Start(){
        player = GameObject.FindGameObjectWithTag ("Player").transform;
		viewAngle = spotlight.spotAngle;
		originalSpotlightColour = spotlight.color;

        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for(int i = 0; i < waypoints.Length; i++){
            waypoints[i] = pathHolder.GetChild(i).position;

            //to make the guard not sink
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y-0.2f, waypoints[i].z);
        }
        StopCoroutine(FollowPath(waypoints));
        StartCoroutine(FollowPath(waypoints));
    }
    void Update(){
        if (CanSeePlayer ()) {
            //
            //stillMoving = true;
            //speed = 0;
            stillMoving =true;
            //StopCoroutine(FollowPath());
			spotlight.color = Color.red;
		} else {
			spotlight.color = originalSpotlightColour;
		}
    }
    bool CanSeePlayer() {
        
        if (Vector3.Distance(transform.position,player.position) < viewDistance) {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle (transform.forward, dirToPlayer);
            if (angleBetweenGuardAndPlayer < viewAngle / 2f) {
                // if it did not hit something
                if (!Physics.Linecast (transform.position, player.position, viewMask)) {
                    //see player
                    return true;
                }
            }
        }
        
		return false;
	}



    IEnumerator TurnToFace(Vector3 lookTarget){
        Vector3 dirToLookTarget = (lookTarget-transform.position).normalized;
        float targetAngle = 90-Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x)*Mathf.Rad2Deg;
        //for turning
        while(Mathf.Abs( Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle))>0.05f){
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed*Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;

        }
    }

    IEnumerator FollowPath(Vector3[] waypoints){
    
        
            transform.position = waypoints[0];
            int targetWaypointIndex = 1;
            Vector3 targetWaypoint = waypoints[targetWaypointIndex];
            transform.LookAt(targetWaypoint);

            while(stillMoving == false){
                transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed*Time.deltaTime);
                if(transform.position == targetWaypoint){
                    //if waypoint equal to this then its going back to zero
                    targetWaypointIndex = (targetWaypointIndex+1)% waypoints.Length;
                    targetWaypoint = waypoints[targetWaypointIndex];
                    yield return new WaitForSeconds(waitTime);
                    yield return StartCoroutine(TurnToFace(targetWaypoint));
                }
                yield return null;
            }
            if(stillMoving == true){
                PathRequestManager.RequestPath(transform.position,player.position, OnPathFound);
            }
            
        
        

    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful) {
			pathChaser = newPath;
			targetIndex = 0;
			StopCoroutine("ChaseTarget");
			StartCoroutine("ChaseTarget");
		}
	}

	IEnumerator ChaseTarget() {
		Vector3 currentWaypoint = pathChaser[0];
		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex ++;
				if (targetIndex >= pathChaser.Length) {
					yield break;
				}
				currentWaypoint = pathChaser[targetIndex];
			}

			transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			yield return null;

		}
	}

    void OnDrawGizmos(){

        if(stillMoving == false){
            Vector3 startPosition = pathHolder.GetChild(0).position;
            Vector3 previousPosition = startPosition;
            foreach(Transform waypoints in pathHolder){
                Gizmos.DrawSphere(waypoints.position, .3f);
                Gizmos.DrawLine(previousPosition, waypoints.position);
                previousPosition = waypoints.position;
            }

            //If we want to make loop
            Gizmos.DrawLine(previousPosition, startPosition);
        
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
        }
        else{
            if (pathChaser != null) {
                for (int i = targetIndex; i < pathChaser.Length; i ++) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(pathChaser[i], Vector3.one);

                    if (i == targetIndex) {
                        Gizmos.DrawLine(transform.position, pathChaser[i]);
                    }
                    else {
                        Gizmos.DrawLine(pathChaser[i-1],pathChaser[i]);
                    }
                }
		    }
        }
    }
}
