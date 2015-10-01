
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
using App.Shared;
using System.IO;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using App.Shared.Config;
using Rock.Mobile.PlatformSpecific.Android.UI;
using App.Shared.PrivateConfig;
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

            public class LandscapeNewsArrayAdapter : ListAdapter
            {
                NewsPrimaryFragment ParentFragment { get; set; }
                public LandscapeNewsArrayAdapter( NewsPrimaryFragment parentFragment )
                {
                    ParentFragment = parentFragment;
                }

                public override int Count 
                {
                    get 
                    { 
                        // start with a top row
                        int numItems = 1;

                        // each row after will show two items
                        double remainingItems = ParentFragment.News.Count - 1;
                        if ( remainingItems > 0 )
                        {
                            // take the rows we'll need and round up
                            double rowsNeeded = remainingItems / 2.0f;

                            rowsNeeded = Math.Ceiling( rowsNeeded );

                            numItems += (int)rowsNeeded;
                        }
                        return numItems;
                    }
                }

                public void RowItemClicked( int itemIndex )
                {
                    ParentFragment.OnClick( itemIndex );
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    ListAdapter.ListItemView item = null;

                    if ( position == 0 )
                    {
                        item = GetPrimaryView( position, convertView, parent );
                    }
                    else
                    {
                        // for standard views, subtract one from the position so we
                        // can more easily convert from row index to correct left index image.
                        item = GetStandardView( position - 1, convertView, parent );
                    }

                    return AddView( item );
                }

                ListAdapter.ListItemView GetPrimaryView( int position, View convertView, ViewGroup parent )
                {
                    SingleNewsListItem listItem = convertView as SingleNewsListItem;
                    if ( listItem == null )
                    {
                        listItem = new SingleNewsListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );

                        int width = NavbarFragment.GetCurrentContainerDisplayWidth( );
                        int height = (int)System.Math.Ceiling( width * PrivateNewsConfig.NewsBannerAspectRatio );
                        listItem.LayoutParameters = new AbsListView.LayoutParams( width, height );
                        listItem.HasImage = false;
                    }
                    else
                    {
                        listItem.FreeImageResources( );
                    }

                    // if we have a valid item
                    if ( position < ParentFragment.News.Count )
                    {
                        // is the image valid?
                        if ( ParentFragment.News[ position ].Image != null )
                        {
                            if ( listItem.HasImage == false )
                            {
                                listItem.HasImage = true;
                                Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( listItem.Billboard, true, null );
                            }

                            listItem.Billboard.SetImageBitmap( ParentFragment.News[ position ].Image );
                        }
                        // then should we use the (details) placeholder?
                        else if ( ParentFragment.DetailsPlaceholder != null )
                        {
                            listItem.Billboard.SetImageBitmap( ParentFragment.DetailsPlaceholder );
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

                    return listItem;
                }

                ListAdapter.ListItemView GetStandardView( int rowIndex, View convertView, ViewGroup parent )
                {
                    // convert the position to the appropriate image index.
                    int leftImageIndex = 1 + ( rowIndex * 2 );

                    // create the item if needed
                    DoubleNewsListItem seriesItem = convertView as DoubleNewsListItem;
                    if ( seriesItem == null )
                    {
                        seriesItem = new DoubleNewsListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        seriesItem.ParentAdapter = this;

                        int height = (int)System.Math.Ceiling( (NavbarFragment.GetCurrentContainerDisplayWidth( ) / 2) * PrivateNewsConfig.NewsMainAspectRatio );
                        seriesItem.LayoutParameters = new AbsListView.LayoutParams( ViewGroup.LayoutParams.WrapContent, height );

                        seriesItem.LeftHasImage = false;
                        seriesItem.RightHasImage = false;
                    }
                    else
                    {
                        seriesItem.FreeImageResources( );
                    }

                    seriesItem.LeftImageIndex = leftImageIndex;

                    // first set the left item
                    if ( leftImageIndex < ParentFragment.News.Count )
                    {
                        // is there an image ready?
                        if ( ParentFragment.News[ leftImageIndex ].Image != null )
                        {
                            if ( seriesItem.LeftHasImage == false )
                            {
                                seriesItem.LeftHasImage = true;
                                Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( seriesItem.LeftImage, true, null );
                            }

                            seriesItem.LeftImage.SetImageBitmap( ParentFragment.News[ leftImageIndex ].Image );
                        }
                        // should we use the placeholder instead?
                        else if ( ParentFragment.Placeholder != null )
                        {
                            seriesItem.LeftImage.SetImageBitmap( ParentFragment.Placeholder );
                        }
                        else
                        {
                            // then just clear it out
                            seriesItem.LeftImage.SetImageBitmap( null );
                        }

                        if ( ParentFragment.News[ leftImageIndex ].News.Developer_Private )
                        {
                            seriesItem.LeftPrivateOverlay.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            seriesItem.LeftPrivateOverlay.Visibility = ViewStates.Gone;
                        }
                    }
                    else
                    {
                        seriesItem.LeftImage.SetImageBitmap( null );
                        seriesItem.LeftPrivateOverlay.Visibility = ViewStates.Gone;
                    }

                    // now if there's a right item, set it
                    int rightImageIndex = leftImageIndex + 1;
                    if ( rightImageIndex < ParentFragment.News.Count )
                    {
                        // is there an image ready?
                        if ( ParentFragment.News[ rightImageIndex ].Image != null )
                        {
                            if ( seriesItem.RightHasImage == false )
                            {
                                seriesItem.RightHasImage = true;
                                Rock.Mobile.PlatformSpecific.Android.UI.Util.FadeView( seriesItem.RightImage, true, null );
                            }

                            seriesItem.RightImage.SetImageBitmap( ParentFragment.News[ rightImageIndex ].Image );
                        }
                        // should we use the placeholder instead?
                        else if ( ParentFragment.Placeholder != null )
                        {
                            seriesItem.RightImage.SetImageBitmap( ParentFragment.Placeholder );
                        }
                        else
                        {
                            // then just clear it out
                            seriesItem.RightImage.SetImageBitmap( null );
                        }

                        if ( ParentFragment.News[ rightImageIndex ].News.Developer_Private )
                        {
                            seriesItem.RightPrivateOverlay.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            seriesItem.RightPrivateOverlay.Visibility = ViewStates.Gone;
                        }
                    }
                    else
                    {
                        seriesItem.RightImage.SetImageBitmap( null );
                        seriesItem.RightPrivateOverlay.Visibility = ViewStates.Gone;
                    }

                    return seriesItem;
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

            /// <summary>
            /// Implementation of the news row that has two images side by side
            /// </summary>
            class DoubleNewsListItem : Rock.Mobile.PlatformSpecific.Android.UI.ListAdapter.ListItemView
            {
                public LandscapeNewsArrayAdapter ParentAdapter { get; set; }

                public int LeftImageIndex { get; set; }

                public RelativeLayout LeftLayout { get; set; }
                public Button LeftButton { get; set; }
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView LeftImage { get; set; }
                public bool LeftHasImage { get; set; }
                public TextView LeftPrivateOverlay { get; set; }

                public RelativeLayout RightLayout { get; set; }
                public Button RightButton { get; set; }
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView RightImage { get; set; }
                public bool RightHasImage { get; set; }
                public TextView RightPrivateOverlay { get; set; }

                public DoubleNewsListItem( Context context ) : base( context )
                {
                    Orientation = Orientation.Horizontal;

                    SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    LeftLayout = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    LeftLayout.LayoutParameters = new LinearLayout.LayoutParams( NavbarFragment.GetCurrentContainerDisplayWidth( ) / 2, LayoutParams.WrapContent );
                    AddView( LeftLayout );

                    LeftImage = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    LeftImage.LayoutParameters = new LinearLayout.LayoutParams( NavbarFragment.GetCurrentContainerDisplayWidth( ) / 2, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)LeftImage.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    LeftImage.SetScaleType( ImageView.ScaleType.CenterCrop );
                    LeftLayout.AddView( LeftImage );

                    LeftButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    LeftButton.LayoutParameters = LeftImage.LayoutParameters;
                    LeftButton.Background = null;
                    LeftButton.Click += (object sender, EventArgs e ) =>
                        {
                            // notify our parent that the image index was clicked
                            ParentAdapter.RowItemClicked( LeftImageIndex );
                        };
                    LeftLayout.AddView( LeftButton );

                    LeftPrivateOverlay = new TextView( context );
                    LeftPrivateOverlay.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    LeftPrivateOverlay.SetBackgroundColor( Android.Graphics.Color.Red );
                    LeftPrivateOverlay.Alpha = .60f;
                    LeftPrivateOverlay.Text = "Private";
                    LeftPrivateOverlay.Gravity = GravityFlags.Center;
                    LeftLayout.AddView( LeftPrivateOverlay );


                    RightLayout = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    RightLayout.LayoutParameters = new LinearLayout.LayoutParams( NavbarFragment.GetCurrentContainerDisplayWidth( ) / 2, LayoutParams.WrapContent );
                    AddView( RightLayout );

                    RightImage = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    RightImage.LayoutParameters = new LinearLayout.LayoutParams( NavbarFragment.GetCurrentContainerDisplayWidth( ) / 2, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)RightImage.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    RightImage.SetScaleType( ImageView.ScaleType.CenterCrop );
                    RightLayout.AddView( RightImage );

                    RightButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    RightButton.LayoutParameters = RightImage.LayoutParameters;
                    RightButton.Background = null;
                    RightButton.Click += (object sender, EventArgs e ) =>
                        {
                            // notify our parent that the image index was clicked
                            ParentAdapter.RowItemClicked( LeftImageIndex + 1 );
                        };
                    RightLayout.AddView( RightButton );

                    RightPrivateOverlay = new TextView( context );
                    RightPrivateOverlay.LayoutParameters = new RelativeLayout.LayoutParams( NavbarFragment.GetCurrentContainerDisplayWidth( ) / 2, ViewGroup.LayoutParams.WrapContent );
                    RightPrivateOverlay.SetBackgroundColor( Android.Graphics.Color.Red );
                    RightPrivateOverlay.Alpha = .60f;
                    RightPrivateOverlay.Text = "Private";
                    RightPrivateOverlay.Gravity = GravityFlags.Center;
                    RightLayout.AddView( RightPrivateOverlay );
                }

                public override void Destroy()
                {
                    FreeImageResources( );
                }

                public void FreeImageResources( )
                {
                    if ( LeftImage != null && LeftImage.Drawable != null )
                    {
                        LeftImage.Drawable.Dispose( );
                        LeftImage.SetImageBitmap( null );
                    }

                    if ( RightImage != null && RightImage.Drawable != null )
                    {
                        RightImage.Drawable.Dispose( );
                        RightImage.SetImageBitmap( null );
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
                public Bitmap DetailsPlaceholder { get; set; }

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
                            // in landscape wide, we only want the first item. All the rest
                            // are handled by the rows themselves
                            if ( MainActivity.SupportsLandscapeWide( ) == true )
                            {
                                if( e.Position == 0 )
                                {
                                    OnClick( e.Position );
                                }
                            }
                            else
                            {
                                // in portrait mode, it's fine to use the list
                                // for click detection
                                OnClick( e.Position );
                            }
                        };
                    ListView.SetOnTouchListener( this );

                    if ( MainActivity.SupportsLandscapeWide( ) == true )
                    {
                        ListView.Adapter = new LandscapeNewsArrayAdapter( this );
                    }
                    else
                    {
                        ListView.Adapter = new PortraitNewsArrayAdapter( this );
                    }


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

                    // and if we're a landscape wide device, load the details placeholder as well -- we'll need it.
                    if ( MainActivity.SupportsLandscapeWide( ) )
                    {
                        AsyncLoader.LoadImage( PrivateGeneralConfig.NewsDetailsPlaceholder, true, false,
                            delegate( Bitmap imageBmp )
                            {
                                if ( FragmentActive == true && imageBmp != null )
                                {
                                    DetailsPlaceholder = imageBmp;
                                    imageBmp = null;

                                    RefreshListView( );
                                    return true;
                                }

                                return false;
                            } );
                    }

                    // here we're simply trying to load the images that are already
                    // stored
                    for ( int i = 0; i < News.Count; i++ )
                    {
                        // the top image should use the header if we're a landscape wide device.
                        if ( MainActivity.SupportsLandscapeWide( ) && i == 0 )
                        {
                            TryLoadCachedImageAsync( News[ i ], News[ i ].News.HeaderImageName );
                        }
                        else
                        {
                            TryLoadCachedImageAsync( News[ i ], News[ i ].News.ImageName );
                        }
                    }
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    if ( MainActivity.SupportsLandscapeWide( ) == true )
                    {
                        ListView.Adapter = new LandscapeNewsArrayAdapter( this );
                    }
                    else
                    {
                        ListView.Adapter = new PortraitNewsArrayAdapter( this );
                    }
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

                        // if this is the top news item on a landscape device, use it's 'Header' image instead.
                        if ( MainActivity.SupportsLandscapeWide( ) && i == 0 )
                        {
                            bool fileFound = TryLoadCachedImageAsync( newsEntry, newsEntry.News.HeaderImageName );
                            if ( fileFound == false )
                            {
                                // if not, download it
                                string widthParam = string.Format( "&width={0}", NavbarFragment.GetContainerDisplayWidth_Landscape( ) );
                                FileCache.Instance.DownloadFileToCache( newsEntry.News.HeaderImageURL + widthParam, newsEntry.News.HeaderImageName, 
                                    delegate
                                    {
                                        // and THEN load it
                                        TryLoadCachedImageAsync( newsEntry, newsEntry.News.HeaderImageName );
                                    } );
                            }
                        }
                        else
                        {
                            // see if we can load the file
                            bool fileFound = TryLoadCachedImageAsync( newsEntry, newsEntry.News.ImageName );
                            if ( fileFound == false )
                            {
                                // if not, download it
                                string widthParam = string.Format( "&width={0}", NavbarFragment.GetContainerDisplayWidth_Landscape( ) );
                                FileCache.Instance.DownloadFileToCache( newsEntry.News.ImageURL + widthParam, newsEntry.News.ImageName, 
                                    delegate
                                    {
                                        // and THEN load it
                                        TryLoadCachedImageAsync( newsEntry, newsEntry.News.ImageName );
                                    } );
                            }
                        }
                    }
                }

                void RefreshListView( )
                {
                    if ( IsVisible )
                    {
                        if ( ListView != null && ListView.Adapter != null )
                        {
                            if ( MainActivity.SupportsLandscapeWide( ) == true )
                            {
                                ( ListView.Adapter as LandscapeNewsArrayAdapter ).NotifyDataSetChanged( );
                            }
                            else
                            {
                                ( ListView.Adapter as PortraitNewsArrayAdapter ).NotifyDataSetChanged( );
                            }
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

                    if ( DetailsPlaceholder != null )
                    {
                        DetailsPlaceholder.Recycle( );
                        DetailsPlaceholder.Dispose( );
                        DetailsPlaceholder = null;
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
