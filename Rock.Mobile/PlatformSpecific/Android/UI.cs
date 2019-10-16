#if __ANDROID__
using System;
using Android.App;
using Android.Widget;
using Android.Webkit;
using Android.Views;
using Android.Views.InputMethods;
using Android.Util;
using System.Drawing;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using Rock.Mobile.Animation;
using System.Collections.Generic;
using Android.Content;
using Android;

namespace Rock.Mobile.PlatformSpecific.Android.UI
{
    /// <summary>
    /// Subclass of Android's ScrollView to allow us to disable scrolling.
    /// </summary>
    public class LockableScrollView : ScrollView
    {
        public delegate bool OnInterceptTouchEventDelegate( MotionEvent ev );
        public OnInterceptTouchEventDelegate OnTouchIntercept { get; set; }

        public delegate void OnScrollChangedDelegate( float delta );
        public OnScrollChangedDelegate OnChangedScroll { get; set; }

        /// <summary>
        /// True when the scroll view can scroll. False when it cannot.
        /// </summary>
        public bool ScrollEnabled { get; set; }

        public LockableScrollView( Context c ) : base( c )
        {
            ScrollEnabled = true;
        }

        public LockableScrollView( Context c, global::Android.Util.IAttributeSet ias ) : base( c, ias )
        {
            ScrollEnabled = true;
        }

        //This is a total hack but it works perfectly.
        //For some reason, when focus is changed to a text element, the
        //RelativeView gets focus first. Since it's at 0, 0,
        //The scrollView wants to scroll to the TOP, then when the editText
        //gets focus, it jumps back down to its position.
        // This is not an acceptable long term solution, but I really need to move on right now.
        public override void ScrollTo(int x, int y)
        {
            //base.ScrollTo(x, y);
        }

        public override void ScrollBy(int x, int y)
        {
            //base.ScrollBy(x, y);
        }

        public void ForceScrollTo(int x, int y)
        {
            base.ScrollTo(x, y);
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            // verify from our parent we can scroll, and that scrolling is enabled
            if( ScrollEnabled == true )
            {
                if( OnChangedScroll == null || OnTouchIntercept( ev ) == true )
                {
                    return base.OnInterceptTouchEvent(ev);
                }
            }

            return false;
        }

        protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
        {
            base.OnScrollChanged(l, t, oldl, oldt);

            if( OnChangedScroll != null )
            {
                OnChangedScroll( t - oldt );
            }
        }
    }

    /// <summary>
    /// Because list item references generally aren't stored by a list adapter,
    /// properties of a list item can store references to objects like bitmaps that need to be freed.
    /// This ListAdapter tracks all added items and exposes methods for calling Destroy and allowing
    /// each list item to release any references its holding.
    /// 
    /// Bottom line, when implenting a list in Android, derive the adapter from this.
    /// </summary>
    public abstract class ListAdapter : BaseAdapter
    {
        protected HashSet<ListItemView> Items { get; set; }

        public ListAdapter( ) : base( )
        {
            Items = new HashSet<ListItemView>( );
        }

        public override Java.Lang.Object GetItem (int position) 
        {
            return null;
        }

        public override long GetItemId (int position) 
        {
            return 0;
        }

        /// <summary>
        /// This needs to be called by the derived "GetView" so we can track the items created.
        /// </summary>
        public View AddView( ListItemView newView )
        {
            Items.Add( newView );

            return newView;
        }
        
        public virtual void Destroy( )
        {
            foreach ( ListItemView item in Items )
            {
                item.Destroy( );
            }
        }
        
        public abstract class ListItemView : LinearLayout
        {
            public ListItemView( Context context ) : base( context )
            {
            }

            public abstract void Destroy( );
        }
    }
    
    class WebLayout : RelativeLayout
    {
        class WebViewLayoutClient : WebViewClient
        {
            public WebLayout Parent { get; set; }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);

                Parent.OnPageFinished( view, url );
            }

