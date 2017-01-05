#if __WIN__
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

using MobileApp.Shared.Notes.Styles;
using Rock.Mobile.UI;
using MobileApp.Shared.Config;
using System.Drawing;
using MobileApp.Shared.PrivateConfig;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Container for Text, RevealText and InputText. Manages wrapping 
            /// and alignment of children.
            /// </summary>
            public class EditableParagraph : Paragraph, IEditableUIControl
            {
                // store our parent so we know our bound restrictions
                RectangleF ParentFrame { get; set; }

                BaseControl ParentControl { get; set; }
                
                SizeF ParentSize;
                RectangleF Padding;

                public EditableParagraph( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    // this will be null if the parent is the actual note
                    ParentControl = parentParams.Parent as BaseControl;

                    // take the max width / height we'll be allowed to fit in
                    ParentSize = new SizeF( parentParams.Width, parentParams.Height );

                    // get our margin / padding
                    // note - declare a temp margin on the stack that we'll throw out. We store this in our BaseControl.
                    RectangleF tempMargin;
                    RectangleF tempBounds = new RectangleF( );
                    GetMarginsAndPadding( ref mStyle, ref ParentSize, ref tempBounds, out tempMargin, out Padding );
                    ApplyImmediateMargins( ref tempBounds, ref tempMargin, ref ParentSize );
                }

                public IEditableUIControl ControlAtPoint( PointF point )
                {
                    // see if any of our child controls contain the point
                    //foreach( IUIControl control in ChildControls )
                    //{
                    //    IUIControl consumingControl = control.ControlAtPoint( point );
                    //    if( consumingControl != null)
                    //    {
                    //        return consumingControl;
                    //    }
                    //}

                    if ( GetFrame( ).Contains( point ) )
                    {
                        return this;
                    }

                    return null;
                }

                public void UpdatePosition( float deltaX, float deltaY )
                {
                    AddOffset( deltaX, deltaY );

                    float xPosInParent = Frame.Left;
                    float yPosInParent = Frame.Top;

                    RectangleF parentFrame = ParentControl.GetFrame( );

                    if ( ParentControl != null )
                    {
                        xPosInParent = Frame.Left - parentFrame.Left;
                        yPosInParent = Frame.Top - parentFrame.Top;

                        // dont ever let the paragraph's right edge move outside of the parent

                        

                        //float edgeToParentDelta = (xPosInParent + Frame.Width) - parentFrame.Width;

                        //if( edgeToParentDelta > 0 )
                        //{
                        //    Console.WriteLine( "Blah" );
                        //}

                        ////xPosInParent = Math.Min( , ParentControl.GetFrame( ).Right );
                    }

                    // given this new position, see how much space we actually have now.
                    UpdateLayout( ParentSize.Width - xPosInParent, ParentSize.Height - yPosInParent );

                    
                    // Build our final frame that determines our dimensions
                    RectangleF frame = new RectangleF( 65000, 65000, -65000, -65000 );

                    // for each child control
                    foreach ( IUIControl control in ChildControls )
                    {
                        // enlarge our frame by the current frame and the next child
                        frame = Parser.CalcBoundingFrame( frame, control.GetFrame( ) );
                    }

                    
                    // now set the new width / height
                    int borderPaddingPx = 0;

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        borderPaddingPx = (int)Rock.Mobile.Graphics.Util.UnitToPx( mStyle.mBorderWidth.Value + PrivateNoteConfig.BorderPadding );
                    }

                    Frame = new RectangleF( Frame.Left, 
                                            Frame.Top, 
                                            frame.Width + Padding.Width + Padding.Left + (borderPaddingPx * 2), 
                                            frame.Height + Padding.Height + Padding.Top + (borderPaddingPx * 2) //add in padding
                                           );

                    // and store that as our bounds
                    BorderView.Frame = Frame;

                    SetDebugFrame( Frame );
                }

                void UpdateLayout( float maxWidth, float maxHeight )
                {
                    RectangleF bounds = new RectangleF( 0, 0, maxWidth, maxHeight );
                    RectangleF padding = Padding;
                    int borderPaddingPx = 0;

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        BorderView.BorderWidth = mStyle.mBorderWidth.Value;
                        borderPaddingPx = (int)Rock.Mobile.Graphics.Util.UnitToPx( mStyle.mBorderWidth.Value + PrivateNoteConfig.BorderPadding );
                    }

                    // now calculate the available width based on padding. (Don't actually change our width)
                    float availableWidth = bounds.Width - padding.Left - padding.Width - (borderPaddingPx * 2);

                    // layout all controls
                    // paragraphs are tricky. 
                    // We need to lay out controls horizontally and wrap when we run out of room. 

                    // To align, we need to keep track of each "row". When the row is full,
                    // we calculate its width, and then adjust each item IN that row so
                    // that the row is centered within the max width of the paragraph.
                    // The max width of the paragraph is defined as the widest row.

                    // maintain a list of all our rows so that once they are all generated,
                    // we can align them based on the widest row.
                    float maxRowWidth = 0;
                    List< List<IUIControl> > rowList = new List< List<IUIControl> >( );

                    // track where within a row we need to start a control
                    float rowRemainingWidth = availableWidth;
                    float startingX = bounds.X + padding.Left + borderPaddingPx;

                    // always store the last placed control's height so that should 
                    // our NEXT control need to wrap, we know how far down to wrap.
                    float yOffset = bounds.Y + padding.Top + borderPaddingPx;
                    float lastControlHeight = 0;
                    float rowWidth = 0;

                    //Create our first row and put it in our list
                    List< IUIControl > currentRow = new List<IUIControl>( );
                    rowList.Add( currentRow );

                    IUIControl lastControl = null;

                    foreach( IUIControl control in ChildControls )
                    {
                        RectangleF controlFrame = control.GetFrame( );
                        control.AddOffset( -controlFrame.Left, -controlFrame.Top );
                        controlFrame = control.GetFrame( );

                        // if there is NOT enough room on this row for the next control
                        if ( rowRemainingWidth < controlFrame.Width )
                        {
                            // since we're advancing to the next row, trim leading white space, which, if we weren't wrapping,
                            // would be a space between words.
                            // note: we can safely cast to a NoteText because that's the only child type we allow.
                            string text = ( (NoteText)control ).GetText( ).TrimStart( ' ' );
                            ( (NoteText) control ).SetText( text );

                            // advance to the next row
                            yOffset += lastControlHeight;

                            // Reset values for the new row
                            rowRemainingWidth = availableWidth;
                            startingX = bounds.X + padding.Left + borderPaddingPx;
                            lastControlHeight = 0;
                            rowWidth = 0;

                            currentRow = new List<IUIControl>( );
                            rowList.Add( currentRow );
                        }
                        else
                        {
                            // see if the last control was a reveal box. if it was, this control needs
                            // its leading space restored.
                            if ( lastControl as RevealBox != null )
                            {
                                string text = ( (NoteText)control ).GetText( );

                                if ( text[ 0 ] != ' ' )
                                {
                                    ( (NoteText) control ).SetText( ' ' + text );
                                }
                            }
                        }


                        // Add this next control to the current row
                        currentRow.Add( control );

                        // position this control appropriately
                        control.AddOffset( startingX, yOffset );

                        // update so the next child begins beyond this one.
                        // also reduce the available width by this control's.
                        rowWidth += controlFrame.Width;
                        startingX += controlFrame.Width; //Increment startingX so the next control is placed after this one.
                        rowRemainingWidth -= controlFrame.Width; //Reduce the available width by what this control took.
                        lastControlHeight = controlFrame.Height > lastControlHeight ? controlFrame.Height : lastControlHeight; //Store the height of the tallest control on this row.

                        // track the widest row
                        maxRowWidth = rowWidth > maxRowWidth ? rowWidth : maxRowWidth;

                        lastControl = control;
                    }

                    // give each row the legal bounds it may work with
                    RectangleF availableBounds = new RectangleF( bounds.X + padding.Left + borderPaddingPx, 
                                                                 bounds.Y + borderPaddingPx + padding.Top, 
                                                                 availableWidth, 
                                                                 bounds.Height );

                    // Now that we know the widest row, align all the rows
                    foreach( List<IUIControl> row in rowList )
                    {
                        AlignRow( availableBounds, row, maxRowWidth );
                    }
                    
                    // restore each control's parent paragraph's translation 
                    foreach( IUIControl control in ChildControls )
                    {
                        control.AddOffset( Frame.Left, Frame.Top );
                    }
                }

                void AlignRow( RectangleF bounds, List<IUIControl> currentRow, float maxWidth )
                {
                    // Determine the row's width and height (Height is defined as the tallest control on this line)
                    float rowHeight = 0;
                    float rowWidth = 0;

                    foreach ( IUIControl rowControl in currentRow )
                    {
                        RectangleF controlFrame = rowControl.GetFrame( );

                        rowWidth += controlFrame.Width;
                        rowHeight = rowHeight > controlFrame.Height ? rowHeight : controlFrame.Height;
                    }

                    // the amount each control in the row should adjust is the 
                    // difference of paragraph width (which is defined by the max row width)
                    // and this row's width.
                    float xRowAdjust = 0;
                    switch ( ChildHorzAlignment )
                    {
                        // JHM Note 7-24: Yesterday I changed bounds.Width to be MaxRowWidth. I can't remember why.
                        // Today Jon found that if you put a single line of text, you can't align it because its
                        // width is the max width, which causes no movement in the paragraph.
                        // I made it bounds.width again and can't find any problems with it, but I'm leaving the old calculation
                        // here just in case we need it again. :-\
                        case Alignment.Right:
                            {
                                xRowAdjust = ( bounds.Width - rowWidth );
                                break;
                            }
                        case Alignment.Center:
                            {
                                xRowAdjust = ( ( bounds.Width / 2 ) - ( rowWidth / 2 ) );
                                break;
                            }
                        case Alignment.Left:
                            {
                                xRowAdjust = 0;
                                break;
                            }
                    }

                    // Now adjust each control to be aligned correctly on X and Y
                    foreach ( IUIControl rowControl in currentRow )
                    {
                        // vertically center all items within the row.
                        float yAdjust = rowHeight / 2 - ( rowControl.GetFrame( ).Height / 2 );

                        // set their correct X offset
                        rowControl.AddOffset( xRowAdjust, yAdjust );
                    }
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
