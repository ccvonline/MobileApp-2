
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
    public class SplashFragment : Fragment, View.IOnTouchListener
    {
        public Springboard SpringboardParent { get; set; }

        UISplash SplashView { get; set; }

        public View ContainerView { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            SplashView = new UISplash( );

            RelativeLayout view = inflater.Inflate(Resource.Layout.OOBEView, container, false) as RelativeLayout;

            view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

            view.SetOnTouchListener( this );

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );

            SplashView.Create( view, "", "oobe_splash_logo.png", new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ), 
                delegate
                {
                    SpringboardParent.SplashComplete( );
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
                            SplashView.PerformStartup( );

                            SimpleAnimator_Float viewAlphaAnim = new SimpleAnimator_Float( ContainerView.Alpha, 0.00f, .25f, delegate(float percent, object value )
                                {
                                    ContainerView.Alpha = (float)value;
                                },
                                null );
                            viewAlphaAnim.Start( );
                        } );
                };
            timer.Start( );

            SpringboardParent.ModalFragmentOpened( this );

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            SplashView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            SplashView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
        }

        public override void OnStop()
        {
            base.OnStop( );
            SplashView.Destroy( );
        }
    }
}

