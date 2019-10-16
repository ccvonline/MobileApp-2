#if __ANDROID__
using System;
using Android.Animation;

namespace Rock.Mobile.Animation
{
    /// <summary>
    /// The base implementation for our animators
    /// </summary>
    public abstract class SimpleAnimator : Java.Lang.Object, global::Android.Animation.ValueAnimator.IAnimatorUpdateListener, global::Android.Animation.ValueAnimator.IAnimatorListener
    {
        public delegate void AnimationUpdate( float percent, object value );
        public delegate void AnimationComplete( );

        protected ValueAnimator Animator { get; set; }
        protected AnimationUpdate AnimationUpdateDelegate;
        protected AnimationComplete AnimationCompleteDelegate;

        public enum Style
        {
            Linear,
            CurveEaseIn,
            CurveEaseOut
        }
        Style AnimStyle { get; set; }

        protected void Init( float duration, AnimationUpdate updateDelegate, AnimationComplete completeDelegate )
        {
            Animator = ValueAnimator.OfFloat( 0.00f, 1.00f );

            Animator.AddUpdateListener( this );
            Animator.AddListener( this );

            // convert duration to milliseconds
            Animator.SetDuration( (int) (duration * 1000.0f) );

            AnimationUpdateDelegate = updateDelegate;
            AnimationCompleteDelegate = completeDelegate;
        }

        public void Start( Style animStyle = Style.Linear )
        {
            if ( Animator != null )
            {
                AnimStyle = animStyle;

                Animator.Start( );
            }
        }

        protected abstract void AnimTick( float percent, AnimationUpdate updateDelegate );

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            float percent = 0.00f;

            // check the style of animation they want
            switch( AnimStyle )
            {
                // linear is, well, linear.
                case Style.Linear:
                {
                    percent = (float)animation.CurrentPlayTime / (float)animation.Duration;
                    break;
                }

                // curve ease in starts SLOW and ends FAST
                case Style.CurveEaseIn:
                {
                    float xPerc = (float)animation.CurrentPlayTime / (float)animation.Duration;
                    percent = (float) System.Math.Pow( xPerc, 3.0f );
                    break;
                }

                // curve ease out starts FAST and ends SLOW
                case Style.CurveEaseOut:
                {
                    float xPerc = (float)animation.CurrentPlayTime / (float)animation.Duration;
                    percent = (float) 1 + (float) System.Math.Pow( (xPerc - 1), 3.0f );
                    break;
                }
            }

            AnimTick( System.Math.Min( percent, 1.00f ), AnimationUpdateDelegate );
        }

        public void OnAnimationEnd(Animator animation)
        {
            // give it one more tick at full percentage. This way if 
            // low framerate causes a large delta than we don't handle,
            // we'll at least finish out the animation.
            AnimTick( 1.00f, AnimationUpdateDelegate );

            // now finish it
            if ( AnimationCompleteDelegate != null )
            {
                AnimationCompleteDelegate( );
            }
        }

        public void OnAnimationStart(Animator animation)
        {
        }

        public void OnAnimationRepeat(Animator animation)
        {
        }

        public void OnAnimationCancel(Animator animation)
        {
        }
    }
}
#endif
