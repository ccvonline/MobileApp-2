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
        public class iOSTextView : PlatformTextView
        {
            DynamicUITextView TextView { get; set; }

            /// <summary>
            /// The time to animate the text box as it grows
            /// </summary>
            float SCALE_TIME_SECONDS = .20f;

            NSObject ObserverHandle { get; set; }

            public iOSTextView( )
            {
                TextView = new DynamicUITextView( );
                TextView.Layer.AnchorPoint = new PointF( 0, 0 );
                TextView.TextAlignment = UITextAlignment.Left;


                TextView.Editable = true;
                TextView.ClipsToBounds = true;
            }

            protected void OnTextChanged( NSNotification notification )
            {
                if ( notification.Object == TextView )
                {
                    if ( OnEditCallback != null )
                    {
                        OnEditCallback( this );
                    }
                }
            }

            protected override object getPlatformNativeObject()
            {
                return TextView;
            }

            // Properties
            public override void SetFont( string fontName, float fontSize )
            {
                try
                {
                    TextView.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont(fontName, fontSize);
                } 
                catch
                {
                    throw new Exception( string.Format( "Failed to load font: {0}", fontName ) );
                }
            }

            protected override KeyboardAppearanceStyle getKeyboardAppearance( )
            {
                return (KeyboardAppearanceStyle)TextView.KeyboardAppearance;
            }

            protected override void setKeyboardAppearance( KeyboardAppearanceStyle style )
            {
                TextView.KeyboardAppearance = (UIKeyboardAppearance)style;
            }

            protected override AutoCorrectionType getAutoCorrectionType( )
            {
                return (AutoCorrectionType)TextView.AutocorrectionType;
            }

            protected override void setAutoCorrectionType( AutoCorrectionType type )
            {
                TextView.AutocorrectionType = (UITextAutocorrectionType) type;
            }

            protected override AutoCapitalizationType getAutoCapitalizationType( )
            {
                return (AutoCapitalizationType)TextView.AutocapitalizationType;
            }

            protected override void setAutoCapitalizationType( AutoCapitalizationType type )
            {
                TextView.AutocapitalizationType = (UITextAutocapitalizationType) type;
            }

            protected override void setBorderColor( uint borderColor )
            {
                TextView.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( borderColor ).CGColor;
            }

            protected override uint getBorderColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( TextView.Layer.BorderColor );
            }

            protected override float getBorderWidth()
            {
                return (float) TextView.Layer.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                TextView.Layer.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return (float) TextView.Layer.CornerRadius;
            }
            protected override void setCornerRadius( float radius )
            {
                TextView.Layer.CornerRadius = radius;
            }

            protected override uint getBackgroundColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( TextView.Layer.BackgroundColor );
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                TextView.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( backgroundColor ).CGColor;
            }

            protected override float getOpacity( )
            {
                return TextView.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                TextView.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return TextView.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                TextView.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return TextView.Bounds.ToRectF( );
            }

            protected override void setBounds( RectangleF bounds )
            {
                TextView.Bounds = bounds;
            }

            protected override RectangleF getFrame( )
            {
                return TextView.Frame.ToRectF( );
            }

            protected override void setFrame( RectangleF frame )
            {
                TextView.Frame = frame;
            }

            protected override  PointF getPosition( )
            {
                return TextView.Position;
            }

            protected override void setPosition( PointF position )
            {
                TextView.Position = position;
            }

            protected override bool getHidden( )
            {
                return TextView.Hidden;
            }

            protected override void setHidden( bool hidden )
            {
                TextView.Hidden = hidden;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return TextView.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                TextView.UserInteractionEnabled = enabled;
            }

            protected override uint getTextColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( TextView.TextColor );
            }

            protected override void setTextColor( uint color )
            {
                TextView.TextColor = Rock.Mobile.UI.Util.GetUIColor( color );
            }

            protected override uint getPlaceholderTextColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( TextView.PlaceholderTextColor );
            }

            protected override void setPlaceholderTextColor( uint color )
            {
                TextView.PlaceholderTextColor = Rock.Mobile.UI.Util.GetUIColor( color );
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
                return ( TextAlignment )TextView.TextAlignment;
            }

            protected override void setTextAlignment( TextAlignment alignment )
            {
                TextView.TextAlignment = ( UITextAlignment )alignment;
            }

            protected override string getPlaceholder( )
            {
                return TextView.Placeholder;
            }

            protected override void setPlaceholder( string placeholder )
            {
                TextView.Placeholder = placeholder;
            }

            protected override bool getScaleHeightForText( )
            {
                return TextView.ScaleHeightForText;
            }

            protected override void setScaleHeightForText( bool scale )
            {
                TextView.ScaleHeightForText = scale;
            }

            public override void ResignFirstResponder( )
            {
                TextView.ResignFirstResponder( );
            }

            public override void BecomeFirstResponder( )
            {
                TextView.BecomeFirstResponder( );
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new Exception( "Object passed to iOS AddAsSubview must be a UIView." );
                }

                TextView.AddAsSubview( view );

                // add the handler when we're in a hierarchy
                ObserverHandle = NSNotificationCenter.DefaultCenter.AddObserver( UITextView.TextDidChangeNotification, OnTextChanged );
            }

            public override void RemoveAsSubview( object obj )
            {
                // Obj is only needed by Android, so we ignore it
                TextView.RemoveFromSuperview( );

                // and remove it when we're not
                NSNotificationCenter.DefaultCenter.RemoveObserver( ObserverHandle );
            }

            public override void SizeToFit( )
            {
                TextView.SizeToFit( );
            }

            public override void AnimateOpen( bool becomeFirstResponder )
            {
                if ( TextView.Animating == false && TextView.Hidden == true )
                {
                    // unhide and flag it as animating
                    TextView.Hidden = false;
                    TextView.Animating = true;

                    // and force it to a 0 size so it grows correctly
                    TextView.Bounds = RectangleF.Empty;

                    SimpleAnimator_SizeF animator = new SimpleAnimator_SizeF( TextView.Bounds.Size.ToSizeF( ),
                                                                              TextView.NaturalSize.ToSizeF( ), SCALE_TIME_SECONDS, 
                        delegate(float percent, object value )
                        {
                            SizeF currSize = (SizeF)value;
                            TextView.Bounds = new RectangleF( 0, 0, currSize.Width, currSize.Height );
                        },
                        delegate
                        {
                            TextView.Animating = false;
                            if( becomeFirstResponder == true )
                            {
                                BecomeFirstResponder( );
                            }
                        } );

                    animator.Start( );
                }
            }

            public override void AnimateClosed( )
            {
                if ( TextView.Animating == false && TextView.Hidden == false )
                {
                    TextView.Animating = true;

                    SimpleAnimator_SizeF animator = new SimpleAnimator_SizeF( TextView.Bounds.Size.ToSizeF( ), new SizeF( 0, 0 ), SCALE_TIME_SECONDS, 
                        delegate(float percent, object value )
                        {
                            SizeF currSize = (SizeF)value;
                            TextView.Bounds = new RectangleF( 0, 0, currSize.Width, currSize.Height );
                        },
                        delegate
                        {
                            TextView.Hidden = true;
                            TextView.Animating = false;
                        } );

                    animator.Start( );
                }
            }
        }
    }
}
#endif
