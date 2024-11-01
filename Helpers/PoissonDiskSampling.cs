using System.Collections.Generic;
using UnityEngine;

namespace VisibleMines.Helpers
{
    public static class PoissonDiskSampling
    {
        public static List<Vector3> GeneratePoints(Bounds bounds, int maxPoints, float minDist)
        {
            List<Vector3> points = new List<Vector3>();
            int maxAttempts = 30;
            int curAttempts = 0;

            while (points.Count < maxPoints && curAttempts < maxAttempts)
            {
                Vector3 randPos = new Vector3(
                        Random.Range(bounds.min.x, bounds.max.x), 
                        bounds.center.y, 
                        Random.Range(bounds.min.z, bounds.max.z)
                    );

                bool isValid = true;
                foreach (var point in points)
                {
                    if (Vector3.Distance(randPos, point) < minDist)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    points.Add(randPos);
                    curAttempts = 0;
                }
                else
                {
                    curAttempts++;
                }
            }

            return points;
        }

        public static List<Vector3> GeneratePointsCollider(BoxCollider collider, int maxPoints, float minDist)
        {
            List<Vector3> points = new List<Vector3>();
            int maxAttempts = 30;
            int curAttempts = 0;

            while (points.Count < maxPoints && curAttempts < maxAttempts)
            {
                Vector3 size = collider.size;
                Vector3 sizeHalf = size / 2;
                Vector3 forward = collider.transform.rotation * Vector3.forward;
                Vector3 localRandPos = new Vector3(
                    Random.Range(-sizeHalf.x, sizeHalf.x),
                    collider.center.y,
                    Random.Range(-sizeHalf.z, sizeHalf.z)
                );
                Vector3 worldPos = collider.transform.TransformPoint(localRandPos);

                bool isValid = true;
                foreach (var point in points)
                {
                    if (Vector3.Distance(worldPos, point) < minDist)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    points.Add(worldPos);
                    curAttempts = 0;
                }
                else
                {
                    curAttempts++;
                }
            }

            return points;
        }
    }
}
