#if __WIN__
using System.Drawing;

namespace MobileApp.Shared.Notes
{
    public interface IEditableUIControl
    {
        IEditableUIControl ControlAtPoint( PointF point );

        void EnableEditMode( bool enabled, System.Windows.Controls.Canvas parentCanvas );

        PointF GetPosition( );

        void SetPosition( float xPos, float yPos );

        void ToggleHighlight( object masterView );
    }
}
#endif