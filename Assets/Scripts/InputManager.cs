using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HedgehogTeam.EasyTouch;
using Models;
using Newtonsoft.Json;
using Shady.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    //components
    [SerializeField] private Rigidbody rb;
    [SerializeField] private SphereCollider ballCollider;
    [SerializeField] private Transform ballGfx;
    [SerializeField] private Transform shootPos;
    [SerializeField] private Transform goalPos;
    [SerializeField] private GameObject hitTxt;
    [SerializeField] private GameObject goalKeeper;
    [SerializeField] private GameObject goalObject;

    private Vector3 _firstRigidbodyPosition;
    private Quaternion _firstRigidbodyRotation;
    private Vector3 _firstGKPosition;
    //stats
    [SerializeField] private float shootPowerX = 1f;
    [SerializeField] private float shootPowerY = 2f;
    [SerializeField] private float shootPowerZ = 1f;
    [SerializeField] private float curveFactor = 1f;
    [SerializeField] private float changeTurnDelay = 5f;
    [SerializeField] private float maxSwipeTime = 1.5f;
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private float gkSpeed = 10f;
    [SerializeField] private float gkDelay = 1f;
    [SerializeField, Range(0,1)] private float animationDamp = 0.5f;
    [SerializeField] private int trackEveryXFrames = 5;

    //easy touch
    private Gesture gesture;

    //curve shot
    public List<Vector2> swipeCurvePoints = new List<Vector2>();
    public AnimationCurve animationCurve;
    
    // public AnimationCurve defaultCurve;
    //
    // public AnimationCurve default2Curve;
    //other
    private bool canSwipe = true;
    private Coroutine currenCoroutine;
    private List<Vector3> points = new List<Vector3>();
    private List<Vector2> touchPoints = new List<Vector2>();
    private bool isTracking = false;
    //private float zTrack;
    private float currentSwipeTime;
    //private Vector3 offset;
    private bool hitObstacle = false;
    private int frameTracker = 0;
    private Plane goalPlane;


    private void Start()
    {
        goalPlane = new Plane(Vector3.back, goalObject.transform.position);
        DrawPlane(goalObject.transform.position, Vector3.back);
        _firstRigidbodyPosition = rb.transform.position;
        _firstRigidbodyRotation = rb.transform.rotation;
        _firstGKPosition = goalKeeper.transform.position;
        //SocketManager.Instance.onGameAction = OnShoot;
        //MatchManager.Instance.onMatchReceived = (isPlayerOne) => { canSwipe = isPlayerOne; };
    }

    public async void ChangeTurn()
    {
        canSwipe = false;
        iTween.Stop(goalKeeper);
        print("Change Turn");
        await Task.Delay((int)(changeTurnDelay * 1000));
        canSwipe = true;
        print("After Change Turn");
        //bool isMyTurn = MatchManager.Instance.ChangeTurn;
        //if (isMyTurn)
        //{
        //    canSwipe = true;
        //}
        //else
        //{
        //    canSwipe = false;
        //}
        
        swipeCurvePoints.Clear();
        var defaultCurve = new AnimationCurve();
        defaultCurve.AddKey(0f, 0f);
        defaultCurve.AddKey(1f, 0f);
        animationCurve = defaultCurve;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        ballGfx.transform.localPosition = Vector3.zero;
        ballCollider.center = Vector3.zero;
        rb.transform.position = _firstRigidbodyPosition;
        rb.transform.rotation = _firstRigidbodyRotation;
        hitTxt.gameObject.SetActive(false);
        hitObstacle = false;
        iTween.MoveTo(goalKeeper, iTween.Hash(
            "position", _firstGKPosition
            ));
    }

    private void OnShoot(GameLog log)
    {
        ShootData shootData = JsonConvert.DeserializeObject<ShootData>(log.parameters["data"].ToString());
        if (shootData.userId != User.Instance.id)
        {
            animationCurve = Vector2ToAnimationCurve(shootData.animationCurve);
            //Shoot(shootData.forceDirection);
        }
    }

    private void OnDrawGizmos()
    {
        if (swipeCurvePoints != null && swipeCurvePoints.Count > 0)
        {
            Gizmos.color = Color.white;

            Gizmos.DrawLine(swipeCurvePoints[0], swipeCurvePoints[swipeCurvePoints.Count - 1]);
            Gizmos.color = Color.yellow;
            for (int i = 1; i < swipeCurvePoints.Count; i++)
            {
                Gizmos.DrawLine(swipeCurvePoints[i - 1], swipeCurvePoints[i]);
            }
        }
    }

    private Vector3[] normalizePoints(List<Vector3> list, float lastZ)
    {
        float temp = (16 - list[list.Count-1].z)/list.Count;
        for(int i=1; i<list.Count; i++)
        {
            list[i] = new Vector3(list[i].x, list[i].y, list[i].z + (temp * i));
        }
        return list.ToArray();
    }

    private float damp(float originF, float newF)
    {
        return (originF * animationDamp) + (newF * (1-animationDamp));
    }

    private Vector3 keepItOnFloor(Vector3 position)
    {
        if(position.y < _firstRigidbodyPosition.y)
        {
            position.y = _firstRigidbodyPosition.y;
        }
        return position;
    }

    private Vector3[] screenPointsRacCast(List<Vector2> screenPoints)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoints[screenPoints.Count-1]);
        float distance = 0f;
        goalPlane.Raycast(ray, out distance);
        Vector3 destPos = ray.GetPoint(distance);
        Vector3 perpendicular = Vector3.Cross(Vector3.left, destPos - transform.position);
        Plane shootPlane = new Plane(perpendicular, transform.position);
        DrawPlane(transform.position, perpendicular);
        foreach(Vector2 point in screenPoints)
        {
            ray = Camera.main.ScreenPointToRay(point);
            goalPlane.Raycast(ray, out distance);
            points.Add(ray.GetPoint(distance));
        }
        return points.ToArray();
    }

    private void DrawPlane(Vector3 position, Vector3 normal)
    {

        Vector3 v3;

        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

        var corner0 = position + v3;
        var corner2 = position - v3;
        var q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        var corner1 = position + v3;
        var corner3 = position - v3;

        Debug.DrawLine(corner0, corner2, Color.green);
        Debug.DrawLine(corner1, corner3, Color.green);
        Debug.DrawLine(corner0, corner1, Color.green);
        Debug.DrawLine(corner1, corner2, Color.green);
        Debug.DrawLine(corner2, corner3, Color.green);
        Debug.DrawLine(corner3, corner0, Color.green);
        Debug.DrawRay(position, normal, Color.red);
    }

    private void Update()
    {
        if (!canSwipe) return;

        if (!isTracking)
        {
            if(Input.touchCount > 0)
            {
                currentSwipeTime = 0;
                isTracking = true;
                touchPoints.Add(Input.GetTouch(0).position);
                //zTrack = Mathf.Abs(transform.position.z - Camera.main.transform.position.z);
                //Vector3 touchPos = Camera.main.ScreenToWorldPoint(
                //    new Vector3(touch.position.x, touch.position.y, zTrack)
                //    );
                //offset = transform.position - touchPos;
                //points.Add(transform.position);
            }
        }
        else
        {
            currentSwipeTime += Time.deltaTime;
            if(Input.touchCount < 1 || currentSwipeTime > maxSwipeTime)
            {
                isTracking = false;
                //Shoot(normalizePoints(points, zTrack));
                Shoot(screenPointsRacCast(touchPoints));
            }
            else
            {
                frameTracker = (frameTracker + 1) % trackEveryXFrames;
                if (frameTracker == 0)
                {
                    touchPoints.Add(Input.GetTouch(0).position);
                    //zTrack += ((50 / maxSwipeTime) * Time.deltaTime * trackEveryXFrames);
                    //Vector3 touchPos = Camera.main.ScreenToWorldPoint(
                    //    new Vector3(touch.position.x, touch.position.y, zTrack)
                    //    ) + offset;
                    //points.Add(keepItOnFloor(new Vector3(
                    //    damp(points[points.Count - 1].x, touchPos.x),
                    //    damp(points[points.Count - 1].y, touchPos.y),
                    //    touchPos.z)));
                }
            }
        }

        //gesture = EasyTouch.current;

        //CalculateCurve();

        //if (gesture.type == EasyTouch.EvtType.On_Swipe && gesture.touchCount == 1 && gesture.actionTime > maxSwipeTime)
        //{
        //    var cameraAndGoalDistance = Vector3.Magnitude(goalPos.position - Camera.main.transform.position);
        //    var shootVector = gesture.GetTouchToWorldPoint(cameraAndGoalDistance - 0) - rb.transform.position;
        //    Shoot(shootVector);
        //    canSwipe = false;
        //    return;
        //}

        //if (gesture.type == EasyTouch.EvtType.On_SwipeEnd && gesture.touchCount == 1)
        //{
        //    var cameraAndGoalDistance = Vector3.Magnitude(goalPos.position - Camera.main.transform.position);
        //    var shootDir = gesture.GetTouchToWorldPoint(cameraAndGoalDistance) - rb.transform.position;
        //    Shoot(shootDir);
        //    canSwipe = false;
        //}
    }

    private void onCompletePath()
    {
        rb.velocity = Vector3.Normalize(points[points.Count-1] - points[points.Count-2]) * animationSpeed;
        points.Clear();
        ChangeTurn();
    }

    private Vector3 gkDestination(Vector3 ballDestination)
    {
        return new Vector3(
            ballDestination.x,
            goalKeeper.transform.position.y,
            goalKeeper.transform.position.z
            );
    }

    private void Shoot(Vector3[] shootPath)
    {
        canSwipe = false;
        iTween.MoveTo(gameObject, iTween.Hash(
            "path", shootPath,
            "oncomplete", "onCompletePath",
            "speed", animationSpeed,
            "orienttopath", true,
            "lookahead", 1f,
            "easetype", iTween.EaseType.linear));
        iTween.MoveTo(goalKeeper, iTween.Hash(
            "position", gkDestination(shootPath[shootPath.Length-1]),
            "speed", gkSpeed,
            "delay", gkDelay
            ));
        //ShootData shootData = new ShootData(User.Instance.id,shootDir, AnimationCurveToVector2);
        //var data = new Dictionary<string, object>
        //{
        //    ["data"] = shootData
        //};
        ////GameLog log = new GameLog(SocketManager.Instance.LogNumber, "shoot", data);
        ////SocketManager.Instance.SendAction(log);

        //var shootForce = new Vector3(shootDir.x * shootPowerX, shootDir.y * shootPowerY, shootDir.z * shootPowerZ);
        //rb.AddForceAtPosition(shootForce, shootPos.position, ForceMode.Impulse);
        //currenCoroutine = StartCoroutine(CurveTheBall());
        //ChangeTurn();
    }

    private List<Vector2> AnimationCurveToVector2
    {
        get
        {
            List<Vector2> data = new List<Vector2>();
            foreach (var curve in animationCurve.keys)
            {
                data.Add(new Vector2(curve.time, curve.value));
            }

            return data;
        }
    }

    private AnimationCurve Vector2ToAnimationCurve(List<Vector2> data)
    {
        AnimationCurve curve = new AnimationCurve();
        foreach (var datum in data)
        {
            curve.AddKey(datum.x, datum.y);
        }

        return curve;
    }

    private void CalculateCurve()
    {
        if (gesture.type == EasyTouch.EvtType.On_Swipe && gesture.touchCount == 1)
        {
            var cameraAndGoalDistance = Vector3.Magnitude(goalPos.position - Camera.main.transform.position);
            swipeCurvePoints.Add(gesture.GetTouchToWorldPoint(cameraAndGoalDistance));
        }

        if (gesture.type == EasyTouch.EvtType.On_SwipeEnd)
        {
            var xSum = 0f;
            foreach (var point in swipeCurvePoints)
            {
                xSum += point.x - LineEquation(swipeCurvePoints[0], swipeCurvePoints[swipeCurvePoints.Count - 1],
                    point.y);
            }

            var xAvg = xSum / swipeCurvePoints.Count;
            animationCurve.AddKey(0.5f, xAvg * curveFactor);
        }
    }

    private float LineEquation(Vector2 startPoint, Vector2 endPoint, float y)
    {
        var dx = endPoint.x - startPoint.x;
        if (Mathf.Abs(dx) > 0)
        {
            var a = (endPoint.y - startPoint.y) / dx;
            var c = endPoint.y - endPoint.x * a;
            if (a == 0)
            {
                return startPoint.x;
            }

            return (y - c) / a;
        }

        return startPoint.x;
    }

    private IEnumerator CurveTheBall()
    {
        var lastKey = animationCurve.keys[animationCurve.length - 1];
        var secondToLastKey = animationCurve.keys[animationCurve.length - 2];

        var curveXLast = animationCurve.Evaluate(lastKey.time);
        var curveXSecondToLast = animationCurve.Evaluate(secondToLastKey.time);

        var timer = 0f;
        var firstBallPos = ballGfx.localPosition;
        var firstBallColliderCenter = ballCollider.center;
        var lastGfxPos = Vector3.zero;
        var secondToLastGfxPos = Vector3.zero;

        while (timer < lastKey.time)
        {
            var curveX = animationCurve.Evaluate(timer);
            
            var localPosition = ballGfx.localPosition;
            localPosition = new Vector3(firstBallPos.x + curveX, localPosition.y, localPosition.z);
            ballGfx.localPosition = localPosition;

            ballCollider.center = new Vector3(firstBallColliderCenter.x + curveX, 0, 0);
            
            timer += Time.deltaTime;

            if (secondToLastKey.time < timer)
            {
                secondToLastGfxPos = ballGfx.localPosition;
            }

            yield return null;
        }
        
        lastGfxPos = ballGfx.localPosition;
        ballGfx.localPosition = firstBallPos;
        ballCollider.center = firstBallColliderCenter;

        AddForceAtEnd(new Vector3(curveXLast - curveXSecondToLast, lastGfxPos.y - secondToLastGfxPos.y,
            lastGfxPos.z - secondToLastGfxPos.z));
    }

    private void AddForceAtEnd(Vector3 forceDir)
    {
        rb.AddForce(forceDir * 1, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("hit");
        if (currenCoroutine != null)
        {
            StopCoroutine(currenCoroutine);
        }

        if (other.gameObject.CompareTag("Target"))
        {
            hitTxt.SetActive(true);
            Debug.Log("Hit target");
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            if (!hitObstacle)
            {
                iTween.Stop(gameObject);
                points.Clear();
                ChangeTurn();
            }
            hitObstacle = true;
        }
    }
}