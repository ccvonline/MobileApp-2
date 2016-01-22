
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
using Android.Graphics;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.UI.DroidNative;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using App.Shared;
using Rock.Mobile.PlatformSpecific.Android.UI;
using App.Shared.PrivateConfig;
using Rock.Mobile;
using System.Threading;
using Rock.Mobile.PlatformSpecific.Android.Util;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class ArrayAdapter : ListAdapter
            {
                ConnectPrimaryFragment ParentFragment { get; set; }

                public ArrayAdapter( ConnectPrimaryFragment parentFragment ) : base ( )
                {
                    ParentFragment = parentFragment;
                }

                public override int Count 
                {
                    get 
                    { 
                        return ParentFragment.GetEngagedEntries.Count + 1;
                    }
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    ListItemView returnedView = null;

                    // if 0, return the primary header view
                    if ( position == 0 )
                    {
                        returnedView = GetPrimaryView( convertView, parent );
                    }
                    else
                    {
                        // otherwise, see if this is an item in the 'GetEngaged' list
                        int getEngagedRowIndex = position - 1;
                            
                        returnedView = GetStandardView( ParentFragment.GetEngagedEntries, 
                                                        ParentFragment.GetEngagedBillboards,
                                                        getEngagedRowIndex,
                                                        convertView, 
                                                        parent, 
                                                        true );
                        
                    }

                    // guard against not creating a row item
                    if ( returnedView == null )
                    {
                        throw new NullReferenceException();
                    }
                    return base.AddView( returnedView );
                }

                ListItemView GetPrimaryView( View convertView, ViewGroup parent )
                {
                    PrimaryListItem primaryItem = convertView as PrimaryListItem;
                    if ( primaryItem == null )
                    {
                        primaryItem = new PrimaryListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );

                        int height = (int)System.Math.Ceiling( NavbarFragment.GetCurrentContainerDisplayWidth( ) * PrivateConnectConfig.MainPageHeaderAspectRatio );
                        primaryItem.Billboard.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, height );
                        primaryItem.HasImage = false;
                    }
                    else
                    {
                        primaryItem.FreeImageResources( );
                    }

                    if ( ParentFragment.Billboard != null )
                    {
                        if ( primaryItem.HasImage == false )
                        {
                            primaryItem.HasImage = true;
                            Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( primaryItem.Billboard, true, null );
                        }
                        
                        primaryItem.Billboard.SetImageBitmap( ParentFragment.Billboard );
                    }

                    return primaryItem;
                }

                ListItemView GetStandardView( List<ConnectLink> linkEntries, Bitmap[] linkBillboards, int position, View convertView, ViewGroup parent, bool showSeperator )
                {
                    ListItem seriesItem = convertView as ListItem;
                    if ( seriesItem == null )
                    {
                        seriesItem = new ListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        seriesItem.HasImage = false;
                    }
                    else
                    {
                        seriesItem.FreeImageResources( );
                    }

                    // validate that the billboard needed exists. It could still be loading.
                    if ( position < linkBillboards.Count( ) && linkBillboards[ position ] != null )
                    {
                        if ( seriesItem.HasImage == false )
                        {
                            seriesItem.HasImage = true;
                            Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( seriesItem.Thumbnail, true, null );
                        }
                            
                        seriesItem.Thumbnail.SetImageBitmap( linkBillboards[ position ] );
                        seriesItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    }

                    seriesItem.Title.Text = linkEntries[ position ].Title.ToUpper( );
                    seriesItem.SubTitle.Text = linkEntries[ position ].SubTitle;

                    if ( showSeperator )
                    {
                        seriesItem.Seperator.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        seriesItem.Seperator.Visibility = ViewStates.Gone;
                    }

                    return seriesItem;
                }
            }

            public class PrimaryListItem : Rock.Mobile.PlatformSpecific.Android.UI.ListAdapter.ListItemView
            {
                // stuff that will be set by data
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Billboard { get; set; }
                public TextView Title { get; set; }
                //

                LinearLayout ButtonLayout { get; set; }

                public bool HasImage { get; set; }

                public PrimaryListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Android.Widget.Orientation.Vertical;

                    Billboard = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Billboard.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Billboard );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Title.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    Title.Text = ConnectStrings.Main_Connect_Header;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 2 );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 15 );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).BottomMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 2 );
                    AddView( Title );
                }

                public override void Destroy( )
                {
                    FreeImageResources( );
                }

                public void FreeImageResources( )
                {
                    if ( Billboard != null && Billboard.Drawable != null )
                    {
                        Billboard.Drawable.Dispose( );
                        Billboard.SetImageBitmap( null );
                    }
                }
            }

            public class SeperatorListItem : Rock.Mobile.PlatformSpecific.Android.UI.ListAdapter.ListItemView
            {
                public TextView Title { get; set; }

                public SeperatorListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Android.Widget.Orientation.Vertical;

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Title.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    Title.Text = ConnectStrings.Main_Connect_Seperator;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 2 );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 15 );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).BottomMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 2 );
                    AddView( Title );
                }

                public override void Destroy( )
                {
                }
            }

            public class ListItem : Rock.Mobile.PlatformSpecific.Android.UI.ListAdapter.ListItemView
            {
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Thumbnail { get; set; }

                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView SubTitle { get; set; }
                public TextView Chevron { get; set; }
                public View Seperator { get; set; }
                public bool HasImage { get; set; }

                public ListItem( Context context ) : base( context )
                {
                    Orientation = Android.Widget.Orientation.Vertical;

                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    LinearLayout contentLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    contentLayout.Orientation = Android.Widget.Orientation.Horizontal;
                    contentLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    AddView( contentLayout );

                    Thumbnail = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Thumbnail.LayoutParameters = new LinearLayout.LayoutParams( (int)Rock.Mobile.Graphics.Util.UnitToPx( PrivateConnectConfig.MainPage_ThumbnailDimension ), (int)Rock.Mobile.Graphics.Util.UnitToPx( PrivateConnectConfig.MainPage_ThumbnailDimension ) );
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).TopMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 15 );
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).BottomMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 15 );
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    contentLayout.AddView( Thumbnail );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TitleLayout.Orientation = Android.Widget.Orientation.Vertical;
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 15 );
                    contentLayout.AddView( TitleLayout );



                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Title.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 2 );
                    TitleLayout.AddView( Title );

                    SubTitle = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    SubTitle.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    SubTitle.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    SubTitle.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    SubTitle.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    ( (LinearLayout.LayoutParams)SubTitle.LayoutParameters ).TopMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( -4 );
                    ( (LinearLayout.LayoutParams)SubTitle.LayoutParameters ).BottomMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 2 );
                    TitleLayout.AddView( SubTitle );

                    // fill the remaining space with a dummy view, and that will align our chevron to the right
                    View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    contentLayout.AddView( dummyView );

                    Chevron = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Chevron.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Chevron.LayoutParameters ).Gravity = GravityFlags.CenterVertical | GravityFlags.Right;
                    Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );
                    Chevron.SetTypeface(  fontFace, TypefaceStyle.Normal );
                    Chevron.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateConnectConfig.MainPage_Table_IconSize );
                    Chevron.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Chevron.Text = PrivateConnectConfig.MainPage_Table_Navigate_Icon;
                    contentLayout.AddView( Chevron );

                    // add our own custom seperator at the bottom
                    Seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    Seperator.LayoutParameters.Height = 2;
                    Seperator.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    AddView( Seperator );
                }

                public override void Destroy( )
                {
                    FreeImageResources( );
                }

                public void FreeImageResources( )
                {
                    if ( Thumbnail != null && Thumbnail.Drawable != null )
                    {
                        Thumbnail.Drawable.Dispose( );
                        Thumbnail.SetImageBitmap( null );
                    }
                }
            }

            public class ConnectPrimaryFragment : TaskFragment
            {
                bool FragmentActive { get; set; }
                ListView ListView { get; set; }

                public Bitmap Billboard { get; set; }

                public List<ConnectLink> GetEngagedEntries { get; set; }

                public Bitmap [] GetEngagedBillboards { get; set; }

                public ConnectPrimaryFragment( ) : base( )
                {
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.Connect_Primary, container, false);
                    view.SetOnTouchListener( this );


                    GetEngagedEntries = ConnectLink.BuildGetEngagedList( );

                    GetEngagedBillboards = new Bitmap[ GetEngagedEntries.Count ];

                    ListView = view.FindViewById<ListView>( Resource.Id.connect_primary_list );

                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e ) =>
                        {
                            // ignore clicks to the top banner
                            if( e.Position > 0 )
                            {
                                // get the row index relative to getEngaged
                                int getEngagedRowIndex = e.Position - 1;

                                // if they clicked a non-groupfinder row, get the link they want to visit
                                ParentTask.OnClick( this, getEngagedRowIndex, GetEngagedEntries[ getEngagedRowIndex ] );
                                
                            }
                        };
                    ListView.SetOnTouchListener( this );
                    ListView.Adapter = new ArrayAdapter( this );

                    return view;
                }

                public override void TaskReadyForFragmentDisplay()
                {
                    base.TaskReadyForFragmentDisplay();

                    // do not setup display if the task was ready but WE aren't.
                    if ( View != null )
                    {
                        SetupDisplay( );
                    }
                }

                void SetupDisplay( )
                {
                    // load the top banner
                    AsyncLoader.LoadImage( PrivateConnectConfig.MainPageHeaderImage, true, true,
                        delegate( Bitmap imageBmp )
                        {
                            if( FragmentActive == true && imageBmp != null )
                            {
                                Billboard = imageBmp;

                                ((ListAdapter)ListView.Adapter).NotifyDataSetChanged( );   

                                return true;
                            }
                            return false;
                        } );


                    // load the thumbnails
                    for( int i = 0; i < GetEngagedEntries.Count; i++ )
                    {
                        int imageIndex = i;

                        AsyncLoader.LoadImage( GetEngagedEntries[ i ].ImageName, true, false,
                            delegate( Bitmap imageBmp )
                            {
                                if( FragmentActive == true && imageBmp != null )
                                {
                                    GetEngagedBillboards[ imageIndex ] = imageBmp;

                                    ((ListAdapter)ListView.Adapter).NotifyDataSetChanged( );   
                                    return true;
                                }
                                return false;
                            } );
                    }
                }

                public override void OnResume()
                {
                    base.OnResume();

                    FragmentActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    if ( ParentTask.TaskReadyForFragmentDisplay == true && View != null )
                    {
                        SetupDisplay( );
                    }
                }

                public override void OnPause( )
                {
                    base.OnPause( );

                    FragmentActive = false;

                    FreeImageResources( );
                }

                public override void OnDestroyView()
                {
                    base.OnDestroyView();

                    FreeImageResources( );
                }

                void FreeImageResources( )
                {
                    if ( ListView != null && ListView.Adapter != null )
                    {
                        ( (ArrayAdapter)ListView.Adapter ).Destroy( );
                    }

                    // free bmp resources
                    if ( Billboard != null )
                    {
                        Billboard.Dispose( );
                        Billboard = null;
                    }

                    foreach ( Bitmap image in GetEngagedBillboards )
                    {
                        if ( image != null )
                        {
                            image.Dispose( );
                        }
                    }
                }
            }
        }
    }
}

