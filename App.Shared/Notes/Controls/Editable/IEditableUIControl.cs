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
            Font,
            RevealBox
        }

        //public static ulong CreateSupportedStyles( List<Style> supportedStyles )
        //{
        //    ulong supportedStyleBits = 0;

        //    foreach( Style style in supportedStyles )
        //    {
        //        supportedStyleBits |= (uint) (1 << (int)style);
        //    }

        //    return supportedStyleBits;
        //}

        //public static bool StyleSupported( ulong supportedStyleBits, Style style )
        //{
        //    // create a mask with the style bit set
        //    uint styleMask = (uint) (1 << (int)style);

        //    // mask it off and see if it's non-0
        //    if( (supportedStyleBits & styleMask) != 0 )
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }

    public interface IEditableUIControl
    {
        IEditableUIControl HandleMouseDown( PointF point );
        IEditableUIControl HandleMouseDoubleClick( PointF point );
        IEditableUIControl HandleMouseHover( PointF mousePos );
        
        void HandleKeyUp( KeyEventArgs e );
        void HandleUnderline( );
        void HandleFont( string fontName );

        PointF GetPosition( );

        bool IsEditing( );
        List<EditStyling.Style> GetEditStyles( );

        void SetPosition( float xPos, float yPos );
    }
}
#endif