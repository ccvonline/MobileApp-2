using System;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.PrivateConfig;

namespace App.Shared.UI
{
    /// <summary>
    /// Used to display a result to a user, including a status message
    /// </summary>
    public class UIResultView
    {
        PlatformView View { get; set; }

        PlatformLabel StatusLabel { get; set; }
        //PlatformView StatusBackground { get; set; }

        PlatformCircleView ResultCircle {get; set; }
        PlatformLabel ResultSymbol { get; set; }
        PlatformLabel ResultLabel { get; set; }
        //PlatformView ResultBackground { get; set; }

        PlatformButton DoneButton { get; set; }

        public delegate void DoneClickDelegate( );

        public UIResultView( object parentView, RectangleF frame, DoneClickDelegate onClick )
        {
            View = PlatformView.Create( );
            View.AddAsSubview( parentView );
            View.UserInteractionEnabled = false;

            StatusLabel = PlatformLabel.Create( );
            //StatusBackground = PlatformView.Create( );

            ResultSymbol = PlatformLabel.Create( );
            ResultLabel = PlatformLabel.Create( );
            //ResultBackground = PlatformView.Create( );


            ResultCircle = PlatformCircleView.Create( );
            ResultCircle.AddAsSubview( parentView );


            // setup our UI hierarchy
            //StatusBackground.AddAsSubview( parentView );
            //StatusBackground.UserInteractionEnabled = false;
            //StatusBackground.BorderWidth = .5f;

            StatusLabel.AddAsSubview( parentView );
            StatusLabel.UserInteractionEnabled = false;


            //ResultBackground.AddAsSubview( parentView );
            //ResultBackground.UserInteractionEnabled = false;
            //ResultBackground.BorderWidth = .5f;

            ResultSymbol.AddAsSubview( parentView );
            ResultSymbol.UserInteractionEnabled = false;

            ResultLabel.AddAsSubview( parentView );
            ResultLabel.UserInteractionEnabled = false;

            DoneButton = PlatformButton.Create( );
            DoneButton.AddAsSubview( parentView );
            DoneButton.ClickEvent = ( PlatformButton button ) =>
            {
                if( onClick != null )
                {
                    onClick( );
                }
            };


            // default the view size and opacity
            SetOpacity( 0.00f );

            SetBounds( frame );

            // give it a default style
            SetStyle( );
        }

        void SetOpacity( float opacity )
        {
            View.Opacity = opacity;

            ResultSymbol.Opacity = opacity;
            ResultLabel.Opacity = opacity;
            ResultCircle.Opacity = opacity;
            //ResultBackground.Opacity = opacity;

            StatusLabel.Opacity = opacity;
            //StatusBackground.Opacity = opacity;

            DoneButton.Opacity = opacity;
        }

        public void SetStyle( )
        {
            // setup the text fonts and colors
            StatusLabel.SetFont( ControlStylingConfig.Font_Bold, 24 );
            StatusLabel.TextColor = ControlStylingConfig.TextField_ActiveTextColor;

            ResultSymbol.SetFont( PrivateControlStylingConfig.Icon_Font_Secondary, 64 );
            ResultSymbol.TextColor = ControlStylingConfig.TextField_PlaceholderTextColor;

            ResultCircle.BackgroundColor = ControlStylingConfig.BG_Layer_Color;

            ResultLabel.SetFont( ControlStylingConfig.Font_Light, 14 );
            ResultLabel.TextColor = ControlStylingConfig.TextField_PlaceholderTextColor;

            DoneButton.SetFont( ControlStylingConfig.Font_Regular, 14 );
            DoneButton.TextColor = ControlStylingConfig.Button_TextColor;
            DoneButton.BackgroundColor = ControlStylingConfig.Button_BGColor;

            // setup the background layer colors
            //ResultBackground.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            //ResultBackground.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;

            //StatusBackground.BackgroundColor = layerBgColor;
            //StatusBackground.BorderColor = layerBorderColor;

            View.BackgroundColor = ControlStylingConfig.BackgroundColor;

            DoneButton.CornerRadius = 4;
        }

