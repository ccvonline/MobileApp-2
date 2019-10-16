#if __ANDROID__
using System;
using Android.Graphics;
using Android.Graphics.Drawables.Shapes;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;
using Android.Views;

namespace Rock.Mobile
{
    namespace UI
    {
        namespace DroidNative
        {
            /// <summary>
            /// Special drawable that manages both an outer and inner paint.
            /// Designed for use only with BorderedRectTextView, BorderedRectTextField and BorderedRectView
            /// Note: A transparent center with a colored border doesn't work, because I can't get the stroke'd border to render
            /// like i need it to, and i need to move on to other stuff. Just set the color to the BG color and be done.
            /// </summary>
            internal class BorderedRectPaintDrawable : PaintDrawable
            {
                /// <summary>
                /// The paint to use for rendering the border
                /// </summary>
                /// <value>The border paint.</value>
                public Paint BorderPaint { get; set; }

                /// <summary>
                /// The width of the border in DP units.
                /// </summary>
                public float _BorderWidthDP;

                /// <summary>
                /// The actual width of the border in converted pixel unites
                /// </summary>
                public float _BorderWidth;

                /// <summary>
                /// Property for managing the border width, including conversion from DP to pixel
                /// </summary>
                /// <value>The width of the border.</value>
                public float BorderWidth 
                { 
                    get 
                    {
                        return _BorderWidthDP;
                    }
                    set
                    {
                        _BorderWidthDP = value;
                        _BorderWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, value, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                        // create a shape that will represent the border
                        UpdateShape( );
                    }
                }

                /// <summary>
                /// The radius in DP units.
                /// </summary>
                protected float _RadiusDP;

                /// <summary>
                /// The actual radius in converted pixel unites
                /// </summary>
                protected float _Radius;

                /// <summary>
                /// Property for managing the radius, including converstion from DP to pixel
                /// </summary>
                /// <value>The radius.</value>
                public float Radius 
                { 
                    get 
                    { 
                        return _RadiusDP; 
                    } 

                    set
                    {
                        // first store the radius in DP value in case we must later return it
                        _RadiusDP = value;

                        // convert to pixels
                        _Radius = TypedValue.ApplyDimension(ComplexUnitType.Dip, value, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                        // create a shape with the new radius
                        UpdateShape( );
                    }
                }

                void UpdateShape( )
                {
                    // if there's a border width or corner radius, create a shape to render that represents the border.
                    // This will allow either a bordered button, or a rounded button.
                    if ( _BorderWidth > 0 || _Radius > 0 )
                    {
                        Shape = new RoundRectShape( new float[]
                            { 
                                _Radius, 
                                _Radius, 
                                _Radius, 
                                _Radius, 
                                _Radius, 
                                _Radius, 
                                _Radius, 
                                _Radius 
                            }, null, null );
                    }
                    else
                    {
                        // otherwise, delete our shape so we don't render anything for a border.
                        Shape = null;
                    }
                }

                public BorderedRectPaintDrawable( ) : base( )
                {
                    // default the paint to clear and no width.
                    // Basically an invisible border.
                    BorderPaint = new Paint( );
                    BorderPaint.Color = Color.Transparent;
                    BorderPaint.SetStyle( Paint.Style.Fill );
                    BorderPaint.StrokeWidth = 0;
                }

                protected override void OnDraw( Shape shape, Canvas canvas, Paint paint )
                {
                    // Render the 'border' if there's a valid width. Otherwise skip it,
                    // it's a waste to call.
                    if ( _BorderWidth > 0 )
                    {
                        base.OnDraw( shape, canvas, BorderPaint );

                        // Render the 'fill'
                        float xOffset = _BorderWidth;
                        float yOffset = _BorderWidth;

                        canvas.Translate( xOffset, yOffset );

                        // shrink it down
                        shape.Resize( shape.Width - ( _BorderWidth * 2 ), shape.Height - ( _BorderWidth * 2 ) );

                        // render
                        base.OnDraw( shape, canvas, paint );

                        // restore the original size
                        shape.Resize( shape.Width + ( _BorderWidth * 2 ), shape.Height + ( _BorderWidth * 2 ) );
                    }
                    else
                    {
                        // without a border, simply render the view as normal
                        base.OnDraw( shape, canvas, paint );
                    }
                }
            }

            public class BorderedRectTextView : TextView
            {
                BorderedRectPaintDrawable BorderedPaintDrawable { get; set; }

                public float BorderWidth 
                { 
                    get { return BorderedPaintDrawable.BorderWidth; } 
                    set { BorderedPaintDrawable.BorderWidth = value; }
                }

                public float Radius
                {
                    get { return BorderedPaintDrawable.Radius; }
                    set { BorderedPaintDrawable.Radius = value; }
                }

