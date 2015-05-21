
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
using Android.Graphics;
using Rock.Mobile.UI;
using App.Shared.Config;
using App.Shared.Strings;
using Android.Text.Method;
using App.Shared;
using System.IO;
using App.Shared.PrivateConfig;
using Rock.Mobile.IO;
using Rock.Mobile.PlatformSpecific.Android;
using Rock.Mobile.PlatformSpecific.Android.Util;
using Rock.Mobile.Animation;
using System.Threading;

namespace Droid
{
    namespace Tasks
    {
        namespace News
        {
            public class NewsDetailsFragment : TaskFragment
            {
                bool IsFragmentActive { get; set; }

                public App.Shared.Network.RockNews NewsItem { get; set; }

                Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView ImageBanner { get; set; }
                Bitmap HeaderImage { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.News_Details, container, false);
                    view.SetOnTouchListener( this );
                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    // set the banner
                    ImageBanner = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Activity );
                    ( (LinearLayout)view ).AddView( ImageBanner, 0 );

                    int height = (int)System.Math.Ceiling( NavbarFragment.GetContainerDisplayWidth( ) * PrivateNewsConfig.NewsBannerAspectRatio );
                    ImageBanner.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, height );

                    TextView title = view.FindViewById<TextView>( Resource.Id.news_details_title );
                    title.Text = NewsItem.Title;
                    title.SetSingleLine( );
                    title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    title.SetMaxLines( 1 );
                    title.SetHorizontallyScrolling( true );
                    ControlStyling.StyleUILabel( title, ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );

                    // set the description
                    TextView description = view.FindViewById<TextView>( Resource.Id.news_details_details );
                    description.Text = NewsItem.Description;
                    description.MovementMethod = new ScrollingMovementMethod();
                    ControlStyling.StyleUILabel( description, ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );

                    Button launchUrlButton = view.FindViewById<Button>(Resource.Id.news_details_launch_url);
                    launchUrlButton.Click += (object sender, EventArgs e) => 
                        {
                            // move to the next page..somehow.
                            ParentTask.OnClick( this, launchUrlButton.Id );
                        };
                    ControlStyling.StyleButton( launchUrlButton, NewsStrings.LearnMore, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );

                    // hide the button if there's no reference URL.
                    if ( string.IsNullOrEmpty( NewsItem.ReferenceURL ) == true )
                    {
                        launchUrlButton.Visibility = ViewStates.Invisible;
                    }


                    // get the placeholder image in case we need it
                    // attempt to load the image from cache. If that doesn't work, use a placeholder
                    HeaderImage = null;

                    bool imageExists = TryLoadBanner( NewsItem.HeaderImageName );
                    if ( imageExists == false )
                    {
                        // use the placeholder and request the image download
                        FileCache.Instance.DownloadFileToCache( NewsItem.HeaderImageURL, NewsItem.HeaderImageName, delegate
                            {
                                TryLoadBanner( NewsItem.HeaderImageName );
                            } );


                        AsyncLoader.LoadImage( PrivateGeneralConfig.NewsDetailsPlaceholder, true, false,
                            delegate( Bitmap imageBmp )
                            {
                                if ( IsFragmentActive == true )
                                {
                                    HeaderImage = imageBmp;
                                    ImageBanner.SetImageBitmap( HeaderImage );
                                    ImageBanner.Invalidate( );

                                    Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( ImageBanner, true, null );

                                    return true;
                                }

                                return false;
                            } );
                    }

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    IsFragmentActive = true;
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
                                if ( IsFragmentActive == true )
                                {
                                    // if for some reason it loaded corrupt, remove it.
                                    if ( imageBmp == null )
                                    {
                                        FileCache.Instance.RemoveFile( filename );
                                    }

                                    HeaderImage = imageBmp;

                                    if( ImageBanner.Drawable != null )
                                    {
                                        ImageBanner.Drawable.Dispose( );
                                    }

                                    ImageBanner.SetImageBitmap( HeaderImage );
                                    ImageBanner.Invalidate( );

                                    Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( ImageBanner, true, null );

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

                public override void OnPause()
                {
                    base.OnPause();

                    IsFragmentActive = false;

                    if ( HeaderImage != null )
                    {
                        HeaderImage.Dispose( );
                        HeaderImage = null;
                    }

                    if ( ImageBanner.Drawable != null )
                    {
                        ImageBanner.Drawable.Dispose( );
                        ImageBanner.SetImageBitmap( null );
                    }
                }
            }
        }
    }
}
