using System;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.Animation;
using App.Shared.Network;
using Rock.Mobile.Util.Strings;

namespace App.Shared.UI
{
    public class UIGroupFinderSearch
    {
        PlatformView Backer { get; set; }
        public PlatformView View { get; set; }


        int GroupID { get; set; }

        PlatformLabel Title { get; set; }

        PlatformLabel Details { get; set; }

        public PlatformTextField Street { get; set; }
        PlatformView StreetLayer { get; set; }

        public PlatformTextField City { get; set; }
        PlatformView CityLayer { get; set; }

        public PlatformTextField State { get; set; }
        PlatformView StateLayer { get; set; }

        public PlatformTextField ZipCode { get; set; }
        PlatformView ZipCodeLayer { get; set; }

        PlatformButton SearchButton { get; set; }

        Rock.Mobile.UI.PlatformButton.OnClick SearchClicked { get; set; }

        public UIGroupFinderSearch( )
        {
        }

        static float sBackerOpacity = .45f;
        static float sModalOffset = 15;

        public void Create( object masterView, RectangleF frame, Rock.Mobile.UI.PlatformButton.OnClick searchClicked )
        {
            Backer = PlatformView.Create( );
            Backer.BackgroundColor = 0x000000FF;
            Backer.Opacity = sBackerOpacity;
            Backer.AddAsSubview( masterView );
            Backer.Frame = frame;

            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.AddAsSubview( masterView );
            View.Frame = new RectangleF( frame.Left + sModalOffset, 
                frame.Top + sModalOffset, 
                frame.Width - (sModalOffset * 2), 
                frame.Height - (sModalOffset * 2));

            Title = PlatformLabel.Create( );
            Title.AddAsSubview( View.PlatformNativeObject );
            Title.SetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
            Title.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            Title.TextAlignment = TextAlignment.Center;

            Details = PlatformLabel.Create( );
            Details.AddAsSubview( View.PlatformNativeObject );
            Details.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
            Details.TextColor = ControlStylingConfig.TextField_ActiveTextColor;


            // Name Info
            StreetLayer = PlatformView.Create( );
            StreetLayer.AddAsSubview( View.PlatformNativeObject );
            StreetLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            StreetLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            StreetLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            Street = PlatformTextField.Create( );
            Street.AddAsSubview( View.PlatformNativeObject );
            Street.SetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            Street.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            Street.Placeholder = ConnectStrings.GroupFinder_StreetPlaceholder;
            Street.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            Street.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            Street.AutoCapitalizationType = AutoCapitalizationType.Words;
            Street.AutoCorrectionType = AutoCorrectionType.No;


            CityLayer = PlatformView.Create( );
            CityLayer.AddAsSubview( View.PlatformNativeObject );
            CityLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            CityLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            CityLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            City = PlatformTextField.Create( );
            City.AddAsSubview( View.PlatformNativeObject );
            City.SetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            City.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            City.Placeholder = ConnectStrings.GroupFinder_CityPlaceholder;
            City.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            City.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            City.AutoCapitalizationType = AutoCapitalizationType.Words;
            City.AutoCorrectionType = AutoCorrectionType.No;


            StateLayer = PlatformView.Create( );
            StateLayer.AddAsSubview( View.PlatformNativeObject );
            StateLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            StateLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            StateLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            State = PlatformTextField.Create( );
            State.AddAsSubview( View.PlatformNativeObject );
            State.SetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            State.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            State.Placeholder = ConnectStrings.GroupFinder_StatePlaceholder;
            State.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            State.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            State.AutoCapitalizationType = AutoCapitalizationType.Words;
            State.AutoCorrectionType = AutoCorrectionType.No;

            ZipCodeLayer = PlatformView.Create( );
            ZipCodeLayer.AddAsSubview( View.PlatformNativeObject );
            ZipCodeLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            ZipCodeLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            ZipCodeLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            ZipCode = PlatformTextField.Create( );
            ZipCode.AddAsSubview( View.PlatformNativeObject );
            ZipCode.SetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ZipCode.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            ZipCode.Placeholder = ConnectStrings.GroupFinder_ZipPlaceholder;
            ZipCode.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            ZipCode.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            ZipCode.AutoCapitalizationType = AutoCapitalizationType.None;
            ZipCode.AutoCorrectionType = AutoCorrectionType.No;


            // Search Button
            SearchClicked = searchClicked;
            SearchButton = PlatformButton.Create( );
            SearchButton.AddAsSubview( View.PlatformNativeObject );
            SearchButton.BackgroundColor = ControlStylingConfig.Button_BGColor;
            SearchButton.TextColor = ControlStylingConfig.Button_TextColor;
            SearchButton.CornerRadius = ControlStylingConfig.Button_CornerRadius;
            SearchButton.Text = GeneralStrings.Search;
            SearchButton.SizeToFit( );
            SearchButton.ClickEvent = ( PlatformButton b ) =>
                {
                    // treat the search button as if Return was pressed
                    ShouldReturn( );
                };

            LayoutChanged( frame );
        }

