using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using App.Shared.Network;
using CoreGraphics;
using Rock.Mobile.Network;
using App.Shared.Notes.Model;
using System.Xml;
using System.IO;
using RestSharp;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.UI;
using System.Net;
using App.Shared;
using System.Threading.Tasks;
using System.Threading;
using App.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared.PrivateConfig;
using Rock.Mobile.IO;

namespace iOS
{
    partial class NotesMainUIViewController : TaskUIViewController
    {
        public class TableSource : UITableViewSource 
        {
            /// <summary>
            /// Definition for the primary (top) cell, which advertises the current series
            /// more prominently
            /// </summary>
            class SeriesPrimaryCell : UITableViewCell
            {
                public static string Identifier = "SeriesPrimaryCell";

                public TableSource Parent { get; set; }

                public UIImageView Image { get; set; }


                public UILabel Title { get; set; }
                public UILabel Date { get; set; }
                public UILabel Speaker { get; set; }

                public UIButton WatchButton { get; set; }
                public UILabel WatchButtonIcon { get; set; }
                public UILabel WatchButtonLabel { get; set; }

                public UIButton TakeNotesButton { get; set; }
                public UILabel TakeNotesButtonIcon { get; set; }
                public UILabel TakeNotesButtonLabel { get; set; }

                public UILabel BottomBanner { get; set; }

                public SeriesPrimaryCell( CGRect parentSize, UITableViewCellStyle style, string cellIdentifier, UIImage imagePlaceholder ) : base( style, cellIdentifier )
                {
                    BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

                    Image = new UIImageView( );
                    Image.ContentMode = UIViewContentMode.ScaleAspectFit;
                    Image.Layer.AnchorPoint = CGPoint.Empty;

                    // Banner Image
                    Image.Image = imagePlaceholder;
                    Image.SizeToFit( );

                    // resize the image to fit the width of the device
                    nfloat imageAspect = Image.Bounds.Height / Image.Bounds.Width;
                    Image.Frame = new CGRect( 0, 0, parentSize.Width, parentSize.Width * imageAspect );
                    AddSubview( Image );


                    // Create the title
                    Title = new UILabel( );
                    Title.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
                    Title.Layer.AnchorPoint = CGPoint.Empty;
                    Title.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    Title.BackgroundColor = UIColor.Clear;
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    Title.Text = "PLACEHOLDER PLACEHOLDER";
                    Title.SizeToFit( );
                    Title.Frame = new CGRect( 5, Image.Frame.Bottom + 5, parentSize.Width - 10, Title.Frame.Height + 5 );
                    AddSubview( Title );

                    // Date & Speaker
                    Date = new UILabel( );
                    Date.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    Date.Layer.AnchorPoint = CGPoint.Empty;
                    Date.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Date.BackgroundColor = UIColor.Clear;
                    Date.LineBreakMode = UILineBreakMode.TailTruncation;
                    Date.Text = "88/88/8888";
                    Date.SizeToFit( );
                    Date.Frame = new CGRect( 5, Title.Frame.Bottom - 5, parentSize.Width, Date.Frame.Height + 5 );
                    AddSubview( Date );

                    Speaker = new UILabel( );
                    Speaker.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    Speaker.Layer.AnchorPoint = CGPoint.Empty;
                    Speaker.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Speaker.BackgroundColor = UIColor.Clear;
                    Speaker.LineBreakMode = UILineBreakMode.TailTruncation;
                    Speaker.Text = "PLACEHOLDER";
                    Speaker.SizeToFit( );
                    Speaker.Frame = new CGRect( parentSize.Width - Speaker.Bounds.Width - 5, Title.Frame.Bottom - 5, parentSize.Width, Speaker.Frame.Height + 5 );
                    AddSubview( Speaker );


                    // Watch & Take Notes Buttons
                    WatchButton = new UIButton( UIButtonType.Custom );
                    WatchButton.TouchUpInside += (object sender, EventArgs e) => { Parent.WatchButtonClicked( ); };
                    WatchButton.Layer.AnchorPoint = CGPoint.Empty;
                    WatchButton.BackgroundColor = UIColor.Clear;
                    WatchButton.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ).CGColor;
                    WatchButton.Layer.BorderWidth = 1;
                    WatchButton.SizeToFit( );
                    WatchButton.Bounds = new CGRect( 0, 0, parentSize.Width / 2 + 6, WatchButton.Bounds.Height + 10 );
                    WatchButton.Layer.Position = new CGPoint( -5, Speaker.Frame.Bottom + 15 );
                    AddSubview( WatchButton );

