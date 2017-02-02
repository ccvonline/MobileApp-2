#if __WIN__
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;

namespace MobileApp.Shared.Notes
{
    // This class helps manage the editable styles each control supports.
    // Editable styles use bitwise operators to set / read what is supported
    public static class EditStyling
    {
        public enum Style
        {
            Underline,
            FontSize,
            FontName,
            RevealBox
        }
    }

    public interface IEditableUIControl
    {
        IEditableUIControl HandleMouseDown( PointF point );
        IEditableUIControl HandleMouseDoubleClick( PointF point );
        IEditableUIControl HandleMouseHover( PointF mousePos );
        
        void HandleKeyUp( KeyEventArgs e );

        PointF GetPosition( );

        bool IsEditing( );
        List<EditStyling.Style> GetEditStyles( );
        object GetStyleValue( EditStyling.Style style );
        void SetStyleValue( EditStyling.Style style, object value );

        // Sigh. This is NOT the EditStyle referred to above. This is the Note Styling object
        // used by the notes platform.
        MobileApp.Shared.Notes.Styles.Style GetControlStyle( );

        // a child will call this on its parent when a style changes.
        // gives the parent a chance to respond and deal with it.
        void HandleChildStyleChanged( EditStyling.Style style, IEditableUIControl childControl );

        void SetPosition( float xPos, float yPos );
    }
}
#endif