                public BorderedRectTextView( Android.Content.Context context ) : base( context )
                {
                    // create our special bordered rect
                    BorderedPaintDrawable = new BorderedRectPaintDrawable( );
                    BorderedPaintDrawable.Paint.SetStyle( Android.Graphics.Paint.Style.Fill );
                    BorderedPaintDrawable.Paint.Color = Color.Transparent;

                    Background = BorderedPaintDrawable;
                }

                public override void SetBackgroundColor( Android.Graphics.Color color )
                {
                    // put the color in the regular 'paint', which is really our fill color,
                    // but to the end user is the background color.
                    BorderedPaintDrawable.Paint.Color = color;
                }

                public void SetBorderColor( Android.Graphics.Color color )
                {
                    // set the color of the border paint, which is the paint used
                    // for our border outline
                    BorderedPaintDrawable.BorderPaint.Color = color;
                }

                protected override void OnDraw(Android.Graphics.Canvas canvas)
                {
                    BorderedRectPaintDrawable paintDrawable = Background as BorderedRectPaintDrawable;
                    if( paintDrawable != null )
                    {
                        float borderWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, paintDrawable.BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                        // perform just a vertical translation to ensure the text is centered
                        canvas.Translate( 0, borderWidth );

                        // create a scalar to uniformly scale down the text so it fits within the border
                        float scalar = ( canvas.Width - (borderWidth * 2) ) / canvas.Width;
                        canvas.Scale( scalar, scalar ); 
                    }

                    base.OnDraw(canvas);
                }

                // hide the base methods for measurement so we can apply border dimensions
                public new int MeasuredWidth { get; set; }
                public new int MeasuredHeight { get; set; }

                public new void Measure( int widthMeasureSpec, int heightMeasureSpec )
                {
                    base.Measure( widthMeasureSpec, heightMeasureSpec );

                    // if there's no text, we don't want to provide any border. There's no text TO border.
                    float borderSize = 0;
                    if ( string.IsNullOrWhiteSpace( Text ) != true )
                    {
                        // now adjust for the border
                        borderSize = TypedValue.ApplyDimension( ComplexUnitType.Dip, BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics );
                    }
                        
                    MeasuredWidth = base.MeasuredWidth + (int)( borderSize * 2 );
                    MeasuredHeight = base.MeasuredHeight + (int)( borderSize * 2 );
                }
            }

            public class BorderedRectEditText : EditText
            {
                BorderedRectPaintDrawable BorderedPaintDrawable { get; set; }

                public float BorderWidth 
                { 
                    get { return BorderedPaintDrawable.BorderWidth; } 
                    set { BorderedPaintDrawable.BorderWidth = value; }
                }

                public float Radius
                {
                    get { return BorderedPaintDrawable.Radius; }
                    set { BorderedPaintDrawable.Radius = value; }
                }

                public BorderedRectEditText( Android.Content.Context context ) : base( context )
                {
                    // create our special bordered rect
                    BorderedPaintDrawable = new BorderedRectPaintDrawable( );
                    BorderedPaintDrawable.Paint.SetStyle( Android.Graphics.Paint.Style.Fill );
                    BorderedPaintDrawable.Paint.Color = Color.Transparent;

                    Background = BorderedPaintDrawable;
                }

                public override void SetBackgroundColor( Android.Graphics.Color color )
                {
                    // put the color in the regular 'paint', which is really our fill color,
                    // but to the end user is the background color.
                    BorderedPaintDrawable.Paint.Color = color;
                }

                public void SetBorderColor( Android.Graphics.Color color )
                {
                    // set the color of the border paint, which is the paint used
                    // for our border outline
                    BorderedPaintDrawable.BorderPaint.Color = color;
                }

                protected override void OnDraw(Android.Graphics.Canvas canvas)
                {
                    BorderedRectPaintDrawable paintDrawable = Background as BorderedRectPaintDrawable;
                    if( paintDrawable != null )
                    {
                        float borderWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, paintDrawable.BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                        // perform just a vertical translation to ensure the text is centered
                        canvas.Translate( 0, borderWidth );

                        // create a scalar to uniformly scale down the text so it fits within the border
                        float scalar = ( canvas.Width - (borderWidth * 2) ) / canvas.Width;
                        canvas.Scale( scalar, scalar ); 
                    }

                    base.OnDraw(canvas);
                }

                // hide the base methods for measurement so we can apply border dimensions
                public new int MeasuredWidth { get; set; }
                public new int MeasuredHeight { get; set; }

                public new void Measure( int widthMeasureSpec, int heightMeasureSpec )
                {
                    base.Measure( widthMeasureSpec, heightMeasureSpec );

                    // now adjust for the border
                    float borderSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                    MeasuredWidth = base.MeasuredWidth + (int)(borderSize * 2);
                    MeasuredHeight = base.MeasuredHeight + (int)(borderSize * 2);
                }
            }

