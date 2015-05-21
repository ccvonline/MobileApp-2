using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using App.Shared.Network;
using CoreGraphics;
using App.Shared.Config;
using Rock.Mobile.UI;
using App.Shared.Strings;
using System.IO;
using App.Shared;
using App.Shared.PrivateConfig;
using Rock.Mobile.IO;

namespace iOS
{
	public class NewsDetailsUIViewController : TaskUIViewController
	{
        public RockNews NewsItem { get; set; }

        bool IsVisible { get; set; }

        UILabel NewsTitle { get; set; }
        UITextView NewsDescription { get; set; }
        UIImageView ImageBanner { get; set; }
        UIButton LearnMoreButton { get; set; }


		public NewsDetailsUIViewController( ) : base ( )
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // setup the news title
            NewsTitle = new UILabel( );
            NewsTitle.Layer.AnchorPoint = CGPoint.Empty;
            View.AddSubview( NewsTitle );
            ControlStyling.StyleUILabel( NewsTitle, ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
            NewsTitle.Text = NewsItem.Title;
            NewsTitle.SizeToFit( );

            // populate the details view with this news item.
            NewsDescription = new UITextView( );
            NewsDescription.Layer.AnchorPoint = CGPoint.Empty;
            View.AddSubview( NewsDescription );
            NewsDescription.Text = NewsItem.Description;
            NewsDescription.BackgroundColor = UIColor.Clear;
            NewsDescription.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
            NewsDescription.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
            NewsDescription.TextContainerInset = UIEdgeInsets.Zero;
            NewsDescription.TextContainer.LineFragmentPadding = 0;
            NewsDescription.Editable = false;

            // we should always assume images are in cache. If they aren't, show a placeholder.
            // It is not our job to download them.
            ImageBanner = new UIImageView( );
            ImageBanner.Layer.AnchorPoint = CGPoint.Empty;

            // scale the image down to fit the contents of the window, but allow cropping.
            ImageBanner.BackgroundColor = UIColor.Green;
            ImageBanner.ContentMode = UIViewContentMode.ScaleAspectFill;
            ImageBanner.ClipsToBounds = true;

            View.AddSubview( ImageBanner );

            // do we have the real image?
            if( TryLoadHeaderImage( NewsItem.HeaderImageName ) == false )
            {
                // no, so use a placeholder and request the actual image
                ImageBanner.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + PrivateGeneralConfig.NewsDetailsPlaceholder );

                // resize the image to fit the width of the device
                nfloat imageAspect = ImageBanner.Image.Size.Height / ImageBanner.Image.Size.Width;
                ImageBanner.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Width * imageAspect );

                // request!
                FileCache.Instance.DownloadFileToCache( NewsItem.HeaderImageURL, NewsItem.HeaderImageName, delegate
                    {
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate {
                            if( IsVisible == true )
                            {
                                TryLoadHeaderImage( NewsItem.HeaderImageName );
                            }
                        });
                    } );
            }


            // finally setup the Learn More button
            LearnMoreButton = UIButton.FromType( UIButtonType.System );
            View.AddSubview( LearnMoreButton );
            LearnMoreButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    TaskWebViewController viewController = new TaskWebViewController( NewsItem.ReferenceURL, Task );
                    Task.PerformSegue( this, viewController );
                };

            // if there's no URL associated with this news item, hide the learn more button.
            if ( string.IsNullOrEmpty( NewsItem.ReferenceURL ) == true )
            {
                LearnMoreButton.Hidden = true;
            }
            ControlStyling.StyleButton( LearnMoreButton, NewsStrings.LearnMore, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            LearnMoreButton.SizeToFit( );
        }

        public bool TryLoadHeaderImage( string imageName )
        {
            bool success = false;

            if( FileCache.Instance.FileExists( imageName ) == true )
            {
                MemoryStream imageStream = null;
                try
                {
                    imageStream = (MemoryStream)FileCache.Instance.LoadFile( NewsItem.HeaderImageName );

                    NSData imageData = NSData.FromStream( imageStream );
                    ImageBanner.Image = new UIImage( imageData );

                    // resize the image to fit the width of the device
                    nfloat imageAspect = ImageBanner.Image.Size.Height / ImageBanner.Image.Size.Width;
                    ImageBanner.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Width * imageAspect );

                    success = true;
                }
                catch( Exception )
                {
                    FileCache.Instance.RemoveFile( NewsItem.HeaderImageName );
                    Rock.Mobile.Util.Debug.WriteLine( string.Format( "Image {0} is corrupt. Removing.", NewsItem.HeaderImageName ) );
                }
                imageStream.Dispose( );
            }

            return success;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            IsVisible = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsVisible = false;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            float textHorzPadding = 20;
            float textVertPadding = 50;

            // resize the image to fit the width of the device
            nfloat imageAspect = ImageBanner.Image.Size.Height / ImageBanner.Image.Size.Width;
            ImageBanner.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Width * imageAspect );

            // adjust the news title to have padding on the left and right.
            NewsTitle.Frame = new CGRect( textHorzPadding, ImageBanner.Frame.Bottom + ((textVertPadding - NewsTitle.Frame.Height) / 2), View.Bounds.Width - (textHorzPadding * 2), NewsTitle.Bounds.Height );

            // put the learn more button at the bottom center
            nfloat learnMoreWidth = View.Bounds.Width * .45f;
            LearnMoreButton.Frame = new CGRect( ( View.Bounds.Width - learnMoreWidth ) / 2, View.Bounds.Height - LearnMoreButton.Bounds.Height - Task.NavToolbar.Bounds.Height - 10, learnMoreWidth, LearnMoreButton.Bounds.Height );

            // and fit the news description in between the title and learn more
            NewsDescription.Frame = new CGRect( textHorzPadding, ImageBanner.Frame.Bottom + textVertPadding, View.Bounds.Width - (textHorzPadding * 2), 0 );
            NewsDescription.SizeToFit( );


            // determine whether we can use the height of the description, or limit it and enable scrolling
            nfloat paddedLearnMoreTop = ( LearnMoreButton.Frame.Top - 10 );
            nfloat descriptionHeight = NewsDescription.Frame.Bottom > paddedLearnMoreTop ? paddedLearnMoreTop - NewsTitle.Frame.Bottom - 10 : NewsDescription.Frame.Height;

            NewsDescription.Frame = new CGRect( NewsDescription.Frame.Left, NewsDescription.Frame.Top, NewsDescription.Frame.Width, descriptionHeight );

            // if the description needs to scroll, enable user interaction (which disables the nav toolbar)
            CGSize size = NewsDescription.SizeThatFits( new CGSize( NewsDescription.Bounds.Width, NewsDescription.Bounds.Height ) );
            if ( size.Height > NewsDescription.Frame.Height )
            {
                NewsDescription.UserInteractionEnabled = true;
            }
            else
            {
                NewsDescription.UserInteractionEnabled = false;
            }
        }
	}
}
