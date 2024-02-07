/*
 * File: ScheduleItemComparer.cs
 * Date: October 11, 2023
 * Description: This file defines the ScheduleItemComparer class, which implements IEqualityComparer for ScheduleItem objects.
 */

using System;
using System.Collections.Generic;
using TravelAgency.Models;

public class ScheduleItemComparer : IEqualityComparer<ScheduleItem>
{
    // Determines whether two ScheduleItem objects are equal.
    public bool Equals(ScheduleItem x, ScheduleItem y)
    {
        if (x == null && y == null)
            return true;
        if (x == null || y == null)
            return false;

        return x.Origin == y.Origin &&
               x.OriginTime == y.OriginTime &&
               x.Destination == y.Destination &&
               x.DestinationTime == y.DestinationTime;
    }

    // Returns a hash code for a ScheduleItem object.
    public int GetHashCode(ScheduleItem obj)
    {
        return obj.Origin.GetHashCode() ^
               obj.OriginTime.GetHashCode() ^
               obj.Destination.GetHashCode() ^
               obj.DestinationTime.GetHashCode();
    }
}
