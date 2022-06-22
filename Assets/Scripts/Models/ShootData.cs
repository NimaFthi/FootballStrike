
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class ShootData
    {
        public string deviceId;
        public Vector3 forceDirection;
        public List<Vector2> animationCurve;

        public ShootData(string deviceId,Vector3 forceDirection,List<Vector2> animationCurve)
        {
            this.deviceId = deviceId;
            this.forceDirection = forceDirection;
            this.animationCurve = animationCurve;
        }
    }
}