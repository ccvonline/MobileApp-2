
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
using Android.Media;
using App.Shared.Strings;
using Rock.Mobile.UI;
using App.Shared;
using App.Shared.Analytics;
using App.Shared.UI;
using App.Shared.Config;
using App.Shared.PrivateConfig;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            //todo:
            // either split the videoView and mediaPlayer out, and use this same class for audio or video.
            // alternatively, create a seperate NotesListenFragment, and use a mediaPlayer as service and more basic UI

            public class NotesWatchFragment : TaskFragment, Android.Media.MediaPlayer.IOnPreparedListener, Android.Media.MediaPlayer.IOnErrorListener, Android.Media.MediaPlayer.IOnSeekCompleteListener
            {
                VideoView VideoPlayer { get; set; }
                MediaController MediaController { get; set; }
                ProgressBar ProgressBar { get; set; }

                public string MediaUrl { get; set; }
                public string ShareUrl { get; set; }
                public string Name { get; set; }

                UIResultView ResultView { get; set; }

                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    MediaController = new MediaController( Rock.Mobile.PlatformSpecific.Android.Core.Context );

                    RelativeLayout view = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    view.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    view.SetBackgroundColor( Android.Graphics.Color.Black );
                    view.SetOnTouchListener( this );

                    VideoPlayer = new VideoView( Activity );
                    VideoPlayer.SetMediaController( MediaController );
                    VideoPlayer.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    ( (RelativeLayout.LayoutParams)VideoPlayer.LayoutParameters ).AddRule( LayoutRules.CenterInParent );

                    ( ( view as RelativeLayout ) ).AddView( VideoPlayer );

                    VideoPlayer.SetOnPreparedListener( this );
                    VideoPlayer.SetOnErrorListener( this );

                    ProgressBar = new ProgressBar( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ProgressBar.Indeterminate = true;
                    ProgressBar.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( 0 ) );
                    ProgressBar.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (RelativeLayout.LayoutParams)ProgressBar.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
                    view.AddView( ProgressBar );
                    ProgressBar.BringToFront();

                    ResultView = new UIResultView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), delegate { TryPlayMedia( ); } );

                    return view;
                }

                public void OnPrepared( MediaPlayer mp )
                {
                    // now that the video is ready we can hide the progress bar
                    ProgressBar.Visibility = ViewStates.Gone;

                    MediaController.SetAnchorView( VideoPlayer );

                    // setup a seek listener
                    mp.SetOnSeekCompleteListener( this );

                    // log the series they tapped on.
                    MessageAnalytic.Instance.Trigger( MessageAnalytic.Watch, Name );

                    // if this is a new video, store the URL
                    if ( App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaUrl != MediaUrl )
                    {
                        App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaUrl = MediaUrl;
                        VideoPlayer.Start( );

                        // once the video starts, if we're in landscape wide, go full screen
                        if ( MainActivity.IsLandscapeWide( ) )
                        {
                            ParentTask.NavbarFragment.ToggleFullscreen( true );
                        }
                    }
                    else
                    {
                        // otherwise, resume where we left off
                        mp.SeekTo( (int)App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaPos );
                    }
                }

                public void OnSeekComplete( MediaPlayer mp )
                {
                    VideoPlayer.Start( );

                    // once the video starts, if we're in landscape wide, go full screen
                    if ( MainActivity.IsLandscapeWide( ) )
                    {
                        ParentTask.NavbarFragment.ToggleFullscreen( true );
                    }
                }

                public bool OnError( MediaPlayer mp, MediaError error, int extra )
                {
                    ProgressBar.Visibility = ViewStates.Gone;

                    ResultView.Show( MessagesStrings.Error_Title, 
                        PrivateControlStylingConfig.Result_Symbol_Failed, 
                        MessagesStrings.Error_Watch_Playback,
                        GeneralStrings.Retry );

                    ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );

                    return true;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    // this is contrary to the other Note fragments, but here, if we can't do
                    // landscape, we want to ALLOW full sensor so the user can view the video in fullscreen
                    if ( MainActivity.SupportsLandscapeWide( ) == false )
                    {
                        Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;
                    }

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( true );

                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( true, delegate 
                        {
                            // Generate an email advertising this video.
                            Intent sendIntent = new Intent();
                            sendIntent.SetAction( Intent.ActionSend );

                            sendIntent.PutExtra( Intent.ExtraSubject, MessagesStrings.Watch_Share_Subject );

                            string noteString = MessagesStrings.Watch_Share_Header_Html + string.Format( MessagesStrings.Watch_Share_Body_Html, ShareUrl );

                            // if they set a mobile app url, add that.
                            if( string.IsNullOrEmpty( MessagesStrings.Watch_Mobile_App_Url ) == false )
                            {
                                noteString += string.Format( MessagesStrings.Watch_Share_DownloadApp_Html, MessagesStrings.Watch_Mobile_App_Url );
                            }

                            sendIntent.PutExtra( Intent.ExtraText, Android.Text.Html.FromHtml( noteString ) );
                            sendIntent.SetType( "text/html" );
                            StartActivity( sendIntent );
                        });

                    if ( string.IsNullOrEmpty( MediaUrl ) == true )
                    {
                        throw new Exception( "MediaUrl must be valid." );
                    }

                    TryPlayMedia( );
                }

                void TryPlayMedia( )
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    ResultView.Hide( );

                    VideoPlayer.StopPlayback( );
                    VideoPlayer.SetVideoURI( Android.Net.Uri.Parse( MediaUrl ) );
                    VideoPlayer.Pause( );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    ParentTask.NavbarFragment.ToggleFullscreen( false );

                    // and if we leave, and don't support landscape, put it back to portrait
                    if ( MainActivity.SupportsLandscapeWide( ) == false )
                    {
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( true );
                        Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                    }

                    // see if we should store the playback position for resuming
                    if ( VideoPlayer.Duration > 0 )
                    {
                        // if we're within 1 and 98 percent, do it
                        float playbackPerc = (float)VideoPlayer.CurrentPosition / (float)VideoPlayer.Duration;
                        if ( playbackPerc > .01f && playbackPerc < .98f )
                        {
                            App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaPos = VideoPlayer.CurrentPosition;
                        }
                        else
                        {
                            // otherwise plan on starting from the beginning
                            App.Shared.Network.RockMobileUser.Instance.LastStreamingMediaPos = 0;
                        }
                    }


                    // stop playback
                    VideoPlayer.StopPlayback( );
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    // if we're entering landscape (wide or regular, we don't care)
                    if( newConfig.Orientation == Android.Content.Res.Orientation.Landscape )
                    {
                        // go fullscreen
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( false );
                        ParentTask.NavbarFragment.ToggleFullscreen( true );
                        ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                    }
                    else
                    {
                        // if we're going portrait, turn off fullscreen
                        ParentTask.NavbarFragment.ToggleFullscreen( false );

                        // and if we're NOT in wide, enable the reveal button.
                        if ( MainActivity.IsLandscapeWide( ) == false )
                        {
                            ParentTask.NavbarFragment.EnableSpringboardRevealButton( true );
                        }
                    }

                    ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                }
            }
        }
    }
}

