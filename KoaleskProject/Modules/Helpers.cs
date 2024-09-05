using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KoaleskMod.Modules
{
    internal class Helpers
    {
        public static List<Vector3> DistributePointsEvenlyOnSphereCap(float radius, float objectCount, Vector3 center, Quaternion rotation)
        {
            List<Vector3> positions = new List<Vector3>();
            float TotalObjectLength = 0f;

            for (int i = 0; i < objectCount; i++)
            {
                //I am using the bounding box to find the size, I am using an object inside an empty
                float Size = 2.5f;
                TotalObjectLength += Size;
            }

            float MaxSizeOfCurve = 360f; //The original cake we want to cut
            float Circumference = 2 * Mathf.PI * radius; //This is the length of the cake if it was a straight line
            float Scale = TotalObjectLength / Circumference; //This is how much of that line we want to use
            float SizeOfCurve = Scale * 360f; //How much of that cake we want to keep

            if (Scale < 1f)
            {
                MaxSizeOfCurve = SizeOfCurve;
            }

            float DivideTheCircle = MaxSizeOfCurve / objectCount;//Slice the cake piece

            for (int i = 0; i < objectCount; i++)
            {
                float ObjectAngle = DivideTheCircle * i;
                ObjectAngle = ObjectAngle * Mathf.Deg2Rad;
                ObjectAngle += 0f * Mathf.Deg2Rad;

                //Cos is the X axis of a circle with raduis 1, and Sin is the Y axis
                //This means we can use Cos and Sin to get the location of the object using degrees.
                Vector3 LocationOnCircle = new Vector3(Mathf.Cos(ObjectAngle), Mathf.Sin(ObjectAngle), 0);

                LocationOnCircle = LocationOnCircle * radius;
                LocationOnCircle = rotation * LocationOnCircle;

                positions.Add(center + LocationOnCircle);
            }
            return positions;
        }
    }
}
