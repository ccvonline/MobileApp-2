#if __IOS__
using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;
using CoreText;
using Rock.Mobile.UI.iOSNative;
using Rock.Mobile.PlatformSpecific.Util;
using Rock.Mobile.Animation;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The iOS implementation of a text field.
        /// </summary>
        public class iOSTextField : PlatformTextField
        {
            UITextField TextField { get; set; }
            UIColor _PlaceholderTextColor { get; set; }

            public iOSTextField( )
            {
                TextField = new UITextField( );
                TextField.Layer.AnchorPoint = new PointF( 0, 0 );
                TextField.TextAlignment = UITextAlignment.Left;
                TextField.Placeholder = "Placeholder";
                _PlaceholderTextColor = UIColor.Black;

                //TextField.ClipsToBounds = true;
            }

            protected override object getPlatformNativeObject()
            {
                return TextField;
            }

            // Properties
            public override void SetFont( string fontName, float fontSize )
            {
                try
                {
                    TextField.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont(fontName, fontSize);
                } 
                catch
                {
                    throw new Exception( string.Format( "Failed to load font: {0}", fontName ) );
                }
            }

            protected override KeyboardAppearanceStyle getKeyboardAppearance( )
            {
                return (KeyboardAppearanceStyle)TextField.KeyboardAppearance;
            }

            protected override void setKeyboardAppearance( KeyboardAppearanceStyle style )
            {
                TextField.KeyboardAppearance = (UIKeyboardAppearance)style;
            }

            protected override AutoCorrectionType getAutoCorrectionType( )
            {
                return (AutoCorrectionType)TextField.AutocorrectionType;
            }

            protected override void setAutoCorrectionType( AutoCorrectionType type )
            {
                TextField.AutocorrectionType = (UITextAutocorrectionType) type;
            }

            protected override AutoCapitalizationType getAutoCapitalizationType( )
            {
                return (AutoCapitalizationType)TextField.AutocapitalizationType;
            }

            protected override void setAutoCapitalizationType( AutoCapitalizationType type )
            {
                TextField.AutocapitalizationType = (UITextAutocapitalizationType) type;
            }

            protected override void setBorderColor( uint borderColor )
            {
                TextField.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( borderColor ).CGColor;
            }

            protected override uint getBorderColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( TextField.Layer.BorderColor );
            }

            protected override float getBorderWidth()
            {
                return (float) TextField.Layer.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                TextField.Layer.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return (float) TextField.Layer.CornerRadius;
            }
            protected override void setCornerRadius( float radius )
            {
                TextField.Layer.CornerRadius = radius;
            }

            protected override uint getBackgroundColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( TextField.Layer.BackgroundColor );
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                TextField.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( backgroundColor ).CGColor;
            }

            protected override float getOpacity( )
            {
                return TextField.Layer.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                TextField.Layer.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return (float)TextField.Layer.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                TextField.Layer.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return TextField.Bounds.ToRectF( );
            }

            protected override void setBounds( RectangleF bounds )
            {
                TextField.Bounds = bounds;
            }

            protected override RectangleF getFrame( )
            {
                return TextField.Frame.ToRectF( );
            }

            protected override void setFrame( RectangleF frame )
            {
                TextField.Frame = frame;
            }

            protected override  PointF getPosition( )
            {
                return TextField.Layer.Position.ToPointF( );
            }

            protected override void setPosition( PointF position )
            {
                TextField.Layer.Position = position;
            }

            protected override bool getHidden( )
            {
                return TextField.Hidden;
            }

            protected override void setHidden( bool hidden )
            {
                TextField.Hidden = hidden;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return TextField.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                TextField.UserInteractionEnabled = enabled;
            }

            protected override uint getTextColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( TextField.TextColor );
            }

            protected override void setTextColor( uint color )
            {
                TextField.TextColor = Rock.Mobile.UI.Util.GetUIColor( color );
            }

            protected override uint getPlaceholderTextColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( _PlaceholderTextColor );
            }

            protected override void setPlaceholderTextColor( uint color )
            {
                _PlaceholderTextColor = Rock.Mobile.UI.Util.GetUIColor( color );
                TextField.AttributedPlaceholder = new NSAttributedString( TextField.Placeholder, null, _PlaceholderTextColor );
            }

            protected override string getText( )
            {
                return TextField.Text;
            }

            protected override void setText( string text )
            {
                TextField.Text = text;
            }

            protected override TextAlignment getTextAlignment( )
            {
                return ( TextAlignment )TextField.TextAlignment;
            }

            protected override void setTextAlignment( TextAlignment alignment )
            {
                TextField.TextAlignment = ( UITextAlignment )alignment;
            }

            protected override string getPlaceholder( )
            {
                return TextField.Placeholder;
            }

            protected override void setPlaceholder( string placeholder )
            {
                TextField.AttributedPlaceholder = new NSAttributedString( placeholder, null, _PlaceholderTextColor );
            }

            public override void ResignFirstResponder( )
            {
                TextField.ResignFirstResponder( );
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new Exception( "Object passed to iOS AddAsSubview must be a UIView." );
                }

                view.AddSubview( TextField );
            }

            public override void RemoveAsSubview( object obj )
            {
                // Obj is only needed by Android, so we ignore it
                TextField.RemoveFromSuperview( );
            }

            public override void SizeToFit( )
            {
                TextField.SizeToFit( );
            }
        }
    }
}
#endif
