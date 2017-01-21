#if __WIN__
using System.Drawing;
using System.Windows.Input;

namespace MobileApp.Shared.Notes
{
    public interface IEditableUIControl
    {
        IEditableUIControl HandleMouseDown( PointF point );
        IEditableUIControl HandleMouseDoubleClick( PointF point );
        IEditableUIControl HandleMouseHover( PointF mousePos );
        
        void HandleKeyUp( KeyEventArgs e );
        void HandleUnderline( );

        PointF GetPosition( );

        bool IsEditing( );

        void SetPosition( float xPos, float yPos );
    }
}
#endif