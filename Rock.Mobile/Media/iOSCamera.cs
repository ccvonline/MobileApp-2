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
using AVFoundation;

namespace Rock.Mobile
{
    namespace Media
    {
        class iOSCamera : PlatformCamera
        {
            string ImageDest { get; set; }

            class CameraController : UIImagePickerController
            {
                static CameraController _Instance = new CameraController( );
                public static CameraController Instance { get { return _Instance; } }

                /*public override bool ShouldAutorotate()
                {
                    if ( UIDeviceOrientation.Portrait == UIDevice.CurrentDevice.Orientation )
                    {
                        return true;
                    }
                    return false;
                }

                public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations( )
                {
                    return UIInterfaceOrientationMask.Portrait;
                }

                public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation( )
                {
                    return UIInterfaceOrientation.Portrait;
                }*/
            }

            protected CaptureImageEvent CaptureImageEventDelegate { get; set; }

            public override bool IsAvailable( )
            {
                // get the permission settings
                AVAuthorizationStatus authStatus = AVCaptureDevice.GetAuthorizationStatus( AVMediaType.Video );

                // see if there's even a camera ON the device
                bool isTypeAvailable = UIImagePickerController.IsSourceTypeAvailable( UIImagePickerControllerSourceType.Camera );

                // if there IS a camera, and the permission is allowed or notdetermined, then yes, it's available.
                if ( isTypeAvailable == true && (authStatus == AVAuthorizationStatus.Authorized || authStatus == AVAuthorizationStatus.NotDetermined) )
                {
                    return true;
                }

                // otherwise it isn't.
                return false;
            }

            public override void CaptureImage( object imageDest, object context, CaptureImageEvent callback )
            {
                // first ensure they passed in the correct context type
                UIViewController controller = context as UIViewController;
                if( context == null )
                {
                    throw new Exception( "Context must be a UIViewController" );
                }

                string imageDestStr = imageDest as string;
                if( imageDestStr == null )
                {
                    throw new Exception( "imageDest must be of type string." );
                }
                ImageDest = imageDestStr;

                // store our callback event
                CaptureImageEventDelegate = callback;

                // create our camera controller
                CameraController.Instance.SourceType = UIImagePickerControllerSourceType.Camera;

                // when media is chosen
                CameraController.Instance.FinishedPickingMedia -= CameraImageCaptured;
                CameraController.Instance.FinishedPickingMedia += CameraImageCaptured;

                // when picking is cancelled.
                CameraController.Instance.Canceled -= CameraImageCanceled;
                CameraController.Instance.Canceled += CameraImageCanceled;

                controller.PresentViewController( CameraController.Instance, true, null );

            }

            void CameraImageCanceled(object sender, EventArgs e)
            {
                CameraFinishedCallback( true, null );
            }

            void CameraImageCaptured(object sender, UIImagePickerMediaPickedEventArgs e)
            {
                bool result = false;
                string imagePath = null;

                // create a url of the path for the file to write
                NSUrl imageDestUrl = NSUrl.CreateFileUrl( new string[] { ImageDest } );

                // create a CGImage destination that converts the image to jpeg
                CGImageDestination cgImageDest = CGImageDestination.Create( imageDestUrl, MobileCoreServices.UTType.JPEG, 1 );

                if ( cgImageDest != null )
                {
                    // note: the edited image is saved "correctly", so we don't have to rotate.

                    // rotate the image 0 degrees since we consider portrait to be the default position.
                    CIImage ciImage = new CIImage( e.OriginalImage.CGImage );

                    float rotationDegrees = 0.00f;
                    switch ( e.OriginalImage.Orientation )
                    {
                        case UIImageOrientation.Up:
                        {
                            // don't do anything. The image space and the user space are 1:1
                            break;
                        }
                        case UIImageOrientation.Left:
                        {
                            // the image space is rotated 90 degrees from user space,
                            // so do a CCW 90 degree rotation
                            rotationDegrees = 90.0f;
                            break;
                        }
                        case UIImageOrientation.Right:
                        {
                            // the image space is rotated -90 degrees from user space,
                            // so do a CW 90 degree rotation
                            rotationDegrees = -90.0f;
                            break;
                        }
                        case UIImageOrientation.Down:
                        {
                            rotationDegrees = 180;
                            break;
                        }
                    }

                    // create our transform and apply it to the image
                    CGAffineTransform transform = CGAffineTransform.MakeIdentity( );
                    transform.Rotate( rotationDegrees * Rock.Mobile.Math.Util.DegToRad );
                    CIImage rotatedImage = ciImage.ImageByApplyingTransform( transform );

                    // create a context and render it back out to a CGImage. (Cast to ints so we account for any floating point error)
                    CIContext ciContext = CIContext.FromOptions( null );
                    CGImage rotatedCGImage = ciContext.CreateCGImage( rotatedImage, new System.Drawing.RectangleF( (int)rotatedImage.Extent.X, 
                                                                                                                   (int)rotatedImage.Extent.Y, 
                                                                                                                   (int)rotatedImage.Extent.Width, 
                                                                                                                   (int)rotatedImage.Extent.Height ) );

                    // put the image in the destination, converting it to jpeg.
                    cgImageDest.AddImage( rotatedCGImage );


                    // close and dispose.
                    if ( cgImageDest.Close( ) )
                    {
                        result = true;
                        imagePath = ImageDest;

                        cgImageDest.Dispose( );
                    }
                }

                CameraFinishedCallback( result, imagePath );
            }

            /// <summary>
            /// Wrapper for closing the camera controller and notifying our callback
            /// </summary>
            /// <param name="result">If set to <c>true</c> result.</param>
            /// <param name="imagePath">Image path.</param>
            /// <param name="cameraController">Camera controller.</param>
            protected void CameraFinishedCallback( bool result, string imagePath )
            {
                // notify the callback on the UI thread
                Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                    {
                        CameraController.Instance.DismissViewController( true, delegate { } );
                        CaptureImageEventDelegate( this, new CaptureImageEventArgs( result, imagePath ) );
                    });
            }
        }
    }
}
#endif
