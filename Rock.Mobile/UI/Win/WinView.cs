#if __WIN__
using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The Windows implementation of a view
        /// </summary>
        public class WinView : PlatformView
        {
            Border Border { get; set; }
            protected Canvas CanvasView { get; set; }

            public WinView( )
            {
                CanvasView = new Canvas( );

                Border = new Border( );
                Border.Child = CanvasView;
                
                setBorderColor( 0 );
                setBackgroundColor( 0 );

                // initialize it to 0
                setFrame( new RectangleF( 0, 0, 0, 0 ) );
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                System.Windows.Media.Color color = Rock.Mobile.UI.Util.GetUIColor( backgroundColor );
                CanvasView.Background = new SolidColorBrush( color );
            }

            protected override uint getBackgroundColor( )
            {
                SolidColorBrush colorBrush = CanvasView.Background as SolidColorBrush;
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
                // we only support uniform border thickness, so just return left
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
                return (float)Canvas.GetZIndex( Border );
            }

            protected override void setZPosition( float zPosition )
            {
                Canvas.SetZIndex( Border, (int)zPosition );
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

                // Note: It's ok that CanvasView's Width & Height are NaN. For WPF, that just means "not set", so it will defer to its parent.
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

            protected override PointF getPosition( )
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

            protected override bool getHidden( )
            {
                return Border.IsVisible;
            }

            protected override void setHidden( bool hidden )
            {
                Border.Visibility = hidden == true ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return true;//View.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                //View.UserInteractionEnabled = enabled;
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be a Windows Canvas
                Canvas view = masterView as Canvas;
                if( view == null )
                {
                    throw new Exception( "Object passed to Windows AddAsSubview must be a Canvas." );
                }
                view.Children.Add( Border );
            }

            public override void RemoveAsSubview( object obj )
            {
                Canvas view = obj as Canvas;
                if( view == null )
                {
                    throw new Exception( "Object passed to Windows RemoveAsSubview must be a Canvas." );
                }
                view.Children.Remove( Border );
            }

            protected override object getPlatformNativeObject( )
            {
                return Border;
            }
        }
    }
}
#endif
