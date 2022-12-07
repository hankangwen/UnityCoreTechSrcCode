using UnityEngine;
using System.Collections;

public class Algorithm : MonoBehaviour
{

      public const float EPSILON = 0.00001f;

      public static bool IsEqual(float a, float b)
      {
          if (Mathf.Abs(a - b) < EPSILON)
          {
              return true;
          } 
          else
          {
              return false;
          }
      }
    
      public static bool IsEqual(Vector3 a, Vector3 b)
      {
          if (Mathf.Abs(a.x - b.x) < EPSILON && Mathf.Abs(a.y - b.y) < EPSILON && Mathf.Abs(a.z - b.z) < EPSILON)
          {
              return true;
          } 
          else
          {
              return false;
          }
      }

      public static float Distance(Vector3 a, Vector3 b)
      {
          return Vector3.Distance(a, b);
      }

      public static float ClampAngle(float angle, float min, float max)
      {
          if (angle < -360)
              angle += 360;
          if (angle > 360)
              angle -= 360;

          return Mathf.Clamp(angle, min, max);
      }

      public static float Clamp(float distances, float mindis, float maxdis)
      {
         return  Mathf.Clamp(distances, mindis, maxdis);
      }

      public static bool EqualString(string a, string b)
      {
          if (a.CompareTo(b) == 0)
          {
              return true;
          } 
          else
          {
              return false;
          }
      }
}
