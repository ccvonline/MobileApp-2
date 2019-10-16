#if __IOS__
using System;
using CoreAnimation;
using UIKit;
using System.Drawing;
using Foundation;
using CoreGraphics;
using Rock.Mobile.PlatformSpecific.Util;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Platform Specific implementations of the carousel need only to manage
        /// the animation requests made by PlatformCarousel, since we dont have a platform agnostic
        /// implementation of animation systems.
        /// </summary>
        public class iOSCardCarousel : PlatformCardCarousel
        {
            class CaourselAnimDelegate : CAAnimationDelegate
            {
                public iOSCardCarousel Parent { get; set; }
                public UIView Card { get; set; }

                public override void AnimationStarted(CAAnimation anim)
                {

                }

                public override void AnimationStopped(CAAnimation anim, bool finished)
                {
                    Parent.AnimationStopped( anim, Card, finished );
                }
            }

            /// <summary>
            /// Tracks the last position of panning so delta can be applied
            /// </summary>
            CGPoint PanLastPos { get; set; }

            UIPanGestureRecognizer PanGesture { get; set; } 

            public iOSCardCarousel( object parentView, float cardWidth, float cardHeight, RectangleF boundsInParent, float animationDuration ) : base( parentView, cardWidth, cardHeight, boundsInParent, animationDuration )
            {
                foreach ( Card card in Cards )
                {
                    card.View.AddAsSubview( ParentView );
                }

                // setup our pan gesture
                PanGesture = new UIPanGestureRecognizer( iOSPanGesture );
                PanGesture.MinimumNumberOfTouches = 1;
                PanGesture.MaximumNumberOfTouches = 1;

                // add the gesture and all cards to our view
                ((UIView)ParentView).AddGestureRecognizer( PanGesture );
            }

            public void iOSPanGesture( UIPanGestureRecognizer obj)
            {
                Rock.Mobile.Util.Debug.WriteLine( "Panning" );

                // get the required data from the gesture and call our base function
                CGPoint currVelocity = obj.VelocityInView( (UIView)ParentView );
                CGPoint deltaPan = new CGPoint( 0, 0 );

                PlatformCardCarousel.PanGestureState state = PlatformCardCarousel.PanGestureState.Began;
                switch ( obj.State )
                {
                    case UIGestureRecognizerState.Began:
                    {
                        PanLastPos = new PointF( 0, 0 );

                        CommitCardPositions( );

                        state = PlatformCardCarousel.PanGestureState.Began;
                        break;
                    }

                    case UIGestureRecognizerState.Changed:
                    {
                        CGPoint absolutePan = obj.TranslationInView( (UIView)ParentView );
                        deltaPan = new CGPoint( absolutePan.X - PanLastPos.X, 0 );

                        PanLastPos = absolutePan;

                        state = PlatformCardCarousel.PanGestureState.Changed;
                        break;
                    }

                    case UIGestureRecognizerState.Ended:
                    {
                        UpdateCardPositions( );

                        state = PlatformCardCarousel.PanGestureState.Ended;
                        break;
                    }
                }

                base.OnPanGesture( state, currVelocity.ToPointF( ), deltaPan.ToPointF( ) );
            }

            public override void LayoutChanged( float cardWidth, float cardHeight, RectangleF boundsInParent )
            {
                base.LayoutChanged( cardWidth, cardHeight, boundsInParent );

                // clear out any pending animations for the cards, since we want to reset their positions
                foreach ( Card card in Cards )
                {
                    // first get the UIViews backing these PlatformViews
                    UIView cardView = (UIView) card.View.PlatformNativeObject;

                    // stop all animations
                    cardView.Layer.RemoveAllAnimations( );
                }
            }

            void CommitCardPositions( )
            {
                // when touch begins, remove all animations
                foreach ( Card card in Cards )
                {
                    // first get the UIViews backing these PlatformViews
                    UIView cardView = (UIView) card.View.PlatformNativeObject;

                    // and commit the animated positions as the actual card positions.
                    cardView.Layer.Position = cardView.Layer.PresentationLayer.Position;

                    // stop all animations
                    cardView.Layer.RemoveAllAnimations( );
                }

                // this has the effect of freezing & stopping the animation in motion.
                // OnAnimationEnded will be called, but finished will be false, so
                // we'll know it was stopped manually
            }

            public override void TouchesBegan( )
            {
                Rock.Mobile.Util.Debug.WriteLine( "Touches Began" );

                CommitCardPositions( );
            }

            public override void TouchesEnded()
            {
                UpdateCardPositions( );

                Rock.Mobile.Util.Debug.WriteLine( "Touches Ended" );

                base.TouchesEnded();
            }

            /// <summary>
            /// Animates a card from startPos to endPos over time
            /// </summary>
            protected override void AnimateCard( object platformObject, string animName, PointF startPos, PointF endPos, float duration, PlatformCardCarousel parentDelegate )
            {
                // make sure we're not already running an animation
                UIView cardView = platformObject as UIView;
                if ( cardView.Layer.AnimationForKey( animName ) == null )
                {
                    CABasicAnimation cardAnim = CABasicAnimation.FromKeyPath( "position" );

                    cardAnim.From = NSValue.FromPointF( startPos );
                    cardAnim.To = NSValue.FromPointF( endPos );

                    cardAnim.Duration = duration;
                    cardAnim.TimingFunction = CAMediaTimingFunction.FromName( CAMediaTimingFunction.EaseInEaseOut );

                    // these ensure we maintain the card position when finished
                    cardAnim.FillMode = CAFillMode.Forwards;
                    cardAnim.RemovedOnCompletion = false;


                    // if a delegate was provided, give it to the card
                    if ( parentDelegate != null )
                    {
                        cardAnim.Delegate = new CaourselAnimDelegate() { Parent = this, Card = cardView };
                    }

                    // 
                    cardView.Layer.AddAnimation( cardAnim, animName );
                }
            }

            /// <summary>
            /// Called when card movement is complete.
            /// </summary>
            /// <param name="anim">Animation.</param>
            /// <param name="finished">If set to <c>true</c> finished.</param>
            void AnimationStopped( CAAnimation anim, UIView cardView, bool finished )
            {
                // when finished, commit the card position
                cardView.Layer.Position = cardView.Layer.PresentationLayer.Position;

                // all we need to do is flag Animating as false (if it FINISHED)
                // so we know how to control panning.
                if( finished == true )
                {
                    Animating = false;
                    //Rock.Mobile.Util.Debug.WriteLine( "Animation Stopped" );
                }
            }
        }
    }
}
#endif