#if __IOS__

using System;
using CoreAnimation;
using System.Drawing;
using Foundation;
using UIKit;
using CoreGraphics;
using Rock.Mobile.Animation;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Derived from iOSLabel, this is a custom label that 
        /// can hide a word and reveal it with a fade in using an animation.
        /// </summary>
        public class iOSRevealLabel : iOSLabel
        {
            float Scale { get; set; }

            /// <summary>
            /// The amount of time to take to scale. This should be adjusted based on
            /// the width of the text label
            /// </summary>
            float SCALE_TIME_SECONDS = .20f; 

            float MaxScale { get; set; }

            const float ZeroScale = .0001f;

            public iOSRevealLabel( ) : base()
            {
                MaxScale = 2.0f;
                Scale = ZeroScale;

                // get a path to our custom fonts folder
                String imagePath = NSBundle.MainBundle.BundlePath + "/spot_mask.png";

                Label.Layer.Mask = new CALayer();
                Label.Layer.Mask.Contents = new UIImage( imagePath ).CGImage;
                Label.Layer.Mask.AnchorPoint = Label.Layer.AnchorPoint;
                ApplyMaskScale( Scale );

                AddUnderline( );
            }

            protected override void setBounds(RectangleF bounds)
            {
                base.setBounds( bounds );

                // Update the mask
                Label.Layer.Mask.Bounds = bounds;
                ApplyMaskScale(Scale);
            }

            protected override void setFrame( RectangleF frame )
            {
                base.setFrame( frame );

                // Update the mask
                Label.Layer.Mask.Bounds = new RectangleF( 0, 0, frame.Width, frame.Height );
                ApplyMaskScale(Scale);
            }

            protected override void setPosition( PointF position )
            {
                // let the label update
                base.setPosition( position );
            }

            public override void AddAsSubview( object masterView )
            {
                base.AddAsSubview( masterView );
            }

            public override void RemoveAsSubview( object masterView )
            {
                base.RemoveAsSubview( masterView );
            }

            public override float GetFade()
            {
                return Scale / MaxScale;
            }

            public override void SetFade( float fadeAmount )
            {
                Scale = System.Math.Max(ZeroScale, fadeAmount * MaxScale);
                ApplyMaskScale( Scale );
            }

            public override void AnimateToFade( float fadeAmount )
            {
                fadeAmount = System.Math.Max(ZeroScale, fadeAmount * MaxScale);

                SimpleAnimator_Float animator = new SimpleAnimator_Float( Scale, fadeAmount, SCALE_TIME_SECONDS, 
                    delegate(float percent, object value )
                    {
                        Scale = (float)value;
                        ApplyMaskScale( Scale );
                    },
                    delegate
                    {
                        Scale = fadeAmount;
                        ApplyMaskScale( Scale );
                    } );

                animator.Start( );
            }

            public override void SizeToFit( )
            {
                base.SizeToFit( );

                // Update the mask
                Label.Layer.Mask.Bounds = new CGRect( 0, 0, Label.Frame.Width, Label.Frame.Height );
                ApplyMaskScale(Scale);
            }

            void ApplyMaskScale( float scale)
            {
                // ultimately this scales the layer from the center out (rather than top/left)

                // create a transform that translates the layer by half its width/height
                // and then scales it
                CATransform3D translateScale = new CATransform3D();
                translateScale = CATransform3D.Identity;
                translateScale = translateScale.Scale( scale );
                translateScale = translateScale.Translate( -(Label.Layer.Mask.Bounds.Width / 2), -(Label.Layer.Mask.Bounds.Height / 2), 0 );

                // now apply a transform that puts it back by its width/height, effectively re-centering it.
                CATransform3D postScale = new CATransform3D();
                postScale = CATransform3D.Identity;
                postScale = postScale.Translate( (Label.Layer.Mask.Bounds.Width / 2), (Label.Layer.Mask.Bounds.Height / 2), 0 );

                // and now concat the post scale and apply
                Label.Layer.Mask.Transform = translateScale.Concat( postScale );
            }
        }
    }
}

#endif
