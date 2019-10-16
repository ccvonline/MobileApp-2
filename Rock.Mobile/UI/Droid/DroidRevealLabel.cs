#if __ANDROID__

using System;
using Android.Views;
using Droid;
using Android.Graphics.Drawables;
using Android.Widget;
using Rock.Mobile.UI.DroidNative;
using System.IO;
using Android.Graphics;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Derived from DroidLabel, this is a custom label that 
        /// can hide a word and reveal it with a fade in using an animation.
        /// </summary>
        public class DroidRevealLabel : DroidLabel
        {
            /// <summary>
            /// This defines the RATE of animation. The lower the number the faster
            /// the label will be revealed.
            /// </summary>
            static float MASK_TIME_SCALER = .013f;

            /// <summary>
            /// This should not be changed, and controls how wide to scale up the mask so the entire word is revealed.
            /// </summary>
            static float MASK_WIDTH_SCALER = .013f;

            /// <summary>
            /// Dependent on the label's width, determines how large
            /// the mask must scale to reveal the whole word.
            /// </summary>
            float MaxScale = 1;

            public DroidRevealLabel( )
            {
                Label = new FadeTextView( Rock.Mobile.PlatformSpecific.Android.Core.Context ) as BorderedRectTextView;
                Label.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );

                AddUnderline( );
            }

            protected override void setBounds(System.Drawing.RectangleF bounds)
            {
                base.setBounds( bounds );

                // update the scale value for the fading
                MaxScale = UnderlineView.LayoutParameters.Width * MASK_WIDTH_SCALER;
            }

            protected override void setPosition( System.Drawing.PointF position )
            {
                // let the label update
                base.setPosition( position );
            }

            public override float GetFade()
            {
                return ((FadeTextView)Label).MaskScale / MaxScale;
            }

            public override void SetFade( float fadeAmount )
            {
                // if we're setting the absolute fade, invalidate now so we redraw
                ((FadeTextView)Label).MaskScale = MaxScale * fadeAmount;

                Label.Invalidate();
            }

            public override void AnimateToFade( float fadeAmount )
            {
                // because this was originally tuned in milliseconds, simply convert it to seconds.
                long msDuration = (long) (MaxScale / MASK_TIME_SCALER);
                ( (FadeTextView)Label ).AnimateMaskScale( MaxScale * fadeAmount, msDuration / 1000.0f );
            }

            public override void SizeToFit( )
            {
                base.SizeToFit( );

                // update the scale value for the fading
                MaxScale = UnderlineView.LayoutParameters.Width * MASK_WIDTH_SCALER;
            }
        }
    }
}

#endif
