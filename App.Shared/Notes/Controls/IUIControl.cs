using System;
using System.Collections.Generic;
using Rock.Mobile.UI;
using System.Drawing;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Interface with methods allowing abstracted management of UI Controls.
            /// </summary>
            public interface IUIControl
            {
                void AddOffset( float xOffset, float yOffset );

                RectangleF GetFrame( );
                RectangleF GetMargin( );

                void GetControlOfType<TControlType>( List<IUIControl> controlList ) where TControlType : class;

                void AddToView( object obj );

                void RemoveFromView( object obj );

                bool TouchesBegan( PointF touch );

                void TouchesMoved( PointF touch );

                IUIControl TouchesEnded( PointF touch );

                string GetActiveUrl( );

                Styles.Alignment GetHorzAlignment( );

                bool ShouldShowBulletPoint();

                void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes );

                PlatformBaseUI GetPlatformControl( );
            }
        }
    }
}