        public void Show( string statusLabel, string resultSymbol, string resultLabel, string buttonLabel )
        {
            SetHidden( false );

            // set and position the status label
            /*if ( string.IsNullOrEmpty( statusLabel ) == false )
            {
                StatusBackground.Hidden = false;
            }
            else
            {
                StatusBackground.Hidden = true;
            }*/
            StatusLabel.Text = statusLabel;
            StatusLabel.SizeToFit( );


            // set the result symbol
            ResultSymbol.Text = resultSymbol;
            ResultSymbol.SizeToFit( );


            // set the result text
            /*if ( string.IsNullOrEmpty( resultLabel ) == false )
            {
                ResultBackground.Hidden = false;
            }
            else
            {
                ResultBackground.Hidden = true;
            }*/
            ResultLabel.Text = resultLabel;

            // set the done button
            if ( string.IsNullOrEmpty( buttonLabel ) == false )
            {
                DoneButton.Hidden = false;
            }
            else
            {
                DoneButton.Hidden = true;
            }
            DoneButton.Text = buttonLabel;
            DoneButton.SizeToFit( );

            SetOpacity( 1.00f );

            SetBounds( View.Bounds );
        }

        public void Hide( )
        {
            SetHidden( true );

            SetOpacity( 0.00f );
        }

        public void SetBounds( RectangleF containerBounds )
        {
            View.Bounds = containerBounds;

            // setup the background layers
            /*StatusBackground.Frame = new RectangleF( View.Frame.X, 
                View.Frame.Top + Rock.Mobile.Graphics.Util.UnitToPx( 10 ), 
                View.Frame.Width, 
                Rock.Mobile.Graphics.Util.UnitToPx( 44 ) );*/

            /*ResultBackground.Frame = new RectangleF( View.Frame.X, 
                View.Frame.Height / 3, 
                View.Frame.Width, 
                Rock.Mobile.Graphics.Util.UnitToPx( 150 ) );*/
            

            // and the labels
            StatusLabel.Frame = new RectangleF( 0, 0, View.Frame.Width - Rock.Mobile.Graphics.Util.UnitToPx( 40 ), 0 );
            StatusLabel.SizeToFit( );
            StatusLabel.Frame = new RectangleF( ( View.Frame.Width - StatusLabel.Frame.Width ) / 2, 
                View.Frame.Top + Rock.Mobile.Graphics.Util.UnitToPx( 10 ), 
                StatusLabel.Frame.Width, 
                StatusLabel.Frame.Height );

            ResultSymbol.Frame = new RectangleF( 0, 0, View.Frame.Width - Rock.Mobile.Graphics.Util.UnitToPx( 40 ), 0 );
            ResultSymbol.SizeToFit( );

            float circleWidth = ResultSymbol.Frame.Width * 1.25f;
            ResultCircle.Frame = new RectangleF( ( View.Frame.Width - circleWidth ) / 2, Rock.Mobile.Graphics.Util.UnitToPx( 140 ), circleWidth, circleWidth );

            ResultSymbol.Frame = new RectangleF( ResultCircle.Frame.X + (ResultCircle.Frame.Width - ResultSymbol.Frame.Width) / 2,  
                ResultCircle.Frame.Y + (ResultCircle.Frame.Height - ResultSymbol.Frame.Height) / 2, 
                ResultSymbol.Frame.Width, 
                ResultSymbol.Frame.Height );
            

            ResultLabel.Frame = new RectangleF( 0, 0, View.Frame.Width - Rock.Mobile.Graphics.Util.UnitToPx( 40 ), 0 );
            ResultLabel.SizeToFit( );
            ResultLabel.Frame = new RectangleF( ( View.Frame.Width - ResultLabel.Frame.Width ) / 2, ResultCircle.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 5 ), ResultLabel.Frame.Width, ResultLabel.Frame.Height );

            // lastly the button
            float doneWidth = Rock.Mobile.Graphics.Util.UnitToPx( 122 );
            DoneButton.Frame = new RectangleF( ( View.Frame.Width - doneWidth ) / 2, ResultLabel.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 10 ), doneWidth, DoneButton.Frame.Height );
        }

        void SetHidden( bool hidden )
        {
            View.Hidden = hidden;
            DoneButton.Hidden = hidden;

            StatusLabel.Hidden = hidden;
            //StatusBackground.Hidden = hidden;

            ResultCircle.Hidden = hidden;
            ResultSymbol.Hidden = hidden;
            ResultLabel.Hidden = hidden;
            //ResultBackground.Hidden = hidden;
        }
    }
}
