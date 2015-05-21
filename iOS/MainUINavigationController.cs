using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;
using App.Shared.Config;
using Rock.Mobile.UI;
using Rock.Mobile.PlatformSpecific.iOS.Graphics;
using App.Shared.PrivateConfig;

namespace iOS
{
    /// <summary>
    /// The entire app lives underneath a main navigation bar. This is the control
    /// that drives that navigation bar and manages sliding in and out to reveal the springboard.
    /// </summary>
	public class MainUINavigationController : UINavigationController
	{
        /// <summary>
        /// Flag determining whether the springboard is revealed. Revealed means
        /// this view controller has been slid over to show the springboard.
        /// </summary>
        /// <value><c>true</c> if springboard revealed; otherwise, <c>false</c>.</value>
        protected bool SpringboardRevealed { get; set; }

        /// <summary>
        /// True when this view controller is in the process of moving.
        /// </summary>
        /// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
        protected bool Animating { get; set; }

        /// <summary>
        /// The view controller that actually contains the active content.
        /// </summary>
        /// <value>The container.</value>
        protected ContainerViewController Container { get; set; }

        protected UIPanGestureRecognizer PanGesture { get; set; }

        public SpringboardViewController ParentSpringboard { get; set; }

        /// <summary>
        /// Tracks the last position of panning so delta can be applied
        /// </summary>
        protected CGPoint PanLastPos { get; set; }

        /// <summary>
        /// Direction we're currently panning. Important for syncing the card positions
        /// </summary>
        protected int PanDir { get; set; }

        /// <summary>
        /// A wrapper for Container.CurrentTask, since Container is protected.
        /// </summary>
        /// <value>The current task.</value>
        public Task CurrentTask { get { return Container != null ? Container.CurrentTask : null; } }

        protected UIView DarkPanel { get; set; }

        public MainUINavigationController ( ) : base ()
        {
        }

        /// <summary>
        /// Determines whether the springboard is fully closed.
        /// If its state is open OR animation is going on, consider it is not closed.
        /// DO NOT USE THE INVERSE TO KNOW ITS OPEN. USE IsSpringboardOpen()
        /// </summary>
        public bool IsSpringboardClosed( )
        {
            if( SpringboardRevealed == false && Animating == false )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the springboard is fully open.
        /// If its state is closed OR animation is going on, consider it not open.
        /// DO NOT USE THE INVERSE TO KNOW ITS CLOSED. USE IsSpringboardClosed()
        /// </summary>
        public bool IsSpringboardOpen( )
        {
            if ( SpringboardRevealed == true && Animating == false )
            {
                return true;
            }

            return false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // MainNavigationController must have a black background so that the ticks
            // before the task displays don't cause a flash
            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            View.Layer.AnchorPoint = CGPoint.Empty;
            View.Layer.Position = CGPoint.Empty;

            DarkPanel = new UIView();
            DarkPanel.Layer.AnchorPoint = CGPoint.Empty;
            DarkPanel.Frame = View.Frame;
            DarkPanel.Layer.Opacity = 0.0f;
            DarkPanel.BackgroundColor = UIColor.Black;
            View.AddSubview( DarkPanel );

            // setup the style of the nav bar
            NavigationBar.TintColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );

            UIImage solidColor = new UIImage();
            UIGraphics.BeginImageContext( new CGSize( 1, 1 ) );
            CGContext context = UIGraphics.GetCurrentContext( );

            context.SetFillColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TopNavToolbar_BackgroundColor ).CGColor );
            context.FillRect( new CGRect( 0, 0, 1, 1 ) );

            solidColor = UIGraphics.GetImageFromCurrentImageContext( );

            UIGraphics.EndImageContext( );

            NavigationBar.BarTintColor = UIColor.Clear;
            NavigationBar.SetBackgroundImage( solidColor, UIBarMetrics.Default );
            NavigationBar.Translucent = false;

            // our first (and only) child IS a ContainerViewController.
            Container = new ContainerViewController( );
            PushViewController( Container, false );

            // setup a shadow that provides depth when this panel is slid "out" from the springboard.
            ApplyEdgeShadow( );

