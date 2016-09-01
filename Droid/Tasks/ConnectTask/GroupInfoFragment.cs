
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
            public class GroupInfoFragment : TaskFragment
            {
                public GroupFinder.GroupEntry GroupEntry { get; set; }

                UIGroupInfo GroupInfoView { get; set; }

                ScrollView ScrollView { get; set; }
                RelativeLayout ScrollViewLayout { get; set; }

                public GroupInfoFragment( ) : base( )
                {
                }
                
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

                    GroupInfoView = new UIGroupInfo();

                    View view = inflater.Inflate(Resource.Layout.GroupInfo, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    ScrollView = view.FindViewById<ScrollView>( Resource.Id.scroll_view );
                    ScrollViewLayout = ScrollView.FindViewById<RelativeLayout>( Resource.Id.view_background );
                    ScrollViewLayout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    GroupInfoView.Create( ScrollViewLayout, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), OnJoinClicked );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    GroupInfoView.DisplayView( GroupEntry, delegate 
                        {
                            LayoutChanged( );
                        });

                    LayoutChanged( );
                }

                public override void OnDestroyView()
                {
                    base.OnDestroyView();

                    GroupInfoView.Destroy( );
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    LayoutChanged( );
                }

                void LayoutChanged( )
                {
                    GroupInfoView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );

                    ScrollViewLayout.LayoutParameters.Height = (int)(GroupInfoView.GetControlBottom( ) * 1.05f);
                }

                void OnJoinClicked( )
                {
                    ParentTask.OnClick( this, 0, GroupEntry );
                }
            }
        }
    }
}
