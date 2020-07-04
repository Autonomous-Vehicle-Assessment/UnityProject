using System;
using System.Collections;
using UnityEngine;

public enum SpeedType
{
    KPH,
    MPH,
    MPS
}


public static class GenericFunctions
{
    public static string ToOrdinal(this int value)
    {
        // Start with the most common extension.
        string extension = "th";

        // Examine the last 2 digits.
        int last_digits = value % 100;

        // If the last digits are 11, 12, or 13, use th. Otherwise:
        if (last_digits < 11 || last_digits > 13)
        {
            // Check the last digit.
            switch (last_digits % 10)
            {
                case 1:
                    extension = "st";
                    break;
                case 2:
                    extension = "nd";
                    break;
                case 3:
                    extension = "rd";
                    break;
            }
        }

        return extension;
    }

    public static float GetVectorAngle(Vector2 dir)
    {
        float Angle = Mathf.Atan2(dir[1], dir[0]) * 180/Mathf.PI;
        return Angle;
    }

    public static (string,float) SpeedTypeConverter(SpeedType SpeedType)
    {
        float SpeedCoefficient = 1.0f;
        string SpeedString = "m/s";
        switch (SpeedType)
        {
            case SpeedType.MPH:
                SpeedString = " MPH";
                SpeedCoefficient = 2.23693629f;
                break;
            case SpeedType.KPH:
                SpeedString = " km/h";
                SpeedCoefficient = 3.6f;
                break;
            case SpeedType.MPS:
                SpeedString = " m/s";
                SpeedCoefficient = 1.0f;
                break;
        }
        return (SpeedString, SpeedCoefficient);
    }

}
