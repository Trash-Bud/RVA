

    using UnityEngine;

    public static class DistanceUtils
    {

        public static bool IsClosestToPlayerPlayer1(GameObject target1Obj, GameObject target2Obj)
        {
            // Getting positions and rotations of markers
            var position1 = target1Obj.transform.localPosition;
            var rotation1 = target1Obj.transform.localRotation;
            
            var position2 = target2Obj.transform.localPosition;
            var rotation2 = target2Obj.transform.localRotation;

            // Calculate the vector from the center of one marker to the other
            Vector3 direction = position2 - position1;
            
            Vector3 forward1 = rotation1 * Vector3.forward;
            Vector3 forward2 = rotation2 * Vector3.forward;

            // Calculate the dot product of the forward vectors
            float dotProduct1 =  Vector3.Dot(forward1, direction.normalized);
            float dotProduct2 =  Vector3.Dot(forward2, -direction.normalized);

            // Calculate the angles between the forward vectors
            float angle1 = Mathf.Acos(dotProduct1) * Mathf.Rad2Deg;
            float angle2 = Mathf.Acos(dotProduct2) * Mathf.Rad2Deg;

            return angle1 > angle2;
        }
        
        public static bool IsBattlePosition(GameObject target1Obj, GameObject target2Obj)
        {
            // Getting positions and rotations of markers
            var position1 = target1Obj.transform.localPosition;
            var rotation1 = target1Obj.transform.localRotation;
            
            var position2 = target2Obj.transform.localPosition;
            var rotation2 = target2Obj.transform.localRotation;

            // Calculate the vector from the center of one marker to the other
            Vector3 direction = position2 - position1;
            // Check if the markers are close by on the same plane
            float distanceThreshold = 2.0f; // Adjust this threshold as needed
            if (direction.magnitude < distanceThreshold)
            {
                float angleThreshold = 30f; // Adjust this threshold as needed

                // Calculate the forward vectors of the rotations
                Vector3 forward1 = rotation1 * Vector3.forward;
                Vector3 forward2 = rotation2 * Vector3.forward;

                // Calculate the dot product of the forward vectors
                float dotProduct1 =  Vector3.Dot(forward1, direction.normalized);
                float dotProduct2 =  Vector3.Dot(forward2, -direction.normalized);

                // Calculate the angles between the forward vectors
                float angle1 = Mathf.Acos(dotProduct1) * Mathf.Rad2Deg;
                float angle2 = Mathf.Acos(dotProduct2) * Mathf.Rad2Deg;


                // Check if either angle is within the threshold
                if (angle1 < angleThreshold && angle2 < angleThreshold)
                {
                    return true;
                }
            }

            return false;
        }
        

    }
