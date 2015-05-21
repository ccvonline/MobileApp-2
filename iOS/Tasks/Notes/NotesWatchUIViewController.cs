using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using MediaPlayer;
using CoreGraphics;
using System.Collections.Generic;
using App.Shared.Strings;
using App.Shared;
using App.Shared.Analytics;
using App.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared.Config;
using App.Shared.PrivateConfig;

namespace iOS
{
	partial class NotesWatchUIViewController : TaskUIViewController
	{
        public string MediaUrl { get; set; }
        public string ShareUrl { get; set; }
        public string Name { get; set; }
        public bool AudioOnly { get; set; }

        MPMoviePlayerController MoviePlayer  { get; set; }
        UIActivityIndicatorView ActivityIndicator { get; set; }

        bool PreloadFinished { get; set; }

        bool DidDisplayError { get; set; }

        List<NSObject> ObserverHandles { get; set; }
        bool EnteringFullscreen { get; set; }
        bool ExitingFullscreen { get; set; }

        UIResultView ResultView { get; set; }

		public NotesWatchUIViewController ( )
		{
            ObserverHandles = new List<NSObject>( );
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // don't allow anything if there isn't a watchUrl set
            if ( MediaUrl == null )
            {
                throw new Exception( "MediaUrl must not be null!" );
            }

            // setup our activity indicator
            ActivityIndicator = new UIActivityIndicatorView();
            ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            ActivityIndicator.SizeToFit( );
            ActivityIndicator.StartAnimating( );

            PreloadFinished = false;

            // create the movie player control
            MoviePlayer = new MPMoviePlayerController( );
            View.AddSubview( MoviePlayer.View );

            View.AddSubview( ActivityIndicator );

            ResultView = new UIResultView( View, View.Frame.ToRectF( ), delegate { TryPlayMedia( ); } );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if ( ExitingFullscreen == false )
            {
                // setup a notification so we know when to hide the spinner

                NSObject handle = NSNotificationCenter.DefaultCenter.AddObserver( new NSString("MPMoviePlayerContentPreloadDidFinishNotification"), ContentPreloadDidFinish );
                ObserverHandles.Add( handle );

                // setup a notification so we know when they enter fullscreen, cause we'll need to play the movie again
                handle = NSNotificationCenter.DefaultCenter.AddObserver( MPMoviePlayerController.PlaybackStateDidChangeNotification, PlaybackStateDidChange );
                ObserverHandles.Add( handle );

                handle = NSNotificationCenter.DefaultCenter.AddObserver( MPMoviePlayerController.PlaybackDidFinishNotification, PlaybackDidFinish );
                ObserverHandles.Add( handle );


                // monitor our fullscreen status so we can manage a flag and ignore ViewDidAppear/ViewDidDisappear
                handle = NSNotificationCenter.DefaultCenter.AddObserver( MPMoviePlayerController.WillEnterFullscreenNotification, WillEnterFullscreen );
                ObserverHandles.Add( handle );

                handle = NSNotificationCenter.DefaultCenter.AddObserver( MPMoviePlayerController.DidEnterFullscreenNotification, DidEnterFullscreen );
                ObserverHandles.Add( handle );

                handle = NSNotificationCenter.DefaultCenter.AddObserver( MPMoviePlayerController.WillExitFullscreenNotification, WillExitFullscreen );
                ObserverHandles.Add( handle );

                handle = NSNotificationCenter.DefaultCenter.AddObserver( MPMoviePlayerController.DidExitFullscreenNotification, DidExitFullscreen );
                ObserverHandles.Add( handle );

                ResultView.Hide( );
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // don't do anything if we're simply exiting fullscreen
            if ( ExitingFullscreen == false )
            {
                TryPlayMedia( );
            }
            else
            {
                ActivityIndicator.Hidden = true;
            }
        }

        void TryPlayMedia( )
        {
            ResultView.Hide( );

            ActivityIndicator.Hidden = false;

            DidDisplayError = false;

            // if we're watching the same video we last watched, resume
            if ( MediaUrl == App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaUrl )
            {
                MoviePlayer.InitialPlaybackTime = App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaPos;
            }

            MoviePlayer.ContentUrl = new NSUrl( MediaUrl );
            MoviePlayer.PrepareToPlay( );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            // center the movie window.
            nfloat movieHeight = 0.00f;
            nfloat movieWidth = View.Frame.Width;

            // if we have the movie size, correctly resize it
            if ( MoviePlayer.NaturalSize.Width != 0 && MoviePlayer.NaturalSize.Height != 0 )
            {
                // fit the video into the width of the view
                nfloat aspectRatio = MoviePlayer.NaturalSize.Height / MoviePlayer.NaturalSize.Width;
                movieWidth = View.Frame.Width;
                movieHeight = movieWidth * aspectRatio;

                // if the height is still too large, scale the width down from what our height is
                if ( movieHeight > View.Frame.Height )
                {
                    aspectRatio = MoviePlayer.NaturalSize.Width / MoviePlayer.NaturalSize.Height;

                    movieHeight = View.Frame.Height;
                    movieWidth = View.Frame.Height * aspectRatio;
                }
            }
            else
            {
                // otherwise as a temporary measure, use half the viewing width
                movieWidth = View.Frame.Width;
                movieHeight = View.Frame.Height;
            }

            // center the movie frame and activity indicator
            MoviePlayer.View.Frame = new CGRect( (View.Frame.Width - movieWidth) / 2, (View.Frame.Height - movieHeight) / 2, movieWidth, movieHeight );

            ActivityIndicator.Layer.Position = new CGPoint( ( View.Frame.Width - ActivityIndicator.Frame.Width ) / 2, 
                                                            ( View.Frame.Height - ActivityIndicator.Frame.Height ) / 2 );

            // landscape wide devices MAY show the nav toolbar
            if ( SpringboardViewController.IsLandscapeWide( ) == true )
            {
                //Task.NavToolbar.RevealForTime( 3.0f );
                Task.NavToolbar.Reveal( true );
                Task.NavToolbar.SetBackButtonEnabled( true );
            }
            // landscape non-wide devices should not
            else if ( SpringboardViewController.IsDeviceLandscape( ) == true )
            {
                Task.NavToolbar.Reveal( false );
            }
            else
            {
                //Task.NavToolbar.RevealForTime( 3.0f );
                Task.NavToolbar.Reveal( true );
                Task.NavToolbar.SetBackButtonEnabled( true );
            }

            ResultView.SetBounds( View.Frame.ToRectF( ) );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // only process this if we're not entering fullscreen
            if ( EnteringFullscreen == false )
            {
                MoviePlayer.Stop( );

                foreach ( NSObject handle in ObserverHandles )
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver( handle );
                }

                ObserverHandles.Clear( );
            }
        }

