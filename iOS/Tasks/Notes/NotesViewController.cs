using System;
using CoreGraphics;
using Foundation;
using UIKit;
using System.Collections.Generic;
using System.IO;
using CoreAnimation;

using Rock.Mobile.Network;
using MobileApp.Shared.Notes;
using RestSharp;
using System.Net;
using System.Text;
using MobileApp.Shared.Config;
using Rock.Mobile.UI;
using System.Drawing;
using Rock.Mobile.PlatformSpecific.Util;
using MobileApp.Shared;
using MobileApp.Shared.Analytics;
using MobileApp.Shared.Strings;
using MobileApp.Shared.UI;
using Rock.Mobile.Animation;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.IO;

namespace iOS
{
    // create a subclass of UIScrollView so we can intercept its touch events
    public class CustomScrollView : UIScrollView
    {
        public NotesViewController Interceptor { get; set; }

        // UIScrollView will check for scrolling and suppress touchesBegan
        // if the user is scrolling. We want to allow our controls to consume it
        // before that.
        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            // transform the point into absolute coords (as if there was no scrolling)
            CGPoint absolutePoint = new CGPoint( ( point.X - ContentOffset.X ) + Frame.Left,
                                                 ( point.Y - ContentOffset.Y ) + Frame.Top );

            if ( Frame.Contains( absolutePoint ) )
            {
                // Base OS controls need to know whether to process & consume
                // input or pass it up to the higher level (us.)
                // We decide that based on whether the HitTest intersects any of our controls.
                // By returning true, it can know "Yes, this hits something we need to know about"
                // and it will result in us receiving TouchBegan
                if( Interceptor.HitTest( point ) )
                {
                    return null;
                }
            }
            return base.HitTest(point, uievent);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            if( Interceptor != null )
            {
                Interceptor.TouchesBegan( touches, evt );
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            if( Interceptor != null )
            {
                Interceptor.TouchesMoved( touches, evt );
            }
        }

        public override void TouchesEnded( NSSet touches, UIEvent evt )
        {
            if( Interceptor != null )
            {
                Interceptor.TouchesEnded( touches, evt );
            }
        }
    }

    public partial class NotesViewController : TaskUIViewController
    {
        /// <summary>
        /// Displays when content is being downloaded.
        /// </summary>
        /// <value>The indicator.</value>
        UIActivityIndicatorView Indicator { get; set; }

        /// <summary>
        /// Reloads the NoteScript
        /// </summary>
        /// <value>The refresh button.</value>
        UIButton RefreshButton { get; set; }

        /// <summary>
        /// Displays the actual Note content
        /// </summary>
        /// <value>The user interface scroll view.</value>
        CustomScrollView UIScrollView { get; set; }

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
        /// The URL for this note
        /// </summary>
        /// <value>The note URL.</value>
        public string NoteUrl { get; set; }

        /// <summary>
        /// If the style sheet URLs aren't absolute, this is the domain to prefix.
        /// </summary>
        public string StyleSheetDefaultHostDomain { get; set; }

        /// <summary>
        /// A presentable name for the note. Used for things like email subjects
        /// </summary>
        /// <value>The name of the note presentable.</value>
        public string NoteName { get; set; }

        protected string NoteFileName { get; set; }
        protected string StyleFileName { get; set; }

        /// <summary>
        /// The current orientation of the device. We track this
        /// so we can know when it changes and only rebuild the notes then.
        /// </summary>
        /// <value>The orientation.</value>
		//UIDeviceOrientation Orientation { get; set; }
        int OrientationState { get; set; }

        /// <summary>
        /// The overlay displayed the first time the user enters Notes
        /// </summary>
        UIView TutorialBacker { get; set; }
        UIImageView TutorialOverlay { get; set; }

        /// <summary>
        /// True if the tutorial is fading in or out
        /// </summary>
        /// <value><c>true</c> if animating tutorial; otherwise, <c>false</c>.</value>
        bool AnimatingTutorial { get; set; }

        /// <summary>
        /// True if we've shown the tutorial this session
        /// </summary>
        /// <value><c>true</c> if tutorial displayed; otherwise, <c>false</c>.</value>
        bool TutorialDisplayed { get; set; }

