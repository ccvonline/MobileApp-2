
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Java.IO;
using App.Shared.Config;
using Rock.Mobile.UI;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using Rock.Mobile.Animation;
using App.Shared.PrivateConfig;

namespace Droid
{
    public class ImageCropFragment : Fragment, View.IOnTouchListener
    {
        public Springboard SpringboardParent { get; set; }

        /// <summary>
        /// The last touch position received. Used for calculating the delta
        /// movement when moving the CropView
        /// </summary>
        /// <value>The last tap position.</value>
        PointF LastTapPos { get; set; }

        /// <summary>
        /// The view representing the region of picture that will be kept when cropping.
        /// </summary>
        /// <value>The crop view.</value>
        View CropView { get; set; }

        PointF CropViewMinPos { get; set; }
        PointF CropViewMaxPos { get; set; }


        /// <summary>
        /// Scalar to convert from screen points to image pixels and back
        /// </summary>
        float ScreenToImageScalar { get; set; }

        /// <summary>
        /// The dimensions of the screen, needed for animating the image.
        /// </summary>
        /// <value>The size of the screen.</value>
        System.Drawing.SizeF ScreenSize { get; set; }

        /// <summary>
        /// The image we're cropping
        /// </summary>
        /// <value>The source image.</value>
        Android.Graphics.Bitmap SourceImage { get; set; }

        /// <summary>
        /// The source image scaled to fit the screen
        /// </summary>
        /// <value>The scaled source image.</value>
        Android.Graphics.Bitmap ScaledSourceImage { get; set; }

        /// <summary>
        /// The resulting cropped image
        /// </summary>
        /// <value>The cropped image.</value>
        Android.Graphics.Bitmap CroppedImage { get; set; }

        /// <summary>
        /// The resulting cropped image scaled to fit the screen
        /// </summary>
        /// <value>The scaled cropped image.</value>
        Android.Graphics.Bitmap ScaledCroppedImage { get; set; }

        /// <summary>
        /// The view that displays the source/cropped image
        /// </summary>
        /// <value>The image view.</value>
        AspectScaledImageView ImageView { get; set; }

        /// <summary>
        /// The aspect ratio we should be cropping the picture to.
        /// Example: 1.0f would mean 1:1 width/height, or a square.
        /// 9 / 16 would mean 9:16 (or 16:9), which is "wide screen" like a movie.
        /// </summary>
        /// <value>The crop aspect ratio.</value>
        float CropAspectRatio { get; set; }

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

        Button CancelButton { get; set; }

        Button ConfirmButton { get; set; }

        bool Animating { get; set; }

        Rock.Mobile.PlatformSpecific.Android.Graphics.MaskLayer MaskLayer { get; set; }

        /// <summary>
        /// Time for the cropped animation to scale up or down
        /// </summary>
        const float ImageAnimationTime = .25f;

        /// <summary>
        /// Time for the mask to fade in / our
        /// </summary>
        const float MaskFadeTime = .50f;

        /// <summary>
        /// AMOUNT of opacity the mask should faded in by.
        /// </summary>
        const float MaskFadeAmount = .50f;

        /// <summary>
        /// The orientation when this fragment begins. Allows us to lock the orientation while
        /// active, and restore it when stopping.
        /// </summary>
        Android.Content.PM.ScreenOrientation StartingOrientation { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );
        }

