using System;
using UIKit;
using Rock.Mobile.UI;
using App.Shared.Config;
using Foundation;
using CoreGraphics;

namespace iOS
{
    public class ControlStyling
    {
        public static float ButtonWidth = 122;
        public static float ButtonHeight = 44;

        public static void StyleButton( UIButton button, string text, string font, uint size )
        {
            button.SetTitle( text, UIControlState.Normal );

            button.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Button_TextColor ), UIControlState.Normal );
            button.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Button_BGColor );

            button.Layer.CornerRadius = ControlStylingConfig.Button_CornerRadius;

            button.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( font, size );
        }

        public static void StyleUILabel( UILabel label, string font, uint size )
        {
            label.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
            label.BackgroundColor = UIColor.Clear;

            label.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( font, size );
        }

        public static void StyleBGLayer( UIView view )
        {
            view.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
            view.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ).CGColor;
            view.Layer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;
        }

        public static void StyleTextField( UITextField textField, string placeholderText, string font, uint size )
        {
            textField.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            textField.AttributedPlaceholder = new NSAttributedString( placeholderText, null, Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
            textField.BackgroundColor = UIColor.Clear;

            textField.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( font, size );
        }  
    }

    public class StyledTextField
    {
        public const float StyledFieldHeight = 44;

        public UIView Background { get; set; }
        public UITextField Field { get; set; }

        public StyledTextField( )
        {
            Background = new UIView( );
            Field = new UITextField( );
            Field.KeyboardAppearance = UIKeyboardAppearance.Dark;

            Background.Layer.AnchorPoint = CGPoint.Empty;
            Field.Layer.AnchorPoint = CGPoint.Empty;

            Background.AddSubview( Field );
        }

        public void SetFrame( CGRect frame )
        {
            Background.Frame = frame;
            Field.Frame = new CGRect( 20, 2, Background.Frame.Width - 40, frame.Height * .90f );
        }
    }
}

