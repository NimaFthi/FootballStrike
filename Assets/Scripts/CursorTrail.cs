//Shady
using UnityEngine;
using System.Collections.Generic;

namespace Shady
{
    public class CursorTrail : MonoBehaviour
    {
        [SerializeField] LineRenderer trailPrefab = null;
        [SerializeField] Camera Cam; 
        [SerializeField] float clearSpeed = 1;
        [SerializeField] float distanceFromCamera = 1;
    
        private LineRenderer currentTrail;
        private List<Vector3> points = new List<Vector3>();
 
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                DestroyCurrentTrail();
                CreateCurrentTrail();
                AddPoint();
            }//if end
    
            if (Input.GetMouseButton(0))
            {
                AddPoint();
            }//if end
    
            UpdateTrailPoints();
    
            ClearTrailPoints();
        }//Update() end
 
        private void DestroyCurrentTrail()
        {
            if (currentTrail != null)
            {
                Destroy(currentTrail.gameObject);
                currentTrail = null;
                points.Clear();
            }//if end
        }//DestroyCurrentTrail() end

        private void CreateCurrentTrail()
        {
            currentTrail = Instantiate(trailPrefab);
            currentTrail.transform.SetParent(transform, true);
        }//CreateCurrentTrail() end

        private void AddPoint()
        {
            Vector3 mousePosition = Input.mousePosition;
            points.Add(Cam.ViewportToWorldPoint(new Vector3(mousePosition.x / Screen.width, mousePosition.y / Screen.height, distanceFromCamera)));
        }//AddPoint() end
 
        private void UpdateTrailPoints()
        {
            if (currentTrail != null && points.Count > 1)
            {
                currentTrail.positionCount = points.Count;
                currentTrail.SetPositions(points.ToArray());
            }//if end
            else
            {
                DestroyCurrentTrail();
            }//else end
        }//UpdateTrailPoints() end
 
        private void ClearTrailPoints()
        {
            float clearDistance = Time.deltaTime * clearSpeed;
            while (points.Count > 1 && clearDistance > 0)
            {
                float distance = (points[1] - points[0]).magnitude;
                if (clearDistance > distance)
                {
                    points.RemoveAt(0);
                }//if end
                else
                {
                    points[0] = Vector3.Lerp(points[0], points[1], clearDistance / distance);
                }//else end
                clearDistance -= distance;
            }//loop end
        }//ClearTrailPoints() end

        void OnDisable()
        {
            DestroyCurrentTrail();
        }//OnDisable() end
 
    }//class end
}//namespace end