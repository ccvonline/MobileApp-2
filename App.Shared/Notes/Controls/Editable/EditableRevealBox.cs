#if __WIN__
using System;
using System.Xml;
using Rock.Mobile.UI;
using MobileApp.Shared.Notes.Model;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using RestSharp.Extensions.MonoHttp;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// A text label that is hidden until a user taps on it.
            /// </summary>
            public class EditableRevealBox : RevealBox, IEditableUIControl
            {
                // store the background color so that if we change it for hovering, we can restore it after
                uint OrigBackgroundColor = 0;
                
                // Store the canvas that is actually rendering this control, so we can
                // add / remove edit controls as needed (text boxes, toolbars, etc.)
                System.Windows.Controls.Canvas ParentEditingCanvas;
                bool EditMode_Enabled = false;

                // store our literal parent control so we can notify if we were updated
                IEditableUIControl ParentControl { get; set; }

                public EditableRevealBox( CreateParams parentParams, string text ) : base( parentParams, text )
                {
                    ParentControl = parentParams.Parent as IEditableUIControl;

                    ParentEditingCanvas = null;

                    OrigBackgroundColor = PlatformLabel.BackgroundColor;
                }

                // This constructor is called when explicit Note Text is being declared.
                // This means the XML has "<NoteText>Something</NoteText>. Its used when
                // the user wants to alter a particular piece of text within a paragraph.
                public EditableRevealBox( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentControl = parentParams.Parent as IEditableUIControl;

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
                    return null;
                }

                public bool HandleFocusedControlKeyUp( KeyEventArgs e )
                {
                    switch( e.Key )
                    {
                        case Key.Return:
                        case Key.Escape:
                        {
                            EditMode_Enabled = false;
                            PlatformLabel.BackgroundColor = OrigBackgroundColor;
                            return true;
                        }
                    }

                    return false;
                }

                public void HandleChildStyleChanged( EditStyling.Style style, IEditableUIControl childControl )
                {
                    // nothing this will need to do, since this control can't have children
                }

                public bool IsEditing( )
                {
                    return EditMode_Enabled;
                }

                public List<EditStyling.Style> GetEditStyles( )
                {
                    // shift & or together the styles we support
                    List<EditStyling.Style> styles = new List<EditStyling.Style>( );
                    styles.Add( EditStyling.Style.FontName );
                    styles.Add( EditStyling.Style.FontSize );
                    styles.Add( EditStyling.Style.RevealBox );
                    styles.Add( EditStyling.Style.Underline );
                    styles.Add( EditStyling.Style.BoldParagraph );
                    styles.Add( EditStyling.Style.ItalicizeParagraph );
                    styles.Add( EditStyling.Style.BoldItalicizeParagraph );
                    styles.Add( EditStyling.Style.UnderlineParagraph );
                    styles.Add( EditStyling.Style.BulletParagraph );

                    return styles;
                }

                public object GetStyleValue( EditStyling.Style style )
                {
                    switch( style )
                    {
                        case EditStyling.Style.FontName:
                        {
                            return PlatformLabel.Editable_GetFontName( );
                        }

                        case EditStyling.Style.FontSize:
                        {
                            return PlatformLabel.Editable_GetFontSize( );
                        }

                        //case EditStyling.Style.Underline:
                        //{
                        //    return PlatformLabel.Editable_HasUnderline( );
                        //}

                        case EditStyling.Style.RevealBox:
                        {
                            // here, we're basically saying "YES, we ARE a reveal box" so the system
                            // knows to downgrade us.
                            return true;
                        }

                        case EditStyling.Style.BoldParagraph:
                        {
                            // for bolding the paragraph, forward this to the parent to see if it supports it
                            return ParentControl.GetStyleValue( style );
                        }

                        case EditStyling.Style.UnderlineParagraph:
                        {
                            // for underlining the paragraph, forward this to the parent to see if it supports it
                            return ParentControl.GetStyleValue( style );
                        }

                        case EditStyling.Style.BulletParagraph:
                        {
                            return ParentControl.GetStyleValue( style );
                        }

                        case EditStyling.Style.BoldItalicizeParagraph:
                        {
                            return ParentControl.GetStyleValue( style );
                        }

                        case EditStyling.Style.ItalicizeParagraph:
                        {
                            return ParentControl.GetStyleValue( style );
                        }
                    }

                    return null;
                }

                public void SetStyleValue( EditStyling.Style style, object value )
                {
                    switch( style )
                    {
                        case EditStyling.Style.FontName:
                        {
                            string fontName = value as string;
                            PlatformLabel.Editable_SetFontName( fontName );

                            break;
                        }

                        case EditStyling.Style.FontSize:
                        {
                            float fontSize = (float)value;
                            PlatformLabel.Editable_SetFontSize( fontSize );

                            break;
                        }

                        case EditStyling.Style.BoldParagraph:
                        {
                            // for bolding the paragraph, forward this to the parent
                            ParentControl.SetStyleValue( style, value );
                            break;
                        }

                        case EditStyling.Style.UnderlineParagraph:
                        {
                            // for underlining the paragraph, forward this to the parent to see if it supports it
                            ParentControl.SetStyleValue( style, value );
                            break;
                        }

                        case EditStyling.Style.BulletParagraph:
                        {
                            ParentControl.SetStyleValue( style, value );
                            break;
                        }

                        case EditStyling.Style.BoldItalicizeParagraph:
                        {
                            ParentControl.SetStyleValue( style, value );
                            break;
                        }

                        case EditStyling.Style.ItalicizeParagraph:
                        {
                            ParentControl.SetStyleValue( style, value );
                            break;
                        }
                        //case EditStyling.Style.Underline:
                        //{
                        //    bool enableUnderline = (bool) value;

                        //    if( enableUnderline )
                        //    {
                        //        PlatformLabel.Editable_AddUnderline( );
                        //    }
                        //    else
                        //    {
                        //        PlatformLabel.Editable_RemoveUnderline( );
                        //    }

                        //    break;
                        //}
                    }

                    // first, reset our dimensions and call sizeToFit, which will
                    // fully recalculate our bounds (since our font name / size may have changed.)
                    PlatformLabel.Bounds = new RectangleF( 0, 0, 0, 0 );
                    PlatformLabel.SizeToFit( );

                    // now notify our parent so it can update its layout with our new size
                    ParentControl.HandleChildStyleChanged( style, this );
                }

                // Sigh. This is NOT the EditStyle referred to above. This is the Note Styling object
                // used by the notes platform.
                public MobileApp.Shared.Notes.Styles.Style GetControlStyle( )
                {
                    return mStyle;
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

                public IEditableUIControl ContainerForControl( System.Type controlType, PointF mousePos )
                {
                    // we can't be a container for any control type passed in,
                    // and we don't support any children as controls
                    return null;
                }

                public IUIControl HandleCreateControl( System.Type controlType, PointF mousePos )
                {
                    return null;
                }

                public void ResetBounds( )
                {
                    // reset our dimensions and call sizeToFit, which will
                    // fully recalculate our bounds assuming this should all fit on a single line
                    PlatformLabel.Bounds = new RectangleF( 0, 0, 0, 0 );
                    PlatformLabel.SizeToFit( );
                }

                public void HandleDelete( bool notifyParent )
                {
                    RemoveFromView( ParentEditingCanvas );

                    // notify our parent
                    if( notifyParent )
                    {
                        if ( ParentControl != null )
                        {
                            ParentControl.HandleChildDeleted( this );
                        }
                        // no need to check for a Note parent, because we can't be a direct child of Note
                    }
                }

                public void HandleChildDeleted( IEditableUIControl childControl )
                {
                    // this control doesn't support children
                }

                public IEditableUIControl HandleMouseHover( PointF mousePos )
                {
                    bool mouseHovering = GetFrame( ).Contains( mousePos );
                    if ( mouseHovering == true )
                    {
                        PlatformLabel.Hidden = false;
                        PlatformLabel.BackgroundColor = 0xFFFFFF77;
                        return this;
                    }
                    else
                    {
                        PlatformLabel.Hidden = true;
                        PlatformLabel.BackgroundColor = OrigBackgroundColor;
                        return null;
                    }
                }

                public string Export( RectangleF parentPadding, float currYPos )
                {
                    // only export if there's valid text. If we're simply a blank space, we don't need to be saved.
                    if( string.IsNullOrWhiteSpace( PlatformLabel.Text ) == false )
                    {
                        string encodedText = HttpUtility.HtmlEncode( PlatformLabel.Text );

                        // setup note attributes
                        string attributeStrings = string.Format( "FontName=\"{0}\" FontSize=\"{1}\"", PlatformLabel.Editable_GetFontName(), PlatformLabel.Editable_GetFontSize() );

                        return "<RB " + attributeStrings  + ">" + encodedText.Trim( new char[] { ' ' } ) + "</RB>";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }

                public void ToggleDebugRect( bool enabled )
                {
                    // rely on parent for this
                }
            }
        }
    }
}
#endif