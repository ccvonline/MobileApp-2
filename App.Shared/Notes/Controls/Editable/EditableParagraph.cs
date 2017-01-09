#if __WIN__
using MobileApp.Shared.Notes.Styles;
using MobileApp.Shared.PrivateConfig;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using System.Xml;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            public class EditableParagraph : Paragraph, IEditableUIControl
            {
                BaseControl ParentControl { get; set; }
                
                object ParentView { get; set; }
                
                SizeF ParentSize;
                RectangleF Padding;

                bool EditMode_Enabled = false;
                TextBox EditMode_TextBox = null;

                public EditableParagraph( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    // create our textbox that will display the text being edited.
                    EditMode_TextBox = new TextBox( );
                    EditMode_TextBox.KeyUp += EditMode_TextBox_KeyUp;

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

                public override void AddToView( object obj )
                {
                    // store the parentView so we can add / remove children as needed when editing
                    ParentView = obj;

                    base.AddToView( obj );
                }
                
                private void EditMode_TextBox_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
                {
                    switch ( e.Key )
                    {
                        case System.Windows.Input.Key.Escape:
                        {
                            EnableEditMode( false, null );
                            break;
                        }

                        case System.Windows.Input.Key.Return:
                        {
                            // if they press return, commit the changed text.
                            
                            // remove the existing child controls
                            foreach ( IUIControl control in ChildControls )
                            {
                                control.RemoveFromView( ParentView );
                            }
                            ChildControls.Clear( );

                            SetText( EditMode_TextBox.Text );

                            SetPosition( Frame.Left, Frame.Top );

                            EnableEditMode( false, null );

                            // now add the new child controls
                            foreach ( IUIControl control in ChildControls )
                            {
                                control.AddToView( ParentView );
                            }
                            
                            break;
                        }
                    }
                }

                // Takes a string and replaces the content of the paragraph with it.
                private void SetText( string text )
                {
                    // give the text a style that doesn't include things it shouldn't inherit
                    Styles.Style textStyle = mStyle;
                    textStyle.mBorderColor = null;
                    textStyle.mBorderRadius = null;
                    textStyle.mBorderWidth = null;
                    
                    // now break it into words so we can do word wrapping
                    string[] words = text.Split( ' ' );

                    foreach ( string word in words )
                    {
                        // create labels out of each one
                        if ( string.IsNullOrEmpty( word ) == false )
                        {
                            // if the last thing we added was a special control like a reveal box, we 
                            // need the first label after that to have a leading space so it doesn't bunch up against
                            // the control
                            string nextWord = word;
                            NoteText wordLabel = new NoteText( new CreateParams( this, Frame.Width, Frame.Height, ref textStyle ), nextWord + " " );

                            ChildControls.Add( wordLabel );
                        }
                    }
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

                public void EnableEditMode( bool enabled, System.Windows.Controls.Canvas parentCanvas )
                {
                    // don't allow setting the mode to what it's already set to
                    if( enabled != EditMode_Enabled )
                    {
                        // enter enable mode
                        if ( enabled == true )
                        {
                            parentCanvas.Children.Add( EditMode_TextBox );

                            // position and size the textbox
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox, Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox, Frame.Top );

                            EditMode_TextBox.Width = Frame.Width;
                            EditMode_TextBox.Height = Frame.Height;


                            // get the full text. we can use the build HTML stream code to do this.
                            string htmlStream = string.Empty;
                            string textStream = string.Empty;
                            BuildHTMLContent( ref htmlStream, ref textStream, new List<IUIControl>( ) );

                            // assign the text and we're done
                            EditMode_TextBox.Text = textStream;
                        }
                        else
                        {
                            // exit enable mode. We know the parent is a canvas because of the design
                            (EditMode_TextBox.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox );
                        }

                        // store the change
                        EditMode_Enabled = enabled;
                    }
                }

                public PointF GetPosition( )
                {
                    return new PointF( Frame.Left, Frame.Top );
                }

                public void SetPosition( float xPos, float yPos )
                {
                    // first, if there's a parent control, make sure this paragraph stays within its parent. 
                    if ( ParentControl != null )
                    {
                        RectangleF parentFrame = ParentControl.GetFrame( );
                        
                        // clamp the yPos to the vertical bounds of our parent
                        yPos = Math.Max( yPos, parentFrame.Top );
                        yPos = Math.Min( yPos, parentFrame.Bottom );


                        // now left, which is easy
                        xPos = Math.Max( xPos, parentFrame.Left );


                        // Now do the right edge. This is tricky because we will allow this control to move forward
                        // until it can't wrap any more. Then, we'll clamp its movement to the parent's edge.

                        // Get the width of the widest child. This is the minimum width
                        // required for the paragraph, since it's wrapping will stop at the widest child.
                        float minRequiredWidth = 0;
                        foreach ( IUIControl control in ChildControls )
                        {
                            RectangleF controlFrame = control.GetFrame( );
                            if ( controlFrame.Width > minRequiredWidth )
                            {
                                minRequiredWidth = controlFrame.Width;
                            }
                        }

                        // now, if the control cannot wrap any further, we want to clamp its movement
                        // to the parent's right edge
                        if ( Math.Floor( Frame.Width ) <= Math.Floor( minRequiredWidth ) )
                        {
                            // Right Edge Check
                            xPos = Math.Min( xPos, parentFrame.Right - minRequiredWidth );
                        }
                    }

                    //AddOffset( deltaX, deltaY );
                    float currX = Frame.Left;
                    float currY = Frame.Top;

                    Frame = new RectangleF( xPos, yPos, Frame.Width, Frame.Height );

                    float xOffset = Frame.Left - currX;
                    float yOffset = Frame.Top - currY;

                     // position each interactive label relative to ourselves
                    foreach( IUIControl control in ChildControls )
                    {
                        control.AddOffset( xOffset, yOffset );
                    }

                    BorderView.Position = new PointF( BorderView.Position.X + xOffset,
                                                      BorderView.Position.Y + yOffset );


                    float xPosInParent = Frame.Left;
                    float yPosInParent = Frame.Top;

                    // if the control has a parent, make the positions relative to it
                    if ( ParentControl != null )
                    {
                        RectangleF parentFrame = ParentControl.GetFrame( );

                        xPosInParent = Frame.Left - parentFrame.Left;
                        yPosInParent = Frame.Top - parentFrame.Top;
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
