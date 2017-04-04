using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Rock.Mobile.Network;
using System.Drawing;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Parser contains a set of utility methods to assist in parsing NoteScript.
            /// </summary>
            public class Parser
            {
                public static IUIControl TryParseControl( Notes.BaseControl.CreateParams parentParams, XmlReader reader )
                {
                    // either create/parse a new control, or return null.
                    if ( Paragraph.ElementTagMatches( reader.Name ) )
                    {
#if __WIN__
                        return new EditableParagraph( parentParams, reader );
#else
                        return new Paragraph( parentParams, reader );
#endif
                    }
                    else if ( Canvas.ElementTagMatches( reader.Name ) )
                    {
                        return new Canvas( parentParams, reader );
                    }
                    else if ( StackPanel.ElementTagMatches( reader.Name ) )
                    {
                        return new StackPanel( parentParams, reader );
                    }
                    else if ( List.ElementTagMatches( reader.Name ) )
                    {
                        return new List( parentParams, reader );
                    }
                    else if ( ListItem.ElementTagMatches( reader.Name ) )
                    {
                        return new ListItem( parentParams, reader );
                    }
                    else if ( RevealBox.ElementTagMatches( reader.Name ) )
                    {
#if __WIN__
                        return new EditableRevealBox( parentParams, reader );
#else
                        return new RevealBox( parentParams, reader );
#endif
                    }
                    else if ( Quote.ElementTagMatches( reader.Name ) )
                    {
#if __WIN__
                        return new EditableQuote( parentParams, reader );
#else
                        return new Quote( parentParams, reader );
#endif
                    }
                    else if ( TextInput.ElementTagMatches( reader.Name ) )
                    {
                        return new TextInput( parentParams, reader );
                    }
                    else if ( Header.ElementTagMatches( reader.Name ) )
                    {
#if __WIN__
                        return new EditableHeader( parentParams, reader );
#else
                        return new Header( parentParams, reader );
#endif
                    }
                    else if ( NoteText.ElementTagMatches( reader.Name ) )
                    {
#if __WIN__
                        return new EditableNoteText( parentParams, reader );
#else
                        return new NoteText( parentParams, reader );
#endif
                    }

                    throw new Exception( String.Format( "Control of type {0} does not exist.", reader.Name ) );
                }

                public static NoteText CreateNoteText( Notes.BaseControl.CreateParams parentParams, string text )
                {
#if __WIN__
                    return new EditableNoteText( parentParams, text );
#else
                    return new NoteText( parentParams, text );
#endif
                }

                public static RevealBox CreateRevealBox( Notes.BaseControl.CreateParams parentParams, string revealText )
                {
#if __WIN__
                    return new EditableRevealBox( parentParams, revealText );
#else
                    return new RevealBox( parentParams, revealText );
#endif
                }

#if __WIN__
                // This is used only by Windows in order to create new controls in the note
                public static IUIControl CreateEditableControl( Type controlType, Notes.BaseControl.CreateParams parentParams )
                {
                    if ( controlType == typeof( EditableParagraph ) )
                    {
                        // seed the paragaph with some default text
                        XmlTextReader reader = new XmlTextReader( new StringReader( "<P>New Paragraph</P>" ) );
                        reader.Read( );

                        return new EditableParagraph( parentParams, reader );
                    }
                    else if ( controlType == typeof( EditableHeader ) )
                    {
                        // give the stack panel some back width and color so the user can see it
                        XmlTextReader reader = new XmlTextReader( new StringReader( "<Header> <Title>Title</Title> <Speaker>Speaker</Speaker> <Date>1-1-2018</Date> </Header>" ) );
                        reader.Read( );

                        return new EditableHeader( parentParams, reader );
                    }
                    else if ( controlType == typeof( EditableQuote ) )
                    {
                        // give the stack panel some back width and color so the user can see it
                        XmlTextReader reader = new XmlTextReader( new StringReader( "<Quote Citation=\"Citation\">New Quote Body</Quote>" ) );
                        reader.Read( );

                        return new EditableQuote( parentParams, reader );
                    }

                    return null;
                }
#endif

                public static void ParseBounds( XmlReader reader, ref SizeF parentSize, ref RectangleF bounds )
                {
                    // first check without the Margin prefix.
                    string result = reader.GetAttribute( "Left" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        bounds.X = ParsePositioningValue( result );
                    }

                    result = reader.GetAttribute( "Top" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        bounds.Y = ParsePositioningValue( result );
                    }
                    //TODO: Support Right, Bottom

                    // Get width/height
                    result = reader.GetAttribute( "Width" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        bounds.Width = ParsePositioningValue( result );
                    }

                    result = reader.GetAttribute( "Height" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        bounds.Height = ParsePositioningValue( result );
                    }

                    // Convert percentages to whole values
                    if ( bounds.X < 1 )
                    {
                        bounds.X = parentSize.Width * bounds.X;
                    }
                    else
                    {
                        bounds.X = Rock.Mobile.Graphics.Util.UnitToPx( bounds.X );
                    }

                    if ( bounds.Y < 1 )
                    {
                        bounds.Y = parentSize.Height * bounds.Y;
                    }
                    else
                    {
                        bounds.Y = Rock.Mobile.Graphics.Util.UnitToPx( bounds.Y );
                    }
                    
                    if ( bounds.Width < 1 )
                    {
                        bounds.Width = Math.Max( 1, parentSize.Width - bounds.X ) * bounds.Width;
                        if ( bounds.Width == 0 )
                        {
                            // if 0, just take the our parents width
                            bounds.Width = Math.Max( 1, parentSize.Width - bounds.X );
                        }
                    }
                    else
                    {
                        bounds.Width = Rock.Mobile.Graphics.Util.UnitToPx( bounds.Width );
                    }

                    if ( bounds.Height < 1 )
                    {
                        bounds.Height = Math.Max( 1, parentSize.Height - bounds.Y ) * bounds.Height;
                        if ( bounds.Height == 0 )
                        {
                            // if 0, just take the our parents width
                            bounds.Height = Math.Max( 1, parentSize.Height - bounds.Y );
                        }
                    }
                    else
                    {
                        bounds.Height = Rock.Mobile.Graphics.Util.UnitToPx( bounds.Height );
                    }
                }

                public static float ParsePositioningValue( string value )
                {
                    float denominator = 1.0f;
                    
                    if( value.Contains( "%" ) )
                    {
                        value = value.Trim( '%' );
                        denominator = 100.0f;
                    }

                    float number = float.Parse( value );
                    return number / denominator;
                }

                // Return a rect that contains both rects A and B (sort of a bounding box)
                public static RectangleF CalcBoundingFrame( RectangleF frameA, RectangleF frameB )
                {
                    RectangleF frame = new RectangleF( );

                    // get left edge
                    float leftEdge = frameA.Left < frameB.Left ? frameA.Left : frameB.Left;

                    // get top edge
                    float topEdge = frameA.Top < frameB.Top ? frameA.Top : frameB.Top;

                    // get right edge
                    float rightEdge = frameA.Right > frameB.Right ? frameA.Right : frameB.Right;

                    // get bottom edge
                    float bottomEdge = frameA.Bottom > frameB.Bottom ? frameA.Bottom : frameB.Bottom;

                    frame.X = leftEdge;
                    frame.Y = topEdge;
                    frame.Width = rightEdge - leftEdge;
                    frame.Height = bottomEdge - topEdge;

                    return frame;
                }

                // Returns a rect that is rectA expanded by rectB (inflate in all four directions)
                public static RectangleF CalcExpandedFrame( RectangleF frameA, RectangleF frameB )
                {
                    RectangleF expandedRect = frameA;

                    expandedRect.X += frameB.Left;
                    expandedRect.Y += frameB.Top;
                    expandedRect.Width += frameB.Width;
                    expandedRect.Height += frameB.Height;

                    return expandedRect;
                }
            }
        }
    }
}
