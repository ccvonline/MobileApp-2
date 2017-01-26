#if __WIN__
using System;
using System.Xml;
using System.Collections.Generic;
using Rock.Mobile.UI;
using System.Drawing;
using System.Windows.Input;

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
                // store the background color so that if we change it for hovering, we can restore it after
                uint OrigBackgroundColor = 0;
                
                // Store the canvas that is actually rendering this control, so we can
                // add / remove edit controls as needed (text boxes, toolbars, etc.)
                System.Windows.Controls.Canvas ParentEditingCanvas;
                bool EditMode_Enabled = false;
                
                public EditableNoteText( CreateParams parentParams, string text ) : base( parentParams, text )
                {
                    ParentEditingCanvas = null;

                    OrigBackgroundColor = PlatformLabel.BackgroundColor;
                }

                // This constructor is called when explicit Note Text is being declared.
                // This means the XML has "<NoteText>Something</NoteText>. Its used when
                // the user wants to alter a particular piece of text within a paragraph.
                public EditableNoteText( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;

                    OrigBackgroundColor = PlatformLabel.BackgroundColor; 
                }

                public override void AddToView( object obj )
                {
                    // store our parent canvas so we can toggle editing as needed
                    ParentEditingCanvas = obj as System.Windows.Controls.Canvas;

                    base.AddToView( obj );
                }

                public IEditableUIControl HandleMouseDoubleClick( PointF point )
                {
                    if( PlatformLabel.Frame.Contains( point ) )
                    {
                        EditMode_Enabled = true;
                        PlatformLabel.BackgroundColor = 0xFF222277;

                        return this;
                    }
                    
                    return null;
                }

                public void HandleKeyUp( KeyEventArgs e )
                {
                    EditMode_Enabled = false;
                    PlatformLabel.BackgroundColor = OrigBackgroundColor;
                }

                public bool IsEditing( )
                {
                    return EditMode_Enabled;
                }

                public List<EditStyling.Style> GetEditStyles( )
                {
                    // shift & or together the styles we support
                    List<EditStyling.Style> styles = new List<EditStyling.Style>( );
                    styles.Add( EditStyling.Style.Font );
                    styles.Add( EditStyling.Style.RevealBox );
                    styles.Add( EditStyling.Style.Underline );
                    
                    return styles;
                }

                public object GetStyleValue( EditStyling.Style style )
                {
                    switch( style )
                    {
                        case EditStyling.Style.Font:
                        {
                            string fontName = PlatformLabel.Editable_GetFontName( );
                            float fontSize = PlatformLabel.Editable_GetFontSize( );

                            return new EditStyling.FontStyle( fontName, fontSize );
                        }

                        case EditStyling.Style.Underline:
                        {
                            return PlatformLabel.Editable_HasUnderline( );
                        }
                    }

                    return null;
                }

                public void SetStyleValue( EditStyling.Style style, object value )
                {
                    switch( style )
                    {
                        case EditStyling.Style.Font:
                        {
                            EditStyling.FontStyle fontValue = (EditStyling.FontStyle)value;
                            PlatformLabel.SetFont( fontValue.mName, fontValue.mSize );

                            break;
                        }

                        case EditStyling.Style.Underline:
                        {
                            bool enableUnderline = (bool) value;

                            if( enableUnderline )
                            {
                                PlatformLabel.Editable_AddUnderline( );
                            }
                            else
                            {
                                PlatformLabel.Editable_RemoveUnderline( );
                            }

                            break;
                        }
                    }
                }

                public IEditableUIControl HandleMouseDown( PointF point )
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
                
                public void SetPosition( float xPos, float yPos )
                {
                    PlatformLabel.Position = new PointF( xPos, yPos );

                    SetDebugFrame( PlatformLabel.Frame );
                }

                public IEditableUIControl HandleMouseHover( PointF mousePos )
                {
                    bool mouseHovering = GetFrame( ).Contains( mousePos );
                    if ( mouseHovering == true )
                    {
                        PlatformLabel.BackgroundColor = 0xFFFFFF77;
                        return this;
                    }
                    else
                    {
                        PlatformLabel.BackgroundColor = OrigBackgroundColor;
                        return null;
                    }
                }
            }
        }
    }
}
#endif