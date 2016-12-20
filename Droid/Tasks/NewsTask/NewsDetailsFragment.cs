
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
using MobileApp.Shared.Config;
using MobileApp.Shared.Strings;
using Android.Text.Method;
using MobileApp.Shared;
using System.IO;
using MobileApp.Shared.PrivateConfig;
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

                string Title { get; set; }
                string Description { get; set; }
                string DeveloperInfo { get; set; }

                public string ReferenceURL { get; protected set; }
                public bool ReferenceURLLaunchesBrowser { get; protected set; }
                public bool IncludeImpersonationToken { get; protected set; }

                string HeaderImageName { get; set; }
                string HeaderImageURL { get; set; }

                public void SetNewsInfo( string title, string description, string developerInfo, string referenceURL, bool referenceURLLaunchesBrowser, bool includeImpersonationToken, string headerImageName, string headerImageURL )
                {
                    // this will be called by either our parent task, or the ourselves if we have a valid bundle.
                    Title = title;
                    Description = description;
                    DeveloperInfo = developerInfo;
                    ReferenceURL = referenceURL;
                    ReferenceURLLaunchesBrowser = referenceURLLaunchesBrowser;
                    IncludeImpersonationToken = includeImpersonationToken;
                    HeaderImageName = headerImageName;
                    HeaderImageURL = headerImageURL;
                }

                Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView ImageBanner { get; set; }
                Bitmap HeaderImage { get; set; }

                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );

                    // if we're being restored by Android, get the detail values from our state bundle
                    if ( savedInstanceState != null )
                    {
                        SetNewsInfo( savedInstanceState.GetString( "Title" ),
                                     savedInstanceState.GetString( "Description" ),
                                     savedInstanceState.GetString( "DeveloperInfo" ),
                                     savedInstanceState.GetString( "ReferenceURL" ),
                                     savedInstanceState.GetBoolean( "ReferenceURLLaunchesBrowser" ),
                                     savedInstanceState.GetBoolean( "IncludeImpersonationToken" ),
                                     savedInstanceState.GetString( "HeaderImageName" ),
                                     savedInstanceState.GetString( "HeaderImageURL" ) );
                    }
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    // at this point our properties should be set.

                    View view = inflater.Inflate(Resource.Layout.News_Details, container, false);
                    view.SetOnTouchListener( this );
                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    // set the banner
                    ImageBanner = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Activity );
                    ( (LinearLayout)view ).AddView( ImageBanner, 0 );

                    int width = NavbarFragment.GetCurrentContainerDisplayWidth( );
                    int height = (int)System.Math.Ceiling( width * PrivateNewsConfig.NewsBannerAspectRatio );
                    ImageBanner.LayoutParameters = new LinearLayout.LayoutParams( width, height );

                    TextView title = view.FindViewById<TextView>( Resource.Id.news_details_title );
                    title.Text = Title.ToUpper( );
                    title.SetSingleLine( );
                    title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    title.SetMaxLines( 1 );
                    title.SetHorizontallyScrolling( true );
                    ControlStyling.StyleUILabel( title, ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );

                    // set the description
                    TextView description = view.FindViewById<TextView>( Resource.Id.news_details_details );
                    description.Text = Description;
                    description.MovementMethod = new ScrollingMovementMethod();
                    ControlStyling.StyleUILabel( description, ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );

                    // if we're in developer mode, add the start / end times for this promotion
                    if ( MobileApp.Shared.Network.RockLaunchData.Instance.Data.DeveloperModeEnabled == true )
                    {
                        // if we're in developer mode, add the start / end times for this promotion
                        if ( MobileApp.Shared.Network.RockLaunchData.Instance.Data.DeveloperModeEnabled == true )
                        {
                            description.Text += DeveloperInfo;
                        }
                    }

                    Button launchUrlButton = view.FindViewById<Button>(Resource.Id.news_details_launch_url);
                    launchUrlButton.Click += (object sender, EventArgs e) => 
                        {
                            // move to the next page...
                            ParentTask.OnClick( this, launchUrlButton.Id );
                        };
                    ControlStyling.StyleButton( launchUrlButton, NewsStrings.LearnMore, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );

                    // hide the button if there's no reference URL.
                    if ( string.IsNullOrEmpty( ReferenceURL ) == true )
                    {
                        launchUrlButton.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        launchUrlButton.Visibility = ViewStates.Visible;
                    }

                    return view;
                }

                void SetupDisplay( View view )
                {
                    // get the placeholder image in case we need it
                    // attempt to load the image from cache. If that doesn't work, use a placeholder
                    HeaderImage = null;

                    bool imageExists = TryLoadBanner( HeaderImageName );
                    if ( imageExists == false )
                    {
                        // use the placeholder and request the image download
                        string widthParam = string.Format( "&width={0}", NavbarFragment.GetContainerDisplayWidth_Landscape( ) );
                        FileCache.Instance.DownloadFileToCache( HeaderImageURL + widthParam, HeaderImageName, null,
                            delegate
                            {
                                TryLoadBanner( HeaderImageName );
                            } );


                        AsyncLoader.LoadImage( PrivateGeneralConfig.NewsDetailsPlaceholder, true, false,
                            delegate( Bitmap imageBmp )
                            {
                                if ( IsFragmentActive == true && imageBmp != null )
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
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    IsFragmentActive = true;

                    if ( ParentTask.TaskReadyForFragmentDisplay == true && View != null )
                    {
                        SetupDisplay( View );
                    }
                }

                public override void TaskReadyForFragmentDisplay( )
                {
                    if ( View != null )
                    {
                        SetupDisplay( View );
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
                                if ( IsFragmentActive == true )
                                {
                                    // if for some reason it loaded corrupt, remove it.
                                    if ( imageBmp == null )
                                    {
                                        FileCache.Instance.RemoveFile( filename );

                                        return false;
                                    }
                                    else
                                    {
                                        FreeImageResources( );

                                        HeaderImage = imageBmp;

                                        ImageBanner.SetImageBitmap( HeaderImage );
                                        ImageBanner.Invalidate( );

                                        Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( ImageBanner, true, null );

                                        return true;
                                    }
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

                public override void OnDestroyView()
                {
                    base.OnDestroyView();

                    FreeImageResources( );
                }

                void FreeImageResources( )
                {
                    if ( HeaderImage != null )
                    {
                        HeaderImage.Recycle( );
                        HeaderImage.Dispose( );
                        HeaderImage = null;
                    }

                    if ( ImageBanner != null && ImageBanner.Drawable != null )
                    {
                        ImageBanner.Drawable.Dispose( );
                        ImageBanner.SetImageBitmap( null );
                    }
                }

                public override void OnSaveInstanceState (Bundle outState)
                {
                    base.OnSaveInstanceState (outState);

                    // save the news detail values
                    outState.PutString( "Title", Title );
                    outState.PutString( "Description", Description );
                    outState.PutString( "DeveloperInfo", DeveloperInfo );
                    outState.PutString( "ReferenceURL", ReferenceURL );
                    outState.PutBoolean( "ReferenceURLLaunchesBrowser", ReferenceURLLaunchesBrowser );
                    outState.PutBoolean( "IncludeImpersonationToken", IncludeImpersonationToken );
                    outState.PutString( "HeaderImageName", HeaderImageName );
                    outState.PutString( "HeaderImageURL", HeaderImageURL );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    IsFragmentActive = false;

                    FreeImageResources( );
                }
            }
        }
    }
}
