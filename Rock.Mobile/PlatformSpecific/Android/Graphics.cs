#if __ANDROID__
using System;

// This file is contains graphics classes that should only be used in Android specific code.
using Android.Graphics;
using Java.IO;
using System.Collections.Generic;
using Android.Widget;
using Android.Content;
using Android.Views;
using Rock.Mobile.UI;
using Android.Animation;
using Android.OS;

namespace Rock.Mobile.PlatformSpecific.Android.Graphics
{
    /// <summary>
    /// Contains static helper functions pertaining to graphics.
    /// </summary>
    class Util
    {
        public static Bitmap ApplyMaskToBitmap( Bitmap image, Bitmap mask, int x, int y )
        {
            // create a bitmap that will be our result
            Bitmap result = Bitmap.CreateBitmap( image.Width, image.Height, Bitmap.Config.Argb8888 );

            // create a canvas and render the image
            //Canvas canvas = new Canvas( result );
            using( Canvas canvas = new Canvas( result ) )
            {
                canvas.DrawBitmap( image, 0, 0, null );

                // Render our mask with a paint that Xor's out the blank area of the mask (showing the underlying pic)
                //Paint paint = new Paint( PaintFlags.AntiAlias );
                using( Paint paint = new Paint( PaintFlags.AntiAlias ) )
                {
                    paint.SetXfermode( new PorterDuffXfermode( PorterDuff.Mode.DstOut ) );
                    canvas.DrawBitmap( mask, x, y, paint );
                }
            }

            return result;
        }
    }


    /// <summary>
    /// Simple font manager that stores fonts as we create them.
    /// That way we aren't creating new fonts for every singe label. It saves memory
    /// and speeds up our load times a lot.
    /// </summary>
    public class FontManager
    {
        static FontManager _Instance = new FontManager( );
        public static FontManager Instance { get { return _Instance; } }

        class FontFace
        {
            public Typeface Font { get; set; }
            public string Name { get; set; }
        }

        List<FontFace> FontList { get; set; }

        FontManager( )
        {
            FontList = new List<FontFace>( );
        }

        public Typeface GetFont( string fontName )
        {
            FontFace fontFace = FontList.Find( f => f.Name == fontName );
            if( fontFace == null )
            {
                fontFace = new FontFace()
                    {
                        Name = fontName,
                        Font = Typeface.CreateFromAsset( Rock.Mobile.PlatformSpecific.Android.Core.Context.Assets, "Fonts/" + fontName + ".ttf" )
                    };

                FontList.Add( fontFace );
            }

            return fontFace.Font;
        }
    }

    /// <summary>
    /// Implements a view that acts as a mask, allowing a square region to be masked
    /// and the outside area to be darkened by Opacity.
    /// </summary>
    class MaskLayer : View
    {
        /// <summary>
        /// Represents the bitmap that contains the fullscreen mask with a cutout for the "masked" portion
        /// </summary>
        /// <value>The masked cutout.</value>
        Bitmap Layer { get; set; }

        System.Drawing.SizeF AlphaMask { get; set; }

        /// <summary>
        /// The opacity of the layered region
        /// </summary>
        /// <value>The opacity.</value>
        int _Opacity;
        public float Opacity
        { 
            get
            {
                return (float)  _Opacity / 255.0f;
            }

            set
            {
                _Opacity = (int)( value * 255.0f );
                Invalidate( );
            }
        }

