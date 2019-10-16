#if __IOS__
using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreAnimation;
using CoreGraphics;
using CoreImage;
using AssetsLibrary;

namespace Rock.Mobile.PlatformSpecific.iOS.UI
{
	partial class ImageCropViewController : UIViewController
	{
        class PrivateImageCropConfig
        {
            /// <summary>
            /// The text to display for the 'ok to crop' button.
            /// </summary>
            public const string CropOkButton_Text = "";

            /// <summary>
            /// The size (in font points) of the 'ok to crop' button.
            /// </summary>
            public const int CropOkButton_Size = 36;

            /// <summary>
            /// The text to display for the 'cancel crop' button.
            /// </summary>
            public const string CropCancelButton_Text = "";

            /// <summary>
            /// The size (in font points) of the 'cancel crop' button.
            /// </summary>
            public const int CropCancelButton_Size = 36;

            /// <summary>
            /// The font to use for displaying the Crop / Cancel icons
            /// </summary>
            public const string IconFont = "Bh";
        }

        public enum Request
        {
            ShouldAutorotate,
            SupportedInterfaceOrientations,
            PreferStatusBarHidden,
            IsDeviceLandscape,
            CropperDone
        }

        /// <summary>
        /// Used to get return values and current device status without having a tightly bound parent.
        /// </summary>
        public delegate object GetResponse( Request requestType, object context );
        GetResponse GetResponseCallback;

        /// <summary>
        /// The source image to crop
        /// </summary>
        /// <value>The source image.</value>
        UIImage SourceImage { get; set; }

        /// <summary>
        /// The image view used to display the source and cropped images.
        /// </summary>
        /// <value>The image view.</value>
        UIImageView ImageView { get; set; }

        /// <summary>
        /// The view representing the region of the image to crop to.
        /// </summary>
        /// <value>The crop view.</value>
        UIView CropView { get; set; }

        /// <summary>
        /// Gets or sets the crop view minimum position.
        /// </summary>
        /// <value>The crop view minimum position.</value>
        CGPoint CropViewMinPos { get; set; }

        /// <summary>
        /// Gets or sets the crop view max position.
        /// </summary>
        /// <value>The crop view max position.</value>
        CGPoint CropViewMaxPos { get; set; }

        /// <summary>
        /// Scalar to convert from screen points to image pixels and back
        /// </summary>
        float ScreenToImageScalar { get; set; }

        /// <summary>
        /// The last touch position received. Used for calculating the delta
        /// movement when moving the CropView
        /// </summary>
        /// <value>The last tap position.</value>
        CGPoint LastTapPos { get; set; }

        /// <summary>
        /// The resulting cropped image
        /// </summary>
        /// <value>The cropped image.</value>
        UIImage CroppedImage { get; set; }

        /// <summary>
        /// Crop mode.
        /// </summary>
        enum CropMode
        {
            None,
            Editing,
            Previewing
        }

        /// <summary>
        /// Determines whether we're editing or previewing the crop
        /// </summary>
        /// <value>The mode.</value>
        CropMode Mode { get; set; }

        /// <summary>
        /// The aspect ratio we should be cropping the picture to.
        /// Example: 1.0f would mean 1:1 width/height, or a square.
        /// 9 / 16 would mean 9:16 (or 16:9), which is "wide screen" like a movie.
        /// </summary>
        /// <value>The crop aspect ratio.</value>
        float CropAspectRatio { get; set; }

        UIButton CancelButton { get; set; }
        UIButton EditButton { get; set; }

        UIView FullscreenBlocker { get; set; }
        CAShapeLayer FullscreenBlockerMask { get; set; }

        UIToolbar Toolbar { get; set; }
        UIView ButtonContainer { get; set; }

        int OrientationState { get; set; }

		public ImageCropViewController ( )
		{
		}

        public void Begin( UIImage image, float cropAspectRatio, GetResponse response )
        {
            CropAspectRatio = cropAspectRatio;

            SourceImage = image;

            OrientationState = -1;

            View.SetNeedsLayout( );

            GetResponseCallback = response;
        }

        public override bool ShouldAutorotate()
        {
            return (bool) GetResponseCallback( Request.ShouldAutorotate, null );
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return (UIInterfaceOrientationMask )GetResponseCallback( Request.SupportedInterfaceOrientations, null );
        }

