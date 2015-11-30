using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;
using App.Shared.Config;
using System.Collections.Generic;
using App.Shared;
using App.Shared.Strings;
using App.Shared.PrivateConfig;

namespace iOS
{
	partial class ConnectMainPageViewController : TaskUIViewController
	{
        public class TableSource : UITableViewSource 
        {
            /// <summary>
            /// Definition for the primary (top) cell, which contains the map and search field
            /// </summary>
            class PrimaryCell : UITableViewCell
            {
                public static string Identifier = "PrimaryCell";

                public UIImageView Image { get; set; }
                public UILabel Title { get; set; }

                public PrimaryCell( CGSize parentSize, UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                    SelectionStyle = UITableViewCellSelectionStyle.None;

                    Image = new UIImageView( );
                    Image.BackgroundColor = UIColor.Yellow;
                    Image.ContentMode = UIViewContentMode.ScaleAspectFill;
                    Image.Layer.AnchorPoint = CGPoint.Empty;
                    AddSubview( Image );

                    // Banner Image
                    Image.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + PrivateConnectConfig.MainPageHeaderImage );
                    Image.SizeToFit( );

                    // resize the image to fit the width of the device
                    nfloat imageAspect = Image.Bounds.Height / Image.Bounds.Width;
                    Image.Frame = new CGRect( 0, 0, parentSize.Width, parentSize.Width * imageAspect );


                    Title = new UILabel( );
                    Title.Text = ConnectStrings.Main_Connect_Header;
                    Title.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
                    Title.Layer.AnchorPoint = CGPoint.Empty;
                    Title.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    Title.TextAlignment = UITextAlignment.Center;
                    Title.Frame = new CGRect( 5, Image.Frame.Bottom, parentSize.Width - 10, 0 );
                    Title.SizeToFit( );
                    AddSubview( Title );
                }
            }

            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class SeperatorCell : UITableViewCell
            {
                public static string Identifier = "SeperatorCell";

                public TableSource Parent { get; set; }
                public UILabel Title { get; set; }

                public SeperatorCell( CGSize parentSize, UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                    SelectionStyle = UITableViewCellSelectionStyle.None;

                    Title = new UILabel( );
                    Title.Text = ConnectStrings.Main_Connect_Seperator;
                    Title.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
                    Title.Layer.AnchorPoint = CGPoint.Empty;
                    Title.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    Title.TextAlignment = UITextAlignment.Center;
                    Title.Frame = new CGRect( 5, 0, parentSize.Width - 10, 0 );
                    Title.SizeToFit( );
                    AddSubview( Title );
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
                public UILabel SubTitle { get; set; }
                public UILabel Chevron { get; set; }

                public UIView Seperator { get; set; }

                public SeriesCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Image = new UIImageView( );
                    Image.ContentMode = UIViewContentMode.ScaleAspectFit;
                    Image.Layer.AnchorPoint = CGPoint.Empty;
                    AddSubview( Image );

                    Title = new UILabel( );
                    Title.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Medium_FontSize );
                    Title.Layer.AnchorPoint = CGPoint.Empty;
                    Title.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Title );

                    SubTitle = new UILabel( );
                    SubTitle.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    SubTitle.Layer.AnchorPoint = CGPoint.Empty;
                    SubTitle.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    SubTitle.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( SubTitle );