        public override void AppOnResignActive()
        {
            base.AppOnResignActive();

            SavePlaybackPos( );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate();

            SavePlaybackPos( );
        }

        public void ShareVideo( )
        {
            string noteString = MessagesStrings.Watch_Share_Header_Html + string.Format( MessagesStrings.Watch_Share_Body_Html, ShareUrl );

            // if they set a mobile app url, add that.
            if( string.IsNullOrEmpty( MessagesStrings.Watch_Mobile_App_Url ) == false )
            {
                noteString += string.Format( MessagesStrings.Watch_Share_DownloadApp_Html, MessagesStrings.Watch_Mobile_App_Url );
            }

            var items = new NSObject[] { new NSString( noteString ) };

            UIActivityViewController shareController = new UIActivityViewController( items, null );
            shareController.SetValueForKey( new NSString( MessagesStrings.Watch_Share_Subject ), new NSString( "subject" ) );

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

        void ContentPreloadDidFinish( NSNotification obj )
        {
            // once the movie is ready, hide the spinner
            ActivityIndicator.Hidden = true;

            MoviePlayer.Play( );

            // now that the content is preloaded, update our layout so that
            // we size the window according to the video dimensions.
            LayoutChanged( );

            if ( AudioOnly )
            {
                MessageAnalytic.Instance.Trigger( MessageAnalytic.Listen, Name );
            }
            else
            {
                MessageAnalytic.Instance.Trigger( MessageAnalytic.Watch, Name );
            }
        }

        void WillEnterFullscreen( NSNotification obj )
        {
            EnteringFullscreen = true;
        }

        void DidEnterFullscreen( NSNotification obj )
        {
            EnteringFullscreen = false;
        }

        void WillExitFullscreen( NSNotification obj )
        {
            ExitingFullscreen = true;
        }

        void DidExitFullscreen( NSNotification obj )
        {
            ExitingFullscreen = false;
        }

        void PlaybackStateDidChange( NSNotification obj )
        {
            DidDisplayError = false;

            if ( MoviePlayer.PlaybackState != MPMoviePlaybackState.Playing )
            {
                SavePlaybackPos( );
            }
        }

        void SavePlaybackPos( )
        {
            // store the last video we watched.
            App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaUrl = MediaUrl;

            // see where we are in playback. If it's > 1 and < 98, we'll save the time.
            if ( MoviePlayer.Duration > 0.00f )
            {
                double playbackPerc = MoviePlayer.CurrentPlaybackTime / MoviePlayer.Duration;
                if ( playbackPerc > .01f && playbackPerc < .98f )
                {
                    App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaPos = MoviePlayer.CurrentPlaybackTime;
                }
                else
                {
                    App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaPos = 0;
                }
            }
        }

        void PlaybackDidFinish( NSNotification obj )
        {
            // watch for any playback errors. This would include failing to play the video in the first place.
            int error = (obj.UserInfo[ "MPMoviePlayerPlaybackDidFinishReasonUserInfoKey"] as NSNumber).Int32Value;

            // if there WAS an error, report it to the user. Watch our error flag so we don't show the error
            // more than once
            if( (int)MPMovieFinishReason.PlaybackError == error && DidDisplayError == false )
            {
                DisplayError( );
            }
        }

        void DisplayError( )
        {
            DidDisplayError = true;

            ResultView.Show( MessagesStrings.Error_Title, 
                PrivateControlStylingConfig.Result_Symbol_Failed, 
                MessagesStrings.Error_Watch_Playback,
                GeneralStrings.Retry );

            MoviePlayer.Stop( );
            ActivityIndicator.Hidden = true;
        }
	}
}
