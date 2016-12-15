
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
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesDiscGuideFragment : TaskFragment
            {
                UINoteDiscGuideView NoteDiscGuideView { get; set; }
                public string DiscGuideURL { get; set; }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    RelativeLayout view = inflater.Inflate(Resource.Layout.OOBEView, container, false) as RelativeLayout;

                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );
                    view.SetOnTouchListener( this );

                    Point displaySize = new Point( );
                    Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
                    NoteDiscGuideView = new UINoteDiscGuideView( view, new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ), delegate 
                    {
                        ParentTask.OnClick( this, 3, null );    
                    });

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

                    // update the layout AFTER loading resources, so the image can position correctly
                    Point displaySize = new Point( );
                    Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
                    NoteDiscGuideView.SetBounds( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
                }

                public override void TaskReadyForFragmentDisplay()
                {
                    base.TaskReadyForFragmentDisplay();

                    // do not setup display if the task was ready but WE aren't.
                    if ( View != null )
                    {
                        // update the layout AFTER loading resources, so the image can position correctly
                        Point displaySize = new Point( );
                        Activity.WindowManager.DefaultDisplay.GetSize( displaySize );

                        NoteDiscGuideView.SetBounds( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
                    }
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    Point displaySize = new Point( );
                    Activity.WindowManager.DefaultDisplay.GetSize( displaySize );

                    NoteDiscGuideView.SetBounds( new System.Drawing.RectangleF( 0, 0, displaySize.X, displaySize.Y ) );
                }
            }
        }
    }
}

