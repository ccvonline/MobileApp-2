using System;
using System.Drawing;

namespace Rock.Mobile
{
    namespace Math
    {
        class Vector3
        {
            public float X;
            public float Y;
            public float Z;

            public Vector3( )
            {
                X = 0;
                Y = 0;
                Z = 0;
            }

            public Vector3( float x, float y, float z )
            {
                X = x;
                Y = y;
                Z = z;
            }

            public static Vector3 operator - ( Vector3 lhs, Vector3 rhs )
            {
                return new Vector3( lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z );
            }

            public static Vector3 operator + ( Vector3 lhs, Vector3 rhs )
            {
                return new Vector3( lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z );
            }

            public static Vector3 operator * ( Vector3 lhs, float scalar )
            {
                return new Vector3( lhs.X * scalar, lhs.Y * scalar, lhs.Z * scalar );
            }

            public static Vector3 operator / ( Vector3 lhs, float scalar )
            {
                return new Vector3( lhs.X / scalar, lhs.Y / scalar, lhs.Z / scalar );
            }

            public static float DotProduct( Vector3 v1, Vector3 v2 )
            {
                return ( (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z) );
            }

            public static float MagnitudeSquared( Vector3 v )
            {
                return DotProduct( v, v );
            }

            public static float Magnitude( Vector3 v )
            {
                return (float) System.Math.Sqrt( MagnitudeSquared( v ) );
            }
        }
        
        class Util
        {
            public static float DegToRad = 0.0174532925f;

            public static float DotProduct( PointF v1, PointF v2 )
            {
                return ( (v1.X * v2.X) + (v1.Y * v2.Y) );
            }

            public static float MagnitudeSquared( PointF v )
            {
                return DotProduct( v, v );
            }

            public static float Magnitude( PointF v )
            {
                return (float) System.Math.Sqrt( MagnitudeSquared( v ) );
            }
        }
    }
}

