
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
using Android.Animation;
using Droid.Tasks;
using Android.Graphics;
using App.Shared.Config;
using Rock.Mobile.UI;
using App.Shared.PrivateConfig;

namespace Droid
{
    /// <summary>
    /// The navbar fragment acts as the container for the active task.
    /// </summary>
    public class NavbarFragment : Fragment, Android.Animation.ValueAnimator.IAnimatorUpdateListener
    {
        /// <summary>
        /// Full screen "wall" that continues to read input while the 
        /// actual task input is disabled while the springboard is open
        /// </summary>
        public class InputChecker : View, View.IOnTouchListener
        {
            GestureDetector GestureDetector { get; set; }

            public class SimpleGestureDetector : GestureDetector.SimpleOnGestureListener
            {
                InputChecker Parent { get; set; }

                public SimpleGestureDetector( InputChecker parent )
                {
                    Parent = parent;
                }

                public override bool OnDown(MotionEvent e)
                {
                    // Make the TaskFragment handle this
                    Parent.OnDownGesture( e );
                    return true;
                }

                public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
                {
                    Parent.OnFlingGesture( e1, e2, velocityX, velocityY );
                    return base.OnFling(e1, e2, velocityX, velocityY);
                }

                public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
                {
                    Parent.OnScrollGesture( e1, e2, distanceX, distanceY );
                    return base.OnScroll(e1, e2, distanceX, distanceY);
                }

                public override bool OnDoubleTap(MotionEvent e)
                {
                    Parent.OnDoubleTap( e );
                    return base.OnDoubleTap( e );
                }
            }

            public NavbarFragment NavParent { get; set; }

            public InputChecker( Context context ) : base( context )
            {
                GestureDetector = new GestureDetector( context, new SimpleGestureDetector( this ) );
                SetOnTouchListener( this );
            }

            public virtual bool OnTouch( View v, MotionEvent e )
            {
                if ( GestureDetector.OnTouchEvent( e ) == true )
                {
                    return true;
                }
                else
                {
                    switch ( e.Action )
                    {
                        case MotionEventActions.Up:
                        {
                            NavParent.OnUp( e );
                            break;
                        }
                    }
                    return false;
                }
            }

            public virtual bool OnDownGesture( MotionEvent e )
            {
                NavParent.OnDown( e );
                return false;
            }

            public virtual bool OnDoubleTap(MotionEvent e)
            {
                return false;
            }

            public virtual bool OnFlingGesture( MotionEvent e1, MotionEvent e2, float velocityX, float velocityY )
            {
                // let the navbar know we're flicking
                NavParent.OnFlick( e1, e2, velocityX, velocityY );
                return false;
            }

            public virtual bool OnScrollGesture( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
            {
                // let the navbar know we're scrolling
                NavParent.OnScroll( e1, e2, distanceX, distanceY );
                return false;
            }
        }
        
        /// <summary>
        /// Forwards the finished animation notification to the actual navbar fragment
        /// </summary>
        public class NavbarAnimationListener : Android.Animation.AnimatorListenerAdapter
        {
            public NavbarFragment NavbarFragment { get; set; }

            public override void OnAnimationEnd(Animator animation)
            {
                base.OnAnimationEnd(animation);

                // forward on this message to our parent
                NavbarFragment.OnAnimationEnd( animation );
            }
        }

        /// <summary>
        /// Reference to the currently active task
        /// </summary>
        /// <value>The active task.</value>
        public Tasks.Task ActiveTask { get; protected set; }

        public Springboard SpringboardParent { get; set; }

        /// <summary>
        /// True when the navbar fragment and task are slid "out" to reveal the springboard
        /// </summary>
        /// <value><c>true</c> if springboard revealed; otherwise, <c>false</c>.</value>
        public bool SpringboardRevealed { get; set; }

