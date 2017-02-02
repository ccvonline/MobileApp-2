#if __WIN__
using MobileApp.Shared.Notes.Styles;
using MobileApp.Shared.PrivateConfig;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
                
                System.Windows.Controls.Canvas ParentEditingCanvas { get; set; }
                
                SizeF ParentSize;
                RectangleF Padding;

                bool EditMode_Enabled = false;
                TextBox EditMode_TextBox = null;

                // store the background color so that if we change it for hovering, we can restore it after
                uint OrigBackgroundColor = 0;

                // the size (in pixels) to extend the paragraph's frame
                // for mouse interaction
                const float CornerExtensionSize = 5;
                
                public EditableParagraph( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;

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

                    OrigBackgroundColor = BorderView.BackgroundColor;
                }

                public override void AddToView( object obj )
                {
                    // store the parentView, which we know is a canvas, so we can add / remove children as needed when editing
                    ParentEditingCanvas = obj as System.Windows.Controls.Canvas;

                    base.AddToView( obj );
                }

                public bool IsEditing( )
                {
                    return EditMode_Enabled;
                }

                public void HandleKeyUp( KeyEventArgs e )
                {
                    // ignore
                }

                public IEditableUIControl HandleMouseHover( PointF mousePos )
                {
                    IEditableUIControl consumingControl = null;
                    bool hoveringChildControl = false;
                                        
                    // create a position outside the canvas. As soon as we find an item we're hovering over,
                    // we'll send the rest of the controls this position to force them to discontinue their hover state
                    PointF oobPos = new PointF( -100, -100 );

                    // see if any of our child controls contain the point
                    int i;
                    for( i = 0; i < ChildControls.Count; i++ )
                    {
                        IEditableUIControl editControl = ChildControls[ i ] as IEditableUIControl;
                        if ( editControl != null )
                        {
                            // if we're hovering over any control, flag it, but don't stop checking
                            consumingControl = editControl.HandleMouseHover( mousePos );
                            if( consumingControl != null )
                            {
                                hoveringChildControl = true;
                                break;
                            }
                        }
                    }

                    // now let the remainig children turn off
                    i++;
                    for( ; i < ChildControls.Count; i++ )
                    {
                        IEditableUIControl editableControl = ChildControls[ i ] as IEditableUIControl;
                        if( editableControl != null )
                        {
                            editableControl.HandleMouseHover( oobPos );
                        }
                    }
                    
                    // if we're over a child
                    if( hoveringChildControl == true )
                    {
                        // restore the color
                        BorderView.BackgroundColor = OrigBackgroundColor;
                    }
                    else
                    {
                        // otherwise, see if we're hovering over the paragraph, and if we are, highlight it
                        RectangleF frame = GetFrame( );
                        frame.Inflate( CornerExtensionSize, CornerExtensionSize );

                        // are we hovering over this control?
                        bool mouseHovering = frame.Contains( mousePos );
                        if( mouseHovering == true )
                        {
                            // take us as the consumer
                            consumingControl = this;

                            // set the hover appearance
                            BorderView.BackgroundColor = 0xFFFFFF77;
                        }
                        // we're NOT hovering
                        else
                        {
                            // so revert the color
                            BorderView.BackgroundColor = OrigBackgroundColor;
                        }
                    }

                    return consumingControl;
                }
                
                private void EditMode_TextBox_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
                {
                    switch ( e.Key )
                    {
                        case System.Windows.Input.Key.Escape:
                        {
                            EnableEditMode( false );
                            break;
                        }

                        case System.Windows.Input.Key.Return:
                        {
                            // if they press return, commit the changed text.
                            SetText( EditMode_TextBox.Text );
                                
                            EnableEditMode( false );

                            SetPosition( Frame.Left, Frame.Top );

                            // now add the new child controls
                            foreach ( IUIControl control in ChildControls )
                            {
                                control.AddToView( ParentEditingCanvas );
                            }
                            
                            break;
                        }
                    }
                }

                // Takes a string and replaces the content of the paragraph with it.
                private void SetText( string text )
                {
                    foreach ( IUIControl control in ChildControls )
                    {
                        control.RemoveFromView( ParentEditingCanvas );
                    }
                    ChildControls.Clear( );

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
                            NoteText wordLabel = Parser.CreateNoteText( new CreateParams( this, Frame.Width, Frame.Height, ref textStyle ), nextWord + " " );

                            ChildControls.Add( wordLabel );
                        }
                    }
                }

                public IEditableUIControl HandleMouseDoubleClick( PointF point )
                {
                    // for double click, we need to give all our child controls a chance to consume

                    // see if any of our child controls contain the point
                    foreach ( IUIControl control in ChildControls )
                    {
                        // if this control is editable
                        IEditableUIControl editableControl = control as IEditableUIControl;
                        if ( editableControl != null )
                        {
                            // if it (or a child) consumes, return that
                            IEditableUIControl consumingControl = editableControl.HandleMouseDoubleClick( point );
                            if ( consumingControl != null )
                            {
                                return consumingControl;
                            }
                        }
                    }

                    // create a grabbable corner outside of the paragraph words
                    RectangleF frame = GetFrame( );
                    frame.Inflate( CornerExtensionSize, CornerExtensionSize );

                    if ( frame.Contains( point ) )
                    {
                        // notify the caller we're consuming, and turn on edit mode
                        EnableEditMode( true );
                        return this;
                    }

                    return null;
                }

                public IEditableUIControl HandleMouseDown( PointF point )
                {
                    // for mouse down, we don't want any children to consume

                    // create a grabbable corner outside of the paragraph words
                    RectangleF frame = GetFrame( );
                    frame.Inflate( CornerExtensionSize, CornerExtensionSize );

                    if ( frame.Contains( point ) )
                    {
                        return this;
                    }

                    return null;
                }

                private void EnableEditMode( bool enabled )
                {
                    // don't allow setting the mode to what it's already set to
                    if( enabled != EditMode_Enabled )
                    {
                        // enter enable mode
                        if ( enabled == true )
                        {
                            ParentEditingCanvas.Children.Add( EditMode_TextBox );

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

                            Dispatcher.CurrentDispatcher.BeginInvoke( DispatcherPriority.Input, new Action( delegate() 
                            { 
                                EditMode_TextBox.Focus( );
                                Keyboard.Focus( EditMode_TextBox );
                                EditMode_TextBox.CaretIndex = EditMode_TextBox.Text.Length + 1;
                            }));
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
                    // we're not moving if we're in edit mode
                    if( EditMode_Enabled == false )
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
                }

                public object GetStyleValue( EditStyling.Style style )
                {
                    return null;
                }

                public void SetStyleValue( EditStyling.Style style, object value )
                {
                }

                public List<EditStyling.Style> GetEditStyles( )
                {
                    return new List<EditStyling.Style>( );
                }

                public void HandleChildStyleChanged( EditStyling.Style style, IEditableUIControl childControl )
                {
                    switch( style )
                    {
                        case EditStyling.Style.RevealBox:
                        {
                            // first, find the target in our list
                            int targetIndex = 0;
                            foreach( IUIControl child in ChildControls )
                            {
                                // when we find it
                                if( child.Equals( childControl ) == true )
                                {
                                    // take its index, and remove it from the renderer and our list of children
                                    targetIndex = ChildControls.IndexOf( child );

                                    child.RemoveFromView( ParentEditingCanvas );
                                    ChildControls.RemoveAt( targetIndex );
                                    break;
                                }
                            }

                            // if we received RevealBox, we're either upgrading a NoteText to BE a RevealBox,
                            // or downgrading a RevealBox to be a normal NoteText.
                            EditableNoteText editableNoteText = childControl as EditableNoteText;
                            if ( editableNoteText != null )
                            {
                                // create a new revealBox that has the styling and text of the noteText it's replacing.
                                Style controlStyle = childControl.GetControlStyle( );
                                RevealBox newRevealBox = Parser.CreateRevealBox( new CreateParams( this, Frame.Width, Frame.Height, ref controlStyle ), editableNoteText.GetText( ).Trim( ) );
                                newRevealBox.AddToView( ParentEditingCanvas );
                                
                                // add the new revealBox into the same spot as what it's replacing
                                ChildControls.Insert( targetIndex, newRevealBox );
                            }

                            EditableRevealBox editableRevealBox = childControl as EditableRevealBox;
                            if( editableRevealBox != null )
                            {
                                // create a new revealBox that has the styling and text of the noteText it's replacing.
                                Style controlStyle = childControl.GetControlStyle( );
                                NoteText newNoteText = Parser.CreateNoteText( new CreateParams( this, Frame.Width, Frame.Height, ref controlStyle ), editableRevealBox.GetText( ).Trim( ) );
                                newNoteText.AddToView( ParentEditingCanvas );
                                
                                // add the new revealBox into the same spot as what it's replacing
                                ChildControls.Insert( targetIndex, newNoteText );
                            }

                            break;
                        }
                    }

                    // for now, lets just redo our layout.
                    SetPosition( Frame.Left, Frame.Top );

                    // now see if there's a parent that we should notify
                    IEditableUIControl editableParent = ParentControl as IEditableUIControl;
                    if( editableParent != null )
                    {
                        editableParent.HandleChildStyleChanged( style, this );
                    }
                }

                // Sigh. This is NOT the EditStyle referred to above. This is the Note Styling object
                // used by the notes platform.
                public MobileApp.Shared.Notes.Styles.Style GetControlStyle( )
                {
                    return mStyle;
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

                                    // re-acquire the width, since we just added a space.
                                    controlFrame = control.GetFrame( );
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
            }
        }
    }
}
#endif
