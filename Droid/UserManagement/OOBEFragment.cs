
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

            string imageName = "oobe_splash_logo.png";

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );

            OOBEView.Create( view, "oobe_splash_bg.png", imageName, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), 

                delegate(int index) 
                {
                    SpringboardParent.OOBEUserClick( index );
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
            
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 750;
            timer.AutoReset = false;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                {
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            OOBEView.PerformStartup( );
                        } );
                };
            timer.Start( );

            SpringboardParent.ModalFragmentOpened( this );

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            OOBEView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
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