        /// <summary>
        /// Returns true if the springboard should accept input.
        /// This will basically be false anytime the springboard is CLOSED or animating
        /// </summary>
        public bool ShouldSpringboardAllowInput( )
        {
            if ( MainActivity.IsLandscapeWide( ) == true )
            {
                return true;
            }
            else
            {
                if ( SpringboardRevealed == true && Animating == false && Panning != PanState.Panning )
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns true if the active task should accept input from the user.
        /// This will basically be false anytime the springboard is open or animating
        /// </summary>
        public bool ShouldTaskAllowInput( )
        {
            if ( MainActivity.IsLandscapeWide( ) == true )
            {
                return true;
            }
            else
            {
                if ( SpringboardRevealed == false && Animating == false && Panning != PanState.Panning )
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// True when the navbar fragment and container are in the process of sliding in our out
        /// </summary>
        /// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
        bool Animating { get; set; }

        /// <summary>
        /// The frame that stores the active task
        /// </summary>
        /// <value>The active task frame.</value>
        FrameLayout _ActiveTaskFrame = null;
        public FrameLayout ActiveTaskFrame 
        { 
            get { return _ActiveTaskFrame; }

            set
            {
                if ( InputViewChecker != null )
                {
                    if ( _ActiveTaskFrame != null )
                    {
                        _ActiveTaskFrame.RemoveView( InputViewChecker );
                    }

                    _ActiveTaskFrame = value;

                    _ActiveTaskFrame.AddView( InputViewChecker );
                }
                else
                {
                    _ActiveTaskFrame = value;
                }
            }
        }

        public NavToolbarFragment NavToolbar { get; set; }

        InputChecker InputViewChecker { get; set; }

        Button SpringboardRevealButton { get; set; }

        enum PanState
        {
            None,
            Monitoring,
            Panning
        }
        PanState Panning { get; set; }

        /// <summary>
        /// The amount of frames counted during Pan Monitoring.
        /// </summary>
        /// <value>The frame count.</value>
        float FrameCount { get; set; }

        /// <summary>
        /// The amount panned on Y since the state went to Panning
        /// </summary>
        /// <value>The total pan y.</value>
        float TotalPanY { get; set; }

        /// <summary>
        /// The amount panned on X since the state went to Panning
        /// </summary>
        /// <value>The last pan x.</value>
        float TotalPanX { get; set; }

        /// <summary>
        /// When panning actually BEGINS (not when monitoring begins),
        /// this is the X position. Used to know how much we've already panned.
        /// </summary>
        /// <value>The pan start x.</value>
        float PanStartX { get; set; }

        /// <summary>
        /// The starting position of the panel, so that as you pan it can
        /// move relative to this position.
        /// </summary>
        /// <value>The panel origin x.</value>
        float PanelOriginX { get; set; }

        /// <summary>
        /// Number of frames to track their panning before determining what the user intended.
        /// </summary>
        const int sNumPanTrackingFrames = 5;

        /// <summary>
        /// The backing view that provides a drop shadow.
        /// </summary>
        /// <value>The shadow container.</value>
        FrameLayout DropShadowView { get; set; }

        /// <summary>
        /// The X offset for the shadow
        /// </summary>
        /// <value>The shadow view offset.</value>
        float DropShadowXOffset { get; set; }

        /// <summary>
        /// True when OnResume has been called. False when it has not.
        /// </summary>
        /// <value><c>true</c> if this instance is fragment active; otherwise, <c>false</c>.</value>
        protected bool IsFragmentActive { get; set; }

        /// <summary>
        /// The frame that is used to darken the container when the springboard is revealed
        /// </summary>
        /// <value>The fade out frame.</value>
        FrameLayout FadeOutFrame { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            RetainInstance = true;

            NavToolbar = FragmentManager.FindFragmentById(Resource.Id.navtoolbar) as NavToolbarFragment;
            if (NavToolbar == null)
            {
                NavToolbar = new NavToolbarFragment();
            }

            FadeOutFrame = Activity.FindViewById<FrameLayout>(Resource.Id.fadeOutView) as FrameLayout;
            FadeOutFrame.Alpha = 0.0f;

            // Execute a transaction, replacing any existing
            // fragment with this one inside the frame.
            var ft = FragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.navtoolbar, NavToolbar);
            ft.SetTransition(FragmentTransit.FragmentFade);
            ft.Commit();
        }

        RelativeLayout Navbar { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            //The navbar should basically be a background with logo and a springboard reveal button in the upper left.
            Navbar = inflater.Inflate(Resource.Layout.navbar, container, false) as RelativeLayout;
            Navbar.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TopNavToolbar_BackgroundColor ) );

            // create the springboard reveal button
            CreateSpringboardButton( Navbar );

            DropShadowXOffset = 15;
            DropShadowView = Activity.FindViewById<FrameLayout>(Resource.Id.dropShadowView) as FrameLayout;

            InputViewChecker = new InputChecker( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            InputViewChecker.NavParent = this;

            if ( _ActiveTaskFrame != null )
            {
                _ActiveTaskFrame.AddView( InputViewChecker );
            }

            return Navbar;
        }

        void SetContainerWidth( int containerWidth )
        {
            DropShadowView.LayoutParameters.Width = containerWidth;
            View.LayoutParameters.Width = containerWidth;
            ActiveTaskFrame.LayoutParameters.Width = containerWidth;
            NavToolbar.ButtonLayout.LayoutParameters.Width = containerWidth;
            FadeOutFrame.LayoutParameters.Width = containerWidth;
        }

        public void LayoutChanged( )
        {
            // if we just entered landscape wide mode
            if ( MainActivity.IsLandscapeWide( ) == true )
            {
                // turn off the springboard button
                SpringboardRevealButton.Enabled = false;
                SpringboardRevealed = true;

                // move the container over so the springboard is revealed
                PanContainerViews( Springboard.GetSpringboardDisplayWidth( ) );

                // turn off the shadow
                FadeOutFrame.Alpha = 0.0f;
                FadeOutFrame.Visibility = ViewStates.Invisible;

                // resize the containers to use the remaining width
                int containerWidth = GetContainerDisplayWidth( );

                SetContainerWidth( containerWidth );

                ToggleInputViewChecker( false );
            }
            // we're going back to portrait (or normal landscape)
            else
            {
                // enable the springboard reveal button
                SpringboardRevealed = false;

                // close the springboard
                PanContainerViews( 0 );
                FadeOutFrame.Visibility = ViewStates.Visible;

                // resize the containers to use the full device width
                Point displaySize = new Point( );
                Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
                float displayWidth = displaySize.X;

                SetContainerWidth( (int)displayWidth );

                // only allow the reveal button if the device is in portrait.
                if ( MainActivity.IsPortrait( ) )
                {
                    SpringboardRevealButton.Enabled = true;
                }
                else
                {
                    SpringboardRevealButton.Enabled = false;
                }
            }
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            return false;
        }

        void CreateSpringboardButton( RelativeLayout relativeLayout )
        {
            // create the button
            SpringboardRevealButton = new Button( Activity );

            // clear the background outline
            SpringboardRevealButton.Background = null;

            // position it vertically centered and a little right indented
            SpringboardRevealButton.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ((RelativeLayout.LayoutParams)SpringboardRevealButton.LayoutParameters).AddRule( LayoutRules.CenterVertical );
            SpringboardRevealButton.SetX( 10 );


            // set the font and text
            Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );
            SpringboardRevealButton.SetTypeface( fontFace, TypefaceStyle.Normal );
            SpringboardRevealButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivatePrimaryNavBarConfig.RevealButton_Size );
            SpringboardRevealButton.Text = PrivatePrimaryNavBarConfig.RevealButton_Text;

            // use the completely overcomplicated color states to set the normal vs pressed color state.
            int [][] states = new int[][] 
                {
                    new int[] {  Android.Resource.Attribute.StatePressed },
                    new int[] {  Android.Resource.Attribute.StateEnabled },
                    new int[] { -Android.Resource.Attribute.StateEnabled },
                };

            // let the "pressed" version just use a darker version of the normal color
            uint mutedColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );

