
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
using App.Shared;
using Rock.Mobile.UI;
using System.IO;
using App.Shared.Config;
using App.Shared.Strings;
using App.Shared.Analytics;
using App.Shared.PrivateConfig;
using Rock.Mobile.PlatformSpecific.Android.Util;
using Rock.Mobile.IO;
using Rock.Mobile.PlatformSpecific.Android.UI;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesDetailsArrayAdapter : ListAdapter
            {
                NotesDetailsFragment ParentFragment { get; set; }

                public NotesDetailsArrayAdapter( NotesDetailsFragment parentFragment )
                {
                    ParentFragment = parentFragment;
                }

                public override int Count 
                {
                    get { return ParentFragment.Series.Messages.Count + 1; }
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
                    MessagePrimaryListItem messageItem = convertView as MessagePrimaryListItem;
                    if ( messageItem == null )
                    {
                        messageItem = new MessagePrimaryListItem( ParentFragment.Activity.BaseContext );

                        int height = (int)System.Math.Ceiling( NavbarFragment.GetContainerDisplayWidth( ) * PrivateNoteConfig.NotesMainPlaceholderAspectRatio );
                        messageItem.Thumbnail.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, height );

                        messageItem.HasImage = false;
                    }

                    messageItem.ParentAdapter = this;

                    if ( ParentFragment.SeriesBillboard != null )
                    {
                        if ( messageItem.HasImage == false )
                        {
                            messageItem.HasImage = true;
                            Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( messageItem.Thumbnail, true, null );
                        }
                        
                        messageItem.Thumbnail.SetImageBitmap( ParentFragment.SeriesBillboard );
                        messageItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    }
                    else if ( ParentFragment.PlaceholderImage != null )
                    {
                        messageItem.Thumbnail.SetImageBitmap( ParentFragment.PlaceholderImage );
                        messageItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    }

                    messageItem.Title.Text = ParentFragment.Series.Name;
                    messageItem.DateRange.Text = ParentFragment.Series.DateRanges;
                    messageItem.Desc.Text = ParentFragment.Series.Description;

                    return messageItem;
                }

                ListItemView GetStandardView( int position, View convertView, ViewGroup parent )
                {
                    MessageListItem messageItem = convertView as MessageListItem;
                    if ( messageItem == null )
                    {
                        messageItem = new MessageListItem( ParentFragment.Activity.BaseContext );
                    }

                    messageItem.ParentAdapter = this;
                    messageItem.Position = position;

                    messageItem.Title.Text = ParentFragment.Series.Messages[ position ].Name;
                    messageItem.Date.Text = ParentFragment.Series.Messages[ position ].Date;
                    messageItem.Speaker.Text = ParentFragment.Series.Messages[ position ].Speaker;

                    if ( string.IsNullOrEmpty( ParentFragment.Series.Messages[ position ].AudioUrl ) == true )
                    {
                        messageItem.ToggleListenButton( false );
                    }
                    else
                    {
                        messageItem.ToggleListenButton( true );
                    }

                    if ( string.IsNullOrEmpty( ParentFragment.Series.Messages[ position ].WatchUrl ) == true )
                    {
                        messageItem.ToggleWatchButton( false );
                    }
                    else
                    {
                        messageItem.ToggleWatchButton( true );
                    }

                    if ( string.IsNullOrEmpty( ParentFragment.Series.Messages[ position ].NoteUrl ) == true )
                    {
                        messageItem.ToggleTakeNotesButton( false );
                    }
                    else
                    {
                        messageItem.ToggleTakeNotesButton( true );
                    }

                    return messageItem;
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentFragment.OnClick( position, buttonIndex );
                }
            }

            public class MessagePrimaryListItem : ListAdapter.ListItemView
            {
                public NotesDetailsArrayAdapter ParentAdapter { get; set; }

                // stuff that will be set by data
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Thumbnail { get; set; }
                public TextView Title { get; set; }
                public TextView DateRange { get; set; }
                public TextView Desc { get; set; }
                public bool HasImage { get; set; }
                //

                public MessagePrimaryListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Orientation.Vertical;

                    Thumbnail = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Thumbnail.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Thumbnail );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Title.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = 25;
                    AddView( Title );

                    DateRange = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    DateRange.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    DateRange.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
                    DateRange.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    DateRange.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    ( (LinearLayout.LayoutParams)DateRange.LayoutParameters ).TopMargin = 0;
                    ( (LinearLayout.LayoutParams)DateRange.LayoutParameters ).LeftMargin = 25;
                    AddView( DateRange );

                    Desc = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Desc.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Desc.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
                    Desc.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Desc.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).TopMargin = 10;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).RightMargin = 25;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).BottomMargin = 25;
                    AddView( Desc );
                }

                public override void Destroy()
                {
                    Thumbnail.SetImageBitmap( null );   
                }
            }

            public class MessageListItem : ListAdapter.ListItemView
            {
                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView Date { get; set; }
                public TextView Speaker { get; set; }

                RelativeLayout ButtonFrameLayout { get; set; }
                Button ListenButton { get; set; }
                Button WatchButton { get; set; }
                Button TakeNotesButton { get; set; }

                public NotesDetailsArrayAdapter ParentAdapter { get; set; }
                public int Position { get; set; }

                public MessageListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
                    LayoutParameters = new AbsListView.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );

                    Orientation = Orientation.Vertical;

                    LinearLayout contentLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    contentLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );
                    contentLayout.Orientation = Orientation.Horizontal;
                    AddView( contentLayout );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.WrapContent );
                    TitleLayout.Orientation = Orientation.Vertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).TopMargin = 50;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).BottomMargin = 50;
                    contentLayout.AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    Title.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( Title );

                    Date = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Date.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Date.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    Date.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Date.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Date );

                    Speaker = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Speaker.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Speaker.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                    Speaker.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Speaker.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Speaker.SetMaxLines( 1 );
                    TitleLayout.AddView( Speaker );

                    // add our own custom seperator at the bottom
                    View seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    seperator.LayoutParameters.Height = 2;
                    seperator.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    AddView( seperator );


                    // setup the buttons
                    LinearLayout buttonLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    buttonLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );
                    ( (LinearLayout.LayoutParams)buttonLayout.LayoutParameters ).Weight = 1;
                    buttonLayout.Orientation = Orientation.Horizontal;
                    contentLayout.AddView( buttonLayout );

                    Typeface buttonFontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary );

                    ListenButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ListenButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)ListenButton.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)ListenButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ListenButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    ListenButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateNoteConfig.Details_Table_IconSize );
                    ListenButton.Text = PrivateNoteConfig.Series_Table_Listen_Icon;
                    ListenButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    ListenButton.Background = null;
                    buttonLayout.AddView( ListenButton );

                    WatchButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    WatchButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)WatchButton.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)WatchButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    WatchButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    WatchButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateNoteConfig.Details_Table_IconSize );
                    WatchButton.Text = PrivateNoteConfig.Series_Table_Watch_Icon;
                    WatchButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    WatchButton.Background = null;
                    buttonLayout.AddView( WatchButton );

                    TakeNotesButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TakeNotesButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TakeNotesButton.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)TakeNotesButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    TakeNotesButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    TakeNotesButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateNoteConfig.Details_Table_IconSize );
                    TakeNotesButton.Text = PrivateNoteConfig.Series_Table_TakeNotes_Icon;
                    TakeNotesButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    TakeNotesButton.Background = null;
                    buttonLayout.AddView( TakeNotesButton );

                    ListenButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 0 );
                        };

                    WatchButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 1 );
                        };

                    TakeNotesButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 2 );
                        };
                }

                public void ToggleListenButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        ListenButton.Enabled = true;
                        ListenButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    }
                    else
                    {
                        ListenButton.Enabled = false;

                        uint disabledColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );
                        ListenButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( disabledColor ) );
                    }
                }

                public void ToggleWatchButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        WatchButton.Enabled = true;
                        WatchButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    }
                    else
                    {
                        WatchButton.Enabled = false;

                        uint disabledColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );
                        WatchButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( disabledColor ) );
                    }
                }

                public void ToggleTakeNotesButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        TakeNotesButton.Enabled = true;
                        TakeNotesButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    }
                    else
                    {
                        TakeNotesButton.Enabled = false;
                        
                        uint disabledColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );
                        TakeNotesButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( disabledColor ) );
                    }
                }

                public override void Destroy()
                {
                }
            }

            public class NotesDetailsFragment : TaskFragment
            {
                public Series Series { get; set; }
                public Bitmap SeriesBillboard { get; set; }
                public Bitmap PlaceholderImage { get; set; }

                ListView MessagesListView { get; set; }

                bool FragmentActive { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.Notes_Details, container, false);
                    view.SetOnTouchListener( this );

                    // setup our message list view
                    MessagesListView = view.FindViewById<ListView>( Resource.Id.notes_details_list );
                    MessagesListView.SetOnTouchListener( this );
                    MessagesListView.Divider = null;


                    // load the placeholder and series image
                    SeriesBillboard = null;

                    bool imageExists = TryLoadBanner( NotesTask.FormatBillboardImageName( Series.Name ) );
                    if ( imageExists == false )
                    {
                        // use the placeholder and request the image download
                        FileCache.Instance.DownloadFileToCache( Series.BillboardUrl, NotesTask.FormatBillboardImageName( Series.Name ), delegate
                            {
                                TryLoadBanner( NotesTask.FormatBillboardImageName( Series.Name ) );
                            } );


                        AsyncLoader.LoadImage( PrivateNoteConfig.NotesMainPlaceholder, true, false,
                            delegate( Bitmap imageBmp )
                            {
                                if ( FragmentActive == true )
                                {
                                    PlaceholderImage = imageBmp;
                                     
                                    RefreshList( );

                                    return true;
                                }

                                return false;
                            } );
                    }

                    return view;
                }

                void RefreshList( )
                {
                    if( MessagesListView != null && MessagesListView.Adapter != null )
                    {
                        ( MessagesListView.Adapter as ListAdapter ).NotifyDataSetChanged( );
                    }
                }

                bool TryLoadBanner( string filename )
                {
                    // if the file exists
                    if ( FileCache.Instance.FileExists( filename ) == true )
                    {
                        // load it asynchronously
                        AsyncLoader.LoadImage( filename, false, false,
                            delegate( Bitmap imageBmp )
                            {
                                if ( FragmentActive == true )
                                {
                                    // if for some reason it loaded corrupt, remove it.
                                    if ( imageBmp == null )
                                    {
                                        FileCache.Instance.RemoveFile( filename );
                                    }

                                    SeriesBillboard = imageBmp;

                                    RefreshList( );

                                    return true;
                                }
                                return false;

                            } );

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentTask.OnClick( this, position, buttonIndex );
                }

                public override void OnResume()
                {
                    base.OnResume();

                    FragmentActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    // log the series they tapped on.
                    MessageAnalytic.Instance.Trigger( MessageAnalytic.BrowseSeries, Series.Name );

                    // setup the messages list
                    MessagesListView.Adapter = new NotesDetailsArrayAdapter( this );
                }

                public override void OnPause( )
                {
                    base.OnPause( );

                    FragmentActive = false;
                }

                public override void OnDestroyView()
                {
                    base.OnDestroyView();

                    if ( PlaceholderImage != null )
                    {
                        PlaceholderImage.Dispose( );
                        PlaceholderImage = null;
                    }

                    if ( SeriesBillboard != null )
                    {
                        SeriesBillboard.Dispose( );
                        SeriesBillboard = null;
                    }

                    if ( MessagesListView != null && MessagesListView.Adapter != null )
                    {
                        ( (ListAdapter)MessagesListView.Adapter ).Destroy( );
                    }
                }
            }
        }
    }
}
