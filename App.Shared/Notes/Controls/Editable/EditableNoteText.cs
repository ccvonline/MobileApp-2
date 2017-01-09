#if __WIN__
using System;
using System.Xml;
using System.Collections.Generic;
using Rock.Mobile.UI;
using System.Drawing;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Control that lays out basic text. Used by Paragraphs.
            /// </summary>
            public class EditableNoteText : NoteText, IEditableUIControl
            {
                public EditableNoteText( CreateParams parentParams, string text ) : base( parentParams, text )
                {
                   
                }

                // This constructor is called when explicit Note Text is being declared.
                // This means the XML has "<NoteText>Something</NoteText>. Its used when
                // the user wants to alter a particular piece of text within a paragraph.
                public EditableNoteText( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    
                }
                
                public IEditableUIControl ControlAtPoint( PointF point )
                {
                    if( PlatformLabel.Frame.Contains( point ) )
                    {
                        return this;
                    }
                    
                    return null;
                }

                public PointF GetPosition( )
                {
                    return new PointF( PlatformLabel.Frame.Left, PlatformLabel.Frame.Top );
                }

                public void EnableEditMode( bool enabled, System.Windows.Controls.Canvas parentCanvas )
                {
                }

                public void SetPosition( float xPos, float yPos )
                {
                    PlatformLabel.Position = new PointF( xPos, yPos );

                    SetDebugFrame( PlatformLabel.Frame );
                }

                public void ToggleHighlight( object masterView )
                {
                    ToggleDebug( masterView );
                }
            }
        }
    }
}
#endif