            int [] colors = new int[]
                {
                    Rock.Mobile.UI.Util.GetUIColor( mutedColor ),
                    Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ),
                    Rock.Mobile.UI.Util.GetUIColor( mutedColor ),
                };
            SpringboardRevealButton.SetTextColor( new Android.Content.Res.ColorStateList( states, colors ) );


            // setup the click callback
            SpringboardRevealButton.Click += (object sender, System.EventArgs e) => 
                {
                    RevealSpringboard( !SpringboardRevealed );

                    SpringboardParent.RevealButtonClicked( );
                };

            relativeLayout.AddView( SpringboardRevealButton );
        }

        public void ToggleFullscreen( bool fullscreenEnabled )
        {
            // useful for when a video wants to be fullscreen
            if ( fullscreenEnabled == true )
            {
                Navbar.Visibility = ViewStates.Gone;
                NavToolbar.ButtonLayout.Visibility = ViewStates.Gone;

                // if we're in landscape wide, ensure the springboard is closed while in fullscreen.
                if ( MainActivity.IsLandscapeWide( ) == true )
                {
                    PanContainerViews( 0 );

                    Point displaySize = new Point();
                    Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
                    float displayWidth = displaySize.X;
                    SetContainerWidth( (int)displayWidth );
                }
            }
            else
            {
                Navbar.Visibility = ViewStates.Visible;
                NavToolbar.ButtonLayout.Visibility = ViewStates.Visible;

                // if we're in landscape wide, ensure the springboard is closed while in fullscreen.
                if ( MainActivity.IsLandscapeWide( ) == true )
                {
                    PanContainerViews( Springboard.GetSpringboardDisplayWidth( ) );
                    SetContainerWidth( GetContainerDisplayWidth( ) );
                }
            }
        }