            public override void OnReceivedError(WebView view, ClientError errorCode, string description, string failingUrl)
            {
                base.OnReceivedError(view, errorCode, description, failingUrl);

                Parent.OnReceivedError( errorCode, description, failingUrl );
            }

            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                return Parent.ShouldOverrideUrlLoading(view, url);
            }
        }

        WebView WebView { get; set; }
        ProgressBar ProgressBar { get; set; }
        string ExternalToken { get; set; }

        public delegate void PageLoaded( bool result, string forwardUrl );
        PageLoaded PageLoadedHandler { get; set; }

        //disable CS0618, "This method is obsolete on Android". CookieSyncManager.CreateInstance is causing it, but we need it on older Android versions.
#pragma warning disable 0618
        public WebLayout( global::Android.Content.Context context ) : base( context )
        {
            // required for pre-21 android
            CookieSyncManager.CreateInstance( context.ApplicationContext );

            CookieManager.Instance.RemoveAllCookie( );

            // we will go ahead and use the Activity context, rather than Application Context.
            // This fixes a crash when popups are displayed.
            // We WERE using ApplicationContext to fix a memory leak, but that seems to be gone now,
            // after testing on Android 4.4, 6.1, 6.2, and 7.0 (It's possible it was never that bad to begin with, and
            // only seemed like an issue because of the way worse image memory leaks from 9-2015)
            WebView = new WebView( context );
            WebView.Settings.SaveFormData = false;

            WebView.ClearFormData( );
            WebView.SetWebViewClient( new WebViewLayoutClient( ) { Parent = this } );
            WebView.Settings.JavaScriptEnabled = true;
            WebView.Settings.SetSupportZoom(true);
            WebView.Settings.BuiltInZoomControls = false;
            WebView.Settings.LoadWithOverviewMode = true; //Load 100% zoomed out
            WebView.ScrollBarStyle = ScrollbarStyles.OutsideOverlay;
            WebView.ScrollbarFadingEnabled = true;
            WebView.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );

            WebView.VerticalScrollBarEnabled = true;
            WebView.HorizontalScrollBarEnabled = true;
            AddView( WebView );

            ProgressBar = new ProgressBar( context );
            ProgressBar.Indeterminate = true;
            ProgressBar.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( 0 ) );
            ProgressBar.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (RelativeLayout.LayoutParams)ProgressBar.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
            AddView( ProgressBar );
            ProgressBar.BringToFront();
        }

        public void ResetCookies( )
        {
            CookieManager.Instance.RemoveAllCookie( );
        }
