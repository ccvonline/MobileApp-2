using System;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Animation;
using Android.Graphics;
using Android.App;
using Android.OS;
using App.Shared.Config;
using Rock.Mobile.UI;
using App.Shared.PrivateConfig;

namespace Droid
{
    /// <summary>
    /// Forwards the finished animation notification to the actual navbar fragment
    /// </summary>
    public class NavToolbarAnimationListener : Android.Animation.AnimatorListenerAdapter
    {
        public NavToolbarFragment NavbarToolbar { get; set; }

        public override void OnAnimationEnd(Animator animation)
        {
            base.OnAnimationEnd(animation);

            // forward on this message to our parent
            NavbarToolbar.OnAnimationEnd( animation );
        }
    }

    public class NavToolbarFragment : Fragment, Android.Animation.ValueAnimator.IAnimatorUpdateListener
    {
        public LinearLayout ButtonLayout { get; set; }

        Button BackButton { get; set; }
        bool BackButtonEnabledPreSuspension { get; set; }

        Button ShareButton { get; set; }
        bool ShareButtonEnabledPreSuspension { get; set; }
        EventHandler ShareButtonDelegate { get; set; }

        Button CreateButton { get; set; }
        bool CreateButtonEnabledPreSuspension { get; set; }
        EventHandler CreateButtonDelegate { get; set; }

        ValueAnimator Animator { get; set; }

        bool Revealed { get; set; }

        /// <summary>
        /// Timer monitoring the time the toolbar should be shown before auto-hiding.
        /// </summary>
        /// <value>The nav bar timer.</value>
        protected System.Timers.Timer NavBarTimer { get; set; }

        public NavToolbarFragment( ) : base( )
        {
            BackButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            ShareButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            CreateButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            ButtonLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
        }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // create a timer that can be used to autohide this toolbar.
            NavBarTimer = new System.Timers.Timer();
            NavBarTimer.AutoReset = false;
            NavBarTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                {
                    // when the timer fires, hide the toolbar.
                    // Although the timer fires on a seperate thread, because we queue the reveal
                    // on the main (UI) thread, we don't have to worry about race conditions.
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate { InternalReveal( false ); } );
                };

            ButtonLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
            ButtonLayout.BaselineAligned = false;

            // set the nav subBar color (including opacity)
            Color navColor = Rock.Mobile.UI.Util.GetUIColor( PrivateSubNavToolbarConfig.BackgroundColor );
            navColor.A = (Byte) ( (float) navColor.A * PrivateSubNavToolbarConfig.Opacity );
            ButtonLayout.SetBackgroundColor( navColor );

            ButtonLayout.LayoutParameters.Height = (int)Rock.Mobile.Graphics.Util.UnitToPx( 50.0f );


