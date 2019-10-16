using System;

namespace Rock.Mobile.Animation
{
    ///All these animators derive from SimpleAnimator, which has a platform specific implementation.

    /// <summary>
    /// An animator that will animate a float from start to end along duration, 
    /// and provides optional update and completion callbacks
    /// </summary>
    public class SimpleAnimator_Float : SimpleAnimator
    {
        float StartValue { get; set; }
        float EndValue { get; set; }

        public SimpleAnimator_Float( float start, float end, float duration, AnimationUpdate updateDelegate, AnimationComplete completeDelegate )
        {
            StartValue = start;
            EndValue = end;

            Init( duration, updateDelegate, completeDelegate );
        }

        protected override void AnimTick(float percent, AnimationUpdate updateDelegate)
        {
            float value = StartValue + ((EndValue - StartValue) * percent);

            if ( updateDelegate != null )
            {
                updateDelegate( percent, value );
            }
        }
    }

    public class SimpleAnimator_RectF : SimpleAnimator
    {
        System.Drawing.RectangleF StartValue { get; set; }
        System.Drawing.RectangleF Delta { get; set; }

        public SimpleAnimator_RectF( System.Drawing.RectangleF start, System.Drawing.RectangleF end, float duration, AnimationUpdate updateDelegate, AnimationComplete completeDelegate )
        {
            StartValue = start;
            Delta = new System.Drawing.RectangleF( end.X - start.X, end.Y - start.Y, end.Width - start.Width, end.Height - start.Height );

            Init( duration, updateDelegate, completeDelegate );
        }

        protected override void AnimTick(float percent, AnimationUpdate updateDelegate)
        {
            // get the current value and provide it to the caller
            System.Drawing.RectangleF value = new System.Drawing.RectangleF( StartValue.X + (Delta.X * percent), 
                                                                             StartValue.Y + (Delta.Y * percent), 
                                                                             StartValue.Width + (Delta.Width * percent), 
                                                                             StartValue.Height + (Delta.Height * percent) );
            if ( updateDelegate != null )
            {
                updateDelegate( percent, value );
            }
        }
    }

    public class SimpleAnimator_PointF : SimpleAnimator
    {
        System.Drawing.PointF StartValue { get; set; }
        System.Drawing.PointF Delta { get; set; }

        public SimpleAnimator_PointF( System.Drawing.PointF start, System.Drawing.PointF end, float duration, AnimationUpdate updateDelegate, AnimationComplete completeDelegate )
        {
            StartValue = start;
            Delta = new System.Drawing.PointF( end.X - start.X, end.Y - start.Y );

            Init( duration, updateDelegate, completeDelegate );
        }

        protected override void AnimTick(float percent, AnimationUpdate updateDelegate)
        {
            // get the current value and provide it to the caller
            System.Drawing.PointF value = new System.Drawing.PointF( StartValue.X + (Delta.X * percent), StartValue.Y + (Delta.Y * percent) );
            if ( updateDelegate != null )
            {
                updateDelegate( percent, value );
            }
        }
    }

    public class SimpleAnimator_SizeF : SimpleAnimator
    {
        System.Drawing.SizeF StartValue { get; set; }
        System.Drawing.SizeF Delta { get; set; }

        public SimpleAnimator_SizeF( System.Drawing.SizeF start, System.Drawing.SizeF end, float duration, AnimationUpdate updateDelegate, AnimationComplete completeDelegate )
        {
            StartValue = start;
            Delta = new System.Drawing.SizeF( end.Width - start.Width, end.Height - start.Height );

            Init( duration, updateDelegate, completeDelegate );
        }

        protected override void AnimTick(float percent, AnimationUpdate updateDelegate)
        {
            // get the current value and provide it to the caller
            System.Drawing.SizeF value = new System.Drawing.SizeF( StartValue.Width + (Delta.Width * percent), StartValue.Height + (Delta.Height * percent) );
            if ( updateDelegate != null )
            {
                updateDelegate( percent, value );
            }
        }
    }

    public class SimpleAnimator_Color : SimpleAnimator
    {
        uint StartR { get; set; }
        uint StartG { get; set; }
        uint StartB { get; set; }
        uint StartA { get; set; }

        int DeltaR { get; set; }
        int DeltaG { get; set; }
        int DeltaB { get; set; }
        int DeltaA { get; set; }

        public SimpleAnimator_Color( uint start, uint end, float duration, AnimationUpdate updateDelegate, AnimationComplete completionDelegate )
        {
            Init( duration, updateDelegate, completionDelegate );

            StartR = (start & 0xFF000000) >> 24;
            StartG = (start & 0x00FF0000) >> 16;
            StartB = (start & 0x0000FF00) >> 8;
            StartA = (start & 0xFF);

            uint endR = (end & 0xFF000000) >> 24;
            uint endG = (end & 0x00FF0000) >> 16;
            uint endB = (end & 0x0000FF00) >> 8;;
            uint endA = (end & 0xFF);

            DeltaR = (int) (endR - StartR);
            DeltaG = (int) (endG - StartG);
            DeltaB = (int) (endB - StartB);
            DeltaA = (int) (endA - StartA);
        }

        protected override void AnimTick( float percent, AnimationUpdate updateDelegate )
        {
            // cast to int so we don't lose the sign when adding a negative delta
            uint currR = (uint) ((int)StartR + (int) ( (float)DeltaR * percent ));
            uint currG = (uint) ((int)StartG + (int) ( (float)DeltaG * percent ));
            uint currB = (uint) ((int)StartB + (int) ( (float)DeltaB * percent ));
            uint currA = (uint) ((int)StartA + (int) ( (float)DeltaA * percent ));

            uint currValue = currR << 24 | currG << 16 | currB << 8 | currA;

            if ( updateDelegate != null )
            {
                updateDelegate( percent, currValue );
            }
        }
    }
}
