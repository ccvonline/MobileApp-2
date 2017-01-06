#if __WIN__
using System.Drawing;

namespace MobileApp.Shared.Notes
{
    public interface IEditableUIControl
    {
        IEditableUIControl ControlAtPoint( PointF point );

        PointF GetPosition( );

        void SetPosition( float xPos, float yPos );

        void ToggleHighlight( object masterView );
    }
}
#endif