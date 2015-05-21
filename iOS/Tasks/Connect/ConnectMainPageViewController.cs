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
                public TableSource TableSource { get; set; }
                public UILabel Title { get; set; }
                //public UILabel BottomBanner { get; set; }

                public PrimaryCell( CGSize parentSize, UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

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
                    Title.SizeToFit( );
                    Title.Frame = new CGRect( 5, Image.Frame.Bottom, Frame.Width - 10, Title.Frame.Height );
                    AddSubview( Title );


                    /*BottomBanner = new UILabel( );
                    BottomBanner.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    BottomBanner.Layer.AnchorPoint = new CGPoint( 0, 0 );
                    BottomBanner.Text = ConnectStrings.Main_Connect_OtherWays;
                    BottomBanner.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    BottomBanner.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Table_Footer_Color );
                    BottomBanner.TextAlignment = UITextAlignment.Center;

                    BottomBanner.SizeToFit( );
                    BottomBanner.Bounds = new CGRect( 0, 0, parentSize.Width, BottomBanner.Bounds.Height + 10 );
                    BottomBanner.Layer.Position = new CGPoint( 0, Title.Frame.Bottom + 5 );
                    AddSubview( BottomBanner );*/
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

                    Chevron = new UILabel( );
                    AddSubview( Chevron );
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
            nfloat PendingCellHeight { get; set; }

            PrimaryCell PrimaryTableCell { get; set; }

            public TableSource ( ConnectMainPageViewController parent )
            {
                Parent = parent;

                PrimaryTableCell = new PrimaryCell( parent.View.Bounds.Size, UITableViewCellStyle.Default, PrimaryCell.Identifier );
                PrimaryTableCell.TableSource = this;

                // take the parent table's width so we inherit its width constraint
                PrimaryTableCell.Bounds = parent.View.Bounds;

                // configure the cell colors
                PrimaryTableCell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                PrimaryTableCell.SelectionStyle = UITableViewCellSelectionStyle.None;

                //PendingPrimaryCellHeight = PrimaryTableCell.BottomBanner.Frame.Bottom;
                PendingPrimaryCellHeight = PrimaryTableCell.Title.Frame.Bottom;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return Parent.LinkEntries.Count + 1;
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
                    nfloat imageAspect = PrimaryTableCell.Image.Bounds.Height / PrimaryTableCell.Image.Bounds.Width;
                    PrimaryTableCell.Image.Frame = new CGRect( 0, 0, tableView.Bounds.Width, tableView.Bounds.Width * imageAspect );

                    PrimaryTableCell.Title.SizeToFit( );
                    PrimaryTableCell.Title.Frame = new CGRect( 5, PrimaryTableCell.Image.Frame.Bottom, tableView.Bounds.Width - 10, PrimaryTableCell.Title.Frame.Height );

                    //PrimaryTableCell.BottomBanner.SizeToFit( );
                    //PrimaryTableCell.BottomBanner.Bounds = new CGRect( 0, 0, tableView.Bounds.Width, PrimaryTableCell.BottomBanner.Bounds.Height + 10 );
                    //PrimaryTableCell.BottomBanner.Layer.Position = new CGPoint( 0, PrimaryTableCell.Title.Frame.Bottom + 5 );

                    //PendingPrimaryCellHeight = PrimaryTableCell.BottomBanner.Frame.Bottom;
                    PendingPrimaryCellHeight = PrimaryTableCell.Title.Frame.Bottom;

                    return PrimaryTableCell;
                }
                else
                {
                    return GetStandardCell( tableView, indexPath.Row - 1 );
                }
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
                cell.Image.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + Parent.LinkEntries[ row ].ImageName );
                cell.Image.SizeToFit( );

                // force the image to be sized according to the height of the cell
                cell.Image.Frame = new CGRect( 0, 
                    10, 
                    PrivateConnectConfig.MainPage_ThumbnailDimension, 
                    PrivateConnectConfig.MainPage_ThumbnailDimension );

                nfloat availableTextWidth = cell.Bounds.Width - cell.Chevron.Bounds.Width - cell.Image.Bounds.Width - 10;

                // Chevron
                cell.Chevron.Layer.Position = new CGPoint( cell.Bounds.Width - (cell.Chevron.Bounds.Width / 2) - 5, (PrivateConnectConfig.MainPage_ThumbnailDimension + 10) / 2 );

                // Create the title
                cell.Title.Text = Parent.LinkEntries[ row ].Title;
                cell.Title.SizeToFit( );

                // Position the Title & Date in the center to the right of the image
                nfloat totalTextHeight = cell.Title.Bounds.Height - 1;
                cell.Title.Frame = new CGRect( cell.Image.Frame.Right + 10, ((PrivateConnectConfig.MainPage_ThumbnailDimension + 10) - totalTextHeight) / 2, availableTextWidth - 5, cell.Title.Frame.Height );

                // add the seperator to the bottom
                cell.Seperator.Frame = new CGRect( 0, cell.Image.Frame.Bottom + 10, cell.Bounds.Width, 1 );

                PendingCellHeight = cell.Seperator.Frame.Bottom;

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

        public List<ConnectLink> LinkEntries { get; set; }

		public ConnectMainPageViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( App.Shared.Config.ControlStylingConfig.BackgroundColor );

            LinkEntries = ConnectLink.BuildList( );

            // ensure the first link entry is always group finder.
            ConnectLink link = new ConnectLink( );
            link.Title = ConnectStrings.Main_Connect_GroupFinder;
            link.ImageName = PrivateConnectConfig.GroupFinder_IconImage;
            LinkEntries.Insert( 0, link );

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
                if ( rowIndex == 0 )
                {
                    TaskUIViewController viewController = Storyboard.InstantiateViewController( "GroupFinderViewController" ) as TaskUIViewController;
                    Task.PerformSegue( this, viewController );
                }
                else
                {
                    TaskWebViewController viewController = new TaskWebViewController( LinkEntries[ rowIndex ].Url, Task );
                    Task.PerformSegue( this, viewController );
                }
            }
        }
	}
}