        public void EnableSpringboardRevealButton( bool enabled )
        {
            SpringboardRevealButton.Enabled = enabled;

            if ( enabled == false )
            {
                RevealSpringboard( false );
            }
        }

        public void RevealSpringboard( bool wantReveal )
        {
            if( !Animating )
            {
                Animating = true;

                int xOffset = wantReveal ? (int) Springboard.GetSpringboardDisplayWidth( ) : 0;

                // setup an animation from our current mask scale to the new one.
                ValueAnimator xPosAnimator = ValueAnimator.OfInt((int)View.GetX( ) , xOffset);

                xPosAnimator.AddUpdateListener( this );
                xPosAnimator.AddListener( new NavbarAnimationListener( ) { NavbarFragment = this } );
                xPosAnimator.SetDuration( (int) (PrivatePrimaryContainerConfig.SlideRate * 1000.0f) );
                xPosAnimator.Start();
            }
        }

        public static int GetFullDisplayWidth( )
        {
            Point displaySize = new Point();
            ( (Activity)Rock.Mobile.PlatformSpecific.Android.Core.Context ).WindowManager.DefaultDisplay.GetSize( displaySize );

            return (int)displaySize.X;
        }

        /// <summary>
        /// Used to get the available width for views in the container.
        /// For example, if in portrait mode, it'll be the width device.
        /// If in LandscapeWide mode, it'll be the width minus the springboard width.
        /// </summary>
        public static int GetContainerDisplayWidth( )
        {
            float displayWidthPixels = Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics.WidthPixels;

            // if we're in landscape wide mode, return the container viewing width
            if ( MainActivity.IsLandscapeWide( ) == true )
            {
                float displayWidth = displayWidthPixels;

                return (int)( displayWidth - ( displayWidth * PrivatePrimaryNavBarConfig.Landscape_RevealPercentage_Android ) );
            }
            // otherwise, for portrait, just return the full display width
            else
            {
                return (int)displayWidthPixels;
            }
        }

