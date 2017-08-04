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
            RevealBox,
            BulletParagraph,
            BoldParagraph,
            UnderlineParagraph
        }
    }
    
    /// <summary>
    ///  Values used by the IEditableUIControls
    /// </summary>
    public static class EditableConfig
    {
        public const uint sDebugColor = 0x0000FF77;
        public const string sPlaceholderUrlText = "Optional URL";
    }

    public interface IEditableUIControl
    {
        IEditableUIControl HandleMouseDown( PointF point );
        IEditableUIControl HandleMouseDoubleClick( PointF point );
        IEditableUIControl HandleMouseHover( PointF mousePos );
        IEditableUIControl ContainerForControl( System.Type controlType, PointF mousePos );
        IUIControl HandleCreateControl( System.Type controlType, PointF mousePos );
        
        // true when this is a "direct delete" where a child is being deleted by WinView, and
        // therefore needs to tell its parent so the parent can update layout.
        void HandleDelete( bool notifyParent ); 

        // controls should return true if they want focus released
        bool HandleFocusedControlKeyUp( KeyEventArgs e );

        PointF GetPosition( );
        void ResetBounds( );

        bool IsEditing( );
        List<EditStyling.Style> GetEditStyles( );
        object GetStyleValue( EditStyling.Style style );
        void SetStyleValue( EditStyling.Style style, object value );

        string Export( RectangleF parentPadding, float currYPos );

        // Sigh. This is NOT the EditStyle referred to above. This is the Note Styling object
        // used by the notes platform.
        MobileApp.Shared.Notes.Styles.Style GetControlStyle( );

        // a child will call this on its parent when a style changes.
        // gives the parent a chance to respond and deal with it.
        void HandleChildStyleChanged( EditStyling.Style style, IEditableUIControl childControl );

        // a child will call this on its parent when it's being deleted.
        // gives the parent a chance to respond and deal with it.
        void HandleChildDeleted( IEditableUIControl childControl );

        void SetPosition( float xPos, float yPos );

        void ToggleDebugRect( bool enabled );
    }
}
#endif