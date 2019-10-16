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
using Android.Database;

namespace Rock.Mobile
{
    namespace Media
    {
        /// <summary>
        /// CameraActivity is purely a wrapper / manager for the camera itself. It immediatley forwards
        /// callbacks to the DroidCamera class below this one.
        /// </summary>
        [Activity( Label = "ImagePickerActivity" )]
        class ImagePickerActivity : Activity
        {
            string ImageFile { get; set; }

            protected override void OnCreate( Android.OS.Bundle savedInstanceState )
            {
                base.OnCreate( savedInstanceState );

                // create an intent to browse images
                Intent intent = new Intent( Intent.ActionPick, Android.Provider.MediaStore.Images.Media.ExternalContentUri );

                StartActivityForResult( Intent.CreateChooser( intent, "Select photo" ), 0 );
            }

            protected override void OnResume( )
            {
                base.OnResume( );
            }

            protected override void OnActivityResult( int requestCode, Result resultCode, Intent data )
            {
                base.OnActivityResult( requestCode, resultCode, data );

                string imagePath = null;

                // if data isn't null, they picked something, so resolve the image path
                if( data != null )
                {
                    imagePath = GetPathToImage( data.Data );
                }

                // either way, notify the picker we're done
                (Rock.Mobile.Media.PlatformImagePicker.Instance as DroidImagePicker).Result( resultCode, imagePath );
                Finish( );
            }

            private string GetPathToImage(Uri uri)
            {
                string path = null;

                // set the column we'll be searching for
                string[] filePathColumn = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };

                // get a cursor into the db
                ICursor cursor = this.ContentResolver.Query( uri, filePathColumn, null, null, null );
                if (cursor != null)
                {
                    // reset and browse to our file
                    cursor.MoveToFirst();

                    int columnIndex = cursor.GetColumnIndex( filePathColumn[ 0 ] );
                    path = cursor.GetString(columnIndex);

                    cursor.Close( );
                }

                // finally return the resolved path
                return path;
            }
        }

        class DroidImagePicker : PlatformImagePicker
        {
            protected ImagePickEvent ImagePickEventDelegate { get; set; }

            public override void PickImage( object context, ImagePickEvent callback )
            {
                // ensure the context passed in is valid.
                Activity activity = context as Activity;
                if( activity == null )
                {
                    throw new Exception( "context must be of type Activity." );
                }

                ImagePickEventDelegate = callback;

                // kick off the activity that will manage the camera
                Intent intent = new Intent( activity, typeof( ImagePickerActivity ) );

                activity.StartActivity( intent );
            }

            public void Result( Result resultCode, string imagePath )
            {
                if( resultCode == Android.App.Result.Ok )
                {
                    // notify our caller it went ok and provide the image
                    ImagePickEventDelegate( this, new ImagePickEventArgs( true, imagePath ) );
                }
                else
                {
                    // or provide nothing if it didn't work
                    ImagePickEventDelegate( this, new ImagePickEventArgs( false, null ) );
                }

            }
        }
    }
}
#endif
