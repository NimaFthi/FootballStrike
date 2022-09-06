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

    private Vector3 _firstRigidbodyPosition;
    //stats
    [SerializeField] private float shootPowerX = 1f;
    [SerializeField] private float shootPowerY = 2f;
    [SerializeField] private float shootPowerZ = 1f;
    [SerializeField] private float curveFactor = 1f;

    //easy touch
    private Gesture gesture;

    //curve shot
    public List<Vector2> swipeCurvePoints = new List<Vector2>();
    public AnimationCurve animationCurve;
    
    // public AnimationCurve defaultCurve;
    //
    // public AnimationCurve default2Curve;
    //other
    private bool canSwipe;
    private Coroutine currenCoroutine;


    private void Start()
    {
        _firstRigidbodyPosition = rb.transform.position;
        //SocketManager.Instance.onGameAction = OnShoot;
        //MatchManager.Instance.onMatchReceived = (isPlayerOne) => { canSwipe = isPlayerOne; };
    }

    public async void ChangeTurn()
    {
        print("Change Turn");
        await Task.Delay(5000);
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
        ballGfx.transform.localPosition = Vector3.zero;
        ballCollider.center = Vector3.zero;
        rb.transform.position = _firstRigidbodyPosition;
        hitTxt.gameObject.SetActive(false);
    }

    private void OnShoot(GameLog log)
    {
        ShootData shootData = JsonConvert.DeserializeObject<ShootData>(log.parameters["data"].ToString());
        if (shootData.userId != User.Instance.id)
        {
            animationCurve = Vector2ToAnimationCurve(shootData.animationCurve);
            Shoot(shootData.forceDirection);
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

    private void Update()
    {
        gesture = EasyTouch.current;

        //if (!canSwipe) return;

        CalculateCurve();

        if (gesture.type == EasyTouch.EvtType.On_Swipe && gesture.touchCount == 1 && gesture.actionTime > 1.5f)
        {
            var cameraAndGoalDistance = Vector3.Magnitude(goalPos.position - Camera.main.transform.position);
            var shootVector = gesture.GetTouchToWorldPoint(cameraAndGoalDistance - 0) - rb.transform.position;
            Shoot(shootVector);
            canSwipe = false;
            return;
        }

        if (gesture.type == EasyTouch.EvtType.On_SwipeEnd && gesture.touchCount == 1)
        {
            var cameraAndGoalDistance = Vector3.Magnitude(goalPos.position - Camera.main.transform.position);
            var shootDir = gesture.GetTouchToWorldPoint(cameraAndGoalDistance) - rb.transform.position;
            Shoot(shootDir);
            canSwipe = false;
        }
    }
    

    private void Shoot(Vector3 shootDir)
    {
        print("here");

        ShootData shootData = new ShootData(User.Instance.id,shootDir, AnimationCurveToVector2);
        var data = new Dictionary<string, object>
        {
            ["data"] = shootData
        };
        //GameLog log = new GameLog(SocketManager.Instance.LogNumber, "shoot", data);
        //SocketManager.Instance.SendAction(log);

        var shootForce = new Vector3(shootDir.x * shootPowerX, shootDir.y * shootPowerY, shootDir.z * shootPowerZ);
        rb.AddForceAtPosition(shootForce, shootPos.position, ForceMode.Impulse);
        currenCoroutine = StartCoroutine(CurveTheBall());
        ChangeTurn();
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
    }
}