            // setup our pan gesture
            PanGesture = new UIPanGestureRecognizer( OnPanGesture );
            PanGesture.MinimumNumberOfTouches = 1;
            PanGesture.MaximumNumberOfTouches = 1;
            View.AddGestureRecognizer( PanGesture );
        }

        void ApplyEdgeShadow( )
        {
            UIBezierPath shadowPath = UIBezierPath.FromRect( View.Bounds );
            View.Layer.MasksToBounds = false;
            View.Layer.ShadowColor = UIColor.Black.CGColor;
            View.Layer.ShadowOffset = PrivatePrimaryContainerConfig.ShadowOffset_iOS;
            View.Layer.ShadowOpacity = PrivatePrimaryContainerConfig.ShadowOpacity_iOS;
            View.Layer.ShadowPath = shadowPath.CGPath;
        }

        void OnPanGesture(UIPanGestureRecognizer obj) 
        {
            switch( obj.State )
            {
                case UIGestureRecognizerState.Began:
                {
                    // when panning begins, clear our pan values
                    PanLastPos = new CGPoint( 0, 0 );
                    PanDir = 0;
                    break;
                }

                case UIGestureRecognizerState.Changed:
                {
                    // use the velocity to determine the direction of the pan
                    CGPoint currVelocity = obj.VelocityInView( View );
                    if( currVelocity.X < 0 )
                    {
                        PanDir = -1;
                    }
                    else
                    {
                        PanDir = 1;
                    }

                    // Update the positions of the cards
                    CGPoint absPan = obj.TranslationInView( View );
                    CGPoint delta = new CGPoint( absPan.X - PanLastPos.X, 0 );
                    PanLastPos = absPan;

                    TryPanSpringboard( delta );
                    break;
                }

                case UIGestureRecognizerState.Ended:
                {
                    CGPoint currVelocity = obj.VelocityInView( View );

                    float restingPoint = (float)0.00f;//(View.Layer.Bounds.Width / 2);
                    float currX = (float) (View.Layer.Position.X - restingPoint);

                    // if they slide at least a third of the way, allow a switch
                    float toggleThreshold = (PrivatePrimaryContainerConfig.SlideAmount_iOS / 3);

                    // check whether the springboard is open, because that changes the
                    // context of hte user's intention
                    if( SpringboardRevealed == true )
                    {
                        // since it's open, close it if it crosses the closeThreshold
                        // OR velocty is high
                        float closeThreshold = PrivatePrimaryContainerConfig.SlideAmount_iOS - toggleThreshold;
                        if( currX < closeThreshold || currVelocity.X < -1000 )
                        {
                            RevealSpringboard( false );
                        }
                        else
                        {
                            RevealSpringboard( true );
                        }
                    }
                    else
                    {
                        // since it's closed, allow it to open as long as it's beyond toggleThreshold
                        // OR velocity is high
                        if( currX > toggleThreshold || currVelocity.X > 1000 )
                        {
                            RevealSpringboard( true );
                        }
                        else
                        {
                            RevealSpringboard( false );
                        }
                    }
                    break;
                }
            }
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            // only allow panning if the task is ok with it AND we're in portrait mode.
            if (CurrentTask.CanContainerPan( touches, evt ) == true && 
                SpringboardViewController.IsDeviceLandscape( ) == false )
            {
                PanGesture.Enabled = true;
            }
            else
            {
                PanGesture.Enabled = false;
            }
        }

        public void LayoutChanging( )
        {
            // for landscape regular, permanantly reveal the springboard
            if ( SpringboardViewController.IsLandscapeWide( ) == true )
            {
                View.Frame = new CGRect( PrivatePrimaryContainerConfig.SlideAmount_iOS, 0, SpringboardViewController.TraitSize.Width - PrivatePrimaryContainerConfig.SlideAmount_iOS, SpringboardViewController.TraitSize.Height );

                SpringboardRevealed = true;
                Container.View.UserInteractionEnabled = true;

                DarkPanel.Hidden = true;
                DarkPanel.Layer.Opacity = 0.0f;

                PanGesture.Enabled = false;
            }
            else
            {
                View.Frame = new CGRect( 0, 0, SpringboardViewController.TraitSize.Width, SpringboardViewController.TraitSize.Height );

                DarkPanel.Hidden = false;
                DarkPanel.Layer.Opacity = 0.0f;

                SpringboardRevealed = false;
                Container.View.UserInteractionEnabled = true;

                // only allow panning if we're in portrait. We COULD be going into normal Landscape
                if ( SpringboardViewController.IsDevicePortrait( ) == true )
                {
                    PanGesture.Enabled = true;
                }
                else
                {
                    PanGesture.Enabled = false;
                }
            }

            DarkPanel.Bounds = View.Bounds;

            ApplyEdgeShadow( );

            Container.LayoutChanging( );
        }

        public void LayoutChanged( )
        {
            Container.LayoutChanged( );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            LayoutChanging( );
            Container.LayoutChanged( );
        }

        public bool SupportsLandscape( )
        {
            if ( Container != null )
            {
                return Container.SupportsLandscape( );
            }
            return false;
        }

        public void TryPanSpringboard( CGPoint delta )
        {
            // make sure the springboard is clamped
            float xPos = (float) (View.Layer.Position.X + delta.X);

            float viewHalfWidth = (float)0.00f;//( View.Layer.Bounds.Width / 2 );

            xPos = Math.Max( viewHalfWidth, Math.Min( xPos, PrivatePrimaryContainerConfig.SlideAmount_iOS + viewHalfWidth ) );

            View.Layer.Position = new CGPoint( xPos, View.Layer.Position.Y );

            float percentDark = Math.Max( 0, Math.Min( (xPos - viewHalfWidth) / PrivatePrimaryContainerConfig.SlideAmount_iOS, PrivatePrimaryContainerConfig.SlideDarkenAmount ) );
            DarkPanel.Layer.Opacity = percentDark;
        }

        public void SpringboardRevealButtonTouchUp( )
        {
            // best practice states that we should let the view controller who presented us also dismiss us.
            // however, we have a unique situation where we are the parent to ALL OTHER view controllers,
            // so managing ourselves becomes a lot simpler.
            RevealSpringboard( !SpringboardRevealed );

            ParentSpringboard.RevealButtonClicked( );
        }

        public bool ActivateTask( Task task )
        {
            // don't allow switching activites while we're animating.
            if( Animating == false )
            {
                Container.ActivateTask( task );

                // I don't think this call does anything, but getting this close to
                // shipping, i don't want to remove it.
                PopToRootViewController( false );

                // task activation should only close the springboard if our device isn't wide landscape
                if( SpringboardViewController.IsLandscapeWide( ) == false )
                {
                    RevealSpringboard( false );
                }

                return true;
            }

            return false;
        }

        public void OnActivated( )
        {
            Container.OnActivated( );
        }

        public void WillEnterForeground( )
        {
            Container.WillEnterForeground( );
        }

        public void OnResignActive( )
        {
            Container.OnResignActive( );
        }

        public void DidEnterBackground( )
        {
            Container.DidEnterBackground( );
        }

        public void WillTerminate( )
        {
            Container.WillTerminate( );
        }

        public void RevealSpringboard( bool wantReveal )
        {
            // only do something if there's a change
            //if( wantReveal != SpringboardRevealed )
            {
                // of course don't allow a change while we're animating it.
                if( Animating == false )
                {
                    Animating = true;

                    // Animate the front panel out
                    UIView.Animate( PrivatePrimaryContainerConfig.SlideRate, 0, UIViewAnimationOptions.CurveEaseInOut, 
                        new Action( 
                            delegate 
                            { 
                                float endPos = 0.0f;
                                if( wantReveal == true )
                                {
                                    endPos = (float) PrivatePrimaryContainerConfig.SlideAmount_iOS;
                                    DarkPanel.Layer.Opacity = PrivatePrimaryContainerConfig.SlideDarkenAmount;
                                }
                                else
                                {
                                    endPos = 0.00f;
                                    DarkPanel.Layer.Opacity = 0.0f;
                                }

                                float moveAmount = (float) (endPos - View.Layer.Position.X);
                                View.Layer.Position = new CGPoint( View.Layer.Position.X + moveAmount, View.Layer.Position.Y );
                            })

                        , new Action(
                            delegate
                            {
                                Animating = false;

                                SpringboardRevealed = wantReveal;

                                // if the springboard is open, disable input on app stuff if the device doesn't support
                                // regular landscape
                                if ( SpringboardViewController.IsLandscapeWide( ) == false )
                                {
                                    Container.View.UserInteractionEnabled = !SpringboardRevealed;
                                }
                            })
                    );
                }
            }
        }
	}
}
