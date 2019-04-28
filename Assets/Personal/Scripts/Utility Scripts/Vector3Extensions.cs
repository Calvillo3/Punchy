using UnityEngine;

public static class Vector3Extensions
{
    public static bool EqualToWithinRange(this Vector3 vector, Vector3 compareVector, float squareMagnitudeRange)
    {
        return (vector - compareVector).sqrMagnitude < squareMagnitudeRange;
    }
}