                    Chevron = new UILabel( );
                    AddSubview( Chevron );
                    Chevron.Layer.AnchorPoint = CGPoint.Empty;
                    Chevron.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateConnectConfig.MainPage_Table_IconSize );
                    Chevron.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Chevron.Text = PrivateConnectConfig.MainPage_Table_Navigate_Icon;
                    Chevron.SizeToFit( );

                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                }
            }
            ConnectMainPageViewController Parent { get; set; }

            nfloat PendingPrimaryCellHeight { get; set; }
            nfloat PendingSeriesCellHeight { get; set; }
            nfloat PendingSeperatorCellHeight { get; set; }

            PrimaryCell PrimaryTableCell { get; set; }
            SeperatorCell SeperatorTableCell { get; set; }

            public TableSource ( ConnectMainPageViewController parent )
            {
                Parent = parent;

                // create the primary table cell
                PrimaryTableCell = new PrimaryCell( parent.View.Bounds.Size, UITableViewCellStyle.Default, PrimaryCell.Identifier );
                PrimaryTableCell.Bounds = parent.View.Bounds;
                PendingPrimaryCellHeight = PrimaryTableCell.Title.Frame.Bottom;// + Rock.Mobile.Graphics.Util.UnitToPx( 2 );


                // create the seperator table cell
                SeperatorTableCell = new SeperatorCell( parent.View.Bounds.Size, UITableViewCellStyle.Default, SeperatorCell.Identifier );
                SeperatorTableCell.Bounds = parent.View.Bounds;
                PendingSeperatorCellHeight = SeperatorTableCell.Title.Frame.Bottom;// + Rock.Mobile.Graphics.Util.UnitToPx( 2 );
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return Parent.GetStartedEntries.Count + Parent.GetEngagedEntries.Count + 2;
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
                // If we don't have the cell's height yet (first render), return the table's height
                nfloat rowHeight = tableView.Frame.Height;

                // Depending on the row, we either want the primary cell's height,
                // or a standard row's height.
                if ( indexPath.Row == 0 )
                {
                    if ( PendingPrimaryCellHeight > 0 )
                    {
                        rowHeight = PendingPrimaryCellHeight;
                    }
                }
                else if ( ( indexPath.Row - 1 ) == Parent.GetStartedEntries.Count )
                {
                    if ( PendingSeperatorCellHeight > 0 )
                    {
                        rowHeight = PendingSeperatorCellHeight;
                    }
                }
                else
                {
                    if ( PendingSeriesCellHeight > 0 )
                    {
                        rowHeight = PendingSeriesCellHeight;
                    }
                }

                return rowHeight;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                // if it's our top entry, make it the graphic and title
                if ( indexPath.Row == 0 )
                {
                    return PrimaryTableCell;
                }
                // otherwise, see if it should be a getStarted row or a getEngaged row.
                else
                {
                    // get the row index relative to getStarted
                    int getStartedRowIndex = indexPath.Row - 1;

                    // if it should be a get started row
                    if ( getStartedRowIndex < Parent.GetStartedEntries.Count )
                    {
                        // hide the seperator if this is the last item in the GetStarted list.
                        bool showSeperator = getStartedRowIndex == Parent.GetStartedEntries.Count - 1 ? false : true;

                        return GetActivityCell( tableView, Parent.GetStartedEntries[ getStartedRowIndex ], showSeperator );
                    }
                    // else if it should be the seperator between GetStarted / GetEngaged
                    else if ( getStartedRowIndex == Parent.GetStartedEntries.Count )
                    {
                        return SeperatorTableCell;
                    }
                    else
                    {
                        // create the row index relative to getEngaged.
                        int getEngagedRowIndex = getStartedRowIndex - Parent.GetStartedEntries.Count - 1;

                        return GetActivityCell( tableView, Parent.GetEngagedEntries[ getEngagedRowIndex ], true );
                    }
                }
            }

            UITableViewCell GetActivityCell( UITableView tableView, ConnectLink link, bool showSeperator )
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
                cell.Image.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + link.ImageName );
                cell.Image.SizeToFit( );

                nfloat topPadding = 10;

                // force the image to be sized according to the height of the cell
                cell.Image.Frame = new CGRect( 0, 
                    topPadding, 
                    PrivateConnectConfig.MainPage_ThumbnailDimension, 
                    PrivateConnectConfig.MainPage_ThumbnailDimension );

                nfloat availableTextWidth = cell.Bounds.Width - cell.Chevron.Bounds.Width - cell.Image.Bounds.Width - 10;

                // Chevron
                nfloat chevronYPos = cell.Image.Frame.Top + ((PrivateConnectConfig.MainPage_ThumbnailDimension - cell.Chevron.Bounds.Height) / 2);
                cell.Chevron.Layer.Position = new CGPoint( cell.Bounds.Width - cell.Chevron.Bounds.Width - 5, chevronYPos );

                // Create the title
                cell.Title.Text = link.Title.ToUpper( );
                cell.Title.SizeToFit( );

                cell.SubTitle.Text = link.SubTitle;
                cell.SubTitle.SizeToFit( );

                // Position the Title & Date in the center to the right of the image
                nfloat totalTextHeight = cell.Title.Bounds.Height + cell.SubTitle.Bounds.Height - 6;

                cell.Title.Frame = new CGRect( cell.Image.Frame.Right + 10, ((PrivateConnectConfig.MainPage_ThumbnailDimension - totalTextHeight) / 2) + topPadding, availableTextWidth - 5, cell.Title.Frame.Height );
                cell.SubTitle.Frame = new CGRect( cell.Image.Frame.Right + 10, cell.Title.Frame.Bottom - 6, availableTextWidth - 5, cell.Title.Frame.Height );

                // add the seperator to the bottom
                if ( showSeperator )
                {
                    cell.Seperator.Hidden = false;
                    cell.Seperator.Frame = new CGRect( 0, cell.Image.Frame.Bottom + 10, cell.Bounds.Width, 1 );

                    PendingSeriesCellHeight = cell.Seperator.Frame.Bottom;
                }
                else
                {
                    cell.Seperator.Hidden = true;
                    PendingSeriesCellHeight = cell.Image.Frame.Bottom + 10;
                }

                return cell;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // let the parent know it should reveal the nav bar
                Parent.RowClicked( indexPath.Row - 1 );
            }

            public void RowClicked( int row )
            {
                Parent.RowClicked( row );
            }
        }

        public List<ConnectLink> GetStartedEntries { get; set; }
        public List<ConnectLink> GetEngagedEntries { get; set; }

		public ConnectMainPageViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( App.Shared.Config.ControlStylingConfig.BackgroundColor );

            GetStartedEntries = ConnectLink.BuildGetStartedList( );
            GetEngagedEntries = ConnectLink.BuildGetEngagedList( );

            ConnectTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            ConnectTableView.Source = new TableSource( this );
        }

        public override void LayoutChanged( )
        {
            base.LayoutChanged( );

            // if the layout is changed, the simplest way to fix the UI is to recreate the table source
            ConnectTableView.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height );
            ConnectTableView.ReloadData( );
        }

        public void RowClicked( int rowIndex )
        {
            if ( rowIndex > -1 )
            {
                ConnectLink linkEntry = null;

                // is this a getStarted row?
                if ( rowIndex < GetStartedEntries.Count )
                {
                    linkEntry = GetStartedEntries[ rowIndex ];
                }
                // if it's GREATER (not greater / equal) then it's a GetEngaged. 
                // if it were equal, then it'd be the seperator and we don't care.
                else if ( rowIndex > GetStartedEntries.Count )
                {
                    int getEngagedRowIndex = rowIndex - GetStartedEntries.Count - 1;

                    linkEntry = GetEngagedEntries[ getEngagedRowIndex ];
                }

                // did they pick something valid?
                if ( linkEntry != null )
                {
                    // GroupFinder is unique in that it doesn't use a webView.
                    if ( linkEntry.Title == ConnectStrings.Main_Connect_GroupFinder )
                    {
                        TaskUIViewController viewController = Storyboard.InstantiateViewController( "GroupFinderViewController" ) as TaskUIViewController;
                        Task.PerformSegue( this, viewController );
                    }
                    else
                    {
                        TaskWebViewController.HandleUrl( false, true, linkEntry.Url, Task, this, false, false );
                    }
                }
            }
        }
	}
}
