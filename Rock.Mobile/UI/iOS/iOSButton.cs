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
using Rock.Mobile.PlatformSpecific.iOS.Graphics;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The iOS implementation of a text field.
        /// </summary>
        public class iOSButton : PlatformButton
        {
            UIButton Button { get; set; }

            public iOSButton( )
            {
                Button = UIButton.FromType( UIButtonType.System );
                Button.Layer.AnchorPoint = new PointF( 0, 0 );
                Button.ClipsToBounds = true;

                // setup a callback that will use any provided delegate the user gives us.
                Button.TouchUpInside += (object sender, EventArgs e ) =>
                    {
                        if( ClickCallbackDelegate != null )
                        {
                            ClickCallbackDelegate( this );
                        }
                    };
            }

            // Properties
            public override void SetFont( string fontName, float fontSize )
            {
                try
                {
                    Button.Font = FontManager.GetFont(fontName, fontSize);
                } 
                catch
                {
                    throw new Exception( string.Format( "Failed to load font: {0}", fontName ) );
                }
            }

            protected override void setBorderColor( uint borderColor )
            {
                Button.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( borderColor ).CGColor;
            }

            protected override uint getBorderColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( Button.Layer.BorderColor );
            }

            protected override float getBorderWidth()
            {
                return (float) Button.Layer.BorderWidth;
            }

            protected override void setBorderWidth( float width )
            {
                Button.Layer.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return (float) Button.Layer.CornerRadius;
            }
            protected override void setCornerRadius( float radius )
            {
                Button.Layer.CornerRadius = radius;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                Button.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( backgroundColor ).CGColor;
            }

            protected override uint getBackgroundColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( Button.Layer.BackgroundColor );
            }

            protected override float getOpacity( )
            {
                return Button.Layer.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                Button.Layer.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return (float)Button.Layer.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                Button.Layer.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return Button.Bounds.ToRectF( );
            }

            protected override void setBounds( RectangleF bounds )
            {
                Button.Bounds = bounds;
            }

            protected override RectangleF getFrame( )
            {
                return Button.Frame.ToRectF( );
            }

            protected override void setFrame( RectangleF frame )
            {
                Button.Frame = frame;
            }

            protected override  PointF getPosition( )
            {
                return Button.Layer.Position.ToPointF( );
            }

            protected override void setPosition( PointF position )
            {
                Button.Layer.Position = position;
            }

            protected override bool getHidden( )
            {
                return Button.Hidden;
            }

            protected override void setHidden( bool hidden )
            {
                Button.Hidden = hidden;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return Button.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                Button.UserInteractionEnabled = enabled;
            }

            protected override uint getTextColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( Button.TitleColor( UIControlState.Normal ) );
            }

            protected override void setTextColor( uint color )
            {
                Button.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( color ), UIControlState.Normal );
            }

            protected override string getText( )
            {
                return Button.Title( UIControlState.Normal );
            }

            protected override void setText( string text )
            {
                Button.SetTitle( text, UIControlState.Normal );
            }

            protected override OnClick getClickEvent( )
            {
                return ClickCallbackDelegate;
            }

            protected override void setClickEvent( OnClick clickEvent )
            {
                // store the callback they want. When TouchUpInside is called,
                // it will invoke this delegate
                ClickCallbackDelegate = clickEvent;
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new Exception( "Object passed to iOS AddAsSubview must be a UIView." );
                }

                view.AddSubview( Button );
            }

            public override void RemoveAsSubview( object obj )
            {
                // Obj is only needed by Android, so we ignore it
                Button.RemoveFromSuperview( );
            }

            public override void SizeToFit( )
            {
                Button.SizeToFit( );
            }
        }
    }
}
#endif
