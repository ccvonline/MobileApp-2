using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Drawing;
using System.IO;

using Rock.Mobile.Network;
using MobileApp.Shared.Notes;
using MobileApp.Shared.Config;
using RestSharp;
using Rock.Mobile.UI;
using MobileApp.Shared;
using MobileApp.Shared.Strings;
using MobileApp.Shared.Analytics;
using MobileApp.Shared.UI;
using Rock.Mobile.Animation;
using Android.Graphics;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.IO;
using Rock.Mobile.PlatformSpecific.Android.UI;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesFragment : TaskFragment
            {
                /// <summary>
                /// A simple struct for managing the properties needed for
                /// hiding / revealing the nav toolbar
                /// </summary>
                class NavBarReveal
                {
                    float Timer { get; set; }
                    bool TimerTicked { get; set; }
                    float ScrollStartY { get; set; }
                    public NavToolbarFragment NavToolbar { get; set; }

                    public NavBarReveal( )
                    {
                        Timer = 0.00f;
                        TimerTicked = false;
                        ScrollStartY = 0;
                    }

                    public void BeginTracking( float currScrollY )
                    {
                        // start monitoring for a flick strong enough to toggle the navbar
                        Timer = DateTime.Now.Millisecond;
                        TimerTicked = false;
                        ScrollStartY = currScrollY;
                    }

                    public void Update( float scrollDelta, float currScrollY, float scrollViewHeight )
                    {
                        // see how much time has passed
                        float deltaTime = DateTime.Now.Millisecond - Timer;

                        // if it's been 1/10th a second and we haven't already ticked
                        if ( deltaTime > 100 && TimerTicked == false )
                        {
                            TimerTicked = true;

                            // first, if they are near the top, simply reveal the bar.
                            float scrollPerc = currScrollY / scrollViewHeight;
                            if ( scrollPerc < .10f )
                            {
                                // show the nav bar
                                NavToolbar.Reveal( true );
                            }
                            else
                            {
                                // get the amount scrolled
                                float deltaScroll = currScrollY - ScrollStartY;

                                if ( deltaScroll < -50 )
                                {
                                    // hide the nav bar
                                    NavToolbar.Reveal( true );
                                }
                                else if ( deltaScroll > 150 )
                                {
                                    // hide the nav bar
                                    NavToolbar.Reveal( false );
                                }
                            }
                        }
                    }
                }
                NavBarReveal NavBarRevealTracker { get; set; }

                /// <summary>
                /// Tags for storing the NoteScript and Style XML during an orientation change.
                /// </summary>
                const string XML_NOTE_KEY = "NOTE_XML";
                const string XML_STYLE_KEY = "STYLE_XML";

                /// <summary>
                /// Reloads the NoteScript
                /// </summary>
                /// <value>The refresh button.</value>
                Button RefreshButton { get; set; }

                /// <summary>
                /// Displays the Note
                /// </summary>
                /// <value>The scroll view.</value>
                LockableScrollView ScrollView { get; set; }

                /// <summary>
                /// Immediate child of the ScrollView. Parent to all content
                /// </summary>
                /// <value>The scroll view layout.</value>
                RelativeLayout ScrollViewLayout { get; set; }

                /// <summary>
                /// Displays when content is being downloaded.
                /// </summary>
                /// <value>The indicator.</value>
                ProgressBar Indicator { get; set; }

                /// <summary>
                /// True when notes are being refreshed to prevent multiple simultaneous downloads.
                /// </summary>
                /// <value><c>true</c> if refreshing notes; otherwise, <c>false</c>.</value>
                bool RefreshingNotes { get; set; }

                /// <summary>
                /// Actual Note object created by a NoteScript
                /// </summary>
                /// <value>The note.</value>
                Note Note { get; set; }

                /// <summary>
                /// Our wake lock that will keep the device from sleeping while notes are up.
                /// </summary>
                /// <value>The wake lock.</value>
                PowerManager.WakeLock WakeLock { get; set; }

                /// <summary>
                /// The URL for this note
                /// </summary>
                /// <value>The note URL.</value>
                public string NoteUrl { get; set; }

                protected string NoteFileName { get; set; }
                protected string StyleFileName { get; set; }

                /// <summary>
                /// A presentable name for the note. Used for things like email subjects
                /// </summary>
                /// <value>The name of the note presentable.</value>
                public string NoteName { get; set; }

                /// <summary>
                /// If the style sheet URLs aren't absolute, this is the domain to prefix.
                /// </summary>
                public string StyleSheetDefaultHostDomain { get; set; }

                /// <summary>
                /// True when a user note is being moved. Used to know whether to allow 
                /// panning for the springboard or not
                /// </summary>
                /// <value><c>true</c> if moving user note; otherwise, <c>false</c>.</value>
                public bool MovingUserNote { get; protected set; }

                /// <summary>
                /// True when WE are ready to create notes
                /// </summary>
                /// <value><c>true</c> if fragment ready; otherwise, <c>false</c>.</value>
                bool FragmentReady { get; set; }

                /// <summary>
                /// True when a gesture causes a user note to be created. We then know not to 
                /// pass further gesture input to the note until we receive TouchUp
                /// </summary>
                /// <value><c>true</c> if did gesture create note; otherwise, <c>false</c>.</value>
                bool DidGestureCreateNote { get; set; }

                /// <summary>
                /// The view to use for displaying a download error
                /// </summary>
                UIResultView ResultView { get; set; }

                /// <summary>
                /// The amount of times to try downloading the note before
                /// reporting an error to the user (which should be our last resort)
                /// We set it to 0 in debug because that means we WANT the error, as
                /// the user could be working on notes and need the error.
                /// </summary>
                #if DEBUG
                static int MaxDownloadAttempts = 0;
                #else
                static int MaxDownloadAttempts = 5;
                #endif

                /// <summary>
                /// The overlay displayed the first time the user enters Notes
                /// </summary>
                View TutorialBacker { get; set; }
                ImageView TutorialOverlay { get; set; }

                /// <summary>
                /// True if we've already displayed the tutorial this run.
                /// </summary>
                /// <value><c>true</c> if tutorial displayed; otherwise, <c>false</c>.</value>
                bool TutorialDisplayed { get; set; }

                /// <summary>
                /// True if the tutorial is fading in or out
                /// </summary>
                /// <value><c>true</c> if animating tutorial; otherwise, <c>false</c>.</value>
                bool AnimatingTutorial { get; set; }

                /// <summary>
                /// The amount of times we've attempted to download the current note.
                /// When it hits 0, we'll just fail out and tell the user to check their network settings.
                /// </summary>
                /// <value>The note download retries.</value>
                int NoteDownloadRetries { get; set; }

                /// <summary>
                /// Reference to the tutorial overlay image
                /// </summary>
                Bitmap TutorialImage { get; set; }


                public NotesFragment( ) : base( )
                {
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    base.OnCreateView( inflater, container, savedInstanceState );

                    // get the root control from our .axml
                    var layout = inflater.Inflate(Resource.Layout.Notes, container, false) as RelativeLayout;

                    // get the refresh button from the layout
                    RefreshButton = layout.FindViewById<Button>( Resource.Id.refreshButton );

                    // create our overridden lockable scroll view
                    ScrollView = new LockableScrollView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ScrollView.ScrollBarStyle = ScrollbarStyles.InsideInset;
                    ScrollView.OverScrollMode = OverScrollMode.Always;
                    ScrollView.VerticalScrollbarPosition = ScrollbarPosition.Default;
                    ScrollView.LayoutParameters = new RelativeLayout.LayoutParams( RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
                    ScrollView.OnTouchIntercept = OnInterceptTouchEvent;
                    ScrollView.OnChangedScroll = OnScrollChanged;
                    ((RelativeLayout.LayoutParams)ScrollView.LayoutParameters).AddRule(LayoutRules.CenterHorizontal);
                    ((RelativeLayout.LayoutParams)ScrollView.LayoutParameters).AddRule(LayoutRules.Below, Resource.Id.refreshButton);

                    // add it to our main layout.
                    layout.AddView( ScrollView );

                    Indicator = layout.FindViewById<ProgressBar>( Resource.Id.progressBar );
                    Indicator.Visibility = ViewStates.Gone;
                    Indicator.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( 0 ) );
                    Indicator.BringToFront();

                    // create the layout that will contain the notes
                    ScrollViewLayout = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ScrollView.AddView( ScrollViewLayout );
                    ScrollViewLayout.SetOnTouchListener( this );

                    RefreshButton.Click += (object sender, EventArgs e ) =>
                    {
                        DeleteNote( );

                        PrepareCreateNotes(  );
                    };

                    // if the refresh button isn't enabled, hide it
                    if ( MobileApp.Shared.Network.RockLaunchData.Instance.Data.DeveloperModeEnabled == false )
                    {
                        RefreshButton.Visibility = ViewStates.Gone;
                    }

                    // get our power management control
                    PowerManager pm = PowerManager.FromContext( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    WakeLock = pm.NewWakeLock(WakeLockFlags.Full, "Notes");

                    ResultView = new UIResultView( layout, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), OnResultViewDone );

                    ResultView.Hide( );

                    // setup the tutorial overlay
                    TutorialBacker = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TutorialBacker.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    TutorialBacker.Alpha = 0;
                    TutorialBacker.SetBackgroundColor( Android.Graphics.Color.Black );
                    layout.AddView( TutorialBacker );

                    AnimatingTutorial = false;
                    TutorialOverlay = new ImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TutorialOverlay.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ((RelativeLayout.LayoutParams)TutorialOverlay.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
                    TutorialOverlay.Alpha = 0;
                    TutorialOverlay.SetBackgroundColor( Android.Graphics.Color.Transparent );
                    layout.AddView( TutorialOverlay );

                    NavBarRevealTracker = new NavBarReveal( );

                    return layout;
                }

                void OnResultViewDone( )
                {
                    // if they tap "retry", redownload the notes.
                    DeleteNote( );

                    PrepareCreateNotes( );
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    PrepareCreateNotes( );

                    ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );

                    AnimateTutorialScreen( false );
                }

                public override void OnResume()
                {
                    // when we're resuming, take a lock on the device sleeping to prevent it
                    base.OnResume( );

                    // make sure they're ok with rotation and didn't lock their phone's orientation
                    if( Rock.Mobile.PlatformSpecific.Android.Core.IsOrientationUnlocked( ) )
                    {
                        Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;
                    }

                    NavBarRevealTracker.NavToolbar = ParentTask.NavbarFragment.NavToolbar;

                    WakeLock.Acquire( );

                    // we're resuming, so reset our download count
                    NoteDownloadRetries = MaxDownloadAttempts;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( true, 
                        delegate
                        {
                            Intent sendIntent = new Intent();
                            sendIntent.SetAction( Intent.ActionSend );

                            // build a nice subject line
                            string subject = string.Format( MessagesStrings.Read_Share_Notes, NoteName );
                            sendIntent.PutExtra( Intent.ExtraSubject, subject );

                            string htmlStream = null;
                            string textStream = null;
                            Note.GetNotesForEmail( out htmlStream, out textStream );

                            sendIntent.PutExtra( Intent.ExtraText, Android.Text.Html.FromHtml( htmlStream ) );
                            sendIntent.SetType( "text/html" );
                            StartActivity( sendIntent );
                        } );

                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    // if the task is ready, go ahead and create the notes. Alternatively, 
                    // if we are resuming from a pause, it's safe to create the notes. If we don't,
                    // the user will see a blank screen.
                    FragmentReady = true;
                    if( ParentTask.TaskReadyForFragmentDisplay == true )
                    {
                        PrepareCreateNotes( );
                    }
                }

                public override void TaskReadyForFragmentDisplay()
                {
                    // our parent task is letting us know it's ready.
                    // if we've had our OnResume called, then we're ready too.

                    // Otherwise, when OnResume IS called, the parent task will be ready
                    // and we'll create the notes then.
                    if( FragmentReady == true )
                    {
                        PrepareCreateNotes( );
                    }
                }

                public override void OnPause()
                {
                    // when we're being backgrounded, release our lock so we don't force
                    // the device to stay on
                    base.OnPause( );

                    FragmentReady = false;

                    WakeLock.Release( );

                    // if we don't support full widescreen, enable the reveal button
                    if ( MainActivity.SupportsLandscapeWide( ) == false )
                    {
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( true );

                        // also force the orientation to portait
                        Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                    }

                    ShutdownNotes( null );
                }

                public override void OnSaveInstanceState( Bundle outState )
                {
                    base.OnSaveInstanceState( outState );

                    ShutdownNotes( outState );
                }

                public override void OnDestroy()
                {
                    base.OnDestroy( );

                    ShutdownNotes( null );
                }

                public bool OnInterceptTouchEvent(MotionEvent ev)
                {
                    // called by the LockableScrollView. This allows us to shut the
                    // springboard if it's open and the user touches the note.
                    if ( MainActivity.IsLandscapeWide( ) == false )
                    {
                        if ( ParentTask.NavbarFragment.ShouldSpringboardAllowInput( ) )
                        {
                            ParentTask.NavbarFragment.RevealSpringboard( false );
                            return false;
                        }
                    }

                    // if we get any touch input whatsoever, lose the tutorial screen
                    AnimateTutorialScreen( false );
                    return true;
                }

                public override bool OnDoubleTap(MotionEvent e)
                {
                    // a double tap CAN create a user note. If it did,
                    // we want to know that so we suppress further input until we receive
                    // TouchUp
                    try
                    {
                        DidGestureCreateNote = Note.DidDoubleTap( new System.Drawing.PointF( e.GetX( ), e.GetY( ) ) );

                        // if the gesture did create a note, flag that so we don't show the tutorial anymore.
                        if( DidGestureCreateNote == true )
                        {
                            MobileApp.Shared.Network.RockMobileUser.Instance.UserNoteCreated = true;
                        }
                    }
                    catch( Exception ex )
                    {
                        Springboard.DisplayError( "Notes", ex.Message );
                        DidGestureCreateNote = false;
                    }

                    return true;
                }

                public override bool OnDownGesture( MotionEvent e )
                {
                    // only processes TouchesBegan if we didn't create a note with this gesture.
                    if ( DidGestureCreateNote == false )
                    {
                        if ( Note != null )
                        {
                            if ( Note.TouchesBegan( new System.Drawing.PointF( e.GetX( ), e.GetY( ) ) ) )
                            {
                                ScrollView.ScrollEnabled = false;

                                MovingUserNote = true;
                            }
                        }
                    }
                    return false;
                }

                public override bool OnScrollGesture( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
                {
                    // if we're moving a user note, consume the input so that the 
                    // springboard doesn't receive this, and thus doesn't pan.
                    if ( MovingUserNote == true )
                    {
                        return true;
                    }
                    else
                    {
                        return base.OnScrollGesture( e1, e2, distanceX, distanceY );
                    }
                }

                public void OnScrollChanged( float scrollDelta )
                {
                    // whenever the scrollview is scrolled, let the navBar tracker know
                    NavBarRevealTracker.Update( scrollDelta, ScrollView.ScrollY, ScrollViewLayout.LayoutParameters.Height );
                }

                public class UrlClickParams
                {
                    public bool UseExternalBrowser { get; set; }
                    public bool UseImpersonationToken { get; set; }
                    public string Url { get; set; }
                }

                public override bool OnTouch( View v, MotionEvent e )
                {
                    // check to see if we should monitor navBar reveal
                    if ( e.Action == MotionEventActions.Down )
                    {
                        NavBarRevealTracker.BeginTracking( ScrollView.ScrollY );   
                    }
                    
                    if ( base.OnTouch( v, e ) == true )
                    {
                        return true;
                    }
                    else
                    {
                        switch ( e.Action )
                        {
                            case MotionEventActions.Move:
                            {
                                // if at any point during a move the task is no longer allowed to receive input,
                                // STOP SCROLLING. It means the user began panning out the view
                                if ( ParentTask.NavbarFragment.ShouldTaskAllowInput( ) == false )
                                {
                                    ScrollView.ScrollEnabled = false;
                                }

                                if ( Note != null )
                                {
                                    Note.TouchesMoved( new System.Drawing.PointF( e.GetX( ), e.GetY( ) ) );
                                }

                                break;
                            }

                            case MotionEventActions.Up:
                            {
                                if ( Note != null )
                                {
                                    // if the tutorial is showing, we don't want to do anything but hide it.
                                    // This prevents a URL from changing the page when the user only intended to
                                    // hide the tutorial.
                                    if( TutorialShowing( ) )
                                    {
                                        AnimateTutorialScreen( false );
                                    }
                                    else
                                    {
                                        bool urlLaunchesExternalBrowser = false;
                                        bool urlUsesRockImpersonation = false;

                                        string activeUrl = Note.TouchesEnded( new System.Drawing.PointF( e.GetX( ), e.GetY( ) ), out urlLaunchesExternalBrowser, out urlUsesRockImpersonation );

                                        // create a params object we can send to the parent
                                        UrlClickParams clickParams = new UrlClickParams()
                                        {
                                                UseExternalBrowser = urlLaunchesExternalBrowser,
                                                UseImpersonationToken = urlUsesRockImpersonation,
                                                Url = activeUrl
                                        };

                                        // again, only process this if we didn't create a note. We don't want to treat a double tap
                                        // like a request to view a note
                                        if ( DidGestureCreateNote == false )
                                        {
                                            if ( string.IsNullOrEmpty( activeUrl ) == false )
                                            {
                                                // if the url uses the rock impersonation token, it's safe to assume they tapped the takeaway.
                                                if ( urlUsesRockImpersonation )
                                                {
                                                    MessageAnalytic.Instance.Trigger( MessageAnalytic.Takeaway, activeUrl );
                                                }

                                                ParentTask.OnClick( this, 0, clickParams );
                                            }
                                        }
                                    }
                                }

                                ScrollView.ScrollEnabled = true;
                                MovingUserNote = false;

                                DidGestureCreateNote = false;

                                break;
                            }
                        }
                    }
                    return false;
                }

                protected void ShutdownNotes( Bundle instanceBundle )
                {
                    if ( TutorialImage != null )
                    {
                        TutorialOverlay.SetImageBitmap( null );
                        TutorialImage.Dispose( );
                        TutorialImage = null;
                    }

                    // shutdown notes ensures that user settings are saved
                    // the note is destroyed and references to it are cleared.
                    if( Note != null )
                    {
                        Note.SaveState( (float) ScrollView.ScrollY / (float) ScrollViewLayout.LayoutParameters.Height );

                        Note.Destroy( ScrollViewLayout );
                        Note = null;
                    }
                }

                protected void PrepareCreateNotes( )
                {
                    if( RefreshingNotes == false )
                    {
                        ResultView.Hide( );

                        RefreshingNotes = true;

                        ShutdownNotes( null );

                        // show a busy indicator
                        Indicator.Visibility = ViewStates.Visible;

                        Note.TryDownloadNote( NoteUrl, StyleSheetDefaultHostDomain, false, delegate(bool result )
                            {
                                if( result == true )
                                {
                                    CreateNotes( );
                                }
                                else
                                {
                                    ReportException( "", null );
                                }
                            } );
                    }
                }

                void DisplayMessageBox( string title, string message, Note.MessageBoxResult onResult )
                {
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            AlertDialog.Builder dlgAlert = new AlertDialog.Builder( Rock.Mobile.PlatformSpecific.Android.Core.Context );                      
                            dlgAlert.SetTitle( title ); 
                            dlgAlert.SetMessage( message ); 
                            dlgAlert.SetNeutralButton( GeneralStrings.Yes, delegate
                                {
                                    onResult( 0 );
                                });
                            dlgAlert.SetPositiveButton( GeneralStrings.No, delegate(object sender, DialogClickEventArgs ev )
                                {
                                    onResult( 1 );
                                } );
                            dlgAlert.Create( ).Show( );
                        } );
                }

                protected void CreateNotes( )
                {
                    try
                    {
                        // expect the note and its style sheet to exist.
                        NoteFileName = Rock.Mobile.Util.Strings.Parsers.ParseURLToFileName( NoteUrl );
                        MemoryStream noteData = (MemoryStream)FileCache.Instance.LoadFile( NoteFileName );
                        string noteXML = Encoding.UTF8.GetString( noteData.ToArray( ), 0, (int)noteData.Length );
                        noteData.Dispose( );

                        string styleSheetUrl = Note.GetStyleSheetUrl( noteXML, StyleSheetDefaultHostDomain );
                        StyleFileName = Rock.Mobile.Util.Strings.Parsers.ParseURLToFileName( styleSheetUrl );
                        MemoryStream styleData = (MemoryStream)FileCache.Instance.LoadFile( StyleFileName );
                        string styleXML = Encoding.UTF8.GetString( styleData.ToArray( ), 0, (int)styleData.Length );
                        styleData.Dispose( );

                        Note = new Note( noteXML, styleXML );

                        // Use the metrics and not ScrollView for dimensions, because depending on when this gets called the ScrollView
                        // may not have its dimensions set yet.
                        float scrollPercentOffset = Note.Create( NavbarFragment.GetCurrentContainerDisplayWidth( ), 
                                                                 this.Resources.DisplayMetrics.HeightPixels, 
                                                                 ScrollViewLayout, 
                                                                 NoteFileName + PrivateNoteConfig.UserNoteSuffix, 
                                                                 DisplayMessageBox, 
                                                                 UpdateScrollViewHeight );

                        // set the requested background color
                        ScrollView.SetBackgroundColor( ( Android.Graphics.Color )Rock.Mobile.UI.Util.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value ) );

                        // update the height of the scroll view to fit all content
                        float noteBottom = Note.GetNoteAbsoluteHeight( );

                        int scrollFrameHeight = ( int )noteBottom + ( this.Resources.DisplayMetrics.HeightPixels / 3 );
                        ScrollViewLayout.LayoutParameters.Height = scrollFrameHeight;

                        // scroll to where the user left off
                        ScrollView.Post( new Action( delegate
                            {
                                ScrollView.ForceScrollTo( 0, (int) (scrollPercentOffset * (float)scrollFrameHeight) );
                            }));

                        FinishNotesCreation( );

                        // log the note they are reading.
                        MessageAnalytic.Instance.Trigger( MessageAnalytic.Read, NoteName );

                        // display the tutorial if it hasn't been shown this run of the app, and the user has never created their own note.
                        if( TutorialDisplayed == false && MobileApp.Shared.Network.RockMobileUser.Instance.UserNoteCreated == false )
                        {
                            TutorialDisplayed = true;

                            System.IO.Stream tutorialStream = Activity.BaseContext.Assets.Open( PrivateNoteConfig.TutorialOverlayImage );

                            TutorialImage = Android.Graphics.BitmapFactory.DecodeStream( tutorialStream );
                            TutorialOverlay.SetImageBitmap( TutorialImage );

                            // wait a second before revealing the tutorial overlay
                            System.Timers.Timer timer = new System.Timers.Timer();
                            timer.AutoReset = false;
                            timer.Interval = 750;
                            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                                {
                                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                        {
                                            TutorialBacker.Visibility = ViewStates.Visible;
                                            AnimateTutorialScreen( true );
                                            ScrollView.ScrollEnabled = false;
                                        });
                                };
                            timer.Start( );
                        }

                        ParentTask.NavbarFragment.NavToolbar.Reveal( true );
                    }
                    catch( Exception ex )
                    {
                        ReportException( "", ex );
                    }
                }

                void UpdateScrollViewHeight( )
                {
                    // update the height of the scroll view to fit all content, including user notes
                    // that might go obscenely below the actual note region
                    float noteBottom = Note.GetNoteAbsoluteHeight( );

                    int scrollFrameHeight = ( int )noteBottom + ( this.Resources.DisplayMetrics.HeightPixels / 3 );
                    ScrollViewLayout.LayoutParameters.Height = scrollFrameHeight;
                }

                void FinishNotesCreation( )
                {
                    Indicator.Visibility = ViewStates.Gone;

                    // flag that we're clear to refresh again
                    RefreshingNotes = false;
                }

                void AnimateTutorialScreen( bool fadeIn )
                {
                    // handles fading in / out the tutorial screen
                    float startVal = fadeIn ? 0.00f : 1.00f;
                    float endVal = fadeIn ? 1.00f : 0.00f;

                    // dont do it if the tutorial screen is already in the state we're requesting
                    if ( endVal != TutorialOverlay.Alpha )
                    {
                        if ( AnimatingTutorial == false )
                        {
                            AnimatingTutorial = true;

                            // animate the backer
                            SimpleAnimator_Float backerAnim = new SimpleAnimator_Float( startVal, Math.Min( PrivateNoteConfig.MaxTutorialAlpha, endVal ), .33f, delegate(float percent, object value )
                                {
                                    TutorialBacker.Alpha = (float)value;
                                }, 
                                delegate
                                {
                                    if( fadeIn == false )
                                    {
                                        TutorialBacker.Visibility = ViewStates.Gone;
                                    }
                                } );
                            backerAnim.Start( );

                            // animate the tutorial
                            SimpleAnimator_Float tutorialAnim = new SimpleAnimator_Float( startVal, endVal, .33f, delegate(float percent, object value )
                                {
                                    TutorialOverlay.Alpha = (float)value;
                                }, 
                                delegate
                                {
                                    AnimatingTutorial = false;

                                    if( fadeIn == false )
                                    {
                                        ScrollView.ScrollEnabled = true;
                                    }
                                } );
                            tutorialAnim.Start( );
                        }
                    }
                }

                bool TutorialShowing( )
                {
                    // if it's not hidden and IS opaque, then it's showing.
                    if( TutorialBacker.Visibility == ViewStates.Visible && TutorialBacker.Alpha == PrivateNoteConfig.MaxTutorialAlpha )
                    {
                        return true;
                    }

                    return false;
                }

                protected void DeleteNote( )
                {
                    // delete the existing note files pertaining to this note.
                    if( string.IsNullOrEmpty( NoteFileName ) == false )
                    {
                        FileCache.Instance.RemoveFile( NoteFileName );
                    }

                    if( string.IsNullOrEmpty( StyleFileName ) == false )
                    {
                        FileCache.Instance.RemoveFile( StyleFileName );
                    }
                }

                protected void ReportException( string errorMsg, Exception e )
                {
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            FinishNotesCreation( );

                            // if we have more download attempts, use them before reporting
                            // an error to the user.
                            if( NoteDownloadRetries > 0 )
                            {
                                NoteDownloadRetries--;

                                PrepareCreateNotes( );
                            }
                            else
                            {
                                if ( e != null )
                                {
                                    errorMsg += e.Message;
                                }

                                if ( MobileApp.Shared.Network.RockLaunchData.Instance.Data.DeveloperModeEnabled == true )
                                {
                                    Springboard.DisplayError( "Note Error", errorMsg );
                                }
                                else
                                {
                                    ResultView.Show( MessagesStrings.Error_Title, 
                                                     PrivateControlStylingConfig.Result_Symbol_Failed, 
                                                     MessagesStrings.Error_Message, 
                                                     GeneralStrings.Retry );
                                }
                            }
                        } );
                }
            }
        }
    }
}
