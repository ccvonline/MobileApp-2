using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using App.Shared.Notes.Model;
using System.Collections.Generic;
using CoreGraphics;
using App.Shared;
using App.Shared.Config;
using Rock.Mobile.UI;
using System.IO;
using App.Shared.Analytics;
using App.Shared.Network;
using App.Shared.PrivateConfig;
using Rock.Mobile.IO;

namespace iOS
{
    partial class NotesDetailsUIViewController : TaskUIViewController
	{
        public class TableViewDelegate : NavBarRevealHelperDelegate
        {
            TableSource TableSource { get; set; }

            public TableViewDelegate( TableSource tableSource, NavToolbar toolbar ) : base( toolbar )
            {
                TableSource = tableSource;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return TableSource.GetHeightForRow( tableView, indexPath );
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                TableSource.RowSelected( tableView, indexPath );
            }
        }

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
                public UITextView Desc { get; set; }
                public UILabel Date { get; set; }

                public SeriesPrimaryCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

                    // anything that's constant can be set here once in the constructor
                    Image = new UIImageView( );
                    Image.ContentMode = UIViewContentMode.ScaleAspectFit;
                    Image.Layer.AnchorPoint = CGPoint.Empty;
                    AddSubview( Image );

                    Title = new UILabel( );
                    Title.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
                    Title.Layer.AnchorPoint = CGPoint.Empty;
                    Title.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Title.BackgroundColor = UIColor.Clear;
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Title );

