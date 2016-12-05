
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
using Droid.Tasks;

namespace Droid
{
    public class JingleFragment : TaskFragment
    {
        public Springboard SpringboardParent { get; set; }

        UIJingle JingleView { get; set; }

        public View ContainerView { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            JingleView = new UIJingle( );

            RelativeLayout view = inflater.Inflate(Resource.Layout.OOBEView, container, false) as RelativeLayout;

            view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

            view.SetOnTouchListener( this );

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );

            JingleView.Create( view, "jingle_bells_post.jpg", new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );

            return view;
        }

        public override bool OnTouch( View v, MotionEvent e )
        {
            // consume all input so things tasks underneath don't respond
            return true;
        }

        public override void OnResume()
        {
            base.OnResume();

            MainActivity.JingleBellsEnabled = true;

            if ( ParentTask.TaskReadyForFragmentDisplay == true && View != null )
            {
                JingleView.LoadResources( );
            }

            // update the layout AFTER loading resources, so the image can position correctly
            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            JingleView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
        }

        public override void TaskReadyForFragmentDisplay()
        {
            base.TaskReadyForFragmentDisplay();

            MainActivity.JingleBellsEnabled = true;

            // do not setup display if the task was ready but WE aren't.
            if ( View != null )
            {
                JingleView.LoadResources( );

                // update the layout AFTER loading resources, so the image can position correctly
                Point displaySize = new Point( );
                Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
                JingleView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
            }
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            JingleView.LayoutChanged( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
        }

        public override void OnPause()
        {
            base.OnPause();

            JingleView.FreeResources( );
            MainActivity.JingleBellsEnabled = false;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            JingleView.FreeResources( );
            MainActivity.JingleBellsEnabled = false;
        }
    }
}

