using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using MobileApp.Shared.Network;
using CoreGraphics;
using MobileApp.Shared.Config;
using Rock.Mobile.UI;
using MobileApp.Shared;
using System.IO;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.IO;
using MobileApp.Shared.Analytics;

namespace iOS
{
	partial class NewsMainUIViewController : TaskUIViewController
	{
        public class PortraitTableSource : UITableViewSource 
        {
            NewsMainUIViewController Parent { get; set; }

            List<NewsEntry> News { get; set; }
            UIImage ImagePlaceholder { get; set; }

            nfloat PendingCellHeight { get; set; }

            public PortraitTableSource (NewsMainUIViewController parent, List<NewsEntry> newsList, UIImage imagePlaceholder)
            {
                Parent = parent;

                News = newsList;

                ImagePlaceholder = imagePlaceholder;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return News.Count;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // notify our parent
                Parent.RowClicked( indexPath.Row );
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                // check the height of the image and let that be the height for this row
                if ( PendingCellHeight > 0 )
                {
                    return PendingCellHeight;
                }
                else
                {
                    return tableView.Bounds.Height;
                }
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                PrimaryCell cell = tableView.DequeueReusableCell (PrimaryCell.Identifier) as PrimaryCell;

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new PrimaryCell (UITableViewCellStyle.Default, PrimaryCell.Identifier);
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                }
                cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                // set the image for the cell
                if ( News[ indexPath.Row ].Image != null )
                {
                    cell.ContentView.Layer.Contents = News[ indexPath.Row ].Image.CGImage;
                }
                else
                {
                    cell.ContentView.Layer.Contents = ImagePlaceholder.CGImage;
                }

                cell.PrivateOverlay.Hidden = !News[ indexPath.Row ].News.Developer_Private;

                // scale down the image to the width of the device
                nfloat imageWidth = cell.ContentView.Layer.Contents.Width;
                nfloat imageHeight = cell.ContentView.Layer.Contents.Height;

                nfloat imageAspect = PrivateNewsConfig.NewsMainAspectRatio;
                cell.Bounds = new CGRect( 0, 0, tableView.Bounds.Width, tableView.Bounds.Width * imageAspect );
                cell.PrivateOverlay.Frame = new CGRect( 0, 0, cell.Bounds.Width, 30 );

                PendingCellHeight = cell.Bounds.Height;
                return cell;
            }
        }

        class PrimaryCell : UITableViewCell
        {
            public static string Identifier = "PrimaryCell";
            public UILabel PrivateOverlay { get; set; }

            public PrimaryCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
            {
                PrivateOverlay = new UILabel( );
                PrivateOverlay.Layer.AnchorPoint = CGPoint.Empty;
                PrivateOverlay.Text = "Private";
                PrivateOverlay.BackgroundColor = UIColor.Red;
                PrivateOverlay.Alpha = .60f;
                PrivateOverlay.TextColor = UIColor.Black;
                PrivateOverlay.TextAlignment = UITextAlignment.Center;
                AddSubview( PrivateOverlay );
            }
        }

        public class NewsEntry
        {
            public RockNews News { get; set; }
            public UIImage Image { get; set; }
        }
        List<NewsEntry> News { get; set; }

        bool IsVisible { get; set; }
        UIImage ImagePlaceholder { get; set; }
        UIImage HeaderImagePlaceholder { get; set; }

        UITableView NewsTableView { get; set; }

		public NewsMainUIViewController (IntPtr handle) : base (handle)
		{
            News = new List<NewsEntry>( );

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + PrivateGeneralConfig.NewsMainPlaceholder;
            ImagePlaceholder = new UIImage( imagePath );
		}

        PortraitTableSource PortraitSource { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NewsTableView = new UITableView( );
            View.AddSubview( NewsTableView );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            IsVisible = true;

            // populate our table
            PortraitSource = new PortraitTableSource( this, News, ImagePlaceholder );

            NewsTableView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            NewsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            LoadAndDownloadImages( );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsVisible = false;
        }

        public void UpdateNews( List<RockNews> sourceNews )
        {
            // clear the existing news
            News.Clear( );

            // copy the source into our news objects
            foreach ( RockNews rockEntry in sourceNews )
            {
                NewsEntry newsEntry = new NewsEntry();
                News.Add( newsEntry );

                newsEntry.News = rockEntry;
            }
        }

