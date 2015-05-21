using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using CoreGraphics;
using CoreAnimation;
using Rock.Mobile.UI;
using App.Shared.Config;
using App.Shared.Strings;
using App.Shared.Analytics;
using App.Shared.Network;
using App.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;
using System.Drawing;
using Rock.Mobile.Animation;
using App.Shared.PrivateConfig;

namespace iOS
{
    class PrayerCard
    {
        /// <summary>
        /// Overrides the prayer content text so we can detect when the
        /// user is panning while their finger is within it, and then
        /// move the cards.
        /// </summary>
        class PrayerTextView : UITextView
        {
            public UIView Parent { get; set; } 

            public override void TouchesMoved(NSSet touches, UIEvent evt)
            {
                base.TouchesMoved(touches, evt);

                Parent.TouchesMoved( touches, evt );
            }

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);

                Parent.TouchesBegan( touches, evt );
            }

            public override void TouchesCancelled(NSSet touches, UIEvent evt)
            {
                base.TouchesCancelled(touches, evt);

                Parent.TouchesCancelled( touches, evt );
            }
        }

        public PlatformView View { get; set; }

        UIView NameLayer { get; set; }
        UILabel Name { get; set; }

        UIView DateLayer { get; set; }
        UILabel Date { get; set; }

        UIView CategoryLayer { get; set; }
        UILabel Category { get; set; }

        Rock.Client.PrayerRequest PrayerRequest { get; set; }
        PrayerTextView PrayerText { get; set; }


        UIButton PrayerActionButton { get; set; }
        UIView PrayerActionCircle { get; set; }

        bool Prayed { get; set; }

        public PrayerCard( Rock.Client.PrayerRequest prayer, CGRect bounds )
        {
            //setup the actual "card" outline
            View = PlatformView.Create( );
            View.Bounds = new System.Drawing.RectangleF( (float)bounds.X, (float)bounds.Y, (float)bounds.Width, (float)bounds.Height );
            View.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            View.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            View.CornerRadius = ControlStylingConfig.Button_CornerRadius;
            View.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            // ensure we clip children
            ( (UIView)View.PlatformNativeObject ).ClipsToBounds = true;


            // setup the prayer request text field
            PrayerText = new PrayerTextView( );
            PrayerText.Editable = false;
            PrayerText.BackgroundColor = UIColor.Clear;
            PrayerText.Layer.AnchorPoint = new CGPoint( 0, 0 );
            PrayerText.DelaysContentTouches = false; // don't allow delaying touch, we need to forward it
            PrayerText.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            PrayerText.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            PrayerText.TextContainerInset = UIEdgeInsets.Zero;
            PrayerText.TextContainer.LineFragmentPadding = 0;


            // setup the bottom prayer button, and its fill-in circle
            PrayerActionButton = UIButton.FromType( UIButtonType.Custom );
            PrayerActionButton.Layer.AnchorPoint = new CGPoint( 0, 0 );
            PrayerActionButton.SetTitle( PrayerStrings.Prayer_Before, UIControlState.Normal );

            PrayerActionButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            PrayerActionButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false ) ), UIControlState.Highlighted );

            PrayerActionButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            PrayerActionButton.SizeToFit( );

            PrayerActionCircle = new UIView( );
            PrayerActionCircle.Bounds = new CGRect( 0, 0, 100, 100 );
            PrayerActionCircle.Layer.CornerRadius = PrayerActionCircle.Bounds.Width / 2;
            PrayerActionCircle.Layer.BorderWidth = 1;
            PrayerActionCircle.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ).CGColor;
            PrayerActionCircle.Layer.AnchorPoint = new CGPoint( 0, 0 );

            PrayerActionButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    TogglePrayed( true );
                };
            

            // setup the name field
            NameLayer = new UIView( );
            NameLayer.Layer.AnchorPoint = new CGPoint( 0, 0 );
            NameLayer.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
            NameLayer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor );

            Name = new UILabel( );
            Name.Layer.AnchorPoint = new CGPoint( 0, 0 );
            Name.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            Name.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Medium_FontSize );
            Name.BackgroundColor = UIColor.Clear;

            // setup the date field
            DateLayer = new UIView( );
            DateLayer.Layer.AnchorPoint = new CGPoint( 0, 0 );
            DateLayer.Layer.BorderWidth = 1;
            DateLayer.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
            DateLayer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor );

            Date = new UILabel( );
            Date.Layer.AnchorPoint = new CGPoint( 0, 0 );
            Date.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            Date.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
            Date.BackgroundColor = UIColor.Clear;

            // setup the category field
            CategoryLayer = new UIView( );
            CategoryLayer.Layer.AnchorPoint = new CGPoint( 0, 0 );
            CategoryLayer.Layer.BorderWidth = 1;
            CategoryLayer.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
            CategoryLayer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor );

            Category = new UILabel( );
            Category.Layer.AnchorPoint = new CGPoint( 0, 0 );
            Category.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            Category.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
            Category.BackgroundColor = UIColor.Clear;


            // add the controls
            UIView nativeView = View.PlatformNativeObject as UIView;

            nativeView.AddSubview( NameLayer );
            nativeView.AddSubview( Name );
            nativeView.AddSubview( CategoryLayer );
            nativeView.AddSubview( Category );
            nativeView.AddSubview( DateLayer );
            nativeView.AddSubview( Date );
            nativeView.AddSubview( PrayerText );
            nativeView.AddSubview( PrayerActionCircle );
            nativeView.AddSubview( PrayerActionButton );
            PrayerText.Parent = nativeView;

            SetPrayer( prayer );
        }

        public void TogglePrayed( bool prayed )
        {
            // if the prayer state is changing
            if( prayed != Prayed )
            {
                Prayed = prayed;

                uint currColor = 0;
                uint targetColor = 0;

                // if we are ACTIVATING prayed
                if ( prayed == true )
                {
                    // update the circle color and send an analytic
                    App.Shared.Network.RockApi.Instance.IncrementPrayerCount( PrayerRequest.Id, null );

                    currColor = 0;
                    targetColor = PrayerConfig.PrayedForColor;
                    PrayerActionCircle.Layer.BorderWidth = 0;

                    PrayerActionButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( 0xFFFFFFFF ), UIControlState.Normal );
                    PrayerActionButton.SetTitle( PrayerStrings.Prayer_After, UIControlState.Normal );
                    PrayerActionButton.SizeToFit( );
                }
                else
                {
                    // otherwise just update the color
                    currColor = PrayerConfig.PrayedForColor;
                    targetColor = 0;
                    PrayerActionCircle.Layer.BorderWidth = 1;

                    PrayerActionButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
                    PrayerActionButton.SetTitle( PrayerStrings.Prayer_Before, UIControlState.Normal );
                    PrayerActionButton.SizeToFit( );
                }

                PrayerActionButton.SizeToFit( );
                PositionPrayedLabel( );

                // animate the circle color to its new target
                SimpleAnimator_Color colorAnim = new SimpleAnimator_Color( currColor, targetColor, .35f, 
                    delegate(float percent, object value )
                    {
                        PrayerActionCircle.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( (uint)value );
                    }, null );
                colorAnim.Start( );
            }
        }

        void PositionPrayedLabel( )
        {
            PrayerActionCircle.Layer.Position = new CGPoint( (View.Bounds.Width - PrayerActionCircle.Layer.Bounds.Width) / 2.0f, 
                                                             View.Bounds.Height - PrayerActionCircle.Layer.Bounds.Height / 3.50f );
            
            PrayerActionButton.Layer.Position = new CGPoint( PrayerActionCircle.Layer.Position.X + (PrayerActionCircle.Frame.Width - PrayerActionButton.Frame.Width) / 2, 
                                                             PrayerActionCircle.Layer.Position.Y  );
        }

        const int ViewPadding = 10;
        void SetPrayer( Rock.Client.PrayerRequest prayer )
        {
            PrayerRequest = prayer;

            // set the text for the name, size it so we get the height, then
            // restrict its bounds to the card itself
            Name.Text = prayer.FirstName.ToUpper( );
            Category.Text = PrayerRequest.CategoryId.HasValue ? RockGeneralData.Instance.Data.PrayerIdToCategory( PrayerRequest.CategoryId.Value ) : RockGeneralData.Instance.Data.PrayerCategories[ 0 ].Name;
            Date.Text = string.Format( "{0:MM/dd/yy}", PrayerRequest.EnteredDateTime );
            PrayerText.Text = prayer.Text;

            LayoutChanged( View.Frame );
        }

        public void LayoutChanged( RectangleF bounds )
        {
            View.Bounds = bounds;

            // setup the name layer and name
            float metaDataSpacing = 30;
            NameLayer.Frame = new CGRect( 0, 0, View.Bounds.Width, metaDataSpacing );

            Name.Frame = new CGRect( 0, 0, View.Bounds.Width - (ViewPadding * 2), metaDataSpacing );
            Name.SizeToFit( );
            Name.Frame = new CGRect( ViewPadding, (metaDataSpacing - Name.Bounds.Height) / 2, Name.Frame.Width, Name.Bounds.Height );


            // setup the category layer and category
            CategoryLayer.Frame = new CGRect( -5, NameLayer.Frame.Bottom, View.Bounds.Width / 2 + 6, metaDataSpacing );
                
            Category.Frame = new CGRect( 0, 0, (View.Bounds.Width / 2) - ViewPadding, metaDataSpacing );
            Category.SizeToFit( );
            Category.Frame = new CGRect( ViewPadding, CategoryLayer.Frame.Top + (metaDataSpacing - Category.Bounds.Height) / 2, Category.Frame.Width, Category.Frame.Height );


            // setup the date layer and category
            DateLayer.Frame = new CGRect( View.Bounds.Width / 2, NameLayer.Frame.Bottom, View.Bounds.Width / 2 + 5, metaDataSpacing );

            Date.Frame = new CGRect( 0, 0, (View.Bounds.Width / 2) - ViewPadding, metaDataSpacing );
            Date.SizeToFit( );
            Date.Frame = new CGRect( View.Bounds.Width - Date.Frame.Width - ViewPadding, 
                                     DateLayer.Frame.Top + (metaDataSpacing - Date.Bounds.Height) / 2, 
                                     Date.Frame.Width, 
                                     Date.Frame.Height );


            PrayerText.Frame = new CGRect( ViewPadding, DateLayer.Frame.Bottom, View.Bounds.Width - (ViewPadding * 2), 0 );
            PrayerText.SizeToFit( );
            float prayerHeight = (float) Math.Min( PrayerText.Frame.Height, View.Bounds.Height - PrayerText.Frame.Top - PrayerActionButton.Frame.Height - ViewPadding );
            PrayerText.Frame = new CGRect( PrayerText.Frame.Left, PrayerText.Frame.Top, PrayerText.Frame.Width, prayerHeight );
                        
            PositionPrayedLabel( );
        }
    }

	partial class PrayerMainUIViewController : TaskUIViewController
	{
        /// <summary>
        /// Actual list of prayer requests
        /// </summary>
        /// <value>The prayer requests.</value>
        List<PrayerCard> PrayerRequests { get; set; }

        PlatformCardCarousel Carousel { get; set; }

        bool RequestingPrayers { get; set; }
        bool ViewActive { get; set; }

        CGRect CardSize { get; set; }

        DateTime LastDownload { get; set; }

        UIBlockerView BlockerView { get; set; }

		public PrayerMainUIViewController (IntPtr handle) : base (handle)
		{
            PrayerRequests = new List<PrayerCard>();
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BlockerView = new UIBlockerView( View, View.Frame.ToRectF( ) );

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            float viewRealHeight = (float)( View.Bounds.Height - Task.NavToolbar.Frame.Height);

            float cardSizePerc = .83f;
            float cardWidth = (float)(View.Bounds.Width * cardSizePerc);
            float cardHeight = (float) (viewRealHeight * cardSizePerc);

            // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
            float cardYOffset = ( viewRealHeight * .03f );

            Carousel = PlatformCardCarousel.Create( View, cardWidth, cardHeight, new System.Drawing.RectangleF( 0, cardYOffset, (float)View.Bounds.Width, viewRealHeight ), PrivatePrayerConfig.Card_AnimationDuration );

            CardSize = new CGRect( 0, 0, cardWidth, cardHeight );

            // Setup the request prayers layer
            //setup our appearance
            RetrievingPrayersView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            StatusLabel.Text = PrayerStrings.ViewPrayer_StatusText_Retrieving;
            ControlStyling.StyleUILabel( StatusLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( StatusBackground );

            ControlStyling.StyleUILabel( ResultLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( ResultBackground );

            ControlStyling.StyleButton( RetryButton, GeneralStrings.Retry, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            RetryButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    if( RequestingPrayers == false )
                    {
                        RetrievePrayerRequests( );
                    }
                };

            LastDownload = DateTime.MinValue;
        }

        public void ResetPrayerStatus( )
        {
            // now update the layout for each prayer card
            foreach ( PrayerCard prayerCard in PrayerRequests )
            {
                prayerCard.TogglePrayed( false );
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ViewActive = true;
            Carousel.Hidden = false;

            // this will prevent double requests in the case that we leave and return to the prayer
            // page before the initial request completes
            if ( RequestingPrayers == false )
            {
                TimeSpan deltaTime = DateTime.Now - LastDownload;
                if ( deltaTime.TotalHours > PrivatePrayerConfig.PrayerDownloadFrequency.TotalHours )
                {
                    View.BringSubviewToFront( RetrievingPrayersView );
                    BlockerView.BringToFront( );

                    Rock.Mobile.Util.Debug.WriteLine( "Grabbing Prayers" );
                    RetrievePrayerRequests( );
                }
                else
                {
                    Rock.Mobile.Util.Debug.WriteLine( "Not getting prayers." );

                    // add a read analytic
                    PrayerAnalytic.Instance.Trigger( PrayerAnalytic.Read );
                }
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            Task.NavToolbar.SetCreateButtonEnabled( true, delegate
                {
                    // now disable the button so they can't spam it
                    Task.NavToolbar.SetCreateButtonEnabled( false );

                    Prayer_CreateUIViewController viewController = new Prayer_CreateUIViewController( );
                    Task.PerformSegue( this, viewController );
                }
            );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            float viewRealHeight = (float)( View.Bounds.Height - Task.NavToolbar.Frame.Height);

            float cardSizePerc = .83f;
            float cardWidth = (float)(View.Bounds.Width * cardSizePerc);
            float cardHeight = (float) (viewRealHeight * cardSizePerc);
            float cardYOffset = ( viewRealHeight * .03f );

            // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
            Carousel.LayoutChanged( cardWidth, cardHeight, new System.Drawing.RectangleF( 0, cardYOffset, (float)View.Bounds.Width, viewRealHeight ) );

            CardSize = new CGRect( 0, 0, cardWidth, cardHeight );
            for( int i = 0; i < PrayerRequests.Count; i++ )
            {
                PrayerRequests[ i ].LayoutChanged( CardSize.ToRectF( ) );
            }

            BlockerView.SetBounds( View.Bounds.ToRectF( ) );
        }

        void RetrievePrayerRequests( )
        {
            // show the retrieve layer
            RetrievingPrayersView.Layer.Opacity = 1.00f;
            StatusLabel.Text = PrayerStrings.ViewPrayer_StatusText_Retrieving;
            ResultLabel.Hidden = true;
            RetryButton.Hidden = true;

            BlockerView.Show( delegate
                {
                    RequestingPrayers = true;

                    // request the prayers each time this appears
                    App.Shared.Network.RockApi.Instance.GetPrayers( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.PrayerRequest> prayerRequests )
                        {
                            // force this onto the main thread so that if there's a race condition in requesting prayers we won't hit it.
                            InvokeOnMainThread( delegate
                                {
                                    // only process this if the view is still active. It's possible this request came in after we left the view.
                                    if ( ViewActive == true )
                                    {
                                        PrayerRequests.Clear( );
                                        Carousel.Clear( );

                                        RequestingPrayers = false;

                                        BlockerView.Hide( null );

                                        // somestimes our prayers can be received with errors in the xml, so ensure we have a valid model.
                                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) && prayerRequests != null )
                                        {
                                            if ( prayerRequests.Count > 0 )
                                            {
                                                // sort the prayers based on prayer count (least prayed for first)
                                                prayerRequests.Sort( delegate(Rock.Client.PrayerRequest x, Rock.Client.PrayerRequest y) 
                                                    {
                                                        return x.PrayerCount < y.PrayerCount ? -1 : 1;
                                                    });
                                                
                                                // update our timestamp since this was successful
                                                LastDownload = DateTime.Now;

                                                RetrievingPrayersView.Layer.Opacity = 0.00f;

                                                // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
                                                for( int i = 0; i < Math.Min( prayerRequests.Count, 50 ); i++ )
                                                {
                                                    PrayerCard card = new PrayerCard( prayerRequests[ i ], CardSize );
                                                    PrayerRequests.Add( card );
                                                    Carousel.AddCard( card.View );
                                                }
                                            }
                                            else
                                            {
                                                // let them know there aren't any prayer requests
                                                StatusLabel.Text = PrayerStrings.ViewPrayer_StatusText_NoPrayers;
                                                RetryButton.Hidden = false;
                                                ResultLabel.Hidden = false;
                                                ResultLabel.Text = PrayerStrings.ViewPrayer_Result_NoPrayersText;
                                            }

                                            // add a read analytic
                                            PrayerAnalytic.Instance.Trigger( PrayerAnalytic.Read );
                                        }
                                        else
                                        {
                                            StatusLabel.Text = PrayerStrings.ViewPrayer_StatusText_Failed;
                                            RetryButton.Hidden = false;
                                            ResultLabel.Hidden = false;
                                            ResultLabel.Text = PrayerStrings.Error_Retrieve_Message;

                                            Task.NavToolbar.SetCreateButtonEnabled( false );
                                        }
                                    }
                                } );
                        } );
                } );
        }

        public void MakeInActive()
        {
            ViewActive = false;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan( touches, evt );

            Carousel.TouchesBegan( );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            // don't call the base because we don't want to support the nav toolbar on this page
            Carousel.TouchesEnded( );
        }
	}
}