        public void Begin( string sourceImagePath, float cropAspectRatio )
        {
            SourceImage = BitmapFactory.DecodeFile( sourceImagePath );
            CropAspectRatio = cropAspectRatio;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            ScreenSize = new System.Drawing.SizeF( displaySize.X, displaySize.Y );

            // scale the image to match the view's width
            ScreenToImageScalar = (float) SourceImage.Width / (float) ScreenSize.Width;

            // get the scaled dimensions, maintaining aspect ratio
            float scaledWidth = (float)SourceImage.Width * (1.0f / ScreenToImageScalar);
            float scaledHeight = (float)SourceImage.Height * (1.0f / ScreenToImageScalar);

            // now, if the scaled height is too large, re-calc with Height is the dominant, 
            // so we guarantee a fit within the view.
            if( scaledHeight > ScreenSize.Height )
            {
                ScreenToImageScalar = (float) SourceImage.Height / (float) ScreenSize.Height;

                scaledWidth = (float)SourceImage.Width * (1.0f / ScreenToImageScalar);
                scaledHeight = (float)SourceImage.Height * (1.0f / ScreenToImageScalar);
            }

            ScaledSourceImage = Bitmap.CreateScaledBitmap( SourceImage, (int)scaledWidth, (int)scaledHeight, false );


            // setup our layout for touch input
            RelativeLayout view = inflater.Inflate( Resource.Layout.ImageCrop, container, false ) as RelativeLayout;
            view.SetOnTouchListener( this );


            // create the view that will display the image to crop
            ImageView = new AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            ImageView.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ImageView.LayoutParameters.Width = ScaledSourceImage.Width;
            ImageView.LayoutParameters.Height = ScaledSourceImage.Height;

            // center the image
            ImageView.SetX( (ScreenSize.Width - ImageView.LayoutParameters.Width ) / 2 );
            ImageView.SetY( (ScreenSize.Height - ImageView.LayoutParameters.Height ) / 2 );

            view.AddView( ImageView );

            // create the draggable crop view that will let the user pic which part of the image to use.
            CropView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            CropView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );

            // the crop view's dimensions should be based on what the user wanted to crop to. We'll do width, and then height as a scale of width.
            CropView.LayoutParameters.Width = (int) (scaledWidth < scaledHeight ? scaledWidth : scaledHeight);
            CropView.LayoutParameters.Height = (int) ((float) CropView.LayoutParameters.Width * CropAspectRatio);

            // the crop view should be a nice outlined rounded rect
            float _Radius = 3.0f;
            RoundRectShape rectShape = new RoundRectShape( new float[] { _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius }, null, null );
            // configure its paint
            ShapeDrawable border = new ShapeDrawable( rectShape );
            border.Paint.SetStyle( Paint.Style.Stroke );
            border.Paint.StrokeWidth = 8;
            border.Paint.Color = Color.WhiteSmoke;
            CropView.Background = border;


            // set our clamp values
            CropViewMinPos = new PointF( (ScreenSize.Width - scaledWidth) / 2,
                                         (ScreenSize.Height - scaledHeight) / 2 );

            CropViewMaxPos = new PointF( CropViewMinPos.X + (scaledWidth - CropView.LayoutParameters.Width),
                                         CropViewMinPos.Y + (scaledHeight - CropView.LayoutParameters.Height) );

            view.AddView( CropView );

            // create a mask layer that will block out the parts of the image that will be cropped
            MaskLayer = new Rock.Mobile.PlatformSpecific.Android.Graphics.MaskLayer( (int)ScreenSize.Width, (int)ScreenSize.Height, CropView.LayoutParameters.Width, CropView.LayoutParameters.Height, Rock.Mobile.PlatformSpecific.Android.Core.Context );
            MaskLayer.LayoutParameters = new RelativeLayout.LayoutParams( (int)ScreenSize.Width, (int)ScreenSize.Height );
            MaskLayer.Opacity = 0.00f;
            view.AddView( MaskLayer );



            // Now setup our bottom area with cancel, crop, and text to explain
            RelativeLayout bottomBarLayout = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            bottomBarLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
            ((RelativeLayout.LayoutParams)bottomBarLayout.LayoutParameters).AddRule( LayoutRules.AlignParentBottom );

            // set the nav subBar color (including opacity)
            Color navColor = Rock.Mobile.UI.Util.GetUIColor( PrivateSubNavToolbarConfig.BackgroundColor );
            navColor.A = (Byte) ( (float) navColor.A * PrivateSubNavToolbarConfig.Opacity );
            bottomBarLayout.SetBackgroundColor( navColor );

            bottomBarLayout.LayoutParameters.Height = 150;
            view.AddView( bottomBarLayout );

            // setup the cancel button (which will undo cropping or take you back to the picture taker)
            CancelButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            CancelButton.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            CancelButton.Gravity = GravityFlags.Left;
            ((RelativeLayout.LayoutParams)CancelButton.LayoutParameters).AddRule( LayoutRules.AlignParentLeft );