        public void LoadAndDownloadImages( )
        {
            // go through the news
            for( int i = 0;  i < News.Count; i++ )
            {
                NewsEntry newsEntry = News[ i ];
                if ( TryLoadCachedImage( newsEntry, newsEntry.News.ImageName ) == false )
                {
                    // it failed, so download it and try again.
                    string widthParam = string.Format( "&width={0}", View.Bounds.Width * UIScreen.MainScreen.Scale );
                    string requestUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( newsEntry.News.ImageURL, widthParam );

                    FileCache.Instance.DownloadFileToCache( requestUrl, newsEntry.News.ImageName, null,
                        delegate
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    if ( IsVisible == true )
                                    {
                                        TryLoadCachedImage( newsEntry, newsEntry.News.ImageName );
                                    }
                                } );
                        } );
                }
            }
        }

        bool TryLoadCachedImage( NewsEntry entry, string imageName )
        {
            bool success = false;

            // check the billboard
            if( FileCache.Instance.FileExists( imageName ) == true )
            {
                MemoryStream imageStream = (MemoryStream)FileCache.Instance.LoadFile( imageName );
                if ( imageStream != null )
                {
                    try
                    {
                        // grab the data
                        NSData imageData = NSData.FromStream( imageStream );
                        entry.Image = new UIImage( imageData, UIScreen.MainScreen.Scale );

                        // refresh the table if it's ready. It's possible we're prepping news items before 
                        // we've presented the UI (where the tableView gets created)
                        if( NewsTableView != null )
                        {
                            NewsTableView.ReloadData( );
                        }

                        success = true;
                    }
                    catch( Exception e )
                    {
                        FileCache.Instance.RemoveFile( imageName );
                        Rock.Mobile.Util.Debug.WriteLine( string.Format( "Image {0} is corrupt. Removing. (Exception: {1})", imageName, e.Message ) );
                    }
                    imageStream.Dispose( );
                }
            }

            return success;
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            // make sure this isn't being called before we've loaded
            if ( NewsTableView != null )
            {
                NewsTableView.Source = PortraitSource;
                

                // adjust the table height for our navbar.
                // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
                // or our changes will be overwritten
                NewsTableView.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height );
                NewsTableView.ReloadData( );
            }
        }

        public void RowClicked( int row )
        {
            if ( row < News.Count )
            {
                // mark that they tapped this item.
                NewsAnalytic.Instance.Trigger( NewsAnalytic.Read, News[ row ].News.Title );

                if ( News[ row ].News.SkipDetailsPage == true && string.IsNullOrEmpty( News[ row ].News.ReferenceURL ) == false )
                {
                    // if this is an app-url, then let the task (which forwards it to the springboard) handle it.
                    if( SpringboardViewController.IsAppURL( News[ row ].News.ReferenceURL ) == true )
                    {
                        Task.HandleAppURL( News[ row ].News.ReferenceURL );
                    }
                    else
                    {
                        // copy the news item's relevant members. That way, if we're running in debug,
                        // and they want to override the news item, we can do that below.
                        string newsUrl = News[ row ].News.ReferenceURL;
                        bool newsImpersonation = News[ row ].News.IncludeImpersonationToken;
                        bool newsExternalBrowser = News[ row ].News.ReferenceUrlLaunchesBrowser;

                        // If we're running a debug build, see if we should override the news
                        #if DEBUG
                        if( DebugConfig.News_Override_Item == true )
                        {
                            newsUrl = DebugConfig.News_Override_ReferenceURL;
                            newsImpersonation = DebugConfig.News_Override_IncludeImpersonationToken;
                            newsExternalBrowser = DebugConfig.News_Override_ReferenceUrlLaunchesBrowser;
                        }
                        #endif

                        TaskWebViewController.HandleUrl( newsExternalBrowser, newsImpersonation, newsUrl, Task, this, false, false, false );
                    }
                }
                else
                {
                    NewsDetailsUIViewController viewController = new NewsDetailsUIViewController();
                    viewController.NewsItem = News[ row ].News;

                    Task.PerformSegue( this, viewController );
                }
            }
        }
	}
}