                    Date = new UILabel( );
                    Date.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
                    Date.Layer.AnchorPoint = CGPoint.Empty;
                    Date.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Date.BackgroundColor = UIColor.Clear;
                    Date.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Date );

                    Desc = new UITextView( );
                    Desc.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
                    Desc.Layer.AnchorPoint = CGPoint.Empty;
                    Desc.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Desc.BackgroundColor = UIColor.Clear;
                    Desc.TextContainerInset = UIEdgeInsets.Zero;
                    Desc.TextContainer.LineFragmentPadding = 0;
                    Desc.Editable = false;
                    Desc.UserInteractionEnabled = false;
                    AddSubview( Desc );
                }
            }

            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class SeriesCell : UITableViewCell
            {
                public static string Identifier = "SeriesCell";

                public TableSource Parent { get; set; }

                public UILabel Title { get; set; }
                public UILabel Date { get; set; }
                public UILabel Speaker { get; set; }
                public UIButton ListenButton { get; set; }
                public UIButton WatchButton { get; set; }
                public UIButton TakeNotesButton { get; set; }

                public UIView Seperator { get; set; }

                public int RowIndex { get; set; }

                public SeriesCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
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

                    Speaker = new UILabel( );
                    Speaker.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    Speaker.Layer.AnchorPoint = CGPoint.Empty;
                    Speaker.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Speaker.BackgroundColor = UIColor.Clear;
                    Speaker.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Speaker );

                    ListenButton = new UIButton( UIButtonType.Custom );
                    ListenButton.TouchUpInside += (object sender, EventArgs e) => { Parent.RowButtonClicked( RowIndex, 0 ); };
                    ListenButton.Layer.AnchorPoint = CGPoint.Empty;
                    ListenButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.Details_Table_IconSize );
                    ListenButton.SetTitle( PrivateNoteConfig.Series_Table_Listen_Icon, UIControlState.Normal );
                    ListenButton.BackgroundColor = UIColor.Clear;
                    ListenButton.SizeToFit( );
                    AddSubview( ListenButton );

                    WatchButton = new UIButton( UIButtonType.Custom );
                    WatchButton.TouchUpInside += (object sender, EventArgs e) => { Parent.RowButtonClicked( RowIndex, 1 ); };
                    WatchButton.Layer.AnchorPoint = CGPoint.Empty;
                    WatchButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.Details_Table_IconSize );
                    WatchButton.SetTitle( PrivateNoteConfig.Series_Table_Watch_Icon, UIControlState.Normal );
                    WatchButton.BackgroundColor = UIColor.Clear;
                    WatchButton.SizeToFit( );
                    AddSubview( WatchButton );

                    TakeNotesButton = new UIButton( UIButtonType.Custom );
                    TakeNotesButton.TouchUpInside += (object sender, EventArgs e) => { Parent.RowButtonClicked( RowIndex, 2 ); };
                    TakeNotesButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.Details_Table_IconSize );
                    TakeNotesButton.SetTitle( PrivateNoteConfig.Series_Table_TakeNotes_Icon, UIControlState.Normal );
                    TakeNotesButton.Layer.AnchorPoint = CGPoint.Empty;
                    TakeNotesButton.BackgroundColor = UIColor.Clear;
                    TakeNotesButton.SizeToFit( );
                    AddSubview( TakeNotesButton );

                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                }

                public void ToggleListenButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        ListenButton.Enabled = true;
                        ListenButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ), UIControlState.Normal );
                    }
                    else
                    {
                        ListenButton.Enabled = false;
                        ListenButton.SetTitleColor( UIColor.DarkGray, UIControlState.Normal );
                    }
                }

                public void ToggleWatchButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        WatchButton.Enabled = true;
                        WatchButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ), UIControlState.Normal );
                    }
                    else
                    {
                        WatchButton.Enabled = false;
                        WatchButton.SetTitleColor( UIColor.DarkGray, UIControlState.Normal );
                    }
                }

                public void ToggleTakeNotesButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        TakeNotesButton.Enabled = true;
                        TakeNotesButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ), UIControlState.Normal );
                    }
                    else
                    {
                        TakeNotesButton.Enabled = false;
                        TakeNotesButton.SetTitleColor( UIColor.DarkGray, UIControlState.Normal );
                    }
                }
            }

            NotesDetailsUIViewController Parent { get; set; }
            List<MessageEntry> MessageEntries { get; set; }
            Series Series { get; set; }

            nfloat PendingPrimaryCellHeight { get; set; }
            nfloat PendingCellHeight { get; set; }

            public TableSource (NotesDetailsUIViewController parent, List<MessageEntry> messages, Series series )
            {
                Parent = parent;
                MessageEntries = messages;
                Series = series;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return MessageEntries.Count + 1;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );
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
                        if ( PendingPrimaryCellHeight > 0 )
                        {
                            return PendingPrimaryCellHeight;
                        }
                        break;
                    }

                    default:
                    {

                        if ( PendingCellHeight > 0 )
                        {
                            return PendingCellHeight;
                        }
                        break;
                    }
                }

                // If we don't have the cell's height yet (first render), return the table's height
                return tableView.Frame.Height;
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
                    cell = new SeriesPrimaryCell( UITableViewCellStyle.Default, SeriesCell.Identifier );
                    cell.Parent = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                // Banner Image
                cell.Image.Image = Parent.SeriesBillboard;
                cell.Image.SizeToFit( );

                // resize the image to fit the width of the device
                float imageAspect = (float) (cell.Image.Bounds.Height / cell.Image.Bounds.Width);
                cell.Image.Frame = new CGRect( 0, 0, cell.Bounds.Width, cell.Bounds.Width * imageAspect );

                // Title
                cell.Title.Text = Series.Name;
                cell.Title.SizeToFit( );
                cell.Title.Frame = new CGRect( 5, cell.Image.Frame.Bottom + 5, cell.Frame.Width - 10, cell.Title.Frame.Height + 5 );

                // Date
                cell.Date.Text = Series.DateRanges;
                cell.Date.SizeToFit( );
                cell.Date.Frame = new CGRect( 5, cell.Title.Frame.Bottom - 5, cell.Frame.Width - 5, cell.Date.Frame.Height + 5 );

                // Description
                cell.Desc.Text = Series.Description;
                cell.Desc.Bounds = new CGRect( 0, 0, cell.Frame.Width - 10, float.MaxValue );
                cell.Desc.SizeToFit( );
                cell.Desc.Frame = new CGRect( 5, cell.Date.Frame.Bottom, cell.Frame.Width - 10, cell.Desc.Frame.Height + 5 );

                PendingPrimaryCellHeight = cell.Desc.Frame.Bottom + 5;

                return cell;
            }

            UITableViewCell GetStandardCell( UITableView tableView, int row )
            {
                SeriesCell cell = tableView.DequeueReusableCell( SeriesCell.Identifier ) as SeriesCell;

                // if there are no cells to reuse, create a new one
                if ( cell == null )
                {
                    cell = new SeriesCell( UITableViewCellStyle.Default, SeriesCell.Identifier );
                    cell.Parent = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                // update the cell's row index so on button taps we know which one was tapped
                cell.RowIndex = row;

                float rowHeight = 100;

                // Buttons
                cell.TakeNotesButton.Frame = new CGRect( cell.Bounds.Width - cell.TakeNotesButton.Bounds.Width, 
                                                        (rowHeight - cell.TakeNotesButton.Bounds.Height) / 2, 
                                                             cell.TakeNotesButton.Bounds.Width, 
                                                             cell.TakeNotesButton.Bounds.Height );

                cell.WatchButton.Frame = new CGRect( cell.TakeNotesButton.Frame.Left - cell.WatchButton.Bounds.Width, 
                    (rowHeight - cell.WatchButton.Bounds.Height) / 2, 
                                                         cell.WatchButton.Bounds.Width, 
                                                         cell.WatchButton.Bounds.Height );

                cell.ListenButton.Frame = new CGRect( cell.WatchButton.Frame.Left - cell.ListenButton.Bounds.Width, 
                    (rowHeight - cell.ListenButton.Bounds.Height) / 2, 
                    cell.ListenButton.Bounds.Width, 
                    cell.ListenButton.Bounds.Height );

                nfloat availableWidth = cell.Bounds.Width - cell.ListenButton.Bounds.Width - cell.WatchButton.Bounds.Width - cell.TakeNotesButton.Bounds.Width;

                // disable the button if there's no listen URL
                if ( string.IsNullOrEmpty( Series.Messages[ row ].AudioUrl ) )
                {
                    cell.ToggleListenButton( false );
                }
                else
                {
                    cell.ToggleListenButton( true );
                }

                // disable the button if there's no watch URL
                if ( string.IsNullOrEmpty( Series.Messages[ row ].WatchUrl ) )
                {
                    cell.ToggleWatchButton( false );
                }
                else
                {
                    cell.ToggleWatchButton( true );
                }

                // disable the button if there's no note URL
                if ( string.IsNullOrEmpty( Series.Messages[ 0 ].NoteUrl ) )
                {
                    cell.ToggleTakeNotesButton( false );
                }
                else
                {
                    cell.ToggleTakeNotesButton( true );
                }

                // Create the title
                cell.Title.Text = Series.Messages[ row ].Name;
                cell.Title.SizeToFit( );

                // Date
                cell.Date.Text = Series.Messages[ row ].Date;
                cell.Date.SizeToFit( );

                cell.Speaker.Text = Series.Messages[ row ].Speaker;
                cell.Speaker.SizeToFit( );

                // Position the Title & Date in the center to the right of the image
                nfloat totalTextHeight = cell.Title.Bounds.Height + cell.Date.Bounds.Height + cell.Speaker.Bounds.Height + 15;

                cell.Title.Frame = new CGRect( 5, (rowHeight - totalTextHeight) / 2, availableWidth, cell.Title.Frame.Height + 10);
                cell.Date.Frame = new CGRect( cell.Title.Frame.Left, cell.Title.Frame.Bottom - 5, availableWidth, cell.Date.Frame.Height );
                cell.Speaker.Frame = new CGRect( cell.Title.Frame.Left, cell.Date.Frame.Bottom - 5, availableWidth, cell.Speaker.Frame.Height + 5 );

                // add the seperator to the bottom
                cell.Seperator.Frame = new CGRect( 0, rowHeight - 1, cell.Bounds.Width, 1 );

                PendingCellHeight = rowHeight;

                return cell;
            }

            public void RowButtonClicked( int row, int buttonIndex )
            {
                Parent.RowClicked( row, buttonIndex );
            }
        }

        /// <summary>
        /// A wrapper class that consolidates the message, it's thumbnail and podcast status
        /// </summary>
        public class MessageEntry
        {
            public Series.Message Message { get; set; }
        }

        public Series Series { get; set; }
        public UIImage SeriesBillboard { get; set; }
        public List<MessageEntry> Messages { get; set; }
        bool IsVisible { get; set; }
        UITableView SeriesTable { get; set; }

        public NotesDetailsUIViewController ( Task parentTask )
		{
            Task = parentTask;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            // setup the table view and general background view colors
            SeriesTable = new UITableView( );
            SeriesTable.Layer.AnchorPoint = CGPoint.Empty;
            SeriesTable.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            SeriesTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            View.AddSubview( SeriesTable );

            // setup the messages list
            Messages = new List<MessageEntry>();
            TableSource source = new TableSource( this, Messages, Series );
            SeriesTable.Source = source;
            SeriesTable.Delegate = new TableViewDelegate( source, Task.NavToolbar );

            // log the series they tapped on.
            MessageAnalytic.Instance.Trigger( MessageAnalytic.BrowseSeries, Series.Name );


            IsVisible = true;

            for ( int i = 0; i < Series.Messages.Count; i++ )
            {
                MessageEntry messageEntry = new MessageEntry();
                Messages.Add( messageEntry );

                messageEntry.Message = Series.Messages[ i ];
            }


            // do we have the real image?
            if( TryLoadImage( NotesTask.FormatBillboardImageName( Series.Name ) ) == false )
            {
                // no, so use a placeholder and request the actual image
                SeriesBillboard = new UIImage( NSBundle.MainBundle.BundlePath + "/" + PrivateGeneralConfig.NewsDetailsPlaceholder );

                // request!
                FileCache.Instance.DownloadFileToCache( Series.BillboardUrl, NotesTask.FormatBillboardImageName( Series.Name ), 
                    delegate
                    {
                        Rock.Mobile.Threading.Util.PerformOnUIThread( 
                            delegate
                            {
                                if( IsVisible == true )
                                {
                                    TryLoadImage( NotesTask.FormatBillboardImageName( Series.Name ) );
                                }
                            });
                    } );
            }
        }

        bool TryLoadImage( string imageName )
        {
            bool success = false;

            if( FileCache.Instance.FileExists( imageName ) == true )
            {
                MemoryStream imageStream = null;
                try
                {
                    imageStream = (MemoryStream)FileCache.Instance.LoadFile( imageName );

                    NSData imageData = NSData.FromStream( imageStream );
                    SeriesBillboard = new UIImage( imageData );

                    SeriesTable.ReloadData( );

                    success = true;
                }
                catch( Exception )
                {
                    FileCache.Instance.RemoveFile( imageName );
                    Rock.Mobile.Util.Debug.WriteLine( string.Format( "Image {0} is corrupt. Removing.", imageName ) );
                }
                imageStream.Dispose( );
            }

            return success;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsVisible = false;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // adjust the table height for our navbar.
            // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
            // or our changes will be overwritten
            SeriesTable.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height );
        }

        public override void LayoutChanged( )
        {
            base.LayoutChanged( );

            // if the layout is changed, the simplest way to fix the UI is to recreate the table source
            TableSource source = new TableSource( this, Messages, Series );
            SeriesTable.Source = source;
            SeriesTable.Delegate = new TableViewDelegate( source, Task.NavToolbar );
            SeriesTable.ReloadData( );
        }

        public void RowClicked( int row, int buttonIndex )
        {
           // 0 would be the audio button
            if ( buttonIndex == 0 )
            {
                NotesWatchUIViewController viewController = new NotesWatchUIViewController( );
                viewController.MediaUrl = Series.Messages[ row ].AudioUrl;
                viewController.ShareUrl = Series.Messages[ row ].ShareUrl;
                viewController.Name = Series.Messages[ row ].Name;
                viewController.AudioOnly = true;

                Task.PerformSegue( this, viewController );
            }
            // 1 would be the watch button
            else if ( buttonIndex == 1 )
            {
                NotesWatchUIViewController viewController = new NotesWatchUIViewController( );
                viewController.MediaUrl = Series.Messages[ row ].WatchUrl;
                viewController.ShareUrl = Series.Messages[ row ].ShareUrl;
                viewController.Name = Series.Messages[ row ].Name;
                viewController.AudioOnly = false;

                Task.PerformSegue( this, viewController );
            }
            // and 1 would be the second button, which is Notes
            else if ( buttonIndex == 2 )
            {
                // maybe technically a hack...we know our parent is a NoteTask,
                // so cast it so we can use the existing NotesViewController.
                NotesTask noteTask = Task as NotesTask;
                if ( noteTask != null )
                {
                    noteTask.NoteController.NoteName = Series.Messages[ row ].Name;
                    noteTask.NoteController.NoteUrl = Series.Messages[ row ].NoteUrl;
                    noteTask.NoteController.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                    Task.PerformSegue( this, noteTask.NoteController );
                }
            }
        }
	}
}
