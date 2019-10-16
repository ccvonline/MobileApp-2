#if __ANDROID__
using System;
using System.Drawing;
using Android.Widget;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.App;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Rock.Mobile.UI.DroidNative;
using Android.Util;
using Android.Text;
using Java.Lang;
using Java.Lang.Reflect;
using Rock.Mobile.Animation;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Android implementation of a text view.
        /// </summary>
        public class DroidTextField : PlatformTextField
        {
            protected BorderedRectEditText TextField { get; set; }
            protected uint _BackgroundColor { get; set; }
            protected uint _BorderColor { get; set; }
            protected uint _TextColor { get; set; }
            protected uint _PlaceholderTextColor { get; set; }

            protected static Field CursorResource { get; set; }

            public DroidTextField( )
            {
                TextField = new BorderedRectEditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                TextField.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                TextField.SetScrollContainer( false );
                TextField.SetMaxLines( 1 );
                TextField.SetSingleLine( );

                TextField.SetHorizontallyScrolling( false );

                // use reflection to get a reference to the TextField's cursor resource
                if ( CursorResource == null )
                {
                    CursorResource = Java.Lang.Class.ForName( "android.widget.TextView" ).GetDeclaredField( "mCursorDrawableRes" );
                    CursorResource.Accessible = true;
                }
            }

            protected override object getPlatformNativeObject()
            {
                return TextField;
            }

            // Properties
            protected override KeyboardAppearanceStyle getKeyboardAppearance( )
            {
                // android doesn't support modifying the keyboard appearance
                return (KeyboardAppearanceStyle) 0;
            }

            protected override void setKeyboardAppearance( KeyboardAppearanceStyle style )
            {
                // android doesn't support modifying the keyboard appearance
            }

            protected override AutoCorrectionType getAutoCorrectionType()
            {
                // if it's set, go ahead and return yes
                if ( ( TextField.InputType & InputTypes.TextFlagAutoCorrect ) != 0 )
                {
                    return AutoCorrectionType.Yes;
                }

                return AutoCorrectionType.No;
            }

            protected override void setAutoCorrectionType(AutoCorrectionType style)
            {
                switch ( style )
                {
                    case AutoCorrectionType.Yes:
                    {
                        TextField.InputType |= InputTypes.TextFlagAutoCorrect;
                        break;
                    }

                    case AutoCorrectionType.Default:
                    case AutoCorrectionType.No:
                    {
                        TextField.InputType &= ~InputTypes.TextFlagAutoCorrect;
                        break;
                    }
                }
            }

            protected override AutoCapitalizationType getAutoCapitalizationType()
            {
                // see which capitalizaton mask is active
                if ( (TextField.InputType & InputTypes.TextFlagCapWords) != 0 )
                {
                    return AutoCapitalizationType.Words;
                }
                else if ( (TextField.InputType & InputTypes.TextFlagCapSentences) != 0 )
                {
                    return AutoCapitalizationType.Sentences;
                }
                else if ( (TextField.InputType & InputTypes.TextFlagCapCharacters) != 0 )
                {
                    return AutoCapitalizationType.All;
                }

                return AutoCapitalizationType.None;
            }

            protected override void setAutoCapitalizationType(AutoCapitalizationType style)
            {
                // first clear all fields
                TextField.InputType &= ~(InputTypes.TextFlagCapWords | InputTypes.TextFlagCapCharacters | InputTypes.TextFlagCapSentences);

                switch ( style )
                {
                    case AutoCapitalizationType.All:
                    {
                        // for everything, add it all
                        TextField.InputType |= InputTypes.TextFlagCapCharacters;
                        break;
                    }

                    case AutoCapitalizationType.Sentences:
                    {
                        // for everything, add it all
                        TextField.InputType |= InputTypes.TextFlagCapSentences;
                        break;
                    }

                    case AutoCapitalizationType.Words:
                    {
                        // for everything, add it all
                        TextField.InputType |= InputTypes.TextFlagCapWords;
                        break;
                    }
                    
                    case AutoCapitalizationType.None:
                    {
                        // if "none" was requested, do nothing.
                        break;
                    }
                }
            }

            public override void SetFont( string fontName, float fontSize )
            {
                try
                {
                    Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( fontName );
                    TextField.SetTypeface( fontFace, TypefaceStyle.Normal );
                    TextField.SetTextSize( Android.Util.ComplexUnitType.Dip, fontSize );
                } 
                catch
                {
                    throw new System.Exception( string.Format( "Unable to load font: {0}", fontName ) );
                }
            }

            protected override uint getBackgroundColor()
            {
                return _BackgroundColor;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                _BackgroundColor = backgroundColor;
                TextField.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( backgroundColor ) );
                TextField.Invalidate( );

                // normalize the color so we can determine what color to use for the cursor
                float normalizedColor = (float) backgroundColor / (float)0xFFFFFFFF;

                // background is closer to white, use a dark cursor
                if ( normalizedColor > .50f )
                {
                    CursorResource.Set( TextField, Droid.Resource.Drawable.dark_cursor );
                }
                else
                {
                    // background is closer to black, use a light cursor
                    CursorResource.Set( TextField, Droid.Resource.Drawable.light_cursor );
                }
            }

            protected override void setBorderColor( uint borderColor )
            {
                _BorderColor = borderColor;
                TextField.SetBorderColor( Rock.Mobile.UI.Util.GetUIColor( borderColor ) );
                TextField.Invalidate( );
            }

            protected override uint getBorderColor( )
            {
                return _BorderColor;
            }

            protected override float getBorderWidth()
            {
                return TextField.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                TextField.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return TextField.Radius;
            }
            protected override void setCornerRadius( float radius )
            {
                TextField.Radius = radius;
            }

            protected override float getOpacity( )
            {
                return TextField.Alpha;
            }

            protected override void setOpacity( float opacity )
            {
                TextField.Alpha = opacity;
            }

            protected override float getZPosition( )
            {
                //Android doesn't use/need a Z position for its layers. (It goes based on order added)
                return 0.0f;
            }

            protected override void setZPosition( float zPosition )
            {
                //Android doesn't use/need a Z position for its layers. (It goes based on order added)
            }

            protected override RectangleF getBounds( )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                return new RectangleF( 0, 0, TextField.LayoutParameters.Width, TextField.LayoutParameters.Height );
            }

            protected override void setBounds( RectangleF bounds )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                TextField.SetMinWidth( (int)bounds.Width );
                TextField.SetMaxWidth( TextField.LayoutParameters.Width );

                TextField.LayoutParameters.Height = ( int )bounds.Height;
            }

            protected override RectangleF getFrame( )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                RectangleF frame = new RectangleF( TextField.GetX( ), TextField.GetY( ), TextField.LayoutParameters.Width, TextField.LayoutParameters.Height );
                return frame;
            }

            protected override void setFrame( RectangleF frame )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                setPosition( new System.Drawing.PointF( frame.X, frame.Y ) );

                RectangleF bounds = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );
                setBounds( bounds );
            }

            protected override System.Drawing.PointF getPosition( )
            {
                return new System.Drawing.PointF( TextField.GetX( ), TextField.GetY( ) );
            }

            protected override void setPosition( System.Drawing.PointF position )
            {
                TextField.SetX( position.X );
                TextField.SetY( position.Y );
            }

            protected override bool getHidden( )
            {
                return TextField.Visibility == ViewStates.Gone ? true : false;
            }

            protected override void setHidden( bool hidden )
            {
                TextField.Visibility = hidden == true ? ViewStates.Gone : ViewStates.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                // doesn't matter if we return this or regular Focusable,
                // because we set them both, guaranteeing the same value.
                return TextField.FocusableInTouchMode;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                TextField.FocusableInTouchMode = enabled;
                TextField.Focusable = enabled;
            }

            protected override uint getTextColor( )
            {
                return _TextColor;
            }

            protected override void setTextColor( uint color )
            {
                TextField.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( color ) );
            }

            protected override string getText( )
            {
                return TextField.Text;
            }

            protected override void setText( string text )
            {
                TextField.Text = text;
            }

            protected override uint getPlaceholderTextColor( )
            {
                return _PlaceholderTextColor;
            }

            protected override void setPlaceholderTextColor( uint color )
            {
                TextField.SetHintTextColor( Rock.Mobile.UI.Util.GetUIColor( color ) );
            }

            protected override string getPlaceholder( )
            {
                return TextField.Hint;
            }

            protected override void setPlaceholder( string placeholder )
            {
                TextField.Hint = placeholder;
            }

            protected override TextAlignment getTextAlignment( )
            {
                // gonna have to do a stupid transform
                switch( TextField.Gravity )
                {
                    case GravityFlags.Center:
                        return TextAlignment.Center;
                    case GravityFlags.Left:
                        return TextAlignment.Left;
                    case GravityFlags.Right:
                        return TextAlignment.Right;
                    default:
                        return TextAlignment.Left;
                }
            }

            protected override void setTextAlignment( TextAlignment alignment )
            {
                switch( alignment )
                {
                    case TextAlignment.Center:
                    TextField.Gravity = GravityFlags.Center;
                        break;
                    case TextAlignment.Left:
                    TextField.Gravity = GravityFlags.Left;
                        break;
                    case TextAlignment.Right:
                    TextField.Gravity = GravityFlags.Right;
                        break;
                    default:
                    TextField.Gravity = GravityFlags.Left;
                        break;
                }
            }

            public override void AddAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new System.Exception( "Object passed to Android AddAsSubview must be a ViewGroup or subclass." );
                }

                view.AddView( TextField );
            }

            public override void RemoveAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new System.Exception( "Object passed to Android RemoveAsSubview must be a ViewGroup or subclass." );
                }

                view.RemoveView( TextField );
            }

            public override void SizeToFit( )
            {
                Measure( );

                // update its width
                TextField.SetMinWidth( TextField.MeasuredWidth );
                TextField.SetMaxWidth( TextField.MeasuredWidth );


                TextField.LayoutParameters.Height = TextField.MeasuredHeight;
            }

            public override void ResignFirstResponder( )
            {
                // only allow this text edit to hide the keyboard if it's the text field with focus.
                Activity activity = ( Activity )Rock.Mobile.PlatformSpecific.Android.Core.Context;
                if( activity.CurrentFocus != null && ( activity.CurrentFocus as EditText ) == TextField )
                {
                    InputMethodManager imm = ( InputMethodManager )Rock.Mobile.PlatformSpecific.Android.Core.Context.GetSystemService( Android.Content.Context.InputMethodService );

                    imm.HideSoftInputFromWindow( TextField.WindowToken, 0 );

                    // yeild focus to the dummy view so the text field clears it's caret and the selected outline
                    TextField.ClearFocus( );
                }
            }

            void Measure( )
            {
                // create the specs we want for measurement
                int widthMeasureSpec = View.MeasureSpec.MakeMeasureSpec( TextField.LayoutParameters.Width, MeasureSpecMode.Unspecified );
                int heightMeasureSpec = View.MeasureSpec.MakeMeasureSpec( 0, MeasureSpecMode.Unspecified );

                // measure the label given the current width/height/text
                TextField.Measure( widthMeasureSpec, heightMeasureSpec );
            }
        }
    }
}
#endif