                    WatchButtonIcon = new UILabel( );
                    WatchButton.AddSubview( WatchButtonIcon );
                    WatchButtonIcon.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.Series_Table_IconSize );
                    WatchButtonIcon.Text = PrivateNoteConfig.Series_Table_Watch_Icon;
                    WatchButtonIcon.SizeToFit( );

                    WatchButtonLabel = new UILabel( );
                    WatchButton.AddSubview( WatchButtonLabel );
                    WatchButtonLabel.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    WatchButtonLabel.Text = MessagesStrings.Series_Table_Watch;
                    WatchButtonLabel.SizeToFit( );

                    nfloat labelTotalWidth = WatchButtonIcon.Bounds.Width + WatchButtonLabel.Bounds.Width + 5;
                    WatchButtonIcon.Layer.Position = new CGPoint( (WatchButton.Bounds.Width - labelTotalWidth) / 2 + (WatchButtonIcon.Bounds.Width / 2), WatchButton.Bounds.Height / 2 );
                    WatchButtonLabel.Layer.Position = new CGPoint( WatchButtonIcon.Frame.Right + (WatchButtonLabel.Bounds.Width / 2), WatchButton.Bounds.Height / 2 );
                    


                    TakeNotesButton = new UIButton( UIButtonType.Custom );
                    TakeNotesButton.TouchUpInside += (object sender, EventArgs e) => { Parent.TakeNotesButtonClicked( ); };
                    TakeNotesButton.Layer.AnchorPoint = CGPoint.Empty;
                    TakeNotesButton.BackgroundColor = UIColor.Clear;
                    TakeNotesButton.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ).CGColor;
                    TakeNotesButton.Layer.BorderWidth = 1;
                    TakeNotesButton.SizeToFit( );
                    AddSubview( TakeNotesButton );

                    TakeNotesButton.Bounds = new CGRect( 0, 0, parentSize.Width / 2 + 5, TakeNotesButton.Bounds.Height + 10 );
                    TakeNotesButton.Layer.Position = new CGPoint( (parentSize.Width + 5) - TakeNotesButton.Bounds.Width, Speaker.Frame.Bottom + 15 );


                    TakeNotesButtonIcon = new UILabel( );
                    TakeNotesButton.AddSubview( TakeNotesButtonIcon );
                    TakeNotesButtonIcon.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.Series_Table_IconSize );
                    TakeNotesButtonIcon.Text = PrivateNoteConfig.Series_Table_TakeNotes_Icon;
                    TakeNotesButtonIcon.SizeToFit( );

                    TakeNotesButtonLabel = new UILabel( );
                    TakeNotesButton.AddSubview( TakeNotesButtonLabel );
                    TakeNotesButtonLabel.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    TakeNotesButtonLabel.Text = MessagesStrings.Series_Table_TakeNotes;
                    TakeNotesButtonLabel.SizeToFit( );

                    labelTotalWidth = TakeNotesButtonIcon.Bounds.Width + TakeNotesButtonLabel.Bounds.Width + 5;
                    TakeNotesButtonIcon.Layer.Position = new CGPoint( (TakeNotesButton.Bounds.Width - labelTotalWidth) / 2 + (TakeNotesButtonIcon.Bounds.Width / 2), TakeNotesButton.Bounds.Height / 2 );
                    TakeNotesButtonLabel.Layer.Position = new CGPoint( TakeNotesButtonIcon.Frame.Right + (TakeNotesButtonLabel.Bounds.Width / 2), TakeNotesButton.Bounds.Height / 2 );