            // set the crop button's font
            Android.Graphics.Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );
            CancelButton.SetTypeface( fontFace, Android.Graphics.TypefaceStyle.Normal );
            CancelButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateImageCropConfig.CropCancelButton_Size );
            CancelButton.Text = PrivateImageCropConfig.CropCancelButton_Text;

            CancelButton.Click += (object sender, EventArgs e) => 
                {
                    // don't allow button presses while animations are going on
                    if( Animating == false )
                    {
                        // if they hit cancel while previewing, go back to editing
                        if( Mode == CropMode.Previewing )
                        {
                            SetMode( CropMode.Editing );
                        }
                        else
                        {
                            // they pressed it while they're in editing mode, so go back to camera mode
                            Activity.OnBackPressed( );
                        }
                    }
                };

            bottomBarLayout.AddView( CancelButton );

            // setup the Confirm button, which will use a font to display its graphic
            ConfirmButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            ConfirmButton.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ConfirmButton.Gravity = GravityFlags.Right;
            ((RelativeLayout.LayoutParams)ConfirmButton.LayoutParameters).AddRule( LayoutRules.AlignParentRight );

            // set the crop button's font
            fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );
            ConfirmButton.SetTypeface( fontFace, Android.Graphics.TypefaceStyle.Normal );
            ConfirmButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateImageCropConfig.CropOkButton_Size );
            ConfirmButton.Text = PrivateImageCropConfig.CropOkButton_Text;

            // when clicked, we should crop the image.
            ConfirmButton.Click += (object sender, EventArgs e) => 
                {
                    // don't allow button presses while animations are going on
                    if( Animating == false )
                    {
                        // if they pressed confirm while editing, go to preview
                        if( Mode == CropMode.Editing )
                        {
                            SetMode( CropMode.Previewing );
                        }
                        else
                        {
                            // notify the caller
                            SpringboardParent.ModalFragmentDone( CroppedImage );
                        }
                    }
                };

            bottomBarLayout.AddView( ConfirmButton );

            // start in editing mode (obviously)
            SetMode( CropMode.Editing );

            // start the cropper centered
            MoveCropView( new PointF( (ScreenSize.Width - CropView.LayoutParameters.Width) / 2, (ScreenSize.Height - CropView.LayoutParameters.Height) / 2 ) );

            MaskLayer.Position = new PointF( CropView.GetX( ), CropView.GetY( ) );

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            SpringboardParent.ModalFragmentOpened( this );

            StartingOrientation = Activity.RequestedOrientation;

            // freeze the orientation
            if ( MainActivity.IsLandscapeWide( ) )
            {
                Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            }
            else
            {
                Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            // restore the orientation
            Activity.RequestedOrientation = StartingOrientation;
        }

        public override void OnStop()
        {
            base.OnStop();

            SpringboardParent.ModalFragmentDone( null );
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // force our image view to release its references to our bitmaps
            ImageView.SetImageBitmap( null );

            // free the resources we're done with
            if( SourceImage != null )
            {
                SourceImage.Dispose( );
                SourceImage = null;
            }

            if( ScaledCroppedImage != null )
            {
                ScaledCroppedImage.Dispose( );
                ScaledCroppedImage = null;
            }

            if( ScaledSourceImage != null )
            {
                ScaledSourceImage.Dispose( );
                ScaledSourceImage = null;
            }

            // free the cropped image
            if( CroppedImage != null )
            {
                CroppedImage.Dispose( );
                CroppedImage = null;
            }

            SetMode( CropMode.None );
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
                    // If we're entering edit mode for the first time
                    if ( Mode == CropMode.None )
                    {
                        // Animate in the mask
                        SimpleAnimator_Float floatAnimator = new SimpleAnimator_Float( MaskLayer.Opacity, MaskFadeAmount, MaskFadeTime, 
                            delegate( float percent, object value )
                            {
                                Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                    {
                                        MaskLayer.Opacity = (float)value;
                                    } ); 
                            }, 
                            null );

                        floatAnimator.Start( );


                        // turn on our cropper
                        CropView.Visibility = ViewStates.Visible;

                        // and set the source image to the scaled source.
                        ImageView.SetImageBitmap( ScaledSourceImage );
                    }
                    // else we're coming FROM Preview Mode, so we need to animate
                    else
                    {
                        Animating = true;

                        // setup the dimension changes
                        System.Drawing.SizeF startSize = new System.Drawing.SizeF( ImageView.Width, ImageView.Height );
                        System.Drawing.SizeF endSize = new System.Drawing.SizeF( CropView.Width, CropView.Height );

                        PointF startPos = new PointF( ImageView.GetX( ), ImageView.GetY( ) );
                        PointF endPos = new PointF( CropView.GetX( ), CropView.GetY( ) );


                        // now animate the cropped image up to its full size
                        AnimateImageView( ImageView, startPos, endPos, startSize, endSize, ImageAnimationTime, 
                            delegate 
                            { 
                                ImageView.SetImageBitmap( null );

                                // release any cropped image we had.
                                if ( CroppedImage != null )
                                {
                                    CroppedImage.Dispose( );
                                    CroppedImage = null;
                                }

                                // release the scaled version if we had it
                                if ( ScaledCroppedImage != null )
                                {
                                    ScaledCroppedImage.Dispose( );
                                    ScaledCroppedImage = null;
                                }


                                ImageView.SetImageBitmap( ScaledSourceImage );
                                ImageView.LayoutParameters.Width = ScaledSourceImage.Width;
                                ImageView.LayoutParameters.Height = ScaledSourceImage.Height;

                                // center the image
                                ImageView.SetX( (ScreenSize.Width - ImageView.LayoutParameters.Width ) / 2 );
                                ImageView.SetY( (ScreenSize.Height - ImageView.LayoutParameters.Height ) / 2 );

                                MaskLayer.Visibility = ViewStates.Visible;
                                CropView.Visibility = ViewStates.Visible;

                                SimpleAnimator_Float floatAnimator = new SimpleAnimator_Float( MaskLayer.Opacity, MaskFadeAmount, MaskFadeTime, 
                                    delegate( float percent, object value )
                                    {
                                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                            {
                                                MaskLayer.Opacity = (float)value;
                                                CropView.Alpha = percent;
                                            } ); 
                                    }, 
                                    // FINISHED MASK FADE-OUT
                                    delegate
                                    {
                                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                            {
                                                Animating = false;
                                            });
                                    });
                                floatAnimator.Start( );
                            } );
                    }

                    break;
                }

                case CropMode.Previewing:
                {
                    // don't allow a state change while we're animating
                    Animating = true;

                    SimpleAnimator_Float floatAnimator = new SimpleAnimator_Float( MaskLayer.Opacity, 1.00f, MaskFadeTime, 
                        delegate( float percent, object value )
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    MaskLayer.Opacity = (float)value;
                                    CropView.Alpha = 1.0f - percent;
                                } ); 
                        }, 
                        // FINISHED MASK FADE-IN
                        delegate
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    // hide the mask and cropper
                                    MaskLayer.Visibility = ViewStates.Gone;
                                    CropView.Visibility = ViewStates.Gone;

                                    // create the cropped image
                                    CroppedImage = CropImage( SourceImage, new System.Drawing.RectangleF( CropView.GetX( ) - CropViewMinPos.X, 
                                        CropView.GetY( ) - CropViewMinPos.Y, 
                                        CropView.LayoutParameters.Width, 
                                        CropView.LayoutParameters.Height ) );

                                    // create a scaled version of the cropped image
                                    float scaledWidth = (float)CroppedImage.Width * (1.0f / ScreenToImageScalar);
                                    float scaledHeight = (float)CroppedImage.Height * (1.0f / ScreenToImageScalar);
                                    ScaledCroppedImage = Bitmap.CreateScaledBitmap( CroppedImage, (int)scaledWidth, (int)scaledHeight, false );

                                    // set the scaled cropped image
                                    ImageView.SetImageBitmap( null );
                                    ImageView.SetImageBitmap( ScaledCroppedImage );

                                    // start the scaled cropped image scaled down further to match its size within the full image.
                                    ImageView.SetX( CropView.GetX( ) );
                                    ImageView.SetY( CropView.GetY( ) );
                                    ImageView.LayoutParameters.Width = CropView.Width;
                                    ImageView.LayoutParameters.Height = CropView.Height;


                                    // setup the dimension changes
                                    System.Drawing.SizeF startSize = new System.Drawing.SizeF( ScaledCroppedImage.Width, ScaledCroppedImage.Height );

                                    System.Drawing.SizeF endSize;
                                    if( ScreenSize.Width < ScreenSize.Height )
                                    {
                                        endSize = new System.Drawing.SizeF( ScreenSize.Width, (float)System.Math.Ceiling( ScreenSize.Width * ( startSize.Width / startSize.Height ) ) );
                                    }
                                    else
                                    {
                                        endSize = new System.Drawing.SizeF( (float)System.Math.Ceiling( ScreenSize.Height * ( startSize.Height / startSize.Width ) ), ScreenSize.Height );
                                    }

                                    PointF startPos = new PointF( CropView.GetX( ), CropView.GetY( ) );
                                    PointF endPos = new PointF( (ScreenSize.Width - endSize.Width) / 2, (ScreenSize.Height - endSize.Height) / 2 );


                                    // now animate the cropped image up to its full size
                                    AnimateImageView( ImageView, startPos, endPos, startSize, endSize, ImageAnimationTime, 
                                        delegate 
                                        { 
                                            Animating = false;
                                        } );
                                } ); 
                        } );

                    floatAnimator.Start( );
                    break;
                }
            }

            Mode = mode;
        }

        void AnimateImageView( View imageView, PointF startPos, PointF endPos, System.Drawing.SizeF startSize, System.Drawing.SizeF endSize, float duration, SimpleAnimator.AnimationComplete completeDelegate )
        {
            // calculate the deltas once before we start
            float xDelta = endPos.X - startPos.X;
            float yDelta = endPos.Y - startPos.Y;

            float deltaWidth = endSize.Width - startSize.Width;
            float deltaHeight = endSize.Height - startSize.Height;

            // create an animator
            SimpleAnimator_Float imageAnimator = new SimpleAnimator_Float( 0.00f, 1.00f, duration, 
                delegate( float percent, object value )
                {
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            // each update, interpolate the deltas and apply 
                            imageView.SetX( startPos.X + ( xDelta * percent ) );
                            imageView.SetY( startPos.Y + ( yDelta * percent ) );

                            imageView.LayoutParameters.Width = (int)( startSize.Width + ( deltaWidth * percent ) );
                            imageView.LayoutParameters.Height = (int)( startSize.Height + ( deltaHeight * percent ) );

                            // force the image to re-evaluate its size
                            imageView.RequestLayout( );
                        } ); 
                }, 
                //ANIMATION COMPLETE
                delegate
                {
                    if ( completeDelegate != null )
                    {
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                            {
                                completeDelegate( );
                            } );
                    }
                } );

            imageAnimator.Start( );
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            if ( Mode == CropMode.Editing )
            {
                switch ( e.Action )
                {
                    case MotionEventActions.Down:
                    {
                        // stamp our position so we can update the crop view
                        LastTapPos = new PointF( e.GetX( ), e.GetY( ) );
                        break;
                    }

                    case MotionEventActions.Move:
                    {
                        // adjust by the amount moved
                        PointF delta = new PointF( e.GetX( ) - LastTapPos.X, e.GetY( ) - LastTapPos.Y );

                        MoveCropView( delta );

                        LastTapPos = new PointF( e.GetX( ), e.GetY( ) );
                        break;
                    }

                    case MotionEventActions.Up:
                    {

                        break;
                    }
                }
            }

            return true;
        }

        void MoveCropView( PointF delta )
        {
            // update the crop view by how much it should be moved
            float xPos = CropView.GetX( ) + delta.X;
            float yPos = CropView.GetY( ) + delta.Y;

            // clamp to valid bounds
            xPos = Math.Max( CropViewMinPos.X, Math.Min( xPos, CropViewMaxPos.X ) );
            yPos = Math.Max( CropViewMinPos.Y, Math.Min( yPos, CropViewMaxPos.Y ) );

            CropView.SetX( xPos );
            CropView.SetY( yPos );

            MaskLayer.Position = new PointF( CropView.GetX( ), CropView.GetY( ) );
        }

        Bitmap CropImage( Bitmap image, System.Drawing.RectangleF cropDimension )
        {
            // convert our position on screen to where it should be in the image
            float pixelX = cropDimension.X * ScreenToImageScalar;
            float pixelY = cropDimension.Y * ScreenToImageScalar;

            // same for height, since the image was scaled down to fit the screen.
            float width = (float) cropDimension.Width * ScreenToImageScalar;
            float height = (float) cropDimension.Height * ScreenToImageScalar;

            return Bitmap.CreateBitmap( image, (int) pixelX, (int) pixelY, (int)width, (int)height);
        }
    }
}
