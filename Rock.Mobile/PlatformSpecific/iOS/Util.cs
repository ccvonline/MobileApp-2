#if __IOS__
using System;
using Foundation;
using CoreGraphics;
using UIKit;
using System.IO;
using Rock.Mobile.IO;

namespace Rock.Mobile.PlatformSpecific.Util
{
    public static class DateTimeExtensions
    {
        public static DateTime NSDateToDateTime(this NSDate date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime( 
                new DateTime(2001, 1, 1, 0, 0, 0) );
            return reference.AddSeconds(date.SecondsSinceReferenceDate);
        }

        public static NSDate DateTimeToNSDate(this DateTime date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0) );
            return NSDate.FromTimeIntervalSinceReferenceDate(
                (date - reference).TotalSeconds);
        }
    }

    public static class StringExtensions
    {
        public static NSString UrlEncode( this string url )
        {
            NSString displayUrl = new NSString( url );
            return displayUrl.CreateStringByAddingPercentEscapes( NSStringEncoding.ASCIIStringEncoding );
        }
    }

    public static class CGObjectExtensions
    {
        public static System.Drawing.PointF ToPointF( this CGPoint point )
        {
            return new System.Drawing.PointF( (float) point.X, (float) point.Y );
        }

        public static System.Drawing.SizeF ToSizeF( this CGSize size )
        {
            return new System.Drawing.SizeF( (float)size.Width, (float)size.Height );
        }

        public static System.Drawing.RectangleF ToRectF( this CGRect rect )
        {
            return new System.Drawing.RectangleF( (float) rect.X, (float) rect.Y, (float) rect.Width, (float) rect.Height );
        }
    }

    public static class ImageLoader
    {
        public static bool Load( string filename, ref UIImage rImage )
        {
            bool success = false;

            MemoryStream imageStream = (MemoryStream)FileCache.Instance.LoadFile( filename );
            if ( imageStream != null )
            {
                try
                {
                    NSData imageData = NSData.FromStream( imageStream );
                    rImage = new UIImage( imageData, UIScreen.MainScreen.Scale );
                    success = true;
                }
                catch ( Exception )
                {
                    FileCache.Instance.RemoveFile( filename );
                    Rock.Mobile.Util.Debug.WriteLine( string.Format( "Image {0} was corrupt. Removing.", filename ) );
                }
                imageStream.Dispose( );
            }

            return success;
        }
    }
}
#endif