        /// <summary>
        /// Adjusts the container views based on how far along X they should move.
        /// Also updates the fade out view so it darkens or lightens as needed.
        /// </summary>
        /// <param name="xPos">X position.</param>
        void PanContainerViews( float xPos )
        {
            DropShadowView.SetX( xPos - DropShadowXOffset );
            View.SetX( xPos );
            ActiveTaskFrame.SetX( xPos );
            NavToolbar.ButtonLayout.SetX( xPos );
            FadeOutFrame.SetX( xPos );

            // now determine the alpha
            float maxSlide = Springboard.GetSpringboardDisplayWidth( );
            FadeOutFrame.Alpha = Math.Min( xPos / maxSlide, PrivatePrimaryContainerConfig.SlideDarkenAmount );
        }

        public void OnDown( MotionEvent e )
        {
            Panning = PanState.Monitoring;
            TotalPanX = 0;
            TotalPanY = 0;
            FrameCount = 0;
            PanStartX = 0;
        }

        static float sMinVelocity = 2000.0f;
        public void OnFlick( MotionEvent e1, MotionEvent e2, float velocityX, float velocityY )
        {
            // sanity check, as the events could be null.
            if ( e1 != null && e2 != null )
            {
                // if they flicked it, go ahead and open / close the springboard.
                // to know they intended to flick and not scroll, ensure X is > Y
                if ( Math.Abs( velocityX ) > Math.Abs( velocityY ) )
                {
                    // only allow it if we're NOT animating, the task is ok with us panning, and we're in portrait mode.
                    if ( Animating == false &&
                         ActiveTask.CanContainerPan( ) &&
                         Activity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait )
                    {
                        if ( velocityX > sMinVelocity )
                        {
                            RevealSpringboard( true );
                        }
                        else if ( velocityX < -sMinVelocity )
                        {
                            RevealSpringboard( false );
                        }
                    }
                }
            }
        }