            uint disabledColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 4, false );

            // create the back button
            BackButton.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (LinearLayout.LayoutParams)BackButton.LayoutParameters ).Gravity = GravityFlags.Top;

            // set the back button's font
            Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );
            BackButton.SetTypeface( fontFace, TypefaceStyle.Normal );
            BackButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateSubNavToolbarConfig.BackButton_Size );

            BackButton.Text = PrivateSubNavToolbarConfig.BackButton_Text;
            BackButton.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( 0 ) );
            BackButton.SetPadding( 0, 0, 0, 0 );

            BackButton.Click += delegate{ Activity.OnBackPressed(); };

            // default to NOT enabled
            BackButton.Enabled = false;

            // use the completely overcomplicated color states to set the normal vs pressed color state.
            int [][] states = new int[][] 
                {
                    new int[] { Android.Resource.Attribute.StatePressed },
                    new int[] { Android.Resource.Attribute.StateEnabled },
                    new int[] { -Android.Resource.Attribute.StateEnabled },
                };

            int [] colors = new int[]
                {
                    Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ),
                    Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ),
                    Rock.Mobile.UI.Util.GetUIColor( disabledColor ),
                };
            BackButton.SetTextColor( new Android.Content.Res.ColorStateList( states, colors ) );



            // create the share button
            ShareButton.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (LinearLayout.LayoutParams)ShareButton.LayoutParameters ).Gravity = GravityFlags.Top;

            // set the share button's font
            fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );
            ShareButton.SetTypeface( fontFace, TypefaceStyle.Normal );
            ShareButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateSubNavToolbarConfig.ShareButton_Size );
            ShareButton.SetPadding( 0, 0, 0, 0 );

            ShareButton.Text = PrivateSubNavToolbarConfig.ShareButton_Text;
            ShareButton.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( 0 ) );

            // default to NOT enabled
            ShareButton.Enabled = false;

            // use the completely overcomplicated color states to set the normal vs pressed color state.
            states = new int[][] 
                {
                    new int[] { Android.Resource.Attribute.StatePressed },
                    new int[] { Android.Resource.Attribute.StateEnabled },
                    new int[] { -Android.Resource.Attribute.StateEnabled },
                };

            colors = new int[]
                {
                    Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ),
                    Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ),
                    Rock.Mobile.UI.Util.GetUIColor( disabledColor ),
                };
            ShareButton.SetTextColor( new Android.Content.Res.ColorStateList( states, colors ) );



            // create the "create" button
            CreateButton.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (LinearLayout.LayoutParams)CreateButton.LayoutParameters ).Gravity = GravityFlags.Top;

            // set the create button's font
            fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );
            CreateButton.SetTypeface( fontFace, TypefaceStyle.Normal );
            CreateButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateSubNavToolbarConfig.CreateButton_Size );
            CreateButton.SetPadding( 0, 0, 0, 0 );

            CreateButton.Text = PrivateSubNavToolbarConfig.CreateButton_Text;
            CreateButton.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( 0 ) );

            // default to NOT enabled
            CreateButton.Enabled = false;

            // use the completely overcomplicated color states to set the normal vs pressed color state.
            states = new int[][] 
                {
                    new int[] { Android.Resource.Attribute.StatePressed },
                    new int[] { Android.Resource.Attribute.StateEnabled },
                    new int[] { -Android.Resource.Attribute.StateEnabled },
                };

            colors = new int[]
                {
                    Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ),
                    Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ),
                    Rock.Mobile.UI.Util.GetUIColor( disabledColor ),
                };
            CreateButton.SetTextColor( new Android.Content.Res.ColorStateList( states, colors ) );

            // default its position to hidden
            Revealed = false;
            ButtonLayout.SetY( (int)Rock.Mobile.Graphics.Util.UnitToPx( 50.0f ) );

            return ButtonLayout;
        }

        public override void OnResume()
        {
            base.OnResume();

            UpdateButtons( );
        }

        public void Suspend( bool suspend )
        {
            // if we're going to be suspended, store
            // the current state of the button
            if( suspend == true )
            {
                BackButtonEnabledPreSuspension = BackButton.Enabled;
                ShareButtonEnabledPreSuspension = ShareButton.Enabled;
                CreateButtonEnabledPreSuspension = CreateButton.Enabled;

                BackButton.Enabled = false;
                ShareButton.Enabled = false;
                CreateButton.Enabled = false;
            }
            else
            {
                // restore them to their pre-suspension state
                BackButton.Enabled = BackButtonEnabledPreSuspension;
                ShareButton.Enabled = ShareButtonEnabledPreSuspension;
                CreateButton.Enabled = CreateButtonEnabledPreSuspension;
            }
        }

        public void SetBackButtonEnabled( bool enabled )
        {
            BackButton.Enabled = enabled;
            BackButtonEnabledPreSuspension = BackButton.Enabled;
        }

        public void SetShareButtonEnabled( bool enabled, EventHandler sharePressed )
        {
            ShareButton.Enabled = enabled;
            ShareButtonEnabledPreSuspension = ShareButton.Enabled;

            // if there's a current delegate listening, remove it
            if( ShareButtonDelegate != null )
            {
                ShareButton.Click -= ShareButtonDelegate;
                ShareButtonDelegate = null;
            }

            // set the new one and store a reference to it
            if( sharePressed != null )
            {
                ShareButton.Click += sharePressed;

                ShareButtonDelegate = sharePressed;
            }
        }

        public void SetCreateButtonEnabled( bool enabled, EventHandler createPressed )
        {
            CreateButton.Enabled = enabled;
            CreateButtonEnabledPreSuspension = CreateButton.Enabled;

            // if there's a current delegate listening, remove it
            if( CreateButtonDelegate != null )
            {
                CreateButton.Click -= CreateButtonDelegate;
                CreateButtonDelegate = null;
            }

            // set the new one and store a reference to it
            if( createPressed != null )
            {
                CreateButton.Click += createPressed;

                CreateButtonDelegate = createPressed;
            }
        }

        void UpdateButtons( )
        {
            if( ButtonLayout != null )
            {
                // start by resetting it
                ButtonLayout.RemoveAllViews( );

                // now add each button
                //JHM: 4-13-15 - Google asks us not to add custom back buttons, since they are
                // completely redundant.
                //ButtonLayout.AddView( BackButton );

                ButtonLayout.AddView( ShareButton );

                ButtonLayout.AddView( CreateButton );
            }
        }

        public void RevealForTime( float timeToShow )
        {
            // stop (reset) any current timer
            NavBarTimer.Stop( );

            // convert to milliseconds
            NavBarTimer.Interval = timeToShow * 1000;

            // start the timer
            NavBarTimer.Start( );

            // reveal the toolbar, and when the timer ticks, it will be hidden again.
            InternalReveal( true );
        }

        public void Reveal( bool revealed )
        {
            NavBarTimer.Stop( );

            InternalReveal( revealed );
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            // update the mask scale
            int yPos = ((Java.Lang.Integer)animation.GetAnimatedValue("")).IntValue();

            ButtonLayout.SetY( yPos );
        }

        public void OnAnimationEnd( Animator animation )
        {
            Animator = null;
        }

        void InternalReveal( bool revealed )
        {
            // make sure we're not doubly requesting the same thing. That would
            // cause a hitch in the animation.
            if ( Revealed != revealed )
            {
                Revealed = revealed;

                // if we're currently animating, cancel the animation
                if ( Animator != null )
                {
                    Animator.Cancel( );
                }

                int yOffset = revealed ? 0 : ButtonLayout.LayoutParameters.Height;

                // setup an animation from our current mask scale to the new one.
                Animator = ValueAnimator.OfInt( (int)ButtonLayout.GetY( ), yOffset );

                Animator.AddUpdateListener( this );
                Animator.AddListener( new NavToolbarAnimationListener() { NavbarToolbar = this } );
                Animator.SetDuration( (long)( PrivateSubNavToolbarConfig.SlideRate * 1000.0f ) );

                Animator.Start( );
            }
        }
    }
}
