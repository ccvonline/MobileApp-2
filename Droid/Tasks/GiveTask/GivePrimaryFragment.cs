
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
using App.Shared.Config;
using App.Shared.Strings;

namespace Droid
{
    namespace Tasks
    {
        namespace Give
        {
            public class GivePrimaryFragment : TaskFragment, Android.Media.MediaPlayer.IOnPreparedListener
            {
                VideoView VideoPlayer { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.Give_Primary, container, false);
                    view.SetOnTouchListener( this );


                    RelativeLayout headerLayer = view.FindViewById<RelativeLayout>( Resource.Id.background );
                    ControlStyling.StyleBGLayer( headerLayer );

                    TextView headerLabel = view.FindViewById<TextView>( Resource.Id.headerLabel );
                    headerLabel.Text = GiveStrings.Header;
                    ControlStyling.StyleUILabel( headerLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );

                    Button giveButton = view.FindViewById<Button>( Resource.Id.button );
                    ControlStyling.StyleButton( giveButton, GiveStrings.ButtonLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );

                    giveButton.Click += (object sender, EventArgs e ) =>
                    {
                        LaunchGive( );
                    };

                    return view;
                }

                public void LaunchGive( )
                {
                    // when give is clicked, launch the browser
                    var uri = Android.Net.Uri.Parse( GiveConfig.GiveUrl );
                    var intent = new Intent( Intent.ActionView, uri ); 
                    ((Activity)Rock.Mobile.PlatformSpecific.Android.Core.Context).StartActivity( intent );
                }

                public void OnPrepared( MediaPlayer mp )
                {
                    VideoPlayer.Start( );
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                }
            }
        }
    }
}

