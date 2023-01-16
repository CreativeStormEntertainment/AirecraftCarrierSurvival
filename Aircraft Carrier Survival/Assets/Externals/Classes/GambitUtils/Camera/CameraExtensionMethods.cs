using UnityEngine;

namespace GambitUtils
{
    public static class CameraExtensionMethods
    {
        public static void SetupViewForRender(this Camera camera, BoxCollider boxCollider, Transform cameraPivot, float normalizedOffset = 0.1f)
        {
            bool boxColliderEnabled = boxCollider.enabled;
            boxCollider.enabled = true;

            float cameraDistance = 3f * Mathv.Max(boxCollider.size);
            var cameraTransform = camera.transform;
            cameraPivot.position = boxCollider.transform.TransformPoint(boxCollider.center); //boxCollider.bounds.center;
            cameraTransform.localPosition = new Vector3(0f, 0f, cameraDistance);
            Rect rect = camera.GetViewspaceBoundsRect(boxCollider, out _);
            //Debug.Log("SetupViewForRender: rect: " + rect.min.ToString("f4") + " " + rect.max.ToString("f4") + " " + rect.center.ToString("f2"));
            float destHalfHeight = normalizedOffset * 0.5f + Mathf.Max(Mathf.Abs(0.5f - rect.min.x), Mathf.Abs(rect.max.x - 0.5f), Mathf.Abs(0.5f - rect.min.y), Mathf.Abs(rect.max.y - 0.5f));
            // 0.5f / d1 = destHalfHeight / d2
            // d2 = d1 * destHalfHeight / 0.5f
            float heightScale = destHalfHeight / 0.5f;
            if (camera.orthographic)
            {
                camera.orthographicSize *= heightScale;
            }
            else
            {
                cameraDistance *= heightScale;
                cameraTransform.localPosition = new Vector3(0f, 0f, cameraDistance);
            }

            boxCollider.enabled = boxColliderEnabled;
        }

        public static Rect GetViewspaceBoundsRect(this Camera camera, BoxCollider boxCollider, out float distance)
        {
            Vector3 extents = boxCollider.size * 0.5f;
            Vector3 center = boxCollider.transform.TransformPoint(boxCollider.center); //boxCollider.bounds.center;

            Vector3 max = Vector2.zero;
            Vector3 min = Vector2.one;
            max.z = float.MinValue;
            min.z = float.MaxValue;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + boxCollider.transform.TransformVector(Mathv.Mul(new Vector3(x, y, z), extents));
                        Vector3 vpPoint = camera.WorldToViewportPoint(corner);
                        min = Mathv.Min(vpPoint, min);
                        max = Mathv.Max(vpPoint, max);
                        //Debug.Log(GetType() + ".vpPoint: " + vpPoint.ToString("f2"));
                    }
                }
            }
            distance = (min.z + max.z) * 0.5f;
            return new Rect() { min = min, max = max, };
        }

        public static float GetTanHalfFov(this Camera camera)
        {
            return Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
        }

        public static float GetTanHalfFovFromProjectionMatrix(this Camera camera)
        {
            return camera.projectionMatrix.GetTanHalfFovFromProjection();
        }

        public static float GetFovFromProjectionMatrix(this Camera camera)
        {
            return camera.projectionMatrix.GetFovFromProjection();
        }

        public static Vector3 ScreenToWorldTranslation(this Camera camera, Vector2 screenTranslation, float cameraDistance)
        {
            if (camera.orthographic)
            {
                return screenTranslation * camera.orthographicSize * 2f / Screen.height;
            }
            else
            {
                float cameraWorldPlaneHeight = 2f * Mathf.Abs(cameraDistance) * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView / 2f);
                return screenTranslation * cameraWorldPlaneHeight / Screen.height;
            }
        }

        public static Vector3 ScreenToWorldTranslation(this Camera camera, Vector2 screenTranslation)
        {
            return ScreenToWorldTranslation(camera, screenTranslation, camera.transform.localPosition.z);
        }

        public static bool GetForwardIntersectionWithPlane(this Camera camera, Vector3 planeUp, Vector3 planePos, out Vector3 isecPt, out float distance)
        {
            Ray ray = new Ray(camera.transform.position, camera.transform.forward);
            return Maths.GetRayIntersectionWithPlane(ray, planeUp, planePos, out isecPt, out distance);
        }
    }
}
