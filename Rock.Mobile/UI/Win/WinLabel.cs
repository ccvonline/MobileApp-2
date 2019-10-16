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
        /// The Windows implementation of a text label.
        /// </summary>
        public class WinLabel : PlatformLabel
        {
            protected Border Border { get; set; }
            protected TextBlock Label { get; set; }
            protected Border UnderlineView { get; set; }
            protected Canvas ParentCanvas { get; set; }

            protected float FontSize { get; set; }
            protected string FontName { get; set; }

            public WinLabel( )
            {
                Label = new TextBlock( );
                Label.TextWrapping = System.Windows.TextWrapping.Wrap;
                Label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                setTextColor( 0x000000FF );

                Border = new Border( );
                Border.Child = Label;

                setBorderColor( 0 );
                setBackgroundColor( 0 );

                // initialize it to 0
                setFrame( new RectangleF( 0, 0, 0, 0 ) );

                ParentCanvas = null;
            }

            // Properties

            /// <summary>
            /// Adds an underline to the text. Must be called before adding the label to the UI.
            /// So basically call it immediately after creating.
            /// </summary>
            public override void AddUnderline( )
            {
                // create our border
                if ( UnderlineView == null )
                {
                    UnderlineView = new Border( );
                    UnderlineView.Background = Label.Foreground;
                }
            }

            public override void SetFont( string fontName, float fontSize )
            {
                System.Windows.Media.FontFamily fontFamily = null;
                System.Windows.Media.GlyphTypeface fontTypeface = null;

                FontManager.Instance.GetFont( fontName, out fontFamily, out fontTypeface );
                    
                if( fontFamily != null )
                {
                    Label.FontFamily = fontFamily;
                    Label.FontSize = fontSize;
                    Label.FontWeight = fontTypeface.Weight;

                    // we only support italic and normal. So, if it explicitely uses italic, great. Otherwise, use normal
                    // (this filters out Oblique, which the font typeface has, but we don't support)
                    Label.FontStyle = fontTypeface.Style == FontStyles.Italic ? fontTypeface.Style : FontStyles.Normal;

                    UpdateUnderline( );

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

            public override void Editable_SetFontSize( float size )
            {
                SetFont( FontName, size );
            }

            public override float Editable_GetFontSize( )
            {
                return FontSize;
            }

            public override void Editable_SetFontName( string name )
            {
                SetFont( name, FontSize );
            }

            public override string Editable_GetFontName( )
            {
                return FontName;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                System.Windows.Media.Color color = Rock.Mobile.UI.Util.GetUIColor( backgroundColor );
                Label.Background = new SolidColorBrush( color );
            }

            protected override uint getBackgroundColor( )
            {
                SolidColorBrush colorBrush = Label.Background as SolidColorBrush;
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

                UpdateUnderline();
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

                UpdateUnderline();
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

                UpdateUnderline( );
            }

            protected override uint getTextColor( )
            {
                SolidColorBrush colorBrush = Label.Foreground as SolidColorBrush;
                return Rock.Mobile.UI.Util.UIColorToInt( colorBrush.Color );
            }

            protected override void setTextColor( uint color )
            {
                System.Windows.Media.Color colorObj = Rock.Mobile.UI.Util.GetUIColor( color );
                Label.Foreground = new SolidColorBrush( colorObj );

                // if there's an underline, keep the color sync'd
                if( UnderlineView != null )
                {
                    UnderlineView.Background = new SolidColorBrush( colorObj );
                }
            }

            protected override string getText( )
            {
                return Label.Text;
            }

            protected override void setText( string text )
            {
                Label.Text = text;
            }

            protected override TextAlignment getTextAlignment( )
            {
                switch( Label.TextAlignment )
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
                        Label.TextAlignment = System.Windows.TextAlignment.Left;
                        break;
                    }

                    case TextAlignment.Center:
                    {
                        Label.TextAlignment = System.Windows.TextAlignment.Center;
                        break;
                    }

                    case TextAlignment.Right:
                    {
                        Label.TextAlignment = System.Windows.TextAlignment.Right;
                        break;
                    }
                }
            }

            protected override bool getHidden( )
            {
                return Label.Visibility == System.Windows.Visibility.Hidden ? true : false;
            }

            protected override void setHidden( bool hidden )
            {
                Label.Visibility = hidden == true ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return true;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                //Label.UserInteractionEnabled = enabled;
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

                if ( UnderlineView != null )
                {
                    ParentCanvas.Children.Add( UnderlineView );
                }
            }

            public override void RemoveAsSubview( object obj )
            {
                // we know that masterView will be a Canvas.
                Canvas view = obj as Canvas;
                if( view == null )
                {
                    throw new Exception( "Object passed to Windows AddAsSubview must be a Canvas." );
                }

                if( UnderlineView != null )
                {
                    ParentCanvas.Children.Remove( UnderlineView );
                }

                ParentCanvas.Children.Remove( Border );

                ParentCanvas = null;
            }

            public override void SizeToFit( )
            {
                // SizeToFit will either update width / height for the text to fit on one line,
                // or if a width is already set, update the height so that it fits wrapped text, and
                // then set the width to be what the text actually requires

                // first, measure this text, assuming it will all fit on one line
                var formattedText = new FormattedText( Label.Text, 
                                                       CultureInfo.CurrentUICulture, 
                                                       FlowDirection.LeftToRight, 
                                                       new Typeface( Label.FontFamily, Label.FontStyle, Label.FontWeight, Label.FontStretch ), 
                                                       Label.FontSize, 
                                                       System.Windows.Media.Brushes.Black );

                // if a width was set, then we can't go over that width, so set MaxTextWidth.
                // This will then update the measured height so that it takes into account text wrapping
                if ( Border.Width > 0 )
                {
                    formattedText.MaxTextWidth = Border.Width;
                }

                // round UP so that we don't truncate text due to the border being too short by a pixel in either direction
                setBounds( new RectangleF( 0, 0, (float) global::System.Math.Ceiling( formattedText.WidthIncludingTrailingWhitespace ), (float)global::System.Math.Ceiling( formattedText.Height ) ) );

                UpdateUnderline();
            }

            public override float GetFade()
            {
                return 1.00f;
            }

            public override void SetFade( float fadeAmount )
            {
            }

            public override void AnimateToFade( float fadeAmount )
            {
            }

            void UpdateUnderline()
            {
                // now adjust the border by only the difference
                if ( UnderlineView != null )
                {
                    double xPos = Canvas.GetLeft( Border );
                    double yPos = Canvas.GetTop( Border );

                    Canvas.SetLeft( UnderlineView, xPos );
                    Canvas.SetTop( UnderlineView, Frame.Bottom );

                    UnderlineView.Width = Border.Width;
                    UnderlineView.Height = 2;
                }
            }

            public override void Editable_AddUnderline( )
            {
                // create our border
                if ( UnderlineView == null )
                {
                    UnderlineView = new Border( );
                    ParentCanvas.Children.Add( UnderlineView );

                    setTextColor( getTextColor( ) );
                    UpdateUnderline( );
                }
            }

            public override bool Editable_HasUnderline( )
            {
                return UnderlineView != null ? true : false;
            }

            public override void Editable_RemoveUnderline( )
            {
                if ( UnderlineView != null )
                {
                    ParentCanvas.Children.Remove( UnderlineView );
                    UnderlineView = null;
                }
            }
        }
    }
}
#endif
