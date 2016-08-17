
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
using Android.Text;
using Android.Widget;
using App.Shared.Network;
using Android.Views.InputMethods;
using App.Shared.Strings;
using App.Shared.Config;
using Rock.Mobile.UI;
using Android.Telephony;
using Rock.Mobile.Util.Strings;
using Java.Lang.Reflect;
using App.Shared.UI;
using Rock.Mobile.Animation;
using Android.Graphics;

namespace Droid
{
    public class OOBEFragment : Fragment, View.IOnTouchListener
    {
        public Springboard SpringboardParent { get; set; }

        UIOOBE OOBEView { get; set; }

        bool DidLaunch { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            OOBEView = new UIOOBE( );

            RelativeLayout view = inflater.Inflate(Resource.Layout.OOBEView, container, false) as RelativeLayout;

            view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

            view.SetOnTouchListener( this );

            string imageName = "splash_logo_android.png";

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );

            DisplayMetrics metrics = Resources.DisplayMetrics;

            string bgImageName = string.Format( "oobe_splash_bg_{0}.png", metrics.DensityDpi.ToString( ).ToLower( ) );

            OOBEView.Create( view, bgImageName, imageName, false, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), 

                delegate(int index, bool isCampusSelection) 
                {
                    SpringboardParent.OOBEUserClick( index, isCampusSelection );
                } );

            return view;
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            // consume all input so things tasks underneath don't respond
            return true;
        }

        public override void OnResume()
        {
            base.OnResume();

            // guard against backgrounding / resuming
            if ( DidLaunch == false )
            {
                DidLaunch = true;

                SpringboardParent.ModalFragmentOpened( this );

                OOBEView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
            }
        }

        public void HandleNetworkFixed( )
        {
            OOBEView.HandleNetworkFixed( );
        }

        public void PerformStartup( bool networkSuccess )
        {
            OOBEView.PerformStartup( networkSuccess );
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            OOBEView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
        }

        public override void OnStop()
        {
            base.OnStop( );
            OOBEView.Destroy( );
        }
    }
}

