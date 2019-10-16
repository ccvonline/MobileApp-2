#if __IOS__
using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;
using CoreText;
using Rock.Mobile.PlatformSpecific.Util;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Subclass of UIView to render an underline under the fading text. 
        /// </summary>
        public class UnderlineUIView : UIView
        {
            public override void Draw(CGRect rect)
            {
                base.Draw(rect);

                using( CGContext context = UIGraphics.GetCurrentContext() )
                {
                    context.AddRect( new CGRect( 0, 0, Frame.Width, 1 ) );
                    context.Clip();

                    UIColor color = Rock.Mobile.UI.Util.GetUIColor( 0x777777FF );

                    // probably should change this to not being a gradient
                    CGGradient gradiant = new CGGradient( CGColorSpace.CreateDeviceRGB(), new CGColor[] { color.CGColor, color.CGColor });
                    context.DrawLinearGradient( gradiant, new CGPoint( 0, 0 ), new CGPoint( Frame.Width, 0 ), CGGradientDrawingOptions.DrawsBeforeStartLocation );
                }
            }
        }

        /// <summary>
        /// The iOS implementation of a text label.
        /// </summary>
        public class iOSLabel : PlatformLabel
        {
            /// <summary>
            /// The amount to scale the border by relative to the text width.
            /// Useful if a using gradiants that fade out too early, or in the
            /// case where too solid lines butt against each other.
            /// </summary>
            static float BORDER_WIDTH_SCALER = .99f;

            protected UILabel Label { get; set; }
            protected UnderlineUIView UnderlineView { get; set; }

            public iOSLabel( )
            {
                Label = new UILabel( );
                Label.Layer.AnchorPoint = new PointF( 0, 0 );
                Label.TextAlignment = UITextAlignment.Left;
                Label.LineBreakMode = UILineBreakMode.WordWrap;
                Label.Lines = 0;
                Label.ClipsToBounds = true;
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
                    UnderlineView = new UnderlineUIView();
                    UnderlineView.Layer.AnchorPoint = Label.Layer.AnchorPoint;
                    UnderlineView.BackgroundColor = UIColor.Clear;
                }
            }

            public override void SetFont( string fontName, float fontSize )
            {
                try
                {
                    Label.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont(fontName, fontSize);
                } 
                catch
                {
                    throw new Exception( string.Format( "Failed to load font: {0}", fontName ) );
                }
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                Label.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( backgroundColor ).CGColor;
            }

            protected override uint getBackgroundColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( Label.Layer.BackgroundColor );
            }

            protected override void setBorderColor( uint borderColor )
            {
                Label.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( borderColor ).CGColor;
            }

            protected override uint getBorderColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( Label.Layer.BorderColor );
            }

            protected override float getBorderWidth()
            {
                return (float) Label.Layer.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                Label.Layer.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return (float) Label.Layer.CornerRadius;
            }
            protected override void setCornerRadius( float radius )
            {
                Label.Layer.CornerRadius = radius;
            }

            protected override float getOpacity( )
            {
                return Label.Layer.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                Label.Layer.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return (float) Label.Layer.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                Label.Layer.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return Label.Bounds.ToRectF( );
            }

            protected override void setBounds( RectangleF bounds )
            {
                Label.Bounds = bounds;

                UpdateUnderline();
            }

            protected override RectangleF getFrame( )
            {
                return Label.Frame.ToRectF( );
            }

            protected override void setFrame( RectangleF frame )
            {
                Label.Frame = frame;

                UpdateUnderline();
            }

            protected override  PointF getPosition( )
            {
                return Label.Layer.Position.ToPointF( );
            }

            protected override void setPosition( PointF position )
            {
                // to position the border, first get the amount we'll be moving
                float deltaX = (float) (position.X - Label.Frame.X);
                float deltaY = (float) (position.Y - Label.Frame.Y);

                Label.Layer.Position = position;

                // now adjust the border by only the difference
                if ( UnderlineView != null )
                {
                    UnderlineView.Layer.Position = new PointF( (float) (UnderlineView.Layer.Position.X + deltaX), 
                                                               (float) (UnderlineView.Layer.Position.Y + deltaY) );
                }
            }

            protected override uint getTextColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( Label.TextColor );
            }

            protected override void setTextColor( uint color )
            {
                Label.TextColor = Rock.Mobile.UI.Util.GetUIColor( color );
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
                return ( TextAlignment )Label.TextAlignment;
            }

            protected override void setTextAlignment( TextAlignment alignment )
            {
                Label.TextAlignment = ( UITextAlignment )alignment;
            }

            protected override bool getHidden( )
            {
                return Label.Hidden;
            }

            protected override void setHidden( bool hidden )
            {
                Label.Hidden = hidden;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return Label.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                Label.UserInteractionEnabled = enabled;
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new Exception( "Object passed to iOS AddAsSubview must be a UIView." );
                }
                view.AddSubview( Label );

                if ( UnderlineView != null )
                {
                    view.AddSubview( UnderlineView );
                }
            }

            public override void RemoveAsSubview( object obj )
            {
                //obj is only for Android, so we don't use it.
                if ( UnderlineView != null )
                {
                    UnderlineView.RemoveFromSuperview( );
                }

                Label.RemoveFromSuperview( );
            }

            public override void SizeToFit( )
            {
                Label.SizeToFit( );

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
                if ( UnderlineView != null )
                {
                    // determine how far down the border should start.
                    // The ascender is basically the distance from the top of the highest font to where the baseline is,
                    // which is effectively the character height.
                    float borderYOffset = (float) Label.Font.Ascender + 2;

                    // Same for X, horizontally
                    float borderWidth = (int)( (float)Label.Frame.Width * BORDER_WIDTH_SCALER );
                    float borderXOffset = (float) ( Label.Frame.Width - borderWidth ) / 2;

                    UnderlineView.Layer.Position = new CGPoint( Label.Frame.X + (int)borderXOffset, Label.Frame.Y + (int)borderYOffset );
                    UnderlineView.Layer.Bounds = new CGRect( 0, 0, borderWidth, 5 );
                }
            }
        }
    }
}
#endif