                    // bottom banner
                    BottomBanner = new UILabel( );
                    BottomBanner.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    BottomBanner.Layer.AnchorPoint = new CGPoint( 0, 0 );
                    BottomBanner.Text = MessagesStrings.Series_Table_PreviousMessages;
                    BottomBanner.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    BottomBanner.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Table_Footer_Color );
                    BottomBanner.TextAlignment = UITextAlignment.Center;

                    BottomBanner.SizeToFit( );
                    BottomBanner.Bounds = new CGRect( 0, 0, parentSize.Width, BottomBanner.Bounds.Height + 10 );
                    BottomBanner.Layer.Position = new CGPoint( 0, TakeNotesButton.Frame.Bottom - 1 );
                    AddSubview( BottomBanner );
                }

                public void ToggleWatchButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        WatchButton.Enabled = true;
                        WatchButtonIcon.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                        WatchButtonLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    }
                    else
                    {
                        WatchButton.Enabled = false;
                        WatchButtonIcon.TextColor = UIColor.DarkGray;
                        WatchButtonLabel.TextColor = UIColor.DarkGray;
                    }
                }

                public void ToggleTakeNotesButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        TakeNotesButton.Enabled = true;
                        TakeNotesButtonIcon.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                        TakeNotesButtonLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    }
                    else
                    {
                        TakeNotesButton.Enabled = false;
                        TakeNotesButtonIcon.TextColor = UIColor.DarkGray;
                        TakeNotesButtonLabel.TextColor = UIColor.DarkGray;
                    }
                }
            }

            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class SeriesCell : UITableViewCell
            {
                public static string Identifier = "SeriesCell";

                public TableSource Parent { get; set; }

                public UIImageView Image { get; set; }
                public UILabel Title { get; set; }
                public UILabel Date { get; set; }
                public UILabel Chevron { get; set; }

                public UIView Seperator { get; set; }

                public SeriesCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Image = new UIImageView( );
                    Image.ContentMode = UIViewContentMode.ScaleAspectFit;
                    Image.Layer.AnchorPoint = CGPoint.Empty;
                    AddSubview( Image );

                    Title = new UILabel( );
                    Title.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
                    Title.Layer.AnchorPoint = CGPoint.Empty;
                    Title.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Title.BackgroundColor = UIColor.Clear;
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Title );

                    Date = new UILabel( );
                    Date.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    Date.Layer.AnchorPoint = CGPoint.Empty;
                    Date.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Date.BackgroundColor = UIColor.Clear;
                    Date.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Date );

                    Chevron = new UILabel( );
                    AddSubview( Chevron );
                    Chevron.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.Series_Table_IconSize );
                    Chevron.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Chevron.Text = PrivateNoteConfig.Series_Table_Navigate_Icon;
                    Chevron.SizeToFit( );

                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                }
            }

            NotesMainUIViewController Parent { get; set; }
            List<SeriesEntry> SeriesEntries { get; set; }
            UIImage ImageMainPlaceholder { get; set; }
            UIImage ImageThumbPlaceholder { get; set; }

            nfloat PendingPrimaryCellHeight { get; set; }

            public TableSource (NotesMainUIViewController parent, List<SeriesEntry> series, UIImage imageMainPlaceholder, UIImage imageThumbPlaceholder )
            {
                Parent = parent;
                SeriesEntries = series;
                ImageMainPlaceholder = imageMainPlaceholder;
                ImageThumbPlaceholder = imageThumbPlaceholder;

                // create a dummy cell so we can store the height
                SeriesPrimaryCell cell = new SeriesPrimaryCell( parent.View.Frame, UITableViewCellStyle.Default, SeriesCell.Identifier, ImageMainPlaceholder );
                PendingPrimaryCellHeight = cell.BottomBanner.Frame.Bottom;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return SeriesEntries.Count + 1;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // notify our parent if it isn't the primary row.
                // The primary row only responds to its two buttons
                if ( indexPath.Row > 0 )
                {
                    Parent.RowClicked( indexPath.Row - 1 );
                }
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return GetCachedRowHeight( tableView, indexPath );
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                return GetCachedRowHeight( tableView, indexPath );
            }

            nfloat GetCachedRowHeight( UITableView tableView, NSIndexPath indexPath )
            {
                // Depending on the row, we either want the primary cell's height,
                // or a standard row's height.
                switch ( indexPath.Row )
                {
                    case 0:
                    {
                        return PendingPrimaryCellHeight;
                    }

                    default:
                    {
                        return PrivateNoteConfig.Series_Main_CellHeight;
                    }
                }
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                if ( indexPath.Row == 0 )
                {
                    return GetPrimaryCell( tableView );
                }
                else
                {
                    return GetStandardCell( tableView, indexPath.Row - 1 );
                }
            }

            UITableViewCell GetPrimaryCell( UITableView tableView )
            {
                SeriesPrimaryCell cell = tableView.DequeueReusableCell( SeriesPrimaryCell.Identifier ) as SeriesPrimaryCell;

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new SeriesPrimaryCell( tableView.Frame, UITableViewCellStyle.Default, SeriesCell.Identifier, ImageMainPlaceholder );
                    cell.Parent = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                // Banner Image
                if ( SeriesEntries.Count > 0 )
                {
                    cell.Image.Image = SeriesEntries[ 0 ].mBillboard != null ? SeriesEntries[ 0 ].mBillboard : ImageMainPlaceholder;

                    // Create the title
                    if ( SeriesEntries[ 0 ].Series.Messages.Count > 0 )
                    {
                        cell.Title.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                        cell.Title.SizeToFit( );


                        // Date & Speaker
                        cell.Date.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Date;
                        cell.Date.SizeToFit( );

                        cell.Speaker.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Speaker;
                        cell.Speaker.SizeToFit( );
                        cell.Speaker.Frame = new CGRect( cell.Bounds.Width - cell.Speaker.Bounds.Width - 5, 
                                                         cell.Speaker.Frame.Top, 
                                                         cell.Speaker.Bounds.Width, 
                                                         cell.Speaker.Bounds.Height + 5 );

                        // Watch Button & Labels
                        // disable the button if there's no watch URL
                        if ( string.IsNullOrEmpty( SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl ) )
                        {
                            cell.ToggleWatchButton( false );
                        }
                        else
                        {
                            cell.ToggleWatchButton( true );
                        }


                        // Take Notes Button & Labels
                        // disable the button if there's no note URL
                        if ( string.IsNullOrEmpty( SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl ) )
                        {
                            cell.ToggleTakeNotesButton( false );
                        }
                        else
                        {
                            cell.ToggleTakeNotesButton( true );
                        }
                    }
                    else
                    {
                        cell.ToggleWatchButton( false );
                        cell.ToggleTakeNotesButton( false );
                    }
                }

                PendingPrimaryCellHeight = cell.BottomBanner.Frame.Bottom;
                return cell;
            }

            UITableViewCell GetStandardCell( UITableView tableView, int row )
            {
                SeriesCell cell = tableView.DequeueReusableCell( SeriesCell.Identifier ) as SeriesCell;

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new SeriesCell( UITableViewCellStyle.Default, SeriesCell.Identifier );
                    cell.Parent = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                // Thumbnail Image
                cell.Image.Image = SeriesEntries[ row ].mThumbnail != null ? SeriesEntries[ row ].mThumbnail : ImageThumbPlaceholder;
                cell.Image.SizeToFit( );

                // force the image to be sized according to the height of the cell
                cell.Image.Frame = new CGRect( 0, 
                                               0, 
                                               PrivateNoteConfig.Series_Main_CellWidth, 
                                               PrivateNoteConfig.Series_Main_CellHeight );

                nfloat availableTextWidth = cell.Bounds.Width - cell.Chevron.Bounds.Width - cell.Image.Bounds.Width - 10;

                // Chevron
                cell.Chevron.Layer.Position = new CGPoint( cell.Bounds.Width - (cell.Chevron.Bounds.Width / 2) - 5, (PrivateNoteConfig.Series_Main_CellHeight) / 2 );

                // Create the title
                cell.Title.Text = SeriesEntries[ row ].Series.Name;
                cell.Title.SizeToFit( );

                // Date Range
                cell.Date.Text = SeriesEntries[ row ].Series.DateRanges;
                cell.Date.SizeToFit( );

                // Position the Title & Date in the center to the right of the image
                nfloat totalTextHeight = cell.Title.Bounds.Height + cell.Date.Bounds.Height - 1;
                cell.Title.Frame = new CGRect( cell.Image.Frame.Right + 10, (PrivateNoteConfig.Series_Main_CellHeight - totalTextHeight) / 2, availableTextWidth - 5, cell.Title.Frame.Height );
                cell.Date.Frame = new CGRect( cell.Title.Frame.Left, cell.Title.Frame.Bottom - 6, availableTextWidth - 5, cell.Date.Frame.Height + 5 );

                // add the seperator to the bottom
                cell.Seperator.Frame = new CGRect( 0, cell.Image.Frame.Bottom - 1, cell.Bounds.Width, 1 );

                return cell;
            }

            public void TakeNotesButtonClicked( )
            {
                Parent.TakeNotesClicked( );
            }

            public void WatchButtonClicked( )
            {
                Parent.WatchButtonClicked( );
            }
        }

        /// <summary>
        /// A wrapper class that consolidates the series and its image
        /// </summary>
        public class SeriesEntry
        {
            public Series Series { get; set; }
            public UIImage mBillboard;
            public UIImage mThumbnail;
        }
        List<SeriesEntry> SeriesEntries { get; set; }
        UIImage ImageMainPlaceholder{ get; set; }
        UIImage ImageThumbPlaceholder{ get; set; }

        UIActivityIndicatorView ActivityIndicator { get; set; }

        NotesDetailsUIViewController DetailsViewController { get; set; }

        UIResultView ResultView { get; set; }

        bool IsVisible { get; set; }

        public NotesMainUIViewController (IntPtr handle) : base (handle)
        {
            SeriesEntries = new List<SeriesEntry>();

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + PrivateNoteConfig.NotesMainPlaceholder;
            ImageMainPlaceholder = new UIImage( imagePath );


            imagePath = NSBundle.MainBundle.BundlePath + "/" + PrivateNoteConfig.NotesThumbPlaceholder;
            ImageThumbPlaceholder = new UIImage( imagePath );
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // setup our table
            NotesTableView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            NotesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            ActivityIndicator = new UIActivityIndicatorView( new CGRect( View.Frame.Width / 2, View.Frame.Height / 2, 0, 0 ) );
            ActivityIndicator.StartAnimating( );
            ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            ActivityIndicator.SizeToFit( );

            ResultView = new UIResultView( View, View.Frame.ToRectF( ), delegate { TrySetupSeries( ); } );

            ResultView.Hide( );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // adjust the table height for our navbar.
            // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
            // or our changes will be overwritten
            NotesTableView.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height );
        }

        public override void LayoutChanged( )
        {
            base.LayoutChanged( );

            // if the layout is changed, the simplest way to fix the UI is to recreate the table source
            NotesTableView.Source = new TableSource( this, SeriesEntries, ImageMainPlaceholder, ImageThumbPlaceholder );
            NotesTableView.ReloadData( );

            ResultView.SetBounds( View.Bounds.ToRectF( ) );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsVisible = false;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            DetailsViewController = null;

            IsVisible = true;

            TrySetupSeries( );
        }

        void TrySetupSeries( )
        {
            NotesTableView.Hidden = true;
            ResultView.Hide( );

            // what's the state of the series xml?
            if ( RockLaunchData.Instance.RequestingNoteDB == true )
            {
                // it's in the process of downloading, so wait and poll.
                View.AddSubview( ActivityIndicator );
                View.BringSubviewToFront( ActivityIndicator );
                ActivityIndicator.Hidden = false;

                // kick off a thread that will poll the download status and
                // call "SeriesReady()" when the download is finished.
                Thread waitThread = new Thread( WaitAsync );
                waitThread.Start( );

                while ( waitThread.IsAlive == false );
            }
            else if ( RockLaunchData.Instance.NeedSeriesDownload( ) == true )
            {
                // it hasn't been downloaded, or failed, or something. Point is we
                // don't have anything, so request it.
                View.AddSubview( ActivityIndicator );
                View.BringSubviewToFront( ActivityIndicator );
                ActivityIndicator.Hidden = false;

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
            // only do image work on the main thread
            InvokeOnMainThread( delegate
                {
                    ActivityIndicator.Hidden = true;
                    ActivityIndicator.RemoveFromSuperview( );

                    // if there are now series entries, we're good
                    if ( RockLaunchData.Instance.Data.NoteDB.SeriesList.Count > 0 )
                    {
                        // setup each series entry in our table
                        SetupSeriesEntries( RockLaunchData.Instance.Data.NoteDB.SeriesList );

                        // only update the table if we're still visible
                        if ( IsVisible == true )
                        {
                            NotesTableView.Hidden = false;
                            NotesTableView.Source = new TableSource( this, SeriesEntries, ImageMainPlaceholder, ImageThumbPlaceholder );
                            NotesTableView.ReloadData( );
                        }
                    }
                    else if ( IsVisible == true )
                    {
                        ResultView.Show( MessagesStrings.Series_Error_Title, 
                                         PrivateControlStylingConfig.Result_Symbol_Failed, 
                                         MessagesStrings.Series_Error_Message, 
                                         GeneralStrings.Retry );
                    }
                });
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
                    bool imageExists = TryLoadImage( ref entry.mBillboard, NotesTask.FormatBillboardImageName( entry.Series.Name ) );
                    if ( imageExists == false )
                    {
                        FileCache.Instance.DownloadFileToCache( entry.Series.BillboardUrl, NotesTask.FormatBillboardImageName( entry.Series.Name ), 
                            delegate
                            {
                                Rock.Mobile.Threading.Util.PerformOnUIThread( delegate {
                                    if( IsVisible == true )
                                    {
                                        TryLoadImage( ref entry.mBillboard, NotesTask.FormatBillboardImageName( entry.Series.Name ) );
                                    }
                                });
                            } );
                    }
                }

                // for everything, we care about the thumbnails
                bool thumbExists = TryLoadImage( ref entry.mThumbnail, NotesTask.FormatThumbImageName( entry.Series.Name ) );
                if ( thumbExists == false )
                {
                    FileCache.Instance.DownloadFileToCache( entry.Series.ThumbnailUrl, NotesTask.FormatThumbImageName( entry.Series.Name ), delegate
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate {
                                if( IsVisible == true )
                                {
                                    TryLoadImage( ref entry.mThumbnail, NotesTask.FormatThumbImageName( entry.Series.Name ) );
                                }
                            });
                        } );
                }
            }
        }

        bool TryLoadImage( ref UIImage rImage, string filename )
        {
            bool result = false;

            // does the file exist?
            if ( FileCache.Instance.FileExists( filename ) == true )
            {
                result = ImageLoader.Load( filename, ref rImage );
                if ( result )
                {
                    // let the table refresh
                    NotesTableView.ReloadData( );
                }
            }

            return result;
        }

        /// <summary>
        /// Called when the user pressed the 'Watch' button in the primary cell
        /// </summary>
        public void WatchButtonClicked( )
        {
            NotesWatchUIViewController viewController = Storyboard.InstantiateViewController( "NotesWatchUIViewController" ) as NotesWatchUIViewController;
            viewController.MediaUrl = SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl;
            viewController.ShareUrl = SeriesEntries[ 0 ].Series.Messages[ 0 ].ShareUrl;

            Task.PerformSegue( this, viewController );
        }

        /// <summary>
        /// Called when the user pressed the 'Take Notes' button in the primary cell
        /// </summary>
        public void TakeNotesClicked( )
        {
            // maybe technically a hack...we know our parent is a NoteTask,
            // so cast it so we can use the existing NotesViewController.
            NotesTask noteTask = Task as NotesTask;
            if ( noteTask != null )
            {
                noteTask.NoteController.NoteName = SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                noteTask.NoteController.NoteUrl = SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl;
                noteTask.NoteController.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                Task.PerformSegue( this, noteTask.NoteController );
            }
        }

        public void RowClicked( int row )
        {
            DetailsViewController = new NotesDetailsUIViewController( Task );
            DetailsViewController.Series = SeriesEntries[ row ].Series;
            //DetailsViewController.SeriesBillboard = SeriesEntries[ row ].mBillboard != null ? SeriesEntries[ row ].mBillboard : ImageMainPlaceholder;

            Task.PerformSegue( this, DetailsViewController );
        }
    }
}
