#if __WIN__
using System.Drawing;

namespace MobileApp.Shared.Notes
{
    public interface IEditableUIControl
    {
        IEditableUIControl ControlAtPoint( PointF point );

        void UpdatePosition( float deltaX, float deltaY );

        void ToggleHighlight( object masterView );
    }
}
#endif