            public class BorderedRectButton : Button
            {
                BorderedRectPaintDrawable BorderedPaintDrawable { get; set; }

                public float BorderWidth 
                { 
                    get { return BorderedPaintDrawable.BorderWidth; } 
                    set { BorderedPaintDrawable.BorderWidth = value; }
                }

                public float Radius
                {
                    get { return BorderedPaintDrawable.Radius; }
                    set { BorderedPaintDrawable.Radius = value; }
                }

                public BorderedRectButton( Android.Content.Context context ) : base( context )
                {
                    // create our special bordered rect
                    BorderedPaintDrawable = new BorderedRectPaintDrawable( );
                    BorderedPaintDrawable.Paint.SetStyle( Android.Graphics.Paint.Style.Fill );
                    BorderedPaintDrawable.Paint.Color = Color.Transparent;

                    Background = BorderedPaintDrawable;
                }

                public override void SetBackgroundColor( Android.Graphics.Color color )
                {
                    // put the color in the regular 'paint', which is really our fill color,
                    // but to the end user is the background color.
                    BorderedPaintDrawable.Paint.Color = color;
                }

                public void SetBorderColor( Android.Graphics.Color color )
                {
                    // set the color of the border paint, which is the paint used
                    // for our border outline
                    BorderedPaintDrawable.BorderPaint.Color = color;
                }

                protected override void OnDraw(Android.Graphics.Canvas canvas)
                {
                    BorderedRectPaintDrawable paintDrawable = Background as BorderedRectPaintDrawable;
                    if( paintDrawable != null )
                    {
                        float borderWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, paintDrawable.BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                        // perform just a vertical translation to ensure the text is centered
                        canvas.Translate( 0, borderWidth );

                        // create a scalar to uniformly scale down the text so it fits within the border
                        float scalar = ( canvas.Width - ( borderWidth * 2 ) ) / canvas.Width;
                        canvas.Scale( scalar, scalar ); 
                    }

                    base.OnDraw(canvas);
                }

                // hide the base methods for measurement so we can apply border dimensions
                public new int MeasuredWidth { get; set; }
                public new int MeasuredHeight { get; set; }

                public new void Measure( int widthMeasureSpec, int heightMeasureSpec )
                {
                    base.Measure( widthMeasureSpec, heightMeasureSpec );

                    // now adjust for the border
                    float borderSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                    MeasuredWidth = base.MeasuredWidth + (int)(borderSize * 2);
                    MeasuredHeight = base.MeasuredHeight + (int)(borderSize * 2);
                }
            }

            public class BorderedRectSwitch : Switch
            {
            	BorderedRectPaintDrawable BorderedPaintDrawable { get; set; }

            	public float BorderWidth
            	{
            		get { return BorderedPaintDrawable.BorderWidth; }
            		set { BorderedPaintDrawable.BorderWidth = value; }
            	}

            	public float Radius
            	{
            		get { return BorderedPaintDrawable.Radius; }
            		set { BorderedPaintDrawable.Radius = value; }
            	}

            	public BorderedRectSwitch( Android.Content.Context context ) : base( context )
            	{
            		// create our special bordered rect
            		BorderedPaintDrawable = new BorderedRectPaintDrawable( );
            		BorderedPaintDrawable.Paint.SetStyle( Android.Graphics.Paint.Style.Fill );
            		BorderedPaintDrawable.Paint.Color = Color.Transparent;

            		Background = BorderedPaintDrawable;
            	}

            	public override void SetBackgroundColor( Android.Graphics.Color color )
            	{
            		// put the color in the regular 'paint', which is really our fill color,
            		// but to the end user is the background color.
            		BorderedPaintDrawable.Paint.Color = color;
            	}

            	public void SetBorderColor( Android.Graphics.Color color )
            	{
            		// set the color of the border paint, which is the paint used
            		// for our border outline
            		BorderedPaintDrawable.BorderPaint.Color = color;
            	}

            	protected override void OnDraw( Android.Graphics.Canvas canvas )
            	{
            		BorderedRectPaintDrawable paintDrawable = Background as BorderedRectPaintDrawable;
            		if( paintDrawable != null )
            		{
            			float borderWidth = TypedValue.ApplyDimension( ComplexUnitType.Dip, paintDrawable.BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics );

            			// perform just a vertical translation to ensure the text is centered
            			canvas.Translate( 0, borderWidth );

            			// create a scalar to uniformly scale down the text so it fits within the border
            			float scalar = ( canvas.Width - ( borderWidth * 2 ) ) / canvas.Width;
            			canvas.Scale( scalar, scalar );
            		}

            		base.OnDraw( canvas );
            	}

            	// hide the base methods for measurement so we can apply border dimensions
            	public new int MeasuredWidth { get; set; }
            	public new int MeasuredHeight { get; set; }

