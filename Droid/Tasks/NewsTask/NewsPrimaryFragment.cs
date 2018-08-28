
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
using MobileApp.Shared.Network;
using Android.Graphics;
using MobileApp.Shared;
using System.IO;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using MobileApp.Shared.Config;
using Rock.Mobile.PlatformSpecific.Android.UI;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.IO;
using Rock.Mobile.PlatformSpecific.Android.Util;

namespace Droid
{
    namespace Tasks
    {
        namespace News
        {
            public class PortraitNewsArrayAdapter : ListAdapter
            {
                NewsPrimaryFragment ParentFragment { get; set; }

                public PortraitNewsArrayAdapter( NewsPrimaryFragment parentFragment )
                {
                    ParentFragment = parentFragment;
                }

                public override int Count 
                {
                    get { return ParentFragment.News.Count; }
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    SingleNewsListItem listItem = convertView as SingleNewsListItem;
                    if ( listItem == null )
                    {
                        listItem = new SingleNewsListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );

                        int height = (int)System.Math.Ceiling( NavbarFragment.GetCurrentContainerDisplayWidth( ) * PrivateNewsConfig.NewsMainAspectRatio );
                        listItem.LayoutParameters = new AbsListView.LayoutParams( ViewGroup.LayoutParams.WrapContent, height );
                        listItem.HasImage = false;
                    }
                    else
                    {
                        listItem.FreeImageResources( );
                    }

                    // if we have a valid item
                    if ( position < ParentFragment.News.Count )
                    {
                        // is our image ready?
                        if ( ParentFragment.News[ position ].Image != null )
                        {
                            if ( listItem.HasImage == false )
                            {
                                listItem.HasImage = true;
                                Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( listItem.Billboard, true, null );
                            }

                            listItem.Billboard.SetImageBitmap( ParentFragment.News[ position ].Image );
                        }
                        // only show the "Loading..." if the image isn't actually downloaded.
                        else if ( ParentFragment.Placeholder != null )
                        {
                            listItem.Billboard.SetImageBitmap( ParentFragment.Placeholder );
                        }
                        else
                        {
                            listItem.Billboard.SetImageBitmap( null );
                        }

                        if ( ParentFragment.News[ position ].News.Developer_Private )
                        {
                            listItem.PrivateOverlay.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            listItem.PrivateOverlay.Visibility = ViewStates.Gone;
                        }
                    }
                    else
                    {
                        listItem.Billboard.SetImageBitmap( null );
                        listItem.PrivateOverlay.Visibility = ViewStates.Gone;
                    }

                    return base.AddView( listItem );
                }
            }

            /// <summary>
            /// Implementation of the news row that has a single image
            /// </summary>
            class SingleNewsListItem : Rock.Mobile.PlatformSpecific.Android.UI.ListAdapter.ListItemView
            {
                RelativeLayout RelativeLayout { get; set; }
                public AspectScaledImageView Billboard { get; set; }
                public bool HasImage { get; set; }
                public TextView PrivateOverlay { get; set; }

                public SingleNewsListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    RelativeLayout = new RelativeLayout( context );
                    RelativeLayout.LayoutParameters = new AbsListView.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    AddView( RelativeLayout );

                    Billboard = new AspectScaledImageView( context );
                    Billboard.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );

                    Billboard.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    RelativeLayout.AddView( Billboard );


                    PrivateOverlay = new TextView( context );
                    PrivateOverlay.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    PrivateOverlay.SetBackgroundColor( Android.Graphics.Color.Red );
                    PrivateOverlay.Alpha = .60f;
                    PrivateOverlay.Text = "Private";
                    PrivateOverlay.Gravity = GravityFlags.Center;
                    RelativeLayout.AddView( PrivateOverlay );
                }

                public override void Destroy()
                {
                    FreeImageResources( );
                }

