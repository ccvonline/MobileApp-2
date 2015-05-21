using System;
using CoreGraphics;
using Foundation;
using UIKit;
using System.Collections.Generic;
using System.IO;
using CoreAnimation;

using Rock.Mobile.Network;
using App.Shared.Notes;
using RestSharp;
using System.Net;
using System.Text;
using App.Shared.Config;
using Rock.Mobile.UI;
using System.Drawing;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared;
using App.Shared.Analytics;
using App.Shared.Strings;
using App.Shared.UI;
using Rock.Mobile.Animation;
using App.Shared.PrivateConfig;
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
        /// A list of the handles returned when adding observers to OS events
        /// </summary>
        /// <value>The observer handles.</value>
        List<NSObject> ObserverHandles { get; set; }

        /// <summary>
        /// The overlay displayed the first time the user enters Notes
        /// </summary>
        UIImageView TutorialOverlay { get; set; }

        /// <summary>
        /// True if the tutorial is fading in or out
        /// </summary>
        /// <value><c>true</c> if animating tutorial; otherwise, <c>false</c>.</value>
        bool AnimatingTutorial { get; set; }

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
            ObserverHandles = new List<NSObject>();
        }

        public override void DidReceiveMemoryWarning( )
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning( );

            // Release any cached data, images, etc that aren't in use.
        }

        protected void SaveNoteState( )
        {
            // request quick backgrounding so we can save our user notes
            nint taskID = UIApplication.SharedApplication.BeginBackgroundTask( () => {});

            if( Note != null )
            {
                Note.SaveState( (float) (UIScrollView.ContentOffset.Y / UIScrollView.ContentSize.Height) );
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

                //note: the frame height of the nav bar is what it CURRENTLY is, not what it WILL be after we rotate. So, when we go from Portrait to Landscape,
                // it says 40, but it's gonna be 32. Conversely, going back, we use 32 and it's actually 40, which causes us to start this view 8px too high.
                if ( App.Shared.Network.RockGeneralData.Instance.Data.RefreshButtonEnabled == true )
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
                PrepareCreateNotes( );

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

                PrepareCreateNotes( );
            };

            KeyboardAdjustManager = new Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager( View, UIScrollView );

            ResultView = new UIResultView( UIScrollView, View.Frame.ToRectF( ), OnResultViewDone );

            ResultView.Hide( );

            // setup the tutorial overlay
            AnimatingTutorial = false;
            TutorialOverlay = new UIImageView( );
            TutorialOverlay.Layer.AnchorPoint = CGPoint.Empty;
            TutorialOverlay.Frame = View.Frame;
            TutorialOverlay.ContentMode = UIViewContentMode.ScaleAspectFit;
            TutorialOverlay.Alpha = 0.00f;
            View.AddSubview( TutorialOverlay );
        }

        void OnResultViewDone( )
        {
            // if they tap "Retry", well, retry!
            DeleteNote( );

            PrepareCreateNotes( );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // since we're reappearing, we know we're safe to reset our download count
            NoteDownloadRetries = MaxDownloadAttempts;
            Rock.Mobile.Util.Debug.WriteLine( "Resetting Download Attempts" );

            //PrepareCreateNotes( );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // monitor for text field being edited, and keyboard show/hide notitications
            NSObject handle = NSNotificationCenter.DefaultCenter.AddObserver( Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager.TextFieldDidBeginEditingNotification, KeyboardAdjustManager.OnTextFieldDidBeginEditing);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver( Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager.TextFieldChangedNotification, KeyboardAdjustManager.OnTextFieldChanged);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, KeyboardAdjustManager.OnKeyboardNotification);
            ObserverHandles.Add( handle );

            handle = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, KeyboardAdjustManager.OnKeyboardNotification);
            ObserverHandles.Add( handle );

            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Rock.Mobile.Util.Debug.WriteLine( "Turning idle timer OFF" );
        }

        /*public void OnTextFieldDidBeginEditing( NSNotification notification )
        {
            KeyboardAdjustManager.OnTextFieldDidBeginEditing( notification );

            // when a user note is edited, make sure that we allow enough scroll height to accomodate it.
            TryExpandHeightForUserNote( );
        }

        public void OnTextFieldChanged( NSNotification notification )
        {
            KeyboardAdjustManager.OnTextFieldChanged( notification );

            // when a user note is edited, make sure that we allow enough scroll height to accomodate it.
            // if we're going to grow

            // as they edit, we want to scroll with them so the bottom of the note doesn't go below the screne.
            // so take the current size of the scroll area
            nfloat contentSizeDelta = UIScrollView.ContentSize.Height;

            TryExpandHeightForUserNote( );

            // get the change, but clamp to 0. We want to follow them editing down, but not if they delete,
            // which will shrink the box
            contentSizeDelta = (nfloat)Math.Max( 0, (double)(UIScrollView.ContentSize.Height - contentSizeDelta) );

            // and update our scroll offset by that.
            UIScrollView.ContentOffset = new CGPoint( 0, UIScrollView.ContentOffset.Y + contentSizeDelta );
        }*/

        CGRect GetTappedTextFieldFrame( RectangleF textFrame )
        {
            // first subtract the amount scrolled by the view.
            nfloat yPos = textFrame.Y - UIScrollView.ContentOffset.Y;
            nfloat xPos = textFrame.X - UIScrollView.ContentOffset.X;

            // now add in however far down the scroll view is from the top.
            yPos += UIScrollView.Frame.Y;
            xPos += UIScrollView.Frame.X;

            return new CGRect( xPos, yPos, textFrame.Width, textFrame.Height );
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            foreach ( NSObject handle in ObserverHandles )
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver( handle );
            }

            ObserverHandles.Clear( );
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
            SaveNoteState( );

            DestroyNotes( );

            OrientationState = -1;

            UIApplication.SharedApplication.IdleTimerDisabled = false;
            Rock.Mobile.Util.Debug.WriteLine( "Turning idle timer ON" );
        }

        public void ShareNotes()
        {
            if ( Note != null )
            {
                string emailNote;
                Note.GetNotesForEmail( out emailNote );

                var items = new NSObject[] { new NSString( emailNote ) };

                UIActivityViewController shareController = new UIActivityViewController( items, null );

                string emailSubject = string.Format( App.Shared.Strings.MessagesStrings.Read_Share_Notes, NoteName );
                shareController.SetValueForKey( new NSString( emailSubject ), new NSString( "subject" ) );

                shareController.ExcludedActivityTypes = new NSString[] { UIActivityType.PostToFacebook, 
                                                                         UIActivityType.AirDrop, 
                                                                         UIActivityType.PostToTwitter, 
                                                                         UIActivityType.CopyToPasteboard,
                                                                         UIActivityType.Message };

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

            AnimateTutorialScreen( false );

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                if( Note != null )
                {
                    // should we visit a website?
                    string activeUrl = Note.TouchesEnded( touch.LocationInView( UIScrollView ).ToPointF( ) );
                    if ( string.IsNullOrEmpty( activeUrl ) == false )
                    {
                        SaveNoteState( );

                        DestroyNotes( );

                        TaskWebViewController viewController = new TaskWebViewController( activeUrl, Task );
                        Task.PerformSegue( this, viewController );
                    }
                }
            }

            // when a touch is released, re-enabled scrolling
            UIScrollView.ScrollEnabled = true;
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
                        Note.DidDoubleTap( tap.LocationInView( UIScrollView ).ToPointF( ) );
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

        public void PrepareCreateNotes( )
        {
            if( RefreshingNotes == false )
            {
                ResultView.Hide( );

                // if we're recreating the notes, reset our scrollview.
                UIScrollView.ContentOffset = CGPoint.Empty;

                RefreshingNotes = true;

                SaveNoteState( );

                DestroyNotes( );

                // show a busy indicator
                Indicator.StartAnimating( );

                Note.TryDownloadNote( NoteUrl, StyleSheetDefaultHostDomain, delegate(bool result )
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
                if( App.Shared.Network.RockMobileUser.Instance.NoteTutorialShownCount < PrivateNoteConfig.MaxTutorialDisplayCount )
                {
                    App.Shared.Network.RockMobileUser.Instance.NoteTutorialShownCount = App.Shared.Network.RockMobileUser.Instance.NoteTutorialShownCount + 1;

                    // wait a second before revealing the tutorial overlay
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.AutoReset = false;
                    timer.Interval = 750;
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    if( SpringboardViewController.IsLandscapeWide( ) )
                                    {
                                        TutorialOverlay.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + PrivateNoteConfig.TutorialOverlayImageIPadLS );
                                    }
                                    else
                                    {
                                        TutorialOverlay.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + PrivateNoteConfig.TutorialOverlayImage );
                                    }

                                    TutorialOverlay.ContentMode = UIViewContentMode.ScaleAspectFill;
                                    TutorialOverlay.Frame = View.Frame;

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
                        PrepareCreateNotes( );
                    }
                    else 
                    {
                        // we've tried as many times as we're going to. Give up and error.
                        if( e != null )
                        {
                            errorMsg += "\n" + e.Message;
                        }

                        if ( App.Shared.Network.RockGeneralData.Instance.Data.RefreshButtonEnabled == true )
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

