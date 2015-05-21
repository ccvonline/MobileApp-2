
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
using Android.Webkit;

using App.Shared.Config;
using App.Shared.Strings;
using Android.Graphics;
using Android.Gms.Maps;
using App.Shared;
using App.Shared.Network;
using App.Shared.Analytics;
using Rock.Mobile.Animation;
using Android.Gms.Maps.Model;
using App.Shared.UI;
using Android.Telephony;
using System.Drawing;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class JoinGroupFragment : TaskFragment
            {
                public string GroupTitle { get; set; }
                public string Distance { get; set; }
                public string MeetingTime { get; set; }
                public int GroupID { get; set; }

                UIJoinGroup JoinGroupView { get; set; }
                
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

                    JoinGroupView = new UIJoinGroup();

                    View view = inflater.Inflate(Resource.Layout.JoinGroup, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    RelativeLayout backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.view_background );

                    JoinGroupView.Create( backgroundView, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );


                    // get the native object types so we can hook in necessary support pointers
                    ((View)JoinGroupView.View.PlatformNativeObject).SetOnTouchListener( this );
                    ((EditText)JoinGroupView.CellPhone.PlatformNativeObject).AddTextChangedListener(new PhoneNumberFormattingTextWatcher());

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    JoinGroupView.DisplayView( GroupTitle, MeetingTime, Distance, GroupID );
                    JoinGroupView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    JoinGroupView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                }
            }
        }
    }
}