                public void FreeImageResources( )
                {
                    if ( Billboard != null && Billboard.Drawable != null )
                    {
                        Billboard.Drawable.Dispose( );
                        Billboard.SetImageBitmap( null );
                    }
                }
            }

            public class NewsEntry
            {
                public RockNews News { get; set; }
                public Bitmap Image { get; set; }
            }

            public class NewsPrimaryFragment : TaskFragment
            {
                public List<NewsEntry> News { get; set; }

                ListView ListView { get; set; }

                public Bitmap Placeholder { get; set; }

                bool FragmentActive { get; set; }

                public NewsPrimaryFragment( ) : base( )
                {
                    News = new List<NewsEntry>();
                }

                public void OnClick( int position )
                {
                    ParentTask.OnClick( this, position );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.News_Primary, container, false);
                    view.SetOnTouchListener( this );
                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    return view;
                }

                public override void TaskReadyForFragmentDisplay( )
                {
                    if ( View != null )
                    {
                        SetupDisplay( View );
                    }
                }

                void SetupDisplay( View view )
                {
                    ListView = view.FindViewById<ListView>( Resource.Id.news_primary_list );
                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => 
                        {
                            // in portrait mode, it's fine to use the list
                            // for click detection
                            OnClick( e.Position );
                        };
                    ListView.SetOnTouchListener( this );

                    ListView.Adapter = new PortraitNewsArrayAdapter( this );


                    // load the placeholder image
                    AsyncLoader.LoadImage( PrivateGeneralConfig.NewsMainPlaceholder, true, false,
                        delegate( Bitmap imageBmp )
                        {
                            if ( FragmentActive == true && imageBmp != null )
                            {
                                Placeholder = imageBmp;
                                imageBmp = null;

                                RefreshListView( );
                                return true;
                            }

                            return false;
                        } );

                    // here we're simply trying to load the images that are already
                    // stored
                    for ( int i = 0; i < News.Count; i++ )
                    {
                        TryLoadCachedImageAsync( News[ i ], News[ i ].News.ImageName );
                    }
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    ListView.Adapter = new PortraitNewsArrayAdapter( this );
                }

                public override void OnResume()
                {
                    base.OnResume();

                    FragmentActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    if ( ParentTask.TaskReadyForFragmentDisplay == true && View != null )
                    {
                        SetupDisplay( View );
                    }
                }

                public void UpdateNews( List<RockNews> sourceNews )
                {
                    // free existing news
                    FreeImageResources( );

                    News.Clear( );

                    // copy the new news.
                    int i;
                    for( i = 0; i < sourceNews.Count; i++ )
                    {
                        NewsEntry newsEntry = new NewsEntry();
                        News.Add( newsEntry );

                        newsEntry.News = sourceNews[ i ];
                        // see if we can load the file
                        bool fileFound = TryLoadCachedImageAsync( newsEntry, newsEntry.News.ImageName );
                        if ( fileFound == false )
                        {
                            // if not, download it
                            string widthParam = string.Format( "width={0}", NavbarFragment.GetCurrentContainerDisplayWidth( ) );
                            string requestUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( newsEntry.News.ImageURL, widthParam );

                            FileCache.Instance.DownloadFileToCache( requestUrl, newsEntry.News.ImageName, null,
                                delegate
                                {
                                    // and THEN load it
                                    TryLoadCachedImageAsync( newsEntry, newsEntry.News.ImageName );
                                } );
                        }
                    }
                }

                void RefreshListView( )
                {
                    if ( IsVisible )
                    {
                        if ( ListView != null && ListView.Adapter != null )
                        {
                            ( ListView.Adapter as PortraitNewsArrayAdapter ).NotifyDataSetChanged( );
                        }
                    }
                }

                bool TryLoadCachedImageAsync( NewsEntry entry, string imageName )
                {
                    // if it exists, spawn a thread to load and decode it
                    if ( FileCache.Instance.FileExists( imageName ) == true )
                    {
                        AsyncLoader.LoadImage( imageName, false, false,
                            delegate( Bitmap imageBmp)
                            {
                                if ( FragmentActive == true )
                                {
                                    // if for some reason it loaded corrupt, remove it.
                                    if ( imageBmp == null )
                                    {
                                        FileCache.Instance.RemoveFile( imageName );

                                        return false;
                                    }
                                    else
                                    {
                                        // we might be replacing an existing image, so be sure to dispose our reference to it.
                                        if( entry.Image != null )
                                        {
                                            entry.Image.Dispose( );
                                            entry.Image = null;
                                        }

                                        // flag that we do HAVE the image
                                        entry.Image = imageBmp;

                                        RefreshListView( );

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

                public override void OnPause()
                {
                    base.OnPause();

                    FragmentActive = false;

                    FreeImageResources( );
                }

                public override void OnDestroyView()
                {
                    base.OnDestroyView();

                    FreeImageResources( );
                }

                void FreeImageResources( )
                {
                    // unload all resource
                    if ( ListView != null && ListView.Adapter != null )
                    {
                        ( (ListAdapter)ListView.Adapter ).Destroy( );
                    }

                    // be sure to dump the existing news images so
                    // Dalvik knows it can use the memory
                    foreach ( NewsEntry newsEntry in News )
                    {
                        if ( newsEntry.Image != null )
                        {
                            newsEntry.Image.Recycle( );
                            newsEntry.Image.Dispose( );
                            newsEntry.Image = null;
                        }
                    }

                    if ( Placeholder != null )
                    {
                        Placeholder.Recycle( );
                        Placeholder.Dispose( );
                        Placeholder = null;
                    }
                }
            }
        }
    }
}
