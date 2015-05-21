using System;
using UIKit;
using CoreGraphics;
using Foundation;
using App.Shared.Config;
using App.Shared.Network;
using App.Shared.PrivateConfig;

namespace iOS
{
    public class NotesTask : Task
    {
        NotesMainUIViewController MainViewController { get; set; }
        public NotesViewController NoteController { get; set; }

        public NotesTask( string storyboardName ) : base( storyboardName )
        {
            MainViewController = Storyboard.InstantiateViewController( "MainPageViewController" ) as NotesMainUIViewController;
            MainViewController.Task = this;

            // create the note controller ONCE and let the view controllers grab it as needed.
            // That way, we can hold it in memory and cache notes, rather than always reloading them.
            NoteController = new NotesViewController();
            NoteController.Task = this;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar, CGRect containerBounds )
        {
            base.MakeActive( parentViewController, navToolbar, containerBounds );

            MainViewController.View.Bounds = containerBounds;
            NoteController.View.Bounds = containerBounds;

            parentViewController.PushViewController( MainViewController, false );
        }

        public override void PerformAction( string action )
        {
            base.PerformAction( action );

            switch( action )
            {
                case PrivateGeneralConfig.TaskAction_NotesRead:
                {
                    if ( App.Shared.Network.RockLaunchData.Instance.Data.NoteDB.SeriesList.Count > 0 )
                    {
                        // since we're switching to the read notes VC, pop to the main page root and 
                        // remove it, because we dont' want back history (where would they go back to?)
                        ParentViewController.ClearViewControllerStack( );

                        NoteController.NoteName = RockLaunchData.Instance.Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].Name;
                        NoteController.NoteUrl = RockLaunchData.Instance.Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].NoteUrl;
                        NoteController.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                        ParentViewController.PushViewController( NoteController, false );
                    }
                    break;
                }

                case PrivateGeneralConfig.TaskAction_NotesDownloadImages:
                {
                    //MainViewController.DownloadImages( );
                    break;
                }
            }
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );
        }

        public bool IsReading( )
        {
            return ActiveViewController as NotesViewController != null ? true : false;
        }

        public override void WillShowViewController(TaskUIViewController viewController)
        {
            base.WillShowViewController( viewController );

            // if we're coming from WebView or Notes and going to something else,
            // force the device back to portrait

            // if the notes are active, make sure the share button gets turned on
            if ( ( viewController as NotesViewController ) != null )
            {
                // Let the view controller manage this being enabled, because
                // it's conditional on being in landscape or not.
                NavToolbar.SetCreateButtonEnabled( false, null );
                NavToolbar.SetShareButtonEnabled( true, delegate
                    { 
                        ( viewController as NotesViewController ).ShareNotes( );
                    } );


                // go ahead and show the bar, because we're at the top of the page.
                NavToolbar.Reveal( true );
            }
            else if ( ( viewController as NotesWatchUIViewController ) != null )
            {
                // Let the view controller manage this being enabled, because
                // it's conditional on being in landscape or not.
                NavToolbar.SetCreateButtonEnabled( false, null );
                NavToolbar.SetShareButtonEnabled( true, delegate
                    { 
                        ( viewController as NotesWatchUIViewController ).ShareVideo( );
                    } );
            }
            else if ( ( viewController as NotesDetailsUIViewController ) != null )
            {
                NavToolbar.SetCreateButtonEnabled( false, null );
                NavToolbar.SetShareButtonEnabled( false, null );
                //NavToolbar.RevealForTime( 3.0f );
                NavToolbar.Reveal( true );
            }
            else if ( ( viewController as NotesMainUIViewController ) != null )
            {
                NavToolbar.SetCreateButtonEnabled( false, null );
                NavToolbar.SetShareButtonEnabled( false, null );
                NavToolbar.Reveal( false );
            }
            else if ( ( viewController as TaskWebViewController ) != null )
            {
                NavToolbar.SetCreateButtonEnabled( false, null );
                NavToolbar.SetShareButtonEnabled( false, null );
            }
        }

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);

            // immediately hide the toolbar on the main page
            if ( ActiveViewController == MainViewController )
            {
                NavToolbar.Reveal( false );
            }
            // allow it as long as it's the watch window in portrait mode or landscape wide mode.
            else if ( ( ActiveViewController as NotesWatchUIViewController ) != null )
            {
                if ( SpringboardViewController.IsLandscapeWide( ) || SpringboardViewController.IsDevicePortrait( ) )
                {
                    //NavToolbar.RevealForTime( 3.0f );
                }
            }
        }

        public override bool CanContainerPan( NSSet touches, UIEvent evt )
        {
            NotesViewController notesVC = ActiveViewController as NotesViewController;
            if ( notesVC != null )
            {
                //return the inverse of touching a user note's bool.
                // so false if they ARE touching a note, true if they are not.
                return !notesVC.TouchingUserNote( touches, evt );
            }

            // if the notes aren't active, then of course they can pan
            return true;
        }

        public override void OnActivated( )
        {
            base.OnActivated( );

            ActiveViewController.OnActivated( );
        }

        public override void WillEnterForeground()
        {
            base.WillEnterForeground();

            ActiveViewController.WillEnterForeground( );
        }

        public override void AppOnResignActive()
        {
            base.AppOnResignActive( );

            ActiveViewController.AppOnResignActive( );
        }

        public override void AppDidEnterBackground( )
        {
            base.AppDidEnterBackground( );

            ActiveViewController.AppDidEnterBackground( );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate( );

            ActiveViewController.AppWillTerminate( );
        }

        public override bool SupportsLandscape()
        {
            // if we're using the watch or notes controller, allow landscape
            if ( ( ActiveViewController as NotesViewController ) != null || 
                 ( ActiveViewController as NotesWatchUIViewController ) != null || 
                 ( ActiveViewController as TaskWebViewController ) != null )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool ShouldForceBackButtonEnabled( )
        {
            // otherwise, the one exception is if the webview is open
            if ( ( ActiveViewController as TaskWebViewController ) != null )
            {
                return true;
            }

            // if it isn't, we shouldn't allow it
            return false;
        }

        public static string FormatBillboardImageName( string seriesName )
        {
            return seriesName + "_bb";
        }

        public static string FormatThumbImageName( string seriesName )
        {
            return seriesName + "_thumb";
        }
    }
}
