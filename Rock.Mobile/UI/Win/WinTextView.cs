#if __WIN__
using Rock.Mobile.PlatformSpecific.Win.Graphics;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The Windows implementation of a text view.
        /// </summary>
        public class WinTextView : PlatformTextView
        {
            protected Border Border { get; set; }
            protected TextBlock TextView { get; set; }
            protected Canvas ParentCanvas { get; set; }

            protected float FontSize { get; set; }
            protected string FontName { get; set; }

            public WinTextView( )
            {
                TextView = new TextBlock( );
                TextView.TextWrapping = System.Windows.TextWrapping.Wrap;
                TextView.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                setTextColor( 0x000000FF );

                Border = new Border( );
                Border.Child = TextView;

                setBorderColor( 0 );
                setBackgroundColor( 0 );

                // initialize it to 0
                setFrame( new RectangleF( 0, 0, 0, 0 ) );

                ParentCanvas = null;
            }

            // Properties
            public override void SetFont( string fontName, float fontSize )
            {
                System.Windows.Media.FontFamily fontFamily = null;
                System.Windows.Media.GlyphTypeface fontTypeface = null;

                FontManager.Instance.GetFont( fontName, out fontFamily, out fontTypeface );
                    
                if( fontFamily != null )
                {
                    TextView.FontFamily = fontFamily;
                    TextView.FontSize = fontSize;
                    TextView.FontWeight = fontTypeface.Weight;

                    // we only support italic and normal. So, if it explicitely uses italic, great. Otherwise, use normal
                    // (this filters out Oblique, which the font typeface has, but we don't support)
                    TextView.FontStyle = fontTypeface.Style == FontStyles.Italic ? fontTypeface.Style : FontStyles.Normal;

                    FontSize = fontSize;
                    FontName = fontName;
                }
                else
                {
                    FontSize = 0;
                    FontName = string.Empty;

                    throw new Exception( string.Format( "Failed to load font: {0}", fontName ) );
                }
            }
            
            protected override void setBackgroundColor( uint backgroundColor )
            {
                System.Windows.Media.Color color = Rock.Mobile.UI.Util.GetUIColor( backgroundColor );
                Border.Background = new SolidColorBrush( color );
            }

            protected override uint getBackgroundColor( )
            {
                SolidColorBrush colorBrush = Border.Background as SolidColorBrush;
                return Rock.Mobile.UI.Util.UIColorToInt( colorBrush.Color );
            }

            protected override void setBorderColor( uint borderColor )
            {
                System.Windows.Media.Color color = Rock.Mobile.UI.Util.GetUIColor( borderColor );
                Border.BorderBrush = new SolidColorBrush( color );
            }

            protected override uint getBorderColor( )
            {
                SolidColorBrush colorBrush = Border.BorderBrush as SolidColorBrush;
                return Rock.Mobile.UI.Util.UIColorToInt( colorBrush.Color );
            }

            protected override float getBorderWidth()
            {
                return (float) Border.BorderThickness.Left;
            }
            protected override void setBorderWidth( float width )
            {
                Border.BorderThickness = new System.Windows.Thickness( width );
            }

            protected override float getCornerRadius()
            {
                // we only support uniform corner radiuses (radii?), so just return topLeft
                return (float) Border.CornerRadius.TopLeft;
            }
            protected override void setCornerRadius( float radius )
            {
                System.Windows.CornerRadius cornerRadius = new System.Windows.CornerRadius( radius );
                Border.CornerRadius = cornerRadius;
            }

            protected override float getOpacity( )
            {
                return (float)Border.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                Border.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return (float) Canvas.GetZIndex( Border );
            }

            protected override void setZPosition( float zPosition )
            {
                Canvas.SetZIndex( Border, (int) zPosition );
            }

            protected override RectangleF getBounds( )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: We're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                return new RectangleF( 0, 0, (float)Border.Width, (float)Border.Height );
            }

            protected override void setBounds( RectangleF bounds )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: We're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                Border.Width = bounds.Width;
                Border.Height = bounds.Height;
                
                // Note: It's ok that Label's Width & Height are NaN. For WPF, that just means "not set", so it will defer to its parent.
            }

            protected override RectangleF getFrame( )
            {
                double left = Canvas.GetLeft( Border );
                double top = Canvas.GetTop( Border );

                return new RectangleF( (float) left, (float) top, (float) Border.Width, (float) Border.Height );
            }

            protected override void setFrame( RectangleF frame )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                setPosition( new PointF( frame.Left, frame.Top ) );

                setBounds( new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height ) );
            }

            protected override  PointF getPosition( )
            {
                double left = Canvas.GetLeft( Border );
                double top = Canvas.GetTop( Border );

                return new PointF( (float)left, (float)top );
            }

            protected override void setPosition( PointF position )
            {
                Canvas.SetLeft( Border, position.X );
                Canvas.SetTop( Border, position.Y );
            }

            protected override KeyboardAppearanceStyle getKeyboardAppearance( )
            {
                // not used on Windows
                return KeyboardAppearanceStyle.Dark;
            }

            protected override void setKeyboardAppearance( KeyboardAppearanceStyle style )
            {
                // not used on Windows
            }

            protected override AutoCorrectionType getAutoCorrectionType( )
            {
                // not used on Windows
                return AutoCorrectionType.Default;
            }

            protected override void setAutoCorrectionType( AutoCorrectionType type )
            {
                // not used on Windows
            }

            protected override AutoCapitalizationType getAutoCapitalizationType( )
            {
                // not used on Windows
                return AutoCapitalizationType.Words;
            }

            protected override void setAutoCapitalizationType( AutoCapitalizationType type )
            {
                // not used on Windows
            }

            protected override bool getScaleHeightForText( )
            {
                // not used on Windows
                return false;
            }

            protected override void setScaleHeightForText( bool scale )
            {
                // not used on Windows
            }

            protected override uint getTextColor( )
            {
                SolidColorBrush colorBrush = TextView.Foreground as SolidColorBrush;
                return Rock.Mobile.UI.Util.UIColorToInt( colorBrush.Color );
            }

            protected override void setTextColor( uint color )
            {
                System.Windows.Media.Color colorObj = Rock.Mobile.UI.Util.GetUIColor( color );
                TextView.Foreground = new SolidColorBrush( colorObj );
            }

            protected override string getText( )
            {
                return TextView.Text;
            }

            protected override void setText( string text )
            {
                TextView.Text = text;
            }

            protected override TextAlignment getTextAlignment( )
            {
                switch( TextView.TextAlignment )
                {
                    case System.Windows.TextAlignment.Left: return TextAlignment.Left;
                    case System.Windows.TextAlignment.Center: return TextAlignment.Center;
                    case System.Windows.TextAlignment.Right: return TextAlignment.Right;
                }

                return TextAlignment.Left;
            }

            protected override void setTextAlignment( TextAlignment alignment )
            {
                switch( alignment )
                {
                    case TextAlignment.Left:
                    {
                        TextView.TextAlignment = System.Windows.TextAlignment.Left;
                        break;
                    }

                    case TextAlignment.Center:
                    {
                        TextView.TextAlignment = System.Windows.TextAlignment.Center;
                        break;
                    }

                    case TextAlignment.Right:
                    {
                        TextView.TextAlignment = System.Windows.TextAlignment.Right;
                        break;
                    }
                }
            }

            protected override uint getPlaceholderTextColor( )
            {
                // not used on Windows
                return 0;
            }

            protected override void setPlaceholderTextColor( uint color )
            {
                // not used on Windows
            }

            protected override string getPlaceholder( )
            {
                // not used on Windows
                return string.Empty;
            }

            protected override void setPlaceholder( string placeholder )
            {
                // not used on Windows
            }

            public override void ResignFirstResponder( )
            {
                // not used on Windows
            }

            public override void BecomeFirstResponder( )
            {
                // not used on Windows
            }

            public override void AnimateOpen( bool becomeFirstResponder )
            {
                // not used on Windows
            }

            public override void AnimateClosed( )
            {
                // not used on Windows
            }

            protected override bool getHidden( )
            {
                return TextView.Visibility == System.Windows.Visibility.Hidden ? true : false;
            }

            protected override void setHidden( bool hidden )
            {
                TextView.Visibility = hidden == true ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return true;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                //TextView.UserInteractionEnabled = enabled;
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be a Canvas.
                Canvas view = masterView as Canvas;
                if( view == null )
                {
                    throw new Exception( "Object passed to Windows AddAsSubview must be a Canvas." );
                }
                ParentCanvas = view;

                ParentCanvas.Children.Add( Border );
            }

            public override void RemoveAsSubview( object obj )
            {
                // we know that masterView will be a Canvas.
                Canvas view = obj as Canvas;
                if( view == null )
                {
                    throw new Exception( "Object passed to Windows AddAsSubview must be a Canvas." );
                }

                ParentCanvas.Children.Remove( Border );

                ParentCanvas = null;
            }

            protected override object getPlatformNativeObject()
            {
                return TextView;
            }

            public override void SizeToFit( )
            {
                // not needed
            }
        }
    }
}
#endif
