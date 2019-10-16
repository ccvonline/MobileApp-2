#if __ANDROID__
using System;
using System.Drawing;
using Android.Views;
using Android.Widget;
using Android.Animation;
using System.Collections.Generic;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Platform Specific implementations of the carousel need only to manage
        /// the animation requests made by PlatformCarousel, since we dont have a platform agnostic
        /// implementation of animation systems.
        /// </summary>
        public class DroidCardCarousel : PlatformCardCarousel
        {
            /// <summary>
            /// Forwards the finished animation notification
            /// </summary>
            public class CarouselAnimationListener : Android.Animation.AnimatorListenerAdapter, Android.Animation.ValueAnimator.IAnimatorUpdateListener
            {
                public DroidCardCarousel Parent { get; set; }

                public override void OnAnimationEnd(Animator animation)
                {
                    base.OnAnimationEnd(animation);

                    // forward on this message to our parent
                    Parent.OnAnimationEnd( animation );
                }

                public void OnAnimationUpdate(ValueAnimator animation)
                {
                    // update the container position
                    Parent.OnAnimationUpdate( animation );
                }
            }

            /// <summary>
            /// Subclass value animator so we can track what card it's animating.
            /// </summary>
            public class CardValueAnimator : ValueAnimator
            {
                public View Card { get; set; }
            }
            List<CardValueAnimator> ActiveAnimators { get; set; }

            bool IsPanning { get; set; }
            PointF AbsolutePosition { get; set; }

            public override void TouchesBegan( )
            {
                Rock.Mobile.Util.Debug.WriteLine( "TouchesBegan (OnDown)" );

                foreach(CardValueAnimator animator in ActiveAnimators )
                {
                    animator.Cancel( );
                }
                ActiveAnimators.Clear( );

                IsPanning = false;
            }

            public override void TouchesEnded( )
            {
                UpdateCardPositions( );

                if( IsPanning == true )
                {
                    //Rock.Mobile.Util.Debug.WriteLine( "Was panning. Don't call base.TouchesEnded" );
                    IsPanning = false;
                    base.OnPanGesture( PanGestureState.Ended, new PointF( 0, 0 ), new PointF( 0, 0 ) );
                }
                else
                {
                    //Rock.Mobile.Util.Debug.WriteLine( "TouchesEnded" );
                    base.TouchesEnded();
                }
            }

            public bool OnFling( MotionEvent e1, MotionEvent e2, float velocityX, float velocityY )
            {
                //Rock.Mobile.Util.Debug.WriteLine( "OnFling: distanceX {0}", velocityX );

                IsPanning = true;
                return false;
            }

            public bool OnScroll( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
            {
                //Rock.Mobile.Util.Debug.WriteLine( "OnScroll: distanceX {0}", distanceX );

                // flip X so it's consistent with ios, where right is positive.
                distanceX = -distanceX;
                IsPanning = true;

                base.OnPanGesture( PanGestureState.Changed, new PointF( distanceX < 0 ? -1 : 1, 0 ), new PointF( distanceX, distanceY ) );

                return false;
            }

            public DroidCardCarousel( object parentView, float cardWidth, float cardHeight, RectangleF boundsInParent, float animationDuration ) : base( parentView, cardWidth, cardHeight, boundsInParent, animationDuration )
            {
                ActiveAnimators = new List<CardValueAnimator>( );
            }

            /// <summary>
            /// Animates a card from startPos to endPos over time
            /// </summary>
            protected override void AnimateCard( object platformObject, string animName, PointF startPos, PointF endPos, float duration, PlatformCardCarousel parentDelegate )
            {
                // setup an animation from our current mask scale to the new one.
                CardValueAnimator animator = new CardValueAnimator();
                animator.SetIntValues( (int)startPos.X, (int)endPos.X );

                CarouselAnimationListener listener = new CarouselAnimationListener( ) { Parent = this } ;

                animator.AddUpdateListener( listener );
                animator.AddListener(listener  );
                animator.SetDuration( 500 );
                animator.Card = platformObject as View;

                animator.Start();

                ActiveAnimators.Add( animator );
            }

            public void OnAnimationUpdate(ValueAnimator animation)
            {
                int xPos = ((Java.Lang.Integer)animation.GetAnimatedValue("")).IntValue();

                CardValueAnimator cardAnimator = animation as CardValueAnimator;
                cardAnimator.Card.SetX( xPos );
            }

            /// <summary>
            /// Called when card movement is complete.
            /// </summary>
            public void OnAnimationEnd(Animator animation)
            {
                Animating = false;

                //UpdateCardPositions( );

                //Rock.Mobile.Util.Debug.WriteLine( "Animation Stopped" );
            }
        }
    }
}
#endif
