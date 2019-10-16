#if __ANDROID__
using System;
using System.Collections.Generic;

using Android.Content;
using Android.Content.PM;
using Android.Provider;
using Java.IO;
using Android.Graphics;
using Android.App;

using Uri = Android.Net.Uri;
using Environment = Android.OS.Environment;
using Android.OS;
using Android.Views;
using Android.Media;
using System.IO;

namespace Rock.Mobile
{
    namespace Media
    {
        /// <summary>
        /// CameraActivity is purely a wrapper / manager for the camera itself. It immediatley forwards
        /// callbacks to the DroidCamera class below this one.
        /// </summary>
        [Activity( Label = "CameraActivity" )]
        class CameraActivity : Activity
        {
            // Code if we ever manually need to use a camera.
            /*class MyOrientationListener : OrientationEventListener
            {
                public MyOrientationListener( Context context, Android.Hardware.SensorDelay delay ) : base( context, delay )
                {
                }

                public override void OnOrientationChanged(int orientation)
                {
                    Rock.Mobile.Util.Debug.WriteLine( "Orientation changed" );

                    for ( int i = 0; i < Android.Hardware.Camera.NumberOfCameras; i++ )
                    {
                        Android.Hardware.Camera.CameraInfo camInfo = new Android.Hardware.Camera.CameraInfo();
                        Android.Hardware.Camera.GetCameraInfo( i, camInfo );

                        orientation = (orientation + 45) / 90 * 90;

                        int rotation = 0;
                        if (camInfo.Facing == Android.Hardware.CameraFacing.Front )
                        {
                            rotation = (camInfo.Orientation - orientation + 360) % 360;
                        }
                        else 
                        {  
                            // back-facing camera
                            rotation = (camInfo.Orientation + orientation) % 360;
                        }

                        Android.Hardware.Camera camera = Android.Hardware.Camera.Open( i );
                        Android.Hardware.Camera.Parameters camParams = camera.GetParameters( );
                        camParams.SetRotation( rotation );
                        camera.SetParameters( camParams );
                        //Android.Hardware.Camera.Parameters.SetRotation( rotation );
                        //camInfo.
                    }
                }
            }*/

            string ImageFile { get; set; }

            //MyOrientationListener Listener { get; set; }

            protected override void OnCreate( Android.OS.Bundle savedInstanceState )
            {
                base.OnCreate( savedInstanceState );

                //Listener = new MyOrientationListener( this, Android.Hardware.SensorDelay.Normal );
                //Listener.Enable( );

                bool didStartCamera = false;
                if( savedInstanceState != null )
                {
                    // grab the last active element
                    didStartCamera = savedInstanceState.GetBoolean( "DidStartCamera" );
                    ImageFile = savedInstanceState.GetString( "ImageFile" );
                }

                // make sure the camera hasn't already been started, which will happen if
                // the orientation of the device is changed while looking at the camera's preview
                // image.
                if ( didStartCamera == false )
                {
                    // retrieve the desired location
                    Java.IO.File imageFile = (Java.IO.File)this.Intent.Extras.Get( "ImageDest" );

                    // create our intent and launch the camera
                    Intent intent = new Intent( MediaStore.ActionImageCapture );

                    // notify the intent where the captured image should go.
                    intent.PutExtra( MediaStore.ExtraOutput, Uri.FromFile( imageFile ) );

                    // store it as an aboslute string
                    ImageFile = imageFile.AbsolutePath;

                    StartActivityForResult( intent, 0 );
                }
            }

            protected override void OnSaveInstanceState( Bundle outState )
            {
                base.OnSaveInstanceState( outState );

                // store the last activity we were in
                outState.PutBoolean( "DidStartCamera", true );
                outState.PutString( "ImageFile", ImageFile );
            }

            protected override void OnResume( )
            {
                base.OnResume( );
            }

            protected override void OnStop()
            {
                base.OnStop();
            }

            protected override void OnDestroy()
            {
                base.OnDestroy();
            }

            protected override void OnActivityResult( int requestCode, Result resultCode, Intent data )
            {
                base.OnActivityResult( requestCode, resultCode, data );

                // forward this to the camera
                (Rock.Mobile.Media.PlatformCamera.Instance as DroidCamera).CameraResult( resultCode, ImageFile );

                Finish( );
            }
        }

        class DroidCamera : PlatformCamera
        {
            protected CaptureImageEvent CaptureImageEventDelegate { get; set; }
            public Java.IO.File ImageFileDest { get; set; }

            public override bool IsAvailable( )
            {
                // is there an activity that can get pictures (which is true only if there's a camera)
                Intent intent = new Intent( MediaStore.ActionImageCapture );
                IList<ResolveInfo> availableActivities = Rock.Mobile.PlatformSpecific.Android.Core.Context.PackageManager.QueryIntentActivities( intent, PackageInfoFlags.MatchDefaultOnly );

                return availableActivities != null && availableActivities.Count > 0;
            }


            public override void CaptureImage( object imageDest, object context, CaptureImageEvent callback )
            {
                // ensure the context passed in is valid.
                Activity activity = Rock.Mobile.PlatformSpecific.Android.Core.Context as Activity;
                if( activity == null )
                {
                    throw new Exception( "Rock.Mobile.PlatformSpecific.Android.Core.Context must be of type Activity." );
                }

                // store the location they want the file to be in.
                Java.IO.File imageFileDest = imageDest as Java.IO.File;
                if( imageFileDest == null )
                {
                    throw new Exception( "imageDest must be of type File" );
                }

                CaptureImageEventDelegate = callback;

                // kick off the activity that will manage the camera
                Intent intent = new Intent( activity, typeof( CameraActivity ) );
                intent.PutExtra( "ImageDest", imageFileDest );

                activity.StartActivity( intent );
            }

            public void CameraResult( Result resultCode, string imageFile )
            {
                switch ( resultCode )
                {
                    case Result.Ok: 
                    {
                        

                        // send off the success notification
                        CaptureImageEventDelegate( this, new CaptureImageEventArgs( true, imageFile ) ); 
                        break;
                    }

                    case Result.Canceled: CaptureImageEventDelegate( this, new CaptureImageEventArgs( true, null ) ); break;
                    default: CaptureImageEventDelegate( this, new CaptureImageEventArgs( false, null ) ); break;
                }
            }
        }
    }
}
#endif