        PointF _Position;
        public PointF Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
                Invalidate( );
            }
        }

        public MaskLayer( int layerWidth, int layerHeight, int maskWidth, int maskHeight, Context context ) : base( context )
        {
            Position = new PointF( );
            Opacity = 1.00f;

            // first create the full layer
            Layer = Bitmap.CreateBitmap( layerWidth, layerHeight, Bitmap.Config.Alpha8 );
            Layer.EraseColor( Color.Black );

            // now define the mask portion
            AlphaMask = new System.Drawing.SizeF( maskWidth, maskHeight );

            // XOR'ing (which is required) is not supported in Hardware on pre-18 Android versions.
            // So in that case, drop to software for this view and spit out a warning.
            if ( int.Parse( Build.VERSION.Sdk ) < 18 )
            {
                SetLayerType( LayerType.Software, null );
                Rock.Mobile.Util.Debug.WriteLine( "Device is running API < 18. Mask Layer must render in SOFTWARE. (Could be slow)" );
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            // Note: The reason we do it like this, so simply, is that there's a huge performance hit to rendering into our own canvas. I'm not sure why,
            // because i haven't taken the time to R&D it, but I think it has something to do with the canvas they provide being GPU based and mine being on the CPU, or
            // at least causing the GPU texture to have to be flushed and re-DMA'd.

            // Source is what this MaskLayer contains and will draw into canvas.
            // Destination is the buffer IN canvas
            using( Paint paint = new Paint( ) )
            {
                paint.Alpha = _Opacity;

                // set a clipping region that excludes the alpha mask region
                canvas.ClipRect( new Rect( (int)Position.X, (int)Position.Y, (int)Position.X + (int)AlphaMask.Width, (int)Position.Y + (int)AlphaMask.Height ), Region.Op.Xor );

                // and render the full image into the canvas (which will have the effect of masking only the area we care about
                canvas.DrawBitmap( Layer, 0, 0, paint );
            }
        }
    }

    /// <summary>
    /// Implements a simple circle view that can be used in the Android UI Hierarchy
    /// </summary>
    public class CircleView : View
    {
        float _StrokeWidth;
        public float StrokeWidth
        {
            get
            {
                return _StrokeWidth;
            }
            set
            {
                _StrokeWidth = value;
                UpdatePaint( );
            }
        }

        Color _Color;
        public Color Color
        {
            get
            {
                return _Color;
            }
            set
            {
                _Color = value;
                UpdatePaint( );
            }
        }

        global::Android.Graphics.Paint.Style _Style;
        public global::Android.Graphics.Paint.Style Style
        {
            get
            {
                return _Style;
            }
            set
            {
                _Style = value;
                UpdatePaint( );
            }
        }

        Paint Paint { get; set; }

        void UpdatePaint( )
        {
            Paint.SetStyle( Style );
            Paint.Color = Color;
            Paint.StrokeWidth = Rock.Mobile.Graphics.Util.UnitToPx( StrokeWidth );
        }

        public CircleView( global::Android.Content.Context c ) : base( c )
        {
            Paint = new Paint();

            // default the style to stroke only
            Style = global::Android.Graphics.Paint.Style.Stroke;

            UpdatePaint( );
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw( canvas );

            // center the drawing area
            float xPos = canvas.Width / 2;
            float yPos = canvas.Height / 2;

            // and render
            canvas.DrawCircle( xPos, yPos, (Width *.95f) / 2, Paint );
        }
    }

    /// <summary>
    /// An image view that can be resized according to its width and will maintain 
    /// the correct height aspect ratio.
    /// </summary>
    public class AspectScaledImageView : ImageView
    {
        public AspectScaledImageView( Context context ) : base( context )
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if ( Drawable != null )
            {
                int width = MeasureSpec.GetSize( widthMeasureSpec );
                int height = (int)System.Math.Ceiling( width * ( (float)Drawable.IntrinsicHeight / (float)Drawable.IntrinsicWidth ) );

                // respect the requested limits to width / height
                // not sure if I shuld tho, doesn't this defeat hte purpose of a height wrapping thingy?

                //width = System.Math.Max( this.MinimumWidth, System.Math.Min( width, this.MaxWidth ) );

                //height = System.Math.Max( this.MinimumHeight, System.Math.Min( height, this.MaxHeight ) );

                SetMeasuredDimension( width, height );
            }
            else
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }
        }
    }
}
#endif