        /// <summary>
        /// The manager that ensures views being edited are visible when the keyboard comes up.
        /// </summary>
        /// <value>The keyboard adjust manager.</value>
        Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager KeyboardAdjustManager { get; set; }

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
        /// The amount of times we've attempted to download the current note.
        /// When it hits 0, we'll just fail out and tell the user to check their network settings.
        /// </summary>
        /// <value>The note download retries.</value>
        int NoteDownloadRetries { get; set; }

        /// <summary>
        /// The view to use for displaying a download error
        /// </summary>
        UIResultView ResultView { get; set; }

        public NotesViewController( ) : base( )
        {
        }

        public override void DidReceiveMemoryWarning( )
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning( );

            // Release any cached data, images, etc that aren't in use.
        }

        protected void SaveNoteState( nfloat scrollOffsetPercent )
        {
            // request quick backgrounding so we can save our user notes
            nint taskID = UIApplication.SharedApplication.BeginBackgroundTask( () => {});

            if( Note != null )
            {
                Note.SaveState( (float)scrollOffsetPercent );
            }

            UIApplication.SharedApplication.EndBackgroundTask(taskID);
        }

        public override void ViewDidLayoutSubviews( )
        {
            base.ViewDidLayoutSubviews( );

            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Rock.Mobile.Util.Debug.WriteLine( "Turning idle timer OFF" );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            // get the orientation state. WE consider unknown- 1, profile 0, landscape 1,
            int orientationState = SpringboardViewController.IsDeviceLandscape( ) == true ? 1 : 0;

            // if the states are in disagreement, correct it
            if ( OrientationState != orientationState ) 
            {
                OrientationState = orientationState;

                // get the offset scrolled before changing our frame (which will cause us to lose it)
                nfloat scrollOffsetPercent = UIScrollView.ContentOffset.Y / (nfloat) Math.Max( 1, UIScrollView.ContentSize.Height );

                //note: the frame height of the nav bar is what it CURRENTLY is, not what it WILL be after we rotate. So, when we go from Portrait to Landscape,
                // it says 40, but it's gonna be 32. Conversely, going back, we use 32 and it's actually 40, which causes us to start this view 8px too high.
                if ( MobileApp.Shared.Network.RockLaunchData.Instance.Data.DeveloperModeEnabled == true )
                {
                    // add the refresh button if necessary
                    if ( RefreshButton.Superview == null )
                    {
                        View.AddSubview( RefreshButton );
                    }

                    RefreshButton.Layer.Position = new CGPoint( View.Bounds.Width / 2, ( RefreshButton.Frame.Height / 2 ) );

                    UIScrollView.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height - RefreshButton.Frame.Height );
                    UIScrollView.Layer.Position = new CGPoint( UIScrollView.Layer.Position.X, UIScrollView.Layer.Position.Y + RefreshButton.Frame.Bottom );
                }
                else
                {
                    // remove the refresh button if necessary
                    if ( RefreshButton.Superview != null )
                    {
                        RefreshButton.RemoveFromSuperview( );
                    }

                    UIScrollView.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height );
                    UIScrollView.Layer.Position = new CGPoint( UIScrollView.Layer.Position.X, UIScrollView.Layer.Position.Y );
                }

                Indicator.Layer.Position = new CGPoint (View.Bounds.Width / 2, View.Bounds.Height / 2);

                // re-create our notes with the new dimensions
                PrepareCreateNotes( scrollOffsetPercent, false );

                // since we're changing orientations, hide the tutorial screen
                AnimateTutorialScreen( false );

