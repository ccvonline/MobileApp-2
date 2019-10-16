#if __ANDROID__
using Android.Graphics;
using Rock.Mobile.IO;
using System;
using Rock.Mobile.Threading;

namespace Rock.Mobile.PlatformSpecific.Android.Util
{
    /// <summary>
    /// Implementation of our request object. Stores the filePath, request and handler to be executed later
    /// on the worker thread.
    /// </summary>
    internal class ImageLoadJobObject : Rock.Mobile.Threading.AsyncTaskScheduler.IJobRequest
    {
        /// <summary>
        /// Path for the file to be loaded
        /// </summary>
        string FilePath { get; set; }

        bool Bundled { get; set; }
        bool ScaleForDPI { get; set; }

        /// <summary>
        /// The handler to call when the request is complete
        /// </summary>
        public delegate bool ResultHandlerDelegate( Bitmap resultObj );
        ResultHandlerDelegate ResultHandler { get; set; }

        public ImageLoadJobObject( string filePath, bool bundled, bool scaleForDpi, ResultHandlerDelegate resultHandler )
        {
            FilePath = filePath;

            Bundled = bundled;

            ScaleForDPI = scaleForDpi;

            ResultHandler = resultHandler;
        }

        public void ProcessRequest( )
        {
            BitmapFactory.Options decodeOptions = new BitmapFactory.Options( );
            Bitmap imageBmp = null;

            try
            {
                // if true, the image is a hdpi image and should be scaled to an appropriate size of the device
                if( ScaleForDPI == true )
                {
                    decodeOptions.InSampleSize = (int)System.Math.Ceiling( Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics.Density );
                }

                // if it was bundled, take it from our assets
                if( Bundled == true )
                {
                    System.IO.Stream bundleStream = Rock.Mobile.PlatformSpecific.Android.Core.Context.Assets.Open( FilePath );
                    if( bundleStream != null )
                    {
                        imageBmp = BitmapFactory.DecodeStream( bundleStream, null, decodeOptions );
                        bundleStream.Dispose( );
                    }
                    else
                    {
                        Rock.Mobile.Util.Debug.WriteLine( string.Format( "ASYNCLOAD ERROR: Failed to load image {0}", FilePath ) );
                    }

                }
                // else filecache
                else
                {
                    System.IO.MemoryStream assetStream = (System.IO.MemoryStream)FileCache.Instance.LoadFile( FilePath );
                    if( assetStream != null )
                    {
                        imageBmp = BitmapFactory.DecodeStream( assetStream, null, decodeOptions );
                        if( imageBmp == null )
                        {
                            Rock.Mobile.Util.Debug.WriteLine( string.Format( "ASYNCLOAD ERROR: Image loaded null. {0}", FilePath ) );
                        }
                        assetStream.Dispose( );
                    }
                    else
                    {
                        Rock.Mobile.Util.Debug.WriteLine( string.Format( "ASYNCLOAD ERROR: Failed to load image {0}", FilePath ) );
                    }
                }
            }
            catch( Exception e )
            {
                Rock.Mobile.Util.Debug.WriteLine( string.Format( "ASYNCLOAD ERROR: Failed to load image {0} {1}", FilePath, e ) );
            }

            // on the UI thread, notify each handler that the file is loaded.
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    // if the callback wasn't consumed, we need to dispose the bitmap ourselves.
                    if( ResultHandler( imageBmp ) == false )
                    {
                        if ( imageBmp != null )
                        {
                            imageBmp.Recycle( );
                            imageBmp.Dispose( );
                        }
                    }
                    imageBmp = null;
                });
        }
    }

	
    public class AsyncLoader
    {
        public delegate bool OnLoaded( Bitmap image );
        public static void LoadImage( string imageName, bool bundled, bool scaleForDpi, OnLoaded onLoaded )
        {
            AsyncTaskScheduler.Instance.AddJob( new ImageLoadJobObject( imageName, bundled, scaleForDpi, delegate( Bitmap imageBmp ) { return onLoaded( imageBmp ); } ) );
        }
    }
}
#endif