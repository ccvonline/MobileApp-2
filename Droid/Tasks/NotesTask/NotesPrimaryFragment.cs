
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
using App.Shared.Network;
using Android.Graphics;
using RestSharp;
using Rock.Mobile.Network;
using App.Shared.Notes.Model;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.UI;
using System.Net;
using System.IO;
using App.Shared;
using Rock.Mobile.UI.DroidNative;
using System.Threading;
using App.Shared.UI;
using Rock.Mobile.PlatformSpecific.Android.UI;
using App.Shared.PrivateConfig;
using Rock.Mobile.IO;
using Rock.Mobile.PlatformSpecific.Android.Util;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesArrayAdapter : ListAdapter
            {
                NotesPrimaryFragment ParentFragment { get; set; }

                public NotesArrayAdapter( NotesPrimaryFragment parentFragment )
                {
                    ParentFragment = parentFragment;
                }

                public override int Count 
                {
                    get { return ParentFragment.SeriesEntries.Count + 1; }
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    ListItemView newItem = null;

                    if ( position == 0 )
                    {
                        newItem = GetPrimaryView( convertView, parent );
                    }
                    else
                    {
                        newItem = GetStandardView( position - 1, convertView, parent );
                    }

                    return AddView( newItem );
                }

                ListItemView GetPrimaryView( View convertView, ViewGroup parent )
                {
                    SeriesPrimaryListItem primaryItem = convertView as SeriesPrimaryListItem;
                    if ( primaryItem == null )
                    {
                        primaryItem = new SeriesPrimaryListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );

                        int height = (int)System.Math.Ceiling( NavbarFragment.GetContainerDisplayWidth( ) * PrivateNoteConfig.NotesMainPlaceholderAspectRatio );
                        primaryItem.Billboard.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, height );
                        primaryItem.HasImage = false;
                    }

                    primaryItem.ParentAdapter = this;

                    if ( ParentFragment.SeriesEntries.Count > 0 )
                    {
                        if ( ParentFragment.SeriesEntries[ 0 ].Billboard != null )
                        {
                            if ( primaryItem.HasImage == false )
                            {
                                primaryItem.HasImage = true;
                                Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( primaryItem.Billboard, true, null );
                            }

                            primaryItem.Billboard.SetImageBitmap( ParentFragment.SeriesEntries[ 0 ].Billboard );
                            primaryItem.Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );
                        }
                        else if ( ParentFragment.ImageMainPlaceholder != null )
                        {
                            primaryItem.Billboard.SetImageBitmap( ParentFragment.ImageMainPlaceholder );
                        }

                        // verify there are messages to show
                        if ( ParentFragment.SeriesEntries[ 0 ].Series.Messages.Count > 0 )
                        {
                            primaryItem.Title.Text = ParentFragment.SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                            primaryItem.Speaker.Text = ParentFragment.SeriesEntries[ 0 ].Series.Messages[ 0 ].Speaker;
                            primaryItem.Date.Text = ParentFragment.SeriesEntries[ 0 ].Series.Messages[ 0 ].Date;

                            // toggle the Take Notes button
                            if ( string.IsNullOrEmpty( ParentFragment.SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl ) == false )
                            {
                                primaryItem.ToggleTakeNotesButton( true );
                            }
                            else
                            {
                                primaryItem.ToggleTakeNotesButton( false );
                            }

                            // toggle the Watch button
                            if ( string.IsNullOrEmpty( ParentFragment.SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl ) == false )
                            {
                                primaryItem.ToggleWatchButton( true );
                            }
                            else
                            {
                                primaryItem.ToggleWatchButton( false );
                            }
                        }
                        else
                        {
                            primaryItem.ToggleTakeNotesButton( false );
                            primaryItem.ToggleWatchButton( false );
                        }
                    }

                    return primaryItem;
                }

                ListItemView GetStandardView( int position, View convertView, ViewGroup parent )
                {
                    SeriesListItem seriesItem = convertView as SeriesListItem;
                    if ( seriesItem == null )
                    {
                        seriesItem = new SeriesListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        seriesItem.HasImage = false;
                    }

                    // make sure we don't somehow attempt to render outside our list bounds
                    // this could happen if the list gets unloaded during the last render
                    if ( position < ParentFragment.SeriesEntries.Count )
                    {
                        if ( ParentFragment.SeriesEntries[ position ].Thumbnail != null )
                        {
                            if ( seriesItem.HasImage == false )
                            {
                                seriesItem.HasImage = true;
                                Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( seriesItem.Thumbnail, true, null );
                            }
                            
                            seriesItem.Thumbnail.SetImageBitmap( ParentFragment.SeriesEntries[ position ].Thumbnail );
                            seriesItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                        }
                        else if ( ParentFragment.ImageThumbPlaceholder != null )
                        {
                            seriesItem.Thumbnail.SetImageBitmap( ParentFragment.ImageThumbPlaceholder );
                        }
                        else
                        {
                            seriesItem.Thumbnail.SetImageBitmap( null );
                        }

                        seriesItem.Title.Text = ParentFragment.SeriesEntries[ position ].Series.Name;
                        seriesItem.DateRange.Text = ParentFragment.SeriesEntries[ position ].Series.DateRanges;
                    }

                    return seriesItem;
                }

                public void WatchButtonClicked( )
                {
                    ParentFragment.WatchButtonClicked( );
                }

                public void TakeNotesButtonClicked( )
                {
                    ParentFragment.TakeNotesButtonClicked( );
                }
            }

            internal class BorderedActionButton
            {
                public BorderedRectView Layout { get; set; }
                public Button Button { get; set; }
                public TextView Icon { get; set; }
                public TextView Label { get; set; }

                public void AddToView( ViewGroup parentView )
                {
                    // first, create the layout that will store the button and label
                    Layout = new BorderedRectView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Layout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    parentView.AddView( Layout );

                    // now create the linearLayout that will store the button labels (Symbol & Text)
                    LinearLayout labelLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    labelLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (RelativeLayout.LayoutParams)labelLayout.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
                    Layout.AddView( labelLayout );

                    // add the button, which is just a frame wrapping the entire layout
                    Button = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Button.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    Button.Background = null;
                    Layout.AddView( Button );

                    // now set the icon
                    Icon = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    labelLayout.AddView( Icon );

                    // and lastly the text
                    Label = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Label.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Label.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    labelLayout.AddView( Label );
                }
            }

            public class SeriesPrimaryListItem : ListAdapter.ListItemView
            {
                public NotesArrayAdapter ParentAdapter { get; set; }

                //TextView Header { get; set; }
                LinearLayout DetailsLayout { get; set; }

                // stuff that will be set by data
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Billboard { get; set; }
                public TextView Title { get; set; }
                public TextView Date { get; set; }
                public TextView Speaker { get; set; }
                public bool HasImage { get; set; }
                //

                LinearLayout ButtonLayout { get; set; }
                BorderedActionButton WatchButton { get; set; }
                BorderedActionButton TakeNotesButton { get; set; }

                TextView Footer { get; set; }

                public SeriesPrimaryListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Orientation.Vertical;

                    /*Header = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Header.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Header.Text = MessagesStrings.Series_TopBanner;
                    Header.SetTypeface( Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    Header.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Header.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    ( (LinearLayout.LayoutParams)Header.LayoutParameters ).Gravity = GravityFlags.Center;
                    AddView( Header );*/

                    Billboard = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Billboard.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Billboard );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Title.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = 25;
                    AddView( Title );

                    DetailsLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    DetailsLayout.Orientation = Orientation.Horizontal;
                    DetailsLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).RightMargin = 25;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).BottomMargin = 50;
                    AddView( DetailsLayout );

                    Date = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Date.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Date.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    Date.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Date.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    DetailsLayout.AddView( Date );

                    // fill the remaining space with a dummy view, and that will align our speaker to the right
                    View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    DetailsLayout.AddView( dummyView );

                    Speaker = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Speaker.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Speaker.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    Speaker.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Speaker.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    DetailsLayout.AddView( Speaker );


                    // setup the buttons
                    ButtonLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ButtonLayout.Orientation = Orientation.Horizontal;
                    ButtonLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.WrapContent );
                    AddView( ButtonLayout );


                    // Watch Button
                    WatchButton = new BorderedActionButton();
                    WatchButton.AddToView( ButtonLayout );

                    ( (LinearLayout.LayoutParams)WatchButton.Layout.LayoutParameters ).LeftMargin = -5;
                    ( (LinearLayout.LayoutParams)WatchButton.Layout.LayoutParameters ).RightMargin = -1;
                    ( (LinearLayout.LayoutParams)WatchButton.Layout.LayoutParameters ).Weight = 1;
                    WatchButton.Layout.BorderWidth = 1;
                    WatchButton.Layout.SetBorderColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    WatchButton.Layout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    WatchButton.Icon.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary ), TypefaceStyle.Normal );
                    WatchButton.Icon.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateNoteConfig.Series_Table_IconSize );
                    WatchButton.Icon.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    WatchButton.Icon.Text = PrivateNoteConfig.Series_Table_Watch_Icon;

                    WatchButton.Label.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    WatchButton.Label.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    WatchButton.Label.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    WatchButton.Label.Text = MessagesStrings.Series_Table_Watch;

                    WatchButton.Button.Click += (object sender, EventArgs e ) =>
                    {
                        ParentAdapter.WatchButtonClicked( );
                    };
                    //



                    // TakeNotes Button
                    TakeNotesButton = new BorderedActionButton();
                    TakeNotesButton.AddToView( ButtonLayout );

                    ( (LinearLayout.LayoutParams)TakeNotesButton.Layout.LayoutParameters ).LeftMargin = -2;
                    ( (LinearLayout.LayoutParams)TakeNotesButton.Layout.LayoutParameters ).RightMargin = -5;
                    ( (LinearLayout.LayoutParams)TakeNotesButton.Layout.LayoutParameters ).Weight = 1;
                    TakeNotesButton.Layout.BorderWidth = 1;
                    TakeNotesButton.Layout.SetBorderColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    TakeNotesButton.Layout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    TakeNotesButton.Icon.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary ), TypefaceStyle.Normal );
                    TakeNotesButton.Icon.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateNoteConfig.Series_Table_IconSize );
                    TakeNotesButton.Icon.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TakeNotesButton.Icon.Text = PrivateNoteConfig.Series_Table_TakeNotes_Icon;

                    TakeNotesButton.Label.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    TakeNotesButton.Label.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    TakeNotesButton.Label.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TakeNotesButton.Label.Text = MessagesStrings.Series_Table_TakeNotes;

                    TakeNotesButton.Button.Click += (object sender, EventArgs e ) =>
                    {
                        ParentAdapter.TakeNotesButtonClicked( );
                    };
                    //


                    Footer = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Footer.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Footer.LayoutParameters ).TopMargin = -5;
                    Footer.Text = MessagesStrings.Series_Table_PreviousMessages;
                    Footer.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    Footer.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Footer.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Footer.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Table_Footer_Color ) );
                    Footer.Gravity = GravityFlags.Center;
                    AddView( Footer );
                }

                public void ToggleWatchButton( bool enabled )
                {
                    WatchButton.Button.Enabled = enabled;

                    if ( enabled == true )
                    {
                        WatchButton.Icon.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                        WatchButton.Label.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    }
                    else
                    {
                        WatchButton.Icon.SetTextColor( Color.DimGray );
                        WatchButton.Label.SetTextColor( Color.DimGray );
                    }
                }

                public void ToggleTakeNotesButton( bool enabled )
                {
                    TakeNotesButton.Button.Enabled = enabled;

                    if ( enabled == true )
                    {
                        TakeNotesButton.Icon.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                        TakeNotesButton.Label.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    }
                    else
                    {
                        TakeNotesButton.Icon.SetTextColor( Color.DimGray );
                        TakeNotesButton.Label.SetTextColor( Color.DimGray );
                    }
                }

                public override void Destroy()
                {
                    Billboard.SetImageBitmap( null );   
                }
            }

            public class SeriesListItem : ListAdapter.ListItemView
            {
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Thumbnail { get; set; }

                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView DateRange { get; set; }
                public TextView Chevron { get; set; }
                public View Seperator { get; set; }
                public bool HasImage { get; set; }

                public SeriesListItem( Context context ) : base( context )
                {
                    Orientation = Orientation.Vertical;

                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    LinearLayout contentLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    contentLayout.Orientation = Orientation.Horizontal;
                    contentLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    AddView( contentLayout );

                    Thumbnail = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Thumbnail.LayoutParameters = new LinearLayout.LayoutParams( (int)Rock.Mobile.Graphics.Util.UnitToPx( PrivateNoteConfig.Series_Main_CellWidth ), (int)Rock.Mobile.Graphics.Util.UnitToPx( PrivateNoteConfig.Series_Main_CellHeight ) );
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    contentLayout.AddView( Thumbnail );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TitleLayout.Orientation = Orientation.Vertical;
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).TopMargin = 50;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).BottomMargin = 50;
                    contentLayout.AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Title.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    TitleLayout.AddView( Title );

                    DateRange = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    DateRange.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    DateRange.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    DateRange.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    DateRange.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( DateRange );

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
                    Chevron.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateNoteConfig.Series_Table_IconSize );
                    Chevron.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Chevron.Text = PrivateNoteConfig.Series_Table_Navigate_Icon;
                    contentLayout.AddView( Chevron );

                    // add our own custom seperator at the bottom
                    Seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    Seperator.LayoutParameters.Height = 2;
                    Seperator.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    AddView( Seperator );
                }

                public override void Destroy()
                {
                    Thumbnail.SetImageBitmap( null );
                }
            }

            /// <summary>
            /// A wrapper class that consolidates the message, it's thumbnail and podcast status
            /// </summary>
            public class SeriesEntry
            {
                public Series Series { get; set; }
                public Bitmap Billboard;
                public Bitmap Thumbnail;
            }

            public class NotesPrimaryFragment : TaskFragment
            {
                public List<SeriesEntry> SeriesEntries { get; set; }

                ProgressBar ProgressBar { get; set; }
                ListView ListView { get; set; }

                public Bitmap ImageMainPlaceholder { get; set; }
                public Bitmap ImageThumbPlaceholder { get; set; }

                bool FragmentActive { get; set; }

                UIResultView ResultView { get; set; }

                public NotesPrimaryFragment( ) : base( )
                {
                    SeriesEntries = new List<SeriesEntry>();
                }

                public override void OnDestroyView()
                {
                    base.OnDestroyView();

                    if ( ImageMainPlaceholder != null )
                    {
                        ImageMainPlaceholder.Dispose( );
                        ImageMainPlaceholder = null;
                    }

                    if ( ImageThumbPlaceholder != null )
                    {
                        ImageThumbPlaceholder.Dispose( );
                        ImageThumbPlaceholder = null;
                    }

                    if ( ListView != null && ListView.Adapter != null )
                    {
                        ( (ListAdapter)ListView.Adapter ).Destroy( );
                    }
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }
                                        
					View view = inflater.Inflate(Resource.Layout.Notes_Primary, container, false);
                    view.SetOnTouchListener( this );

                    ProgressBar = view.FindViewById<ProgressBar>( Resource.Id.notes_primary_activityIndicator );
                    ListView = view.FindViewById<ListView>( Resource.Id.notes_primary_list );

                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e ) =>
                        {
                            // we ignore a tap on position 0, because that's the header with Watch/Take Notes
                            if( e.Position > 0 )
                            {
                                ParentTask.OnClick( this, e.Position - 1 );
                            }
                        };
                    ListView.SetOnTouchListener( this );

                    ResultView = new UIResultView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), delegate { TrySetupSeries( ); } );

                    ResultView.Hide( );

                    return view;
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                }

                public void WatchButtonClicked( )
                {
                    // notify the task that the Watch button was clicked
                    ParentTask.OnClick( this, -1, 1 );
                }

                public void TakeNotesButtonClicked( )
                {
                    // notify the task that the Take Notes button was clicked
                    ParentTask.OnClick( this, -1, 2 );
                }

                public Bitmap GetSeriesBillboard( int index )
                {
                    return SeriesEntries[ index ].Billboard != null ? SeriesEntries[ index ].Billboard : ImageMainPlaceholder;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    FragmentActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    if ( ParentTask.TaskReadyForFragmentDisplay )
                    {
                        SetupDisplay( );
                    }
                }

                public override void TaskReadyForFragmentDisplay()
                {
                    base.TaskReadyForFragmentDisplay();

                    SetupDisplay( );
                }

                void SetupDisplay( )
                {
                    AsyncLoader.LoadImage( PrivateNoteConfig.NotesThumbPlaceholder, true, false,
                        delegate( Bitmap loadedBmp )
                        {
                            if ( FragmentActive == true )
                            {
                                ImageThumbPlaceholder = loadedBmp;

                                RefreshList( );
                                return true;
                            }

                            return false;
                        } );

                    AsyncLoader.LoadImage( PrivateNoteConfig.NotesMainPlaceholder, true, false,
                        delegate( Bitmap loadedBmp )
                        {
                            if ( FragmentActive == true )
                            {
                                ImageMainPlaceholder = loadedBmp;

                                RefreshList( );
                                return true;
                            }

                            return false;
                        } );

                    TrySetupSeries( );
                }

                void TrySetupSeries( )
                {
                    ResultView.Hide( );

                    // what's the state of the series xml?
                    if ( RockLaunchData.Instance.RequestingNoteDB == true )
                    {
                        ProgressBar.Visibility = ViewStates.Visible;

                        // kick off a thread that will poll the download status and
                        // call "SeriesReady()" when the download is finished.
                        Thread waitThread = new Thread( WaitAsync );
                        waitThread.Start( );

                        while ( waitThread.IsAlive == false );
                    }
                    else if ( RockLaunchData.Instance.NeedSeriesDownload( ) == true )
                    {
                        ProgressBar.Visibility = ViewStates.Visible;

                        RockLaunchData.Instance.GetNoteDB( delegate
                            {
                                // don't worry about the result. The point is we tried,
                                // and now will either use downloaded data, saved data, or throw an error to the user.
                                SeriesReady( );
                            } );
                    }
                    else
                    {
                        // we have the series, so we can move forward.
                        SeriesReady( );
                    }
                }

                void WaitAsync( )
                {
                    // while we're still requesting the series, simply wait
                    while ( App.Shared.Network.RockLaunchData.Instance.RequestingNoteDB == true );

                    // now that tis' finished, update the notes.
                    SeriesReady( );
                }

                void SeriesReady( )
                {
                    // on the main thread, update the list
                    Rock.Mobile.Threading.Util.PerformOnUIThread(delegate
                        {
                            ProgressBar.Visibility = ViewStates.Gone;

                            // if there are now series entries, we're good
                            if ( RockLaunchData.Instance.Data.NoteDB.SeriesList.Count > 0 )
                            {
                                if( FragmentActive == true )
                                {
                                    ListView.Adapter = new NotesArrayAdapter( this );
                                    SetupSeriesEntries( RockLaunchData.Instance.Data.NoteDB.SeriesList );
                                }
                            }
                            else
                            {
                                if ( FragmentActive == true )
                                {
                                    // error
                                    ResultView.Show( MessagesStrings.Series_Error_Title, 
                                        PrivateControlStylingConfig.Result_Symbol_Failed, 
                                        MessagesStrings.Series_Error_Message, 
                                        GeneralStrings.Retry );
                                }
                            }
                        } );
                }

                void SetupSeriesEntries( List<Series> seriesList )
                {
                    SeriesEntries.Clear( );

                    for( int i = 0; i < seriesList.Count; i++ )
                    {
                        // add the entry to our list
                        SeriesEntry entry = new SeriesEntry();
                        SeriesEntries.Add( entry );

                        // copy over the series and give it a placeholder image
                        entry.Series = seriesList[ i ];


                        // attempt to load / download images.

                        // for billboards, we ONLY CARE about loading the first series' billboard.
                        if ( i == 0 )
                        {
                            bool imageExists = TryLoadBillboardImage( entry, NotesTask.FormatBillboardImageName( entry.Series.Name ) );
                            if ( imageExists == false )
                            {
                                FileCache.Instance.DownloadFileToCache( entry.Series.BillboardUrl, NotesTask.FormatBillboardImageName( entry.Series.Name ), delegate
                                    {
                                        TryLoadBillboardImage( entry, NotesTask.FormatBillboardImageName( entry.Series.Name ) );
                                    } );
                            }
                        }

                        // for everything, we care about the thumbnails
                        bool thumbExists = TryLoadThumbImage( entry, NotesTask.FormatThumbImageName( entry.Series.Name ) );
                        if ( thumbExists == false )
                        {
                            FileCache.Instance.DownloadFileToCache( entry.Series.ThumbnailUrl, NotesTask.FormatThumbImageName( entry.Series.Name ), delegate
                                {
                                    TryLoadThumbImage( entry, NotesTask.FormatThumbImageName( entry.Series.Name ) );
                                } );
                        }
                    }
                }

                bool TryLoadBillboardImage( SeriesEntry entry, string filename )
                {
                    // does the file exist?
                    if ( FileCache.Instance.FileExists( filename ) == true )
                    {
                        AsyncLoader.LoadImage( filename, false, false,
                            delegate( Bitmap loadedBmp )
                            {
                                if ( FragmentActive == true )
                                {
                                    // if for some reason it loaded corrupt, remove it.
                                    if ( loadedBmp == null )
                                    {
                                        FileCache.Instance.RemoveFile( filename );
                                    }

                                    entry.Billboard = loadedBmp;

                                    RefreshList( );

                                    return true;
                                }

                                return false;
                            } );

                        return true;
                    }

                    return false;
                }

                bool TryLoadThumbImage( SeriesEntry entry, string filename )
                {
                    // does the file exist?
                    if ( FileCache.Instance.FileExists( filename ) == true )
                    {
                        AsyncLoader.LoadImage( filename, false, false,
                            delegate( Bitmap loadedBmp )
                            {
                                if ( FragmentActive == true )
                                {
                                    // if for some reason it loaded corrupt, remove it.
                                    if ( loadedBmp == null )
                                    {
                                        FileCache.Instance.RemoveFile( filename );
                                    }

                                    entry.Thumbnail = loadedBmp;

                                    RefreshList( );

                                    return true;
                                }

                                return false;
                            } );

                        return true;
                    }

                    return false;
                }

                void RefreshList( )
                {
                    if ( ListView != null && ListView.Adapter != null )
                    {
                        ( ListView.Adapter as ListAdapter ).NotifyDataSetChanged( );
                    }
                }


                public override void OnPause( )
                {
                    base.OnPause( );

                    FragmentActive = false;
                }
            }
        }
    }
}

