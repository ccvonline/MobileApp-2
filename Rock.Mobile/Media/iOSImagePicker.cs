#if __IOS__
using System;
using UIKit;
using AssetsLibrary;
using Foundation;
using CoreImage;
using CoreGraphics;
using System.Runtime.InteropServices;
using ImageIO;
using CoreFoundation;

namespace Rock.Mobile
{
    namespace Media
    {
        class iOSImagePicker : PlatformImagePicker
        {
            ImagePickEvent ImagePickEventDelegate;

            public override void PickImage( object context, ImagePickEvent callback )
            {
                // first ensure they passed in the correct context type
                UIViewController controller = context as UIViewController;
                if( context == null )
                {
                    throw new Exception( "Context must be a UIViewController" );
                }

                // store our callback event
                ImagePickEventDelegate = callback;

                // setup the image picker
                UIImagePickerController imageController = new UIImagePickerController( );
                imageController.Delegate = new UIImagePickerControllerDelegate( );
                imageController.SourceType = UIImagePickerControllerSourceType.SavedPhotosAlbum;

                // when media is chosen
                imageController.FinishedPickingMedia += (object s, UIImagePickerMediaPickedEventArgs mediaArgs) => 
                    {
                        //NSUrl assetUrl = (NSUrl) mediaArgs.Info[ new NSString("UIImagePickerControllerReferenceURL")];
                        //FinishedCallback( true, assetUrl, imageController );
                        FinishedCallback( true, mediaArgs.OriginalImage, imageController );
                    };

                // if they cancel, simply dismiss the controller
                imageController.Canceled += (object s, EventArgs mediaArgs) => 
                    {
                        FinishedCallback( true, null, imageController );
                    };

                controller.PresentViewController( imageController, true, null );
            }

            /// <summary>
            /// Wrapper for closing the controller and notifying our callback
            /// </summary>
            /// <param name="result">If set to <c>true</c> result.</param>
            /// <param name="imagePath">Image path.</param>
            /// <param name="cameraController">Camera controller.</param>
            protected void FinishedCallback( bool result, UIImage image, UIImagePickerController imageController )
            {
                // notify the callback on the UI thread
                Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                    {
                        imageController.DismissViewController( true, null );
                        ImagePickEventDelegate( this, new ImagePickEventArgs( result, image ) );
                    });
            }
        }
    }
}
#endif