                ResultView.SetBounds( View.Bounds.ToRectF( ) );
            }
        }

        public override void ViewDidLoad( )
        {
            base.ViewDidLoad( );

            OrientationState = -1;

            UIScrollView = new CustomScrollView();
            UIScrollView.Interceptor = this;
            UIScrollView.Frame = View.Frame;
            UIScrollView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( 0x1C1C1CFF );
            UIScrollView.Delegate = new NavBarRevealHelperDelegate( Task.NavToolbar );
            UIScrollView.Layer.AnchorPoint = new CGPoint( 0, 0 );

            UITapGestureRecognizer tapGesture = new UITapGestureRecognizer();
            tapGesture.NumberOfTapsRequired = 2;
            tapGesture.AddTarget( this, new ObjCRuntime.Selector( "DoubleTapSelector:" ) );
            UIScrollView.AddGestureRecognizer( tapGesture );

            View.BackgroundColor = UIScrollView.BackgroundColor;
            View.AddSubview( UIScrollView );

            // add a busy indicator
            Indicator = new UIActivityIndicatorView( UIActivityIndicatorViewStyle.White );
            UIScrollView.AddSubview( Indicator );

            // add a refresh button for debugging
            RefreshButton = UIButton.FromType( UIButtonType.System );
            RefreshButton.SetTitle( "Refresh", UIControlState.Normal );
            RefreshButton.SizeToFit( );

            // if they tap the refresh button, refresh the list
            RefreshButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                DeleteNote( );

                PrepareCreateNotes( 0, true );
            };
            
            ResultView = new UIResultView( UIScrollView, View.Frame.ToRectF( ), OnResultViewDone );

            ResultView.Hide( );

            // setup the tutorial overlay
            TutorialBacker = new UIView( );
            TutorialBacker.Layer.AnchorPoint = CGPoint.Empty;
            TutorialBacker.Alpha = 0.00f;
            TutorialBacker.BackgroundColor = UIColor.Black;
            TutorialBacker.Hidden = true;
            View.AddSubview( TutorialBacker );

            AnimatingTutorial = false;
            TutorialOverlay = new UIImageView( );
            TutorialOverlay.Layer.AnchorPoint = CGPoint.Empty;
            TutorialOverlay.Frame = View.Frame;
            TutorialOverlay.Alpha = 0.00f;
            View.AddSubview( TutorialOverlay );

            KeyboardAdjustManager = new Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager( View );
        }

        void OnResultViewDone( )
        {
            // if they tap "Retry", well, retry!
            DeleteNote( );

            PrepareCreateNotes( 0, true );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // since we're reappearing, we know we're safe to reset our download count
            NoteDownloadRetries = MaxDownloadAttempts;
            Rock.Mobile.Util.Debug.WriteLine( "Resetting Download Attempts" );

            LayoutChanged( );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            KeyboardAdjustManager.Activate( );

            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Rock.Mobile.Util.Debug.WriteLine( "Turning idle timer OFF" );
        }

        /*CGRect GetTappedTextFieldFrame( RectangleF textFrame )
        {
            // first subtract the amount scrolled by the view.
            nfloat yPos = textFrame.Y - UIScrollView.ContentOffset.Y;
            nfloat xPos = textFrame.X - UIScrollView.ContentOffset.X;

            // now add in however far down the scroll view is from the top.
            yPos += UIScrollView.Frame.Y;
            xPos += UIScrollView.Frame.X;

            return new CGRect( xPos, yPos, textFrame.Width, textFrame.Height );
        }*/

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if ( KeyboardAdjustManager != null )
            {
                KeyboardAdjustManager.Deactivate( );
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ViewResigning( );
        }

        public override void OnActivated()
        {
            base.OnActivated();

            // yet another place to drop in an idle timer disable
            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Rock.Mobile.Util.Debug.WriteLine( "Turning idle timer OFF" );

            LayoutChanged( );
        }

        public override void WillEnterForeground( )
        {
            base.WillEnterForeground( );

            // yet another place to drop in an idle timer disable
            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Rock.Mobile.Util.Debug.WriteLine( "Turning idle timer OFF" );

            // force a redraw so the notes are recreated
            LayoutChanged( );
        }

        public override void AppOnResignActive()
        {
            base.AppOnResignActive();

            ViewResigning( );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate();

            ViewResigning( );
        }

        /// <summary>
        /// Called when the view will dissapear, or when the task sees that the app is going into the background.
        /// </summary>
        public void ViewResigning()
        {
            SaveNoteState( UIScrollView.ContentOffset.Y / (nfloat) Math.Max( 1, UIScrollView.ContentSize.Height ) );

            DestroyNotes( );

            OrientationState = -1;

            UIApplication.SharedApplication.IdleTimerDisabled = false;
            Rock.Mobile.Util.Debug.WriteLine( "Turning idle timer ON" );
        }

        public class ExportedNoteSource : UIActivityItemSource
        {
            public NSString TextStream { get; set; }
            public NSString HTMLStream { get; set; }

            public override string GetDataTypeIdentifierForActivity(UIActivityViewController activityViewController, NSString activityType)
            {
                // let iOS know that we can offer it as simply as plan text.
                return "public.plain-text";
            }

            public override NSObject GetItemForActivity(UIActivityViewController activityViewController, NSString activityType)
            {
                // if it's the mail app, use HTML so it's fancy!
                if ( activityType == UIActivityType.Mail )
                {
                    return HTMLStream;
                }
                else
                {
                    // offer everything else plan text.
                    return TextStream;
                }
            }

            public override NSObject GetPlaceholderData(UIActivityViewController activityViewController)
            {
                return TextStream;
            }
        }

        public void ShareNotes()
        {
            if ( Note != null )
            {
                string htmlStream;
                string textStream;
                Note.GetNotesForEmail( out htmlStream, out textStream );

                ExportedNoteSource noteSource = new ExportedNoteSource();
                noteSource.HTMLStream = new NSString( htmlStream );
                noteSource.TextStream = new NSString( textStream );
                var items = new NSObject[] { noteSource };

                UIActivityViewController shareController = new UIActivityViewController( items, null );

                // set the subject line in case the share to email
                string emailSubject = string.Format( MobileApp.Shared.Strings.MessagesStrings.Read_Share_Notes, NoteName );
                shareController.SetValueForKey( new NSString( emailSubject ), new NSString( "subject" ) );

                // if devices like an iPad want an anchor, set it
                if ( shareController.PopoverPresentationController != null )
                {
                    shareController.PopoverPresentationController.SourceView = Task.NavToolbar;
                }
                PresentViewController( shareController, true, null );
            }
        }

        public bool HitTest( CGPoint point )
        {
            if( Note != null )
            {
                AnimateTutorialScreen( false );

                // Base OS controls need to know whether to process & consume
                // input or pass it up to the higher level (us.)
                // We decide that based on whether the HitTest intersects any of our controls.
                // By returning true, it can know "Yes, this hits something we need to know about"
                // and it will result in us receiving TouchBegan
                if( Note.HitTest( point.ToPointF( ) ) == true )
                {
                    return true;
                }
            }

            return false;
        }

        public bool HandleTouchBegan( CGPoint point )
        {
            if( Note != null )
            {
                // if the note consumed touches Began, don't allow the UIScroll View to scroll.
                if( Note.TouchesBegan( point.ToPointF( ) ) == true )
                {
                    UIScrollView.ScrollEnabled = false;
                    return true;
                }
            }

            return false;
        }

        public bool TouchingUserNote( NSSet touches, UIEvent evt )
        {
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && Note != null)
            {
                return Note.TouchingUserNote( touch.LocationInView( UIScrollView ).ToPointF( ) );
            }

            return false;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            Rock.Mobile.Util.Debug.WriteLine( "Touches Began" );

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                HandleTouchBegan( touch.LocationInView( UIScrollView ) );
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved( touches, evt );

            Rock.Mobile.Util.Debug.WriteLine( "Touches MOVED" );

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                if( Note != null )
                {
                    Note.TouchesMoved( touch.LocationInView( UIScrollView ).ToPointF( ) );
                }
            }
        }

        public override void TouchesEnded( NSSet touches, UIEvent evt )
        {
            base.TouchesEnded( touches, evt );

            Rock.Mobile.Util.Debug.WriteLine( "Touches Ended" );

            // if the tutorial is showing, all we want to do is hide it.
            // If we process input, it's possible they'll tap thru it to a URL, which will
            // switch pages and cause a lot of user confustion
            if( TutorialShowing( ) )
            {
                AnimateTutorialScreen( false );
            }
            else
            {
                UITouch touch = touches.AnyObject as UITouch;
                if( touch != null )
                {
                    if( Note != null )
                    {
                        // should we visit a website?
                        bool urlLaunchesExternalBrowser = false;
                        bool urlUsesRockImpersonation = false;

                        string activeUrl = Note.TouchesEnded( touch.LocationInView( UIScrollView ).ToPointF( ), out urlLaunchesExternalBrowser, out urlUsesRockImpersonation );
                        if ( string.IsNullOrEmpty( activeUrl ) == false )
                        {
                            SaveNoteState( UIScrollView.ContentOffset.Y / (nfloat) Math.Max( 1, UIScrollView.ContentSize.Height ) );

                            DestroyNotes( );

                            // if the url uses the rock impersonation token, it's safe to assume they tapped the takeaway.
                            if ( urlUsesRockImpersonation )
                            {
                                MessageAnalytic.Instance.Trigger( MessageAnalytic.Takeaway, activeUrl );
                            }

                            Task.NavToolbar.Reveal( true );
                            Task.NavToolbar.SetBackButtonEnabled( true );
                            TaskWebViewController.HandleUrl( urlLaunchesExternalBrowser, urlUsesRockImpersonation, activeUrl, Task, this, true, false, false );
                        }
                    }
                }

                // when a touch is released, re-enabled scrolling
                UIScrollView.ScrollEnabled = true;
            }
        }

        [Foundation.Export("DoubleTapSelector:")]
        public void HandleTapGesture(UITapGestureRecognizer tap)
        {
            if( Note != null )
            {
                if( tap.State == UIGestureRecognizerState.Ended )
                {
                    try
                    {
                        if( Note.DidDoubleTap( tap.LocationInView( UIScrollView ).ToPointF( ) ) )
                        {
                            MobileApp.Shared.Network.RockMobileUser.Instance.UserNoteCreated = true;
                        }
                    }
                    catch( Exception e )
                    {
                        // we know this exception is the too many notes one. Just show it.
                        SpringboardViewController.DisplayError( "Messages", e.Message );
                    }
                }
            }
        }

        public void DestroyNotes( )
        {
            if( Note != null )
            {
                Note.Destroy( null );
                Note = null;
            }
        }

        public void PrepareCreateNotes( nfloat scrollOffsetPercent, bool forceDownload )
        {
            if( RefreshingNotes == false )
            {
                ResultView.Hide( );

                // if we're recreating the notes, reset our scrollview.
                UIScrollView.ContentOffset = CGPoint.Empty;

                RefreshingNotes = true;

                SaveNoteState( scrollOffsetPercent );

                DestroyNotes( );

                // show a busy indicator
                Indicator.StartAnimating( );

                Note.TryDownloadNote( NoteUrl, StyleSheetDefaultHostDomain, forceDownload, delegate(bool result )
                    {
                        if( result == true )
                        {
                            CreateNotes( );
                        }
                        else
                        {
                            ReportException( "Error downloading note", null );
                        }
                    } );
            }
        }

        void DisplayMessageBox( string title, string message, Note.MessageBoxResult onResult )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    UIAlertView alert = new UIAlertView();
                    alert.Title = title;
                    alert.Message = message;
                    alert.AddButton( GeneralStrings.Yes );
                    alert.AddButton( GeneralStrings.No );
                    alert.Show( ); 
                    alert.Clicked += (object sender, UIButtonEventArgs e) => 
                        {
                            onResult( (int)e.ButtonIndex );
                        };
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

                string styleSheetUrl = Note.GetStyleSheetUrl( noteXML, StyleSheetDefaultHostDomain );
                StyleFileName = Rock.Mobile.Util.Strings.Parsers.ParseURLToFileName( styleSheetUrl );
                MemoryStream styleData = (MemoryStream)FileCache.Instance.LoadFile( StyleFileName );
                string styleXML = Encoding.UTF8.GetString( styleData.ToArray( ), 0, (int)styleData.Length );

                Note = new Note( noteXML, styleXML );

                float scrollPercentOffset = Note.Create( (float)UIScrollView.Bounds.Width, 
                                                         (float)UIScrollView.Bounds.Height, 
                                                         this.UIScrollView, 
                                                         NoteFileName + PrivateNoteConfig.UserNoteSuffix, 
                                                         DisplayMessageBox, 
                                                         UpdateScrollViewHeight );

                // enable scrolling
                UIScrollView.ScrollEnabled = true;

                // take the requested background color
                UIScrollView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStyles.mMainNote.mBackgroundColor.Value );
                View.BackgroundColor = UIScrollView.BackgroundColor; //Make the view itself match too

                // update the height of the scroll view to fit all content
                //CGRect frame = Note.GetFrame( );
                float height = Note.GetNoteAbsoluteHeight( );
                UIScrollView.ContentSize = new CGSize( UIScrollView.Bounds.Width, height + ( UIScrollView.Bounds.Height / 3 ) );

                UIScrollView.ContentOffset = new CGPoint( 0, scrollPercentOffset * UIScrollView.ContentSize.Height );

                FinishNotesCreation( );

                // log the note they are reading.
                MessageAnalytic.Instance.Trigger( MessageAnalytic.Read, NoteName );

                // if the user has never seen it, show them the tutorial screen
                if( TutorialDisplayed == false && MobileApp.Shared.Network.RockMobileUser.Instance.UserNoteCreated == false )
                {
                    TutorialDisplayed = true;

                    // wait a second before revealing the tutorial overlay
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.AutoReset = false;
                    timer.Interval = 750;
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    TutorialOverlay.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + PrivateNoteConfig.TutorialOverlayImage );

                                    TutorialOverlay.ContentMode = UIViewContentMode.Center;
                                    TutorialOverlay.Frame = View.Frame;

                                    TutorialBacker.Frame = View.Frame;
                                    TutorialBacker.Hidden = false;

                                    AnimateTutorialScreen( true );
                                    UIScrollView.ScrollEnabled = false;
                                });
                        };
                    timer.Start( );
                }

                Task.NavToolbar.Reveal( true );
            }
            catch( Exception ex )
            {
                ReportException( "", ex );
            }
        }

        void UpdateScrollViewHeight( )
        {
            // get the height of the note
            float noteHeight = Note.GetNoteAbsoluteHeight( );
            nfloat scrollViewHeight = noteHeight + ( UIScrollView.Bounds.Height / 3 );


            // if that no longer matches our scroll view height, we need
            // to update our scroll view.
            if ( scrollViewHeight != UIScrollView.ContentSize.Height )
            {
                nfloat contentSizeDelta = UIScrollView.ContentSize.Height;

                // expand the content size so the user can scroll to the bottom of their user note.
                UIScrollView.ContentSize = new CGSize( 0, scrollViewHeight );

                // get the change, but clamp to 0. We want to follow them editing down, but not if they delete,
                // which will shrink the box
                contentSizeDelta = (nfloat)Math.Max( 0, (double)(UIScrollView.ContentSize.Height - contentSizeDelta) );

                // and update our scroll offset by that.
                UIScrollView.ContentOffset = new CGPoint( 0, UIScrollView.ContentOffset.Y + contentSizeDelta );
            }
        }

        void FinishNotesCreation( )
        {
            Indicator.StopAnimating( );

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

                    // animate the backer (and don't let it get darker than 80%)
                    SimpleAnimator_Float backerAnim = new SimpleAnimator_Float( startVal, Math.Min( PrivateNoteConfig.MaxTutorialAlpha, endVal ), .15f, delegate(float percent, object value )
                        {
                            TutorialBacker.Alpha = (float)value;
                        }, 
                        delegate
                        {
                            if( fadeIn == false )
                            {
                                TutorialBacker.Hidden = true;
                            }
                        } );
                    backerAnim.Start( );

                    SimpleAnimator_Float tutorialAnim = new SimpleAnimator_Float( startVal, endVal, .15f, delegate(float percent, object value )
                        {
                            TutorialOverlay.Alpha = (float)value;
                        }, 
                        delegate
                        {
                            if( fadeIn == false )
                            {
                                UIScrollView.ScrollEnabled = true;
                            }
                            AnimatingTutorial = false;
                        } );
                    tutorialAnim.Start( );
                }
            }
        }

        bool TutorialShowing( )
        {
            // if it's not hidden and IS opaque, then it's showing.
            if( TutorialBacker.Hidden == false && TutorialBacker.Alpha == PrivateNoteConfig.MaxTutorialAlpha )
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
            new NSObject( ).InvokeOnMainThread( delegate
                {
                    FinishNotesCreation( );

                    DeleteNote( );

                    // since there was an error, try redownloading the notes
                    if( NoteDownloadRetries > 0 )
                    {
                        Rock.Mobile.Util.Debug.WriteLine( "Download error. Trying again" );

                        NoteDownloadRetries--;
                        PrepareCreateNotes( 0, true );
                    }
                    else 
                    {
                        // we've tried as many times as we're going to. Give up and error.
                        if( e != null )
                        {
                            errorMsg += "\n" + e.Message;
                        }

                        if ( MobileApp.Shared.Network.RockLaunchData.Instance.Data.DeveloperModeEnabled == true )
                        {
                            // explain that we couldn't generate notes
                            UIAlertView alert = new UIAlertView( );
                            alert.Title = "Note Error";
                            alert.Message = errorMsg;
                            alert.AddButton( "Ok" );
                            alert.Show( );
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