        public override bool PrefersStatusBarHidden()
        {
            return (bool) GetResponseCallback( Request.PreferStatusBarHidden, null );
        }

        public override void ViewDidLayoutSubviews()
        {
            // get the orientation state. WE consider unknown- 1, profile 0, landscape 1,
            int orientationState = (bool)GetResponseCallback( Request.IsDeviceLandscape, null ) == true ? 1 : 0;

            // if the states are in disagreement, correct it
            if ( OrientationState != orientationState )
            {
                OrientationState = orientationState;

                Toolbar.Frame = new CGRect( 0, View.Bounds.Height - 40, View.Bounds.Width, 40 );

                NSString cancelLabel = new NSString( PrivateImageCropConfig.CropCancelButton_Text );
                CGSize buttonSize = cancelLabel.StringSize( CancelButton.Font );
                CancelButton.Bounds = new CGRect( 0, 0, buttonSize.Width, buttonSize.Height );


                NSString editLabel = new NSString( PrivateImageCropConfig.CropOkButton_Text );
                buttonSize = editLabel.StringSize( EditButton.Font );
                EditButton.Bounds = new CGRect( 0, 0, buttonSize.Width, buttonSize.Height );

                ImageView.Frame = GetImageViewFrame( );

                ButtonContainer.Frame = new CGRect( 0, View.Bounds.Height - 40, View.Bounds.Width, 40 );
                CancelButton.Frame = new CGRect( (CancelButton.Frame.Width / 2), 0, CancelButton.Frame.Width, CancelButton.Frame.Height );
                EditButton.Frame = new CGRect( ButtonContainer.Frame.Width - (EditButton.Frame.Width * 2.5f), 0, EditButton.Frame.Width, EditButton.Frame.Height );

                DisplayLayout( );

                // make sure we reset to editing mode
                if ( CropMode.Editing != Mode )
                {
                    SetMode( CropMode.Editing );
                }
                else
                {
                    AnimateBlocker( true );
                }
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.Black;

            // set the image
            ImageView = new UIImageView( );
            ImageView.BackgroundColor = UIColor.Black;
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            View.AddSubview( ImageView );


            // create our cropper
            CropView = new UIView( );
            CropView.BackgroundColor = UIColor.Clear;
            CropView.Layer.BorderColor = UIColor.White.CGColor;
            CropView.Layer.BorderWidth = 1;
            CropView.Layer.CornerRadius = 4;
            View.AddSubview( CropView );


            // create our fullscreen blocker. It needs to be HUGE so we can center the
            // masked part
            FullscreenBlocker = new UIView();
            FullscreenBlocker.BackgroundColor = UIColor.Black;
            FullscreenBlocker.Layer.Opacity = 0.00f;
            FullscreenBlocker.Bounds = new CGRect( 0, 0, 10000, 10000 );
            FullscreenBlocker.AutoresizingMask = UIViewAutoresizing.None;
            View.AddSubview( FullscreenBlocker );

            FullscreenBlockerMask = new CAShapeLayer();
            FullscreenBlockerMask.FillRule = CAShapeLayer.FillRuleEvenOdd;
            FullscreenBlocker.Layer.Mask = FullscreenBlockerMask;


            // create our bottom toolbar
            Toolbar = new UIToolbar( );

            // create the cancel button
            NSString cancelLabel = new NSString( PrivateImageCropConfig.CropCancelButton_Text );

            CancelButton = new UIButton(UIButtonType.System);
            CancelButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateImageCropConfig.IconFont, PrivateImageCropConfig.CropCancelButton_Size );
            CancelButton.SetTitle( cancelLabel.ToString( ), UIControlState.Normal );

            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // if cancel was pressed while editing, cancel this entire operation
                    if( CropMode.Editing == Mode )
                    {
                        GetResponseCallback( Request.CropperDone, null );
                        Mode = CropMode.None;
                    }
                    else
                    {
                        SetMode( CropMode.Editing );
                    }
                };

            // create the edit button
            NSString editLabel = new NSString( PrivateImageCropConfig.CropOkButton_Text );