            	public new void Measure( int widthMeasureSpec, int heightMeasureSpec )
            	{
            		base.Measure( widthMeasureSpec, heightMeasureSpec );

            		// now adjust for the border
            		float borderSize = TypedValue.ApplyDimension( ComplexUnitType.Dip, BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics );

            		MeasuredWidth = base.MeasuredWidth + (int) (borderSize * 2);
                    MeasuredHeight = base.MeasuredHeight + (int) (borderSize * 2);
                }
            }

            public class BorderedRectImageView : ImageView
            {
                BorderedRectPaintDrawable BorderedPaintDrawable { get; set; }

                public float BorderWidth 
                { 
                    get { return BorderedPaintDrawable.BorderWidth; } 
                    set { BorderedPaintDrawable.BorderWidth = value; }
                }

                public float Radius
                {
                    get { return BorderedPaintDrawable.Radius; }
                    set { BorderedPaintDrawable.Radius = value; }
                }

                public BorderedRectImageView( Android.Content.Context context ) : base( context )
                {
                    // create our special bordered rect
                    BorderedPaintDrawable = new BorderedRectPaintDrawable( );
                    BorderedPaintDrawable.Paint.SetStyle( Android.Graphics.Paint.Style.Fill );
                    BorderedPaintDrawable.Paint.Color = Color.Transparent;

                    Background = BorderedPaintDrawable;
                }

                public override void SetBackgroundColor( Android.Graphics.Color color )
                {
                    // put the color in the regular 'paint', which is really our fill color,
                    // but to the end user is the background color.
                    BorderedPaintDrawable.Paint.Color = color;
                }

                public void SetBorderColor( Android.Graphics.Color color )
                {
                    // set the color of the border paint, which is the paint used
                    // for our border outline
                    BorderedPaintDrawable.BorderPaint.Color = color;
                }

                protected override void OnDraw(Android.Graphics.Canvas canvas)
                {
                    BorderedRectPaintDrawable paintDrawable = Background as BorderedRectPaintDrawable;
                    if( paintDrawable != null )
                    {
                        float borderWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, paintDrawable.BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                        // perform just a vertical translation to ensure the text is centered
                        canvas.Translate( 0, borderWidth );

                        // create a scalar to uniformly scale down the text so it fits within the border
                        float scalar = ( canvas.Width - (borderWidth * 2) ) / canvas.Width;
                        canvas.Scale( scalar, scalar ); 
                    }

                    base.OnDraw(canvas);
                }

                // hide the base methods for measurement so we can apply border dimensions
                public new int MeasuredWidth { get; set; }
                public new int MeasuredHeight { get; set; }

                public new void Measure( int widthMeasureSpec, int heightMeasureSpec )
                {
                    base.Measure( widthMeasureSpec, heightMeasureSpec );

                    // now adjust for the border
                    float borderSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);

                    MeasuredWidth = base.MeasuredWidth + (int)(borderSize * 2);
                    MeasuredHeight = base.MeasuredHeight + (int)(borderSize * 2);
                }
            }

            public class BorderedRectView : RelativeLayout
            {
                BorderedRectPaintDrawable BorderedPaintDrawable { get; set; }

                public float BorderWidth 
                { 
                    get { return BorderedPaintDrawable.BorderWidth; } 
                    set { BorderedPaintDrawable.BorderWidth = value; }
                }

                public float Radius
                {
                    get { return BorderedPaintDrawable.Radius; }
                    set { BorderedPaintDrawable.Radius = value; }
                }

                public BorderedRectView( Android.Content.Context context ) : base( context )
                {
                    // create our special bordered rect
                    BorderedPaintDrawable = new BorderedRectPaintDrawable( );
                    BorderedPaintDrawable.Paint.SetStyle( Android.Graphics.Paint.Style.Fill );
                    BorderedPaintDrawable.Paint.Color = Color.Transparent;

                    Background = BorderedPaintDrawable;
                }

                public override void SetBackgroundColor( Android.Graphics.Color color )
                {
                    // put the color in the regular 'paint', which is really our fill color,
                    // but to the end user is the background color.
                    BorderedPaintDrawable.Paint.Color = color;
                }

                public void SetBorderColor( Android.Graphics.Color color )
                {
                    // set the color of the border paint, which is the paint used
                    // for our border outline
                    BorderedPaintDrawable.BorderPaint.Color = color;
                }

                protected override void OnDraw(Android.Graphics.Canvas canvas)
                {
                    BorderedRectPaintDrawable paintDrawable = Background as BorderedRectPaintDrawable;
                    if( paintDrawable != null )
                    {
                        float borderWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, paintDrawable.BorderWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);
                        canvas.Translate( borderWidth, borderWidth );
                    }

                    base.OnDraw(canvas);
                }
            }
        }
    }
}
#endif
