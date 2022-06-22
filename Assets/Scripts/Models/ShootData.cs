
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class ShootData
    {
        public int userId;
        public Vector3 forceDirection;
        public List<Vector2> animationCurve;

        public ShootData(int userId,Vector3 forceDirection,List<Vector2> animationCurve)
        {
            this.userId = userId;
            this.forceDirection = forceDirection;
            this.animationCurve = animationCurve;
        }
    }
}