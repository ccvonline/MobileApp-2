using System;

namespace Rock.Mobile.Util
{
    public static class DateTimeExtensions
    {
        public static int AsAge( this DateTime date )
        {
            TimeSpan ageSpan = DateTime.Now - date;
            return (int)ageSpan.TotalDays / 365;
        }
    }
}