        public void ShouldReturn( )
        {
            if( ValidateInput( ) )
            {
                // ensure the keyboard hides
                TouchesEnded( );

                SearchClicked( null );
            }
        }

        public float GetControlBottom( )
        {
            return SearchButton.Frame.Bottom;
        }

        public void SetTitle( string title, string details )
        {
            // set the group title
            Title.Text = title;
            Title.SizeToFit( );

            // set the details for the group (distance, meeting time, etc)
            Details.Text = details;
            Details.TextAlignment = TextAlignment.Center;
            Details.SizeToFit( );
        }    

        public void SetAddress( string street, string city, string state, string zip )
        {
            Street.Text = street;
            City.Text = city;
            State.Text = state;
            ZipCode.Text = zip;
        }

        public void Show( )
        {
            Backer.Hidden = false;
            View.Hidden = false;

            SimpleAnimator_Float alphaAnim = new SimpleAnimator_Float( View.Opacity, 1.00f, .33f, 
                delegate(float percent, object value )
                {
                    Backer.Opacity = Math.Min( (float)value, sBackerOpacity );
                    View.Opacity = (float)value;
                },
                null );

            alphaAnim.Start( );
        }

        public void Hide( bool animated )
        {
            if ( animated )
            {
                SimpleAnimator_Float alphaAnim = new SimpleAnimator_Float( View.Opacity, 0.00f, .33f, 
                    delegate(float percent, object value )
                    {
                        View.Opacity = (float)value;
                        Backer.Opacity = Math.Min( (float)value, sBackerOpacity );
                    },
                    delegate
                    {
                        View.Hidden = true;
                        Backer.Hidden = true;
                    } );

                alphaAnim.Start( );
            }
            else
            {
                View.Hidden = true;
                View.Opacity = 0.00f;

                Backer.Hidden = true;
                Backer.Opacity = 0.00f;
            }
        }

        public void LayoutChanged( RectangleF containerBounds )
        {
            Backer.Bounds = containerBounds;

            View.Frame = new RectangleF( containerBounds.Left + sModalOffset, 
                containerBounds.Top + sModalOffset, 
                containerBounds.Width - (sModalOffset * 2), 
                containerBounds.Height - (sModalOffset * 2));
            
            float sectionSpacing = Rock.Mobile.Graphics.Util.UnitToPx( 12 );
            float layerHeight = Rock.Mobile.Graphics.Util.UnitToPx( 44 );
            float textFieldHeight = Rock.Mobile.Graphics.Util.UnitToPx( 40 );
            float textLeftInset = Rock.Mobile.Graphics.Util.UnitToPx( 10 );
            float textTopInset = Rock.Mobile.Graphics.Util.UnitToPx( 2 );

            float buttonWidth = Rock.Mobile.Graphics.Util.UnitToPx( 122 );

            Title.Frame = new RectangleF( 0, containerBounds.Height * .05f, View.Frame.Width, Title.Frame.Height );
            Details.Frame = new RectangleF( 0, Title.Frame.Bottom, View.Frame.Width, Rock.Mobile.Graphics.Util.UnitToPx( 60 ) );

            StreetLayer.Frame = new RectangleF( 0, Details.Frame.Bottom + sectionSpacing, View.Frame.Width, layerHeight );
            Street.Frame = new RectangleF( textLeftInset, StreetLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            CityLayer.Frame = new RectangleF( 0, StreetLayer.Frame.Bottom, View.Frame.Width, layerHeight );
            City.Frame = new RectangleF( textLeftInset, CityLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            StateLayer.Frame = new RectangleF( 0, CityLayer.Frame.Bottom, View.Frame.Width, layerHeight );
            State.Frame = new RectangleF( textLeftInset, StateLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            ZipCodeLayer.Frame = new RectangleF( 0, StateLayer.Frame.Bottom, View.Frame.Width, layerHeight );
            ZipCode.Frame = new RectangleF( textLeftInset, ZipCodeLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            // Search Button
            SearchButton.Frame = new RectangleF( (View.Frame.Width - buttonWidth) / 2, ZipCodeLayer.Frame.Bottom + sectionSpacing, buttonWidth, layerHeight );
        }

        bool ValidateInput( )
        {
            bool result = true;

            // validate there's text in all required fields

            uint targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( Street.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Util.AnimateBackgroundColor( StreetLayer, targetColor );


            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( City.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Util.AnimateBackgroundColor( CityLayer, targetColor );


            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( State.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Util.AnimateBackgroundColor( StateLayer, targetColor );


            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( ZipCode.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Util.AnimateBackgroundColor( ZipCodeLayer, targetColor );

            return result;
        }

        public void TouchesEnded( )
        {
            Street.ResignFirstResponder( );
            City.ResignFirstResponder( );
            State.ResignFirstResponder( );
            ZipCode.ResignFirstResponder( );
        }
    }
}