            EditButton = new UIButton(UIButtonType.System);
            EditButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateImageCropConfig.IconFont, PrivateImageCropConfig.CropOkButton_Size );
            EditButton.SetTitle( editLabel.ToString( ), UIControlState.Normal );
            EditButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;

            EditButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( Mode == CropMode.Previewing )
                    {
                        // confirm we're done
                        GetResponseCallback( Request.CropperDone, CroppedImage );
                        Mode = CropMode.None;
                    }
                    else
                    {
                        SetMode( CropMode.Previewing );
                    }
                };
            
            // create a container that will allow us to align the buttons
            ButtonContainer = new UIView( );
            ButtonContainer.AddSubview( EditButton );
            ButtonContainer.AddSubview( CancelButton );

            CancelButton.BackgroundColor = UIColor.Clear;

            EditButton.BackgroundColor = UIColor.Clear;

            Toolbar.SetItems( new UIBarButtonItem[] { new UIBarButtonItem( ButtonContainer ) }, false );
            View.AddSubview( Toolbar );
        }

        CGRect GetImageViewFrame( )
        {
            // helper function for getting the image View, since it must be smaller that the View in order to accomodate the
            // toolbar
            nfloat imageViewHeight = ( View.Frame.Height - Toolbar.Frame.Height );
            return new CGRect( View.Frame.X, View.Frame.Y, View.Frame.Width, imageViewHeight );
        }

        void DisplayLayout( )
        {
            // scale the image to match the view's width
            ScreenToImageScalar = (float)SourceImage.Size.Width / (float)ImageView.Bounds.Width;

            // get the scaled dimensions, maintaining aspect ratio
            float scaledImageWidth = (float)SourceImage.Size.Width * (1.0f / ScreenToImageScalar);
            float scaledImageHeight = (float)SourceImage.Size.Height * (1.0f / ScreenToImageScalar);

            // if the image's scaled down height would be greater than the device height,
            // recalc based on the height.
            if ( scaledImageHeight > ImageView.Frame.Height )
            {
                ScreenToImageScalar = (float)SourceImage.Size.Height / (float)ImageView.Bounds.Height;

                scaledImageWidth = (float)SourceImage.Size.Width * (1.0f / ScreenToImageScalar);
                scaledImageHeight = (float)SourceImage.Size.Height * (1.0f / ScreenToImageScalar);
            }


            // calculate the image's starting X / Y location
            nfloat imageStartX = ( ImageView.Frame.Width - scaledImageWidth ) / 2;
            nfloat imageStartY = ( ImageView.Frame.Height - scaledImageHeight ) / 2;


            // now calculate the size of the cropper
            nfloat cropperWidth = scaledImageWidth;
            nfloat cropperHeight = scaledImageHeight;


            // get the image's aspect ratio so we can shrink down the cropper correctly
            nfloat aspectRatio = SourceImage.Size.Width / SourceImage.Size.Height;

            // if the cropper should be wider than it is tall (or square)
            if ( CropAspectRatio <= 1.0f )
            {
                // then if the image is wider than it is tall, scale down the cropper's width
                if ( aspectRatio > 1.0f )
                {
                    cropperWidth *= 1 / aspectRatio;
                }

                // and the height should be scaled down from the width
                cropperHeight = cropperWidth * CropAspectRatio;
            }
            else
            {
                // the cropper should be taller than it is wide

                // so if the image is taller than it is wide, scale down the cropper's height
                if ( aspectRatio < 1.0f )
                {
                    cropperWidth *= 1 / aspectRatio;
                }

                // and the width should be scaled down from the height. (Invert CropAspectRatio since it was Width based)
                cropperWidth = cropperHeight * (1 / CropAspectRatio);
            }

            // set the crop bounds
            CropView.Frame = new CGRect( ImageView.Frame.X, ImageView.Frame.Y, cropperWidth, cropperHeight );


            // Now set the min / max movement bounds for the cropper
            CropViewMinPos = new CGPoint( imageStartX, imageStartY );

            CropViewMaxPos = new CGPoint( ( imageStartX + scaledImageWidth ) - cropperWidth,
                                          ( imageStartY + scaledImageHeight ) - cropperHeight );

            // center the cropview
            CropView.Layer.Position = new CGPoint( 0, 0 );
            MoveCropView( new CGPoint( ImageView.Bounds.Width / 2, ImageView.Bounds.Height / 2 ) );



            // setup the mask that will reveal only the part of the image that will be cropped
            FullscreenBlocker.Layer.Opacity = 0.00f;
            UIBezierPath viewFill = UIBezierPath.FromRect( FullscreenBlocker.Bounds );
            UIBezierPath cropMask = UIBezierPath.FromRoundedRect( new CGRect( ( FullscreenBlocker.Bounds.Width - CropView.Bounds.Width ) / 2, 
                ( FullscreenBlocker.Bounds.Height - CropView.Bounds.Height ) / 2, 
                CropView.Bounds.Width, 
                CropView.Bounds.Height ), 4 );
            viewFill.AppendPath( cropMask );
            FullscreenBlockerMask.Path = viewFill.CGPath;

            // and set our source image
            ImageView.Image = SourceImage;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            AnimateBlocker( false );
        }

        void AnimateBlocker( bool visible )
        {
            // animate in the blocker
            UIView.Animate( .5f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                new Action( delegate
                    { 
                        FullscreenBlocker.Layer.Opacity = visible == true ? .60f : 0.00f;
                        CropView.Layer.Opacity = visible == true ? 1.00f : 0.00f;
                    } )
                , new Action( delegate
                    { 
                    } ) );
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            if ( Mode == CropMode.Editing )
            {
                UITouch touch = touches.AnyObject as UITouch;
                if ( touch != null )
                {
                    LastTapPos = touch.LocationInView( View );
                }
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            if ( Mode == CropMode.Editing )
            {
                // adjust by the amount moved
                UITouch touch = touches.AnyObject as UITouch;
                if ( touch != null )
                {
                    CGPoint touchPoint = touch.LocationInView( View );

                    CGPoint delta = new CGPoint( touchPoint.X - LastTapPos.X, touchPoint.Y - LastTapPos.Y );

                    MoveCropView( delta );

                    LastTapPos = touchPoint;
                }
            }
        }

        void MoveCropView( CGPoint delta )
        {
            // update the crop view by how much it should be moved
            float xPos = (float) (CropView.Frame.X + delta.X);
            float yPos = (float) (CropView.Frame.Y + delta.Y);

            // clamp to valid bounds
            xPos = (float) System.Math.Max( CropViewMinPos.X, System.Math.Min( xPos, CropViewMaxPos.X ) );
            yPos = (float) System.Math.Max( CropViewMinPos.Y, System.Math.Min( yPos, CropViewMaxPos.Y ) );

            CropView.Frame = new CGRect( xPos, yPos, CropView.Frame.Width, CropView.Frame.Height );

            // update the position of the blocking view
            FullscreenBlocker.Center = CropView.Layer.Position;
        }

        void SetMode( CropMode mode )
        {
            if( mode == Mode )
            {
                throw new Exception( string.Format( "Crop Mode {0} requested, but already in that mode.", mode ) );
            }

            switch( mode )
            {
                case CropMode.Editing:
                {
                    // if we're entering Editing for the first time, setup is simple.
                    if ( Mode == CropMode.None )
                    {
                        // fade in the blocker, which will help the user
                        // see what they're supposed to be doing
                        AnimateBlocker( true );
                    }
                    // If we're coming BACK from previewing
                    if ( Mode == CropMode.Previewing )
                    {
                        // Then we need to reverse the animation we played to crop the image.
                        UIView.Animate( .5f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                            // ANIMATING
                            new Action( delegate
                                { 
                                    // animate the cropped image down to its original size.
                                    ImageView.Frame = CropView.Frame;
                                } )
                            // DONE ANIMATING
                            , new Action( delegate
                                { 
                                    // done, so now set the original image (which will
                                    // seamlessly replace the cropped image)
                                    ImageView.Frame = GetImageViewFrame( );
                                    ImageView.Image = SourceImage;

                                    // and turn on the blocker fully, so we still only see
                                    // the cropped part of the image
                                    FullscreenBlocker.Layer.Opacity = 1.00f;

                                    // and finally animate down the blocker, so it restores
                                    // the original editing appearance
                                    AnimateBlocker( true );
                                } ) 
                        );
                    }
                    break;
                }

                case CropMode.Previewing:
                {
                    // create the cropped image
                    CroppedImage = CropImage( SourceImage, new CGRect( CropView.Frame.X - CropViewMinPos.X, 
                                                                                          CropView.Frame.Y - CropViewMinPos.Y, 
                                                                                          CropView.Frame.Width , 
                                                                                          CropView.Frame.Height ) );


                    // Kick off an animation that will simulate cropping and scaling up the image.
                    UIView.Animate( .5f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                        // ANIMATING
                        new Action( delegate
                            { 
                                // animate in the blocker fully, which will black out the non-cropped parts of the image.
                                FullscreenBlocker.Layer.Opacity = 1.00f;

                                // fade out the cropper border
                                CropView.Layer.Opacity = 0.00f;
                            } )
                        // DONE ANIMATING
                        , new Action( delegate
                            { 
                                // set the scaled cropped image, seamlessly replacing full image
                                ImageView.Frame = CropView.Frame;
                                ImageView.Image = CroppedImage;

                                // and turn off the blocker completely (nothing to block now, since we're literally using
                                // the cropped image
                                FullscreenBlocker.Layer.Opacity = 0.00f;

                                // and kick off a final (chained) animation that scales UP the cropped image to fill the viewport. 
                                UIView.Animate( .5f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                                    new Action( delegate
                                        { 
                                            ImageView.Frame = GetImageViewFrame( );
                                        } ), null );
                            } ) );
                    break;
                }
            }

            Mode = mode;
        }

        UIImage CropImage( UIImage sourceImage, CGRect cropDimension )
        {
            // step one, transform the crop region into image space.
            // (So pixelX is a pixel in the actual image, not the scaled screen)

            // convert our position on screen to where it should be in the image
            float pixelX = (float) (cropDimension.X * ScreenToImageScalar);
            float pixelY = (float) (cropDimension.Y * ScreenToImageScalar);

            // same for height, since the image was scaled down to fit the screen.
            float width = (float) cropDimension.Width * ScreenToImageScalar;
            float height = (float) cropDimension.Height * ScreenToImageScalar;


            // Now we're going to rotate the image to actually be "up" as the user
            // sees it. To do that, we simply rotate it according to the apple documentation.
            float rotationDegrees = 0.0f;

            switch ( sourceImage.Orientation )
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
            
            // Now get a transform so we can rotate the image to be oriented the same as when the user previewed it
            CGAffineTransform fullImageTransform = GetImageTransformAboutCenter( rotationDegrees, sourceImage.Size );

            // apply to the image
            CIImage ciCorrectedImage = new CIImage( sourceImage.CGImage );
            CIImage ciCorrectedRotatedImage = ciCorrectedImage.ImageByApplyingTransform( fullImageTransform );

            // create a context and render it back out to a CGImage.
            CIContext ciContext = CIContext.FromOptions( null );
            CGImage rotatedCGImage = ciContext.CreateCGImage( ciCorrectedRotatedImage, ciCorrectedRotatedImage.Extent );

            // now the image is properly orientated, so we can crop it.
            CGRect cropRegion = new CGRect( pixelX, pixelY, width, height );
            CGImage croppedImage = rotatedCGImage.WithImageInRect( cropRegion );
            return new UIImage( croppedImage );
        }

        CGAffineTransform GetImageTransformAboutCenter( float angleDegrees, CGSize imageSize )
        {
            // Create a tranform that will rotate our image about its center
            CGAffineTransform transform = CGAffineTransform.MakeIdentity( );

            // setup our transform. Translate it by the image's half width/height so it rotates about its center.
            transform.Translate( -imageSize.Width / 2, -imageSize.Height / 2 );
            transform.Rotate( angleDegrees * Rock.Mobile.Math.Util.DegToRad );

            // now we need to concat on a post-transform that will put the image's pivot back at the top left.
            // get the image's dimensions transformed
            CGRect transformedImageRect = transform.TransformRect( new CGRect( 0, 0, imageSize.Width, imageSize.Height ) );

            // our post transform simply translates the image back
            CGAffineTransform postTransform = CGAffineTransform.MakeIdentity( );
            postTransform.Translate( transformedImageRect.Width / 2, transformedImageRect.Height / 2 );

            // now multiply the transform and postTranform and we have our final transform to use
            return CGAffineTransform.Multiply( transform, postTransform );
        }
	}
}
#endif