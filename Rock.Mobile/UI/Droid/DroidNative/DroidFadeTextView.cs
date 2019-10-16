#if __ANDROID__
using System;
using Android.Widget;
using Android.Graphics;
using Android.Animation;
using Java.IO;
using Rock.Mobile.Animation;

namespace Rock.Mobile
{
    namespace UI
    {
        namespace DroidNative
        {
            /// <summary>
            /// A subclassed TextView that allows fading in the text
            /// </summary>
            public class FadeTextView : BorderedRectTextView
            {
                /// <summary>
                /// An alpha-only version of the RGBAMask. This is what we'll actually use to mask.
                /// </summary>
                /// <value>The alpha mask.</value>
                static Bitmap AlphaMask { get; set; }

                /// <summary>
                /// The text to display rendered to an offscreen bmp
                /// </summary>
                /// <value>The text bmp.</value>
                Bitmap TextBmp { get; set; }

                /// <summary>
                /// The paint that specifies how to render the mask with the text.
                /// </summary>
                /// <value>The text paint.</value>
                Paint TextPaint { get; set; }

                /// <summary>
                /// The transform needed to ensure the mask remains centered on the text.
                /// </summary>
                /// <value>The text transform.</value>
                Matrix TextTransform { get; set; }

                /// <summary>
                /// The offscreen bitmap that the text and mask are both rendered into.
                /// This is what is then rendered to the android back buffer.
                /// </summary>
                /// <value>The result bmp.</value>
                Bitmap ResultBmp { get; set; }

                /// <summary>
                /// The canvas used in conjunction with the ResultBmp
                /// </summary>
                /// <value>The result canvas.</value>
                Android.Graphics.Canvas ResultCanvas { get; set; }

                /// <summary>
                /// The current width of the text. If this changes the text buffer is redrawn.
                /// </summary>
                /// <value>The width of the curr.</value>
                int CurrWidth { get; set; }
                 
                /// <summary>
                /// The current height of the text. If this changes the text buffer is redrawn.
                /// </summary>
                /// <value>The height of the curr.</value>
                int CurrHeight { get; set; }

                /// <summary>
                /// The current value to scale the alpha mask to. (Larger number means bigger mask, and
                /// more visible text underneath.
                /// </summary>
                float _MaskScale;
                public float MaskScale 
                { 
                    get 
                    {
                        return _MaskScale;
                    }

                    set 
                    {
                        _MaskScale = System.Math.Max(value, .01f);
                    }
                }

                /// <summary>
                /// True when an animation is happening. Ensures we don't start an animation
                /// while one is in progress.
                /// </summary>
                /// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
                bool Animating { get; set; }

                public FadeTextView( Android.Content.Context context ) : base( context )
                {
                    MaskScale = 1.0f;

                    TextPaint = new Paint();
                    TextTransform = new Matrix();
                }

                public void AnimateMaskScale( float targetScale, float duration )
                {
                    if( Animating == false )
                    {
                        float clampedValue = System.Math.Max(targetScale, .01f);

                        SimpleAnimator_Float floatAnimator = new SimpleAnimator_Float( _MaskScale, clampedValue, duration, delegate(float percent, object value) 
                            {
                                _MaskScale = (float)value;
                                Invalidate( );
                            }, 
                            delegate 
                            {
                                Animating = false;
                            });
                        floatAnimator.Start( );

                        Animating = true;
                    }
                }

                public static void CreateAlphaMask( Android.Content.Context context, string fileName )
                {
                    // load the stream from assets
                    System.IO.Stream assetStream = context.Assets.Open( fileName );

                    // grab the RGBA mask
                    Bitmap rgbaMask = BitmapFactory.DecodeStream( assetStream );

                    // convert it to an alpha mask
                    AlphaMask = Bitmap.CreateBitmap( rgbaMask.Width, rgbaMask.Height, Bitmap.Config.Alpha8 );

                    // put the 8bit mask into a canvas
                    Android.Graphics.Canvas maskCanvas = new Android.Graphics.Canvas( AlphaMask );

                    // render the rgb mask into the canvas, which writes the result into the AlphaMask bitmap
                    maskCanvas.DrawBitmap( rgbaMask, 0.0f, 0.0f, null );
                }

                protected void CreateTextBitmaps( int width, int height )
                {
                    // create a 32bit text bmp that will store the rendered text.
                    TextBmp = Bitmap.CreateBitmap( width, height, Bitmap.Config.Argb8888 );

                    // create a canvas and place the TextBmp as its target
                    Android.Graphics.Canvas canvas = new Android.Graphics.Canvas( TextBmp );

                    // render our text (which will put it into the TextBmp buffer)
                    base.OnDraw( canvas );

                    // set the TextPaint's shader to render with the Text we just rendered
                    TextPaint.SetShader( new BitmapShader( TextBmp, Shader.TileMode.Clamp, Shader.TileMode.Clamp ) );
                }

                protected void CreateResultBitmaps( int width, int height )
                {
                    // create the 32bit result bmp that we will render the text and alpha mask to
                    ResultBmp = Bitmap.CreateBitmap( width, height, Bitmap.Config.Argb8888 );

                    // store it in the canvas we'll use for rendering
                    ResultCanvas = new Android.Graphics.Canvas( ResultBmp );
                }

                protected override void OnDraw(Android.Graphics.Canvas canvas)
                {
                    // if our render buffers are not valid, generate them
                    if( CurrWidth != this.LayoutParameters.Width || CurrHeight != this.LayoutParameters.Height )
                    {
                        CreateTextBitmaps( this.LayoutParameters.Width, this.LayoutParameters.Height );
                        CreateResultBitmaps( this.LayoutParameters.Width, this.LayoutParameters.Height );

                        CurrWidth = this.LayoutParameters.Width;
                        CurrHeight = this.LayoutParameters.Height;
                    }


                    // keep the spot light centered on the image
                    float xPos = (LayoutParameters.Width / 2) - ((AlphaMask.Width / 2) * MaskScale);
                    float yPos = (LayoutParameters.Height / 2) - ((AlphaMask.Height / 2) * MaskScale);

                    // update the text's transform for the mask scale.
                    // The text's transform should be the inverse of the Canvas' values
                    TextTransform.SetScale( 1.0f / MaskScale, 1.0f / MaskScale );
                    TextTransform.PreTranslate( -xPos, -yPos );

                    TextPaint.Shader.SetLocalMatrix( TextTransform );

                    // clear the bitmap before re-rendering. (NOT EFFICIENT _AT_ _ALL_)
                    ResultBmp.SetPixels( new int[ResultBmp.RowBytes * ResultBmp.Height], 0, ResultBmp.RowBytes, 0, 0, ResultBmp.Width, ResultBmp.Height );

                    // save / restore the canvas settings so we don't accumulate the scale
                    ResultCanvas.Save();
                    ResultCanvas.Translate( xPos, yPos );
                    ResultCanvas.Scale( MaskScale, MaskScale );

                    // render the alpha mask'd text to our result bmp
                    ResultCanvas.DrawBitmap( AlphaMask, 0, 0, TextPaint );

                    // restore the canvas values for next time.
                    ResultCanvas.Restore();

                    // and to the actual android buffer, render our result
                    canvas.DrawBitmap( ResultBmp, 0, 0, null);
                }
            }
        }
    }
}
#endif