        public void OnScroll( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
        {
            // sanity check, as the events could be null.
            if ( e1 != null && e2 != null )
            {
                // only allow it if we're NOT animating, the task is ok with us panning, and we're in portrait mode.
                if ( Animating == false &&
                     ActiveTask.CanContainerPan( ) &&
                     Activity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait )
                {
                    switch ( Panning )
                    {
                        case PanState.Monitoring:
                        {
                            TotalPanY += Math.Abs( e2.RawY - e1.RawY );
                            TotalPanX += Math.Abs( e2.RawX - e1.RawX );

                            FrameCount++;

                            if ( FrameCount > sNumPanTrackingFrames )
                            {
                                // decide how to proceed
                                Panning = PanState.None;

                                // put simply, if their total X was more than their total Y, well then,
                                // lets pan.
                                if ( TotalPanX > TotalPanY )
                                {
                                    Panning = PanState.Panning;

                                    // mark where the panning began, so we can move the field appropriately
                                    PanStartX = e2.RawX;

                                    PanelOriginX = View.GetX( );
                                }
                                else
                                {
                                    // Y was greater than X, so they probably intended to scroll, not pan.
                                    Panning = PanState.None;
                                }

                            }
                            break;
                        }

                        case PanState.Panning:
                        {
                            distanceX = e2.RawX - PanStartX;

                            float xPos = PanelOriginX + distanceX;
                            float revealAmount = Springboard.GetSpringboardDisplayWidth( );
                            xPos = Math.Max( 0, Math.Min( xPos, revealAmount ) );

                            PanContainerViews( xPos );
                            break;
                        }
                    }
                }
            }
        }

        public void OnUp( MotionEvent e )
        {
            // if we were panning
            if ( Panning == PanState.Panning )
            {
                float revealAmount = Springboard.GetSpringboardDisplayWidth( );
                if ( SpringboardRevealed == false )
                {
                    // since the springboard wasn't revealed, require that they moved
                    // at least 1/5th the amount before opening it
                    if ( View.GetX( ) > revealAmount * .20f )
                    {
                        RevealSpringboard( true );
                    }
                    else
                    {
                        RevealSpringboard( false );
                    }
                }
                else
                {
                    if ( View.GetX( ) < revealAmount * .85f )
                    {
                        RevealSpringboard( false );
                    }
                    else
                    {
                        RevealSpringboard( true );
                    }
                }
            }
            else
            {
                // if the task should allowe input, reveal the nav bar
                if ( ShouldTaskAllowInput( ) == true )
                {
                    // let the active task know that the user released input
                    ActiveTask.OnUp( e );
                }
                else if ( ShouldSpringboardAllowInput( ) == true )
                {
                    // else close the springboard
                    RevealSpringboard( false );
                }
            }

            // no matter what, we're done panning
            Panning = PanState.None;
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            // update the container position
            int xPos = ((Java.Lang.Integer)animation.GetAnimatedValue("")).IntValue();

            PanContainerViews( xPos );
        }

        public void OnAnimationEnd( Animator animation )
        {
            Animating = false;

            InputViewChecker.BringToFront( );

            // based on the position, set the springboard flag
            if ( View.GetX( ) == 0 )
            {
                SpringboardRevealed = false;
                NavToolbar.Suspend( false );

                ToggleInputViewChecker( false );
            }
            else
            {
                SpringboardRevealed = true;
                NavToolbar.Suspend( true );

                if ( MainActivity.IsLandscapeWide( ) == false )
                {
                    ToggleInputViewChecker( true );
                }
            }

            // notify the task regarding what happened
            ActiveTask.SpringboardDidAnimate( SpringboardRevealed );
        }

        void ToggleInputViewChecker( bool enabled )
        {
            DisableTaskInput( ActiveTaskFrame, !enabled );

            InputViewChecker.Enabled = enabled;
            InputViewChecker.Focusable = enabled;
            InputViewChecker.FocusableInTouchMode = enabled;
        }

        void DisableTaskInput( ViewGroup frame, bool enable )
        {
            if ( frame as ListView != null || frame as ScrollView != null )
            {
                frame.Enabled = enable;
                frame.Focusable = enable;
                frame.FocusableInTouchMode = enable;
            }

            int i;
            for ( i = 0; i < frame.ChildCount; i++ )
            {
                View child = frame.GetChildAt( i ) as View;
                if ( (child as ViewGroup) != null )
                {
                    DisableTaskInput( (child as ViewGroup), enable );
                }
                else
                {
                    if ( child as ListView != null || child as ScrollView != null )
                    {
                        child.Enabled = enable;
                        child.Focusable = enable;
                        child.FocusableInTouchMode = enable;
                    }
                }
            }
        }

        public override void OnPause( )
        {
            base.OnPause( );

            if( ActiveTask != null )
            {
                ActiveTask.Deactivate( true );
            }

            IsFragmentActive = false;
        }

        public override void OnResume( )
        {
            base.OnResume( );

            IsFragmentActive = true;

            if( ActiveTask != null )
            {
                ActiveTask.Activate( true );
            }

            SpringboardParent.NavbarWasResumed( );

            LayoutChanged( );
        }

        public void SetActiveTask( Tasks.Task newTask )
        {
            // first, are we active? If we aren't, there's no way
            // we ever activated a task, so there's no need to deactivate anything.
            if ( IsFragmentActive == true )
            {
                // we are active, so if we have a current task, deactivate it.
                if ( ActiveTask != null )
                {
                    ActiveTask.Deactivate( false );
                }

                // activate the new task
                newTask.Activate( false );

                // force the springboard to close
                if ( MainActivity.IsLandscapeWide( ) == false )
                {
                    RevealSpringboard( false );
                }
            }
            else
            {
                // activate the new task
                newTask.Activate( false );
            }

            // take our active task. If we didn't activate it because we aren't
            // ready, we'll do it as soon as OnResume is called.
            ActiveTask = newTask;
        }
    }
}