#pragma warning restore 0618
        
        public void LoadUrl( string url, string externalToken, PageLoaded loadedHandler )
        {
            PageLoadedHandler = loadedHandler;

            // if the url begins with this, we'll launch it in an external browser
            ExternalToken = externalToken;

            ProgressBar.Visibility = ViewStates.Visible;
            WebView.LoadUrl( url );
        }

        public bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            // run the url through our processor to see if it needs to be manipulated
            string processedUrl = Rock.Mobile.Util.URL.Override.ProcessURLOverrides( url );
            if ( processedUrl.StartsWith( ExternalToken, StringComparison.InvariantCultureIgnoreCase ) )
            {
                global::Android.Net.Uri uri = global::Android.Net.Uri.Parse( processedUrl.Substring( ExternalToken.Length ) );

                var intent = new Intent( Intent.ActionView, uri ); 
                ((Activity)Rock.Mobile.PlatformSpecific.Android.Core.Context).StartActivity( intent );
                return true;
            }

            return false;
        }

        public bool OnBackPressed( )
        {
            if( WebView.CanGoBack( ) )
            {
                WebView.GoBack( );

                return true;
            }

            return false;
        }

        public void OnReceivedError( ClientError errorCode, string description, string failingUrl )
        {
            ProgressBar.Visibility = ViewStates.Invisible;
            PageLoadedHandler( false, failingUrl );
        }

        public void OnPageFinished( WebView view, string url )
        {
            InputMethodManager imm = ( InputMethodManager )Rock.Mobile.PlatformSpecific.Android.Core.Context.GetSystemService( global::Android.Content.Context.InputMethodService );
            imm.HideSoftInputFromWindow( view.WindowToken, 0 );

            ProgressBar.Visibility = ViewStates.Invisible;
            PageLoadedHandler( true, url );
        }

        public void Destroy( )
        {
            if ( WebView != null )
            {
                RemoveView( WebView );

                WebView.RemoveAllViews( );
                WebView.Destroy( );
                WebView = null;

                System.GC.Collect( GC.MaxGeneration );
            }
        }
    }

    /// <summary>
    /// A simple banner billerboard that sizes to fit the icon and label
    /// given it, and can animate in our out. A delegate is called
    /// when the banner is clicked.
    /// </summary>
    public class NotificationBillboard : RelativeLayout
    {
        /// <summary>
        /// The label representing the icon to display
        /// </summary>
        /// <value>The icon.</value>
        TextView Icon { get; set; }

        /// <summary>
        /// The label that displays the text to show
        /// </summary>
        /// <value>The label.</value>
        TextView Label { get; set; }

        /// <summary>
        /// The even invoked when the banner is clicked
        /// </summary>
        /// <value>The on click action.</value>
        EventHandler OnClickAction { get; set; }

        /// <summary>
        /// An invisible button that covers the entire banner and handles the click
        /// </summary>
        /// <value>The overlay button.</value>
        Button OverlayButton { get; set; }

        /// <summary>
        /// True if the banner is animating (prevents simultaneous animations)
        /// </summary>
        bool Animating { get; set; }

        /// <summary>
        /// The layout that wraps the Icon and Label
        /// </summary>
        LinearLayout TextLayout { get; set; }

        float ScreenWidth { get; set; }

        const float AnimationTime = .25f;

        public void Reveal( )
        {
            // if we're not animating and AREN'T visible
            if ( Animating == false  && Visibility == ViewStates.Gone )
            {
                // reveal the banner and flag that we're animating
                Visibility = ViewStates.Visible;
                Animating = true;
                BringToFront( );

                // create an animator and animate us into view
                SimpleAnimator_Float revealer = new SimpleAnimator_Float( ScreenWidth, 0, AnimationTime, 
                    delegate(float percent, object value )
                    {
                        SetX( (float)value );
                    },
                    delegate
                    {
                        Animating = false;
                    } );

                revealer.Start( );
            }
        }

        public void Hide( )
        {
            // if we're not animating and ARE visible
            if ( Animating == false && Visibility == ViewStates.Visible )
            {
                Animating = true;

                // create a simple animator and animate the banner out of view
                SimpleAnimator_Float revealer = new SimpleAnimator_Float( 0, ScreenWidth, AnimationTime, 
                    delegate(float percent, object value )
                    {
                        SetX( (float)value );
                    },
                    delegate
                    {
                        // when complete, hide the banner, since there's no need to render it
                        Animating = false;
                        Visibility = ViewStates.Gone;
                    } );

                revealer.Start( );
            }
        }

        RelativeLayout BannerLayout { get; set; }
        Button DismissButton { get; set; }

        public NotificationBillboard( float deviceWidth, global::Android.Content.Context context ) : base( context )
        {
            DismissButton = new Button( context );
            DismissButton.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
            DismissButton.Background = null;
            AddView( DismissButton );

            LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );

            BannerLayout = new RelativeLayout( context );
            BannerLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (RelativeLayout.LayoutParams)BannerLayout.LayoutParameters ).AddRule( LayoutRules.AlignParentRight );
            AddView( BannerLayout );

            // create a layout that will horizontally align the icon and label
            TextLayout = new LinearLayout( context );
            TextLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (RelativeLayout.LayoutParams)TextLayout.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
            BannerLayout.AddView( TextLayout );

            Icon = new TextView( context );
            Icon.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (LinearLayout.LayoutParams)Icon.LayoutParameters ).LeftMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 5 );
            ( (LinearLayout.LayoutParams)Icon.LayoutParameters ).RightMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 15 );

            ( (LinearLayout.LayoutParams)Icon.LayoutParameters ).TopMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 5 );
            ( (LinearLayout.LayoutParams)Icon.LayoutParameters ).BottomMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 5 );
            TextLayout.AddView( Icon );

            Label = new TextView( context );
            Label.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (LinearLayout.LayoutParams)Label.LayoutParameters ).RightMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 5 );

            ( (LinearLayout.LayoutParams)Label.LayoutParameters ).TopMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 5 );
            ( (LinearLayout.LayoutParams)Label.LayoutParameters ).BottomMargin = (int)Rock.Mobile.Graphics.Util.UnitToPx( 5 );
            TextLayout.AddView( Label );

            // create the button that wraps the layout and handles input
            OverlayButton = new Button( context );
            OverlayButton.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            OverlayButton.Background = null;
            BannerLayout.AddView( OverlayButton );

            ScreenWidth = deviceWidth;
        }

        public void SetLabel( string iconStr, string iconFont, float iconSize, string labelStr, string labelFont, float labelSize, uint textColor, uint bgColor, EventHandler onClick )
        {
            // don't allow changing WHILE we're animating
            if ( Animating == false )
            {
                // setup the banner
                BannerLayout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( bgColor ) );

                // setup the icon
                Icon.Text = iconStr;
                Icon.SetTypeface( FontManager.Instance.GetFont( iconFont ), global::Android.Graphics.TypefaceStyle.Normal );
                Icon.SetTextSize( ComplexUnitType.Dip, iconSize );
                Icon.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( textColor ) );

                // setup the label
                Label.Text = labelStr;
                Label.SetTypeface( FontManager.Instance.GetFont( labelFont ), global::Android.Graphics.TypefaceStyle.Normal );
                Label.SetTextSize( ComplexUnitType.Dip, labelSize );
                Label.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( textColor ) );

                if ( OnClickAction != null )
                {
                    OverlayButton.Click -= OnClickAction;
                }

                OverlayButton.Click += onClick;
                OnClickAction = onClick;

                // resize the button to fit over the full banner
                int widthMeasureSpec = View.MeasureSpec.MakeMeasureSpec( TextLayout.LayoutParameters.Width, MeasureSpecMode.Unspecified );
                int heightMeasureSpec = View.MeasureSpec.MakeMeasureSpec( TextLayout.LayoutParameters.Height, MeasureSpecMode.Unspecified );
                TextLayout.Measure( widthMeasureSpec, heightMeasureSpec );

                OverlayButton.LayoutParameters.Width = TextLayout.MeasuredWidth;
                OverlayButton.LayoutParameters.Height = TextLayout.MeasuredHeight;

                BannerLayout.LayoutParameters.Width = TextLayout.MeasuredWidth;
                BannerLayout.LayoutParameters.Height = TextLayout.MeasuredHeight;

                // default it to hidden and offscreen
                Visibility = ViewStates.Gone;
                SetX( ScreenWidth );
            }
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            // get the tapped position and the bannerPos's bounding box
            PointF tappedPos = new PointF( e.GetX( ), e.GetY( ) );
            RectangleF bannerBB = new RectangleF( BannerLayout.GetX( ), BannerLayout.GetY( ), BannerLayout.GetX( ) + BannerLayout.Width, BannerLayout.GetY( ) + BannerLayout.Height );

            // if they tapped inside the banner, send the click notification
            if ( bannerBB.Contains( tappedPos ) )
            {
                if ( Visibility == ViewStates.Visible )
                {
                    OnClickAction( null, null );
                }
            }

            // either way dismiss the banner and let the touch input continue
            Hide( );
            return false;
        }
    }

    public class Util
    {
        public static void AnimateViewColor( uint currColor, uint targetColor, View uiView, SimpleAnimator.AnimationComplete complete )
        {
            SimpleAnimator_Color viewAnimator = new SimpleAnimator_Color( currColor, targetColor, .15f, delegate(float percent, object value )
                {
                    uiView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( (uint)value ) );
                }
                ,
                delegate
                {
                    complete( );
                } );
            viewAnimator.Start( );
        }

        public static void FadeView( View view, bool fadeIn, SimpleAnimator.AnimationComplete onComplete )
        {
            float startAlpha = fadeIn == true ? 0.00f : 1.00f;
            float endAlpha = fadeIn == true ? 1.00f : 0.00f;

            SimpleAnimator_Float floatAnim = new SimpleAnimator_Float( startAlpha, endAlpha, .15f, 
                                                 delegate(float percent, object value )
                {
                    view.Alpha = (float)value;
                }, 
                                                 delegate
                {
                    if ( onComplete != null )
                    {
                        onComplete( );
                    }
                } );
            floatAnim.Start( );
        }
    }
}

#endif
