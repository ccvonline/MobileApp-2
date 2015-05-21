using System;
using Android.Widget;
using Rock.Mobile.UI;
using App.Shared.Config;
using Android.Views;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace Droid
{
    public class ControlStyling
    {
        public static void StyleButton( Button button, string text, string font, uint size )
        {
            // load up the rounded drawable and set the color
            Drawable buttonDrawable = (Drawable)Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.GetDrawable( Resource.Drawable.RoundButton );
            buttonDrawable.SetColorFilter( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Button_BGColor ), PorterDuff.Mode.Src );

            button.Background = buttonDrawable;
            button.Text = text;

            button.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            button.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
            button.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Button_TextColor ) );
        }

        public static void StyleUILabel( TextView label, string font, uint size )
        {
            label.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
            label.SetBackgroundColor( Android.Graphics.Color.Transparent );

            label.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            label.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
        }

        public static void StyleBGLayer( View backgroundLayout )
        {
            backgroundLayout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

            View borderView = backgroundLayout.FindViewById<View>( Resource.Id.top_border );
            if ( borderView != null )
            {
                borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
            }

            borderView = backgroundLayout.FindViewById<View>( Resource.Id.bottom_border );
            if ( borderView != null )
            {
                borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
            }
        }

        public static void StyleTextField( EditText textField, string placeholderText, string font, uint size )
        {
            textField.Background = null;
            textField.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );

            textField.Hint = placeholderText;
            textField.SetHintTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );

            textField.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            textField.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
        }  
    }
}

