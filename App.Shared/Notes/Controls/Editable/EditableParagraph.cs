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

using System.Windows.Media;
using RestSharp.Extensions.MonoHttp;
using WinNotes;
using App.Shared;
using System.IO;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            public class EditableParagraph : Paragraph, IEditableUIControl
            {
                Note ParentNote { get; set; }

                public const string sDefaultNewParagraphText = "New Paragraph";

                public const string sDefaultBoldFontName = "OpenSans-Bold";
                public const string sDefaultRegularFontName = "OpenSans-Regular";

                public const string sBulletChar = "•";
                
                System.Windows.Controls.Canvas ParentEditingCanvas { get; set; }
                
                SizeF ParentSize;
                RectangleF Padding;

                bool EditMode_Enabled = false;
                EditModeTextBox EditMode_TextBox = null;
                EditModeTextBox EditMode_TextBox_Url = null;

                // store the background color so that if we change it for hovering, we can restore it after
                uint OrigBackgroundColor = 0;

                // the size (in pixels) to extend the paragraph's frame
                // for mouse interaction
                const float CornerExtensionSize = 5;
                                
                public EditableParagraph( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;

                    // create our textbox that will display the text being edited.
                    EditMode_TextBox = new EditModeTextBox( );
                    EditMode_TextBox.KeyUp += EditMode_TextBox_KeyUp;

                    EditMode_TextBox_Url = new EditModeTextBox( );
                    EditMode_TextBox_Url.KeyUp += EditMode_TextBox_KeyUp;

                    // this will be null if the parent is the actual note
                    ParentNote = parentParams.Parent as Note;

                    // take the max width / height we'll be allowed to fit in (restore the padding, since we actively remove it when dragging / exporting)
                    ParentSize = new SizeF( parentParams.Width + ParentNote.Padding.Left + ParentNote.Padding.Right, parentParams.Height );

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

                    
                    // JHM 5-8-17 - Only show hover for the children. Don't highlight the paragraph. It's pointless and distracting
                    // if we're over a child
                    if( hoveringChildControl == true )
                    {
                        return consumingControl;
                    }
                    
                    return null;
                    
                    // if we're over a child
                    //if( hoveringChildControl == true )
                    //{
                    //    // restore the color
                    //    BorderView.BackgroundColor = OrigBackgroundColor;
                    //}
                    //else
                    //{
                    //    // otherwise, see if we're hovering over the paragraph, and if we are, highlight it
                    //    RectangleF frame = GetFrame( );
                    //    frame.Inflate( CornerExtensionSize, CornerExtensionSize );

                    //    // are we hovering over this control?
                    //    bool mouseHovering = frame.Contains( mousePos );
                    //    if( mouseHovering == true )
                    //    {
                    //        // take us as the consumer
                    //        consumingControl = this;

                    //        // set the hover appearance
                    //        BorderView.BackgroundColor = 0xFFFFFF77;
                    //    }
                    //    // we're NOT hovering
                    //    else
                    //    {
                    //        // so revert the color
                    //        BorderView.BackgroundColor = OrigBackgroundColor;
                    //    }
                    //}

                    //return consumingControl;
                }

                public bool IsEditing( )
                {
                    return EditMode_Enabled;
                }

                public bool HandleFocusedControlKeyUp( KeyEventArgs e )
                {
                    // for controls with textBoxes used for editing, we need
                    // to be consistent with how it will handle input.
                    bool releaseFocus = false;
                    switch( e.Key )
                    {
                        case Key.Return:
                        {
                            // on return, release focus and try to save changes
                            releaseFocus = true;

                            EditMode_End( true );
                            break;
                        }

                        case Key.Escape:
                        {
                            // on escape, always release focus
                            releaseFocus = true;

                            EditMode_End( false );
                            break;
                        }
                    }

                    return releaseFocus;
                }
                
                private void EditMode_TextBox_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
                {
                    switch ( e.Key )
                    {
                        case System.Windows.Input.Key.Escape:
                        {
                            EditMode_End( false );
                            break;
                        }

                        case System.Windows.Input.Key.Return:
                        {
                            EditMode_End( true );
                            break;
                        }
                    }
                }

                private void EditMode_Begin( )
                {
                    if ( EditMode_Enabled == false )
                    {
                        EditMode_Enabled = true;

                        ParentEditingCanvas.Children.Add( EditMode_TextBox );

                        // position and size the textbox
                        System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox, Frame.Left - 5 );
                        System.Windows.Controls.Canvas.SetTop( EditMode_TextBox, Frame.Top - 2 );

                        float availableWidth = ParentSize.Width - Frame.Left;

                        EditMode_TextBox.Width = availableWidth;
                        EditMode_TextBox.Height = Frame.Height * 1.25f;

                        // get the full text. we can use the build HTML stream code to do this.
                        string htmlStream = string.Empty;
                        string textStream = string.Empty;
                        BuildHTMLContent( ref htmlStream, ref textStream, new List<IUIControl>( ) );

                        // if the last character is a URL glyph, remove it
                        textStream = textStream.Trim( new char[] { ' ', PrivateNoteConfig.CitationUrl_Icon[0] } );

                        // assign the text
                        EditMode_TextBox.Text = textStream;

                        // and now the URL support
                        EditMode_TextBox_Url.Text = ActiveUrl == null ? EditableConfig.sPlaceholderUrlText : ActiveUrl;
                        ParentEditingCanvas.Children.Add( EditMode_TextBox_Url );
                        EditMode_TextBox_Url.Width = availableWidth;
                        EditMode_TextBox_Url.Height = 33;
                            
                        System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Url, Frame.Left - 5 );
                        System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Url, Frame.Bottom + 5 );

                        Dispatcher.CurrentDispatcher.BeginInvoke( DispatcherPriority.Input, new Action( delegate() 
                        { 
                            EditMode_TextBox.Focus( );
                            Keyboard.Focus( EditMode_TextBox );
                            EditMode_TextBox.CaretIndex = EditMode_TextBox.Text.Length + 1;

                            if( EditMode_TextBox.Text == sDefaultNewParagraphText )
                            {
                                EditMode_TextBox.SelectAll( );
                            }

                            if ( EditMode_TextBox_Url.Text == EditableConfig.sPlaceholderUrlText )
                            {
                                EditMode_TextBox_Url.SelectAll( );
                            }
                        }));
                    }
                }

                private void EditMode_End( bool saveChanges )
                {
                    // end edit mode without storing the changes
                    if ( EditMode_Enabled == true )
                    {
                        EditMode_Enabled = false;

                        // exit enable mode. We know the parent is a canvas because of the design
                        (EditMode_TextBox.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox );
                        (EditMode_TextBox_Url.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Url );


                        if( saveChanges )
                        {
                            // only store changes if they put valid text in the box
                            if ( string.IsNullOrWhiteSpace( EditMode_TextBox.Text ) == false )
                            {
                                // set the text
                                SetText( EditMode_TextBox.Text, EditMode_TextBox_Url.Text );

                                SetPosition( Frame.Left, Frame.Top );

                                // now add the new child controls
                                foreach ( IUIControl control in ChildControls )
                                {
                                    control.AddToView( ParentEditingCanvas );
                                }
                            }
                        }
                    }
                }
                
                // Takes a string and replaces the content of the paragraph with it.
                private void SetText( string text, string activeUrl )
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

                    if( activeUrl.Trim( ) != EditableConfig.sPlaceholderUrlText && string.IsNullOrWhiteSpace( activeUrl ) == false )
                    {
                        // help them out by adding 'https://' if it isn't there.
                        if ( activeUrl.StartsWith( "https" ) == false && BibleRenderer.IsBiblePrefix( activeUrl ) == false )
                        {
                            activeUrl = activeUrl.Insert( 0, "https://" );
                        }

                        ActiveUrl = activeUrl;
                    }
                    else
                    {
                        ActiveUrl = null;
                    }

                    TryAddUrlGlyph( Frame.Width, Frame.Height );
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
                        EditMode_Begin( );
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

                public PointF GetPosition( )
                {
                    return new PointF( Frame.Left, Frame.Top );
                }
                
                public void SetPosition( float xPos, float yPos )
                {
                    // we're not moving if we're in edit mode
                    if( EditMode_Enabled == false )
                    {
                        // clamp the yPos to the vertical bounds of our parent
                        yPos = Math.Max( yPos, ParentNote.Padding.Top );

                        // now left, which is easy
                        xPos = Math.Max( xPos, ParentNote.Padding.Left );


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

                        // Right Edge Check
                        xPos = Math.Min( xPos, ParentSize.Width - ParentNote.Padding.Right - minRequiredWidth );

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
                        
                        // given this new position, see how much space we actually have now.
                        UpdateLayout( (ParentSize.Width - ParentNote.Padding.Right) - xPosInParent, ParentSize.Height - yPosInParent );

                    
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
                    switch( style )
                    {
                        case EditStyling.Style.BoldParagraph:
                        {
                            return true;
                        }

                        case EditStyling.Style.UnderlineParagraph:
                        {
                            return true;
                        }

                        case EditStyling.Style.BulletParagraph:
                        {
                            return true;
                        }
                    }

                    return null;
                }

                public void SetStyleValue( EditStyling.Style style, object value )
                {
                    switch( style )
                    {
                        case EditStyling.Style.BoldParagraph:
                        {
                            // force all children to bold
                            foreach( IUIControl child in ChildControls )
                            {
                                IEditableUIControl editableChild = child as IEditableUIControl;
                                if( editableChild != null )
                                {
                                    editableChild.SetStyleValue( EditStyling.Style.FontName, EditableParagraph.sDefaultBoldFontName );
                                }
                            }
                            break;
                        }

                        case EditStyling.Style.BulletParagraph:
                        {
                            // create a noteText with the bullet, and then insert it       
                            XmlTextReader reader = new XmlTextReader( new StringReader( "<NT>" + sBulletChar + "</NT>" ) );
                            reader.Read( );

                            IUIControl bulletText = Parser.TryParseControl( new CreateParams( this, ParentSize.Width, ParentSize.Height, ref mStyle ), reader );
                            
                            ChildControls.Insert( 0, bulletText );

                            SetPosition( Frame.Left, Frame.Top );

                            bulletText.AddToView( ParentEditingCanvas );

                            break;
                        }

                        case EditStyling.Style.UnderlineParagraph:
                        {
                            // to underline the whole thing, we need to make it one big NoteText.
                            // we'll gather all the text, convert it to a single NoteText with Underlined Attribute,
                            // remove the existing children, and replace them with the new single underlined one.

                            // get the full text. we can use the build HTML stream code to do this.
                            string htmlStream = string.Empty;
                            string textStream = string.Empty;
                            BuildHTMLContent( ref htmlStream, ref textStream, new List<IUIControl>( ) );

                            // if the last character is a URL glyph, remove it
                            textStream = textStream.Trim( new char[] { ' ', PrivateNoteConfig.CitationUrl_Icon[0] } );
                                
                            XmlTextReader reader = new XmlTextReader( new StringReader( "<NT Underlined=\"True\">" + textStream + "</NT>" ) );
                            reader.Read( );

                            IUIControl noteText = Parser.TryParseControl( new CreateParams( this, ParentSize.Width, ParentSize.Height, ref mStyle ), reader );

                            foreach ( IUIControl control in ChildControls )
                            {
                                control.RemoveFromView( ParentEditingCanvas );
                            }
                            ChildControls.Clear( );

                            
                            ChildControls.Add( noteText );

                            SetPosition( Frame.Left, Frame.Top );

                            noteText.AddToView( ParentEditingCanvas );

                            break;
                        }
                    }
                }

                public List<EditStyling.Style> GetEditStyles( )
                {
                    List<EditStyling.Style> styleList = new List<EditStyling.Style>( );
                    styleList.Add( EditStyling.Style.BoldParagraph );

                    return styleList;
                }

                public void HandleDelete( bool notifyParent )
                {
                    // first, delete all our child controls
                    int i = 0;
                    for( i = ChildControls.Count - 1; i >= 0; i-- )
                    {
                        // since we DO support child controls that are containers, call HandleDelete on all our children
                        IEditableUIControl editableControl = ChildControls[ i ] as IEditableUIControl;
                        if( editableControl != null )
                        {
                            // let them know not to inform their parent, since that's us.
                            editableControl.HandleDelete( false );
                        }
                        else
                        {
                            // if it's not editable, we need to remove it ourselves
                            ChildControls[ i ].RemoveFromView( ParentEditingCanvas );
                        }

                        ChildControls.Remove( ChildControls[ i ] );
                    }

                    // clean ourselves up
                    RemoveFromView( ParentEditingCanvas );

                    // notify our parent if we need to
                    if( notifyParent )
                    {
                        ParentNote.HandleChildDeleted( this );
                    }
                }

                public void HandleChildDeleted( IEditableUIControl childControl )
                {
                    // if any of our children are deleted, we want to delete ourselves completely.
                    // First, remove the child that was just deleted, and then delete all other controls and ourself
                    foreach ( IUIControl child in ChildControls )
                    {
                        if ( child.Equals( childControl ) == true )
                        {
                            ChildControls.Remove( child );
                            break;
                        }
                    }

                    HandleDelete( true );

                    //// if we still have children
                    //if( ChildControls.Count > 0 )
                    //{
                    //    // update our layout
                    //    SetPosition( Frame.Left, Frame.Top );
                    //}
                    //else
                    //{
                    //    // otherwise, delete ourselves and tell our parent
                    //    HandleDelete( true );
                    //}
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
                                // create a new revealBox, but force the text to uppper-case and add the bold font (since this is typically what the Mobile App does)
                                Style controlStyle = childControl.GetControlStyle( );
                                controlStyle.mFont = new FontParams( );
                                controlStyle.mFont.mName = sDefaultBoldFontName;

                                RevealBox newRevealBox = Parser.CreateRevealBox( new CreateParams( this, Frame.Width, Frame.Height, ref controlStyle ), editableNoteText.GetText( ).ToUpper( ).Trim( ) );
                                newRevealBox.AddToView( ParentEditingCanvas );

                                // add the new revealBox into the same spot as what it's replacing
                                ChildControls.Insert( targetIndex, newRevealBox );

                                // make sure we add a space after the reveal box, as that's required.
                                NoteText textLabel = Parser.CreateNoteText( new CreateParams( this, Frame.Width, Frame.Height, ref mStyle ), " " );
                                textLabel.AddToView( ParentEditingCanvas );
                                ChildControls.Insert( targetIndex + 1, textLabel );
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
                }

                // Sigh. This is NOT the EditStyle referred to above. This is the Note Styling object
                // used by the notes platform.
                public MobileApp.Shared.Notes.Styles.Style GetControlStyle( )
                {
                    return mStyle;
                }

                public void ResetBounds( )
                {
                    // nothing to do
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

                                // if text.Length is 0, this was simply a blank space after a reveal box that
                                // was trimmed, and now needs its blank space restored
                                if ( text.Length == 0 || text[ 0 ] != ' ' )
                                {
                                    ( (NoteText) control ).SetText( ' ' + text );

                                    // since adding a blank space could trigger text wrapping on the WinLabel,
                                    // do a bounds reset to fit the text on a single line.
                                    ((IEditableUIControl)control).ResetBounds( );
                                    
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

                public string Export( RectangleF parentPadding, float currYPos )
                {
                    // start by setting our position to our global position, and then we'll translate.
                    float controlLeftPos = Frame.Left;
                    float controlTopPos = Frame.Top;
                    
                    // for vertical, it's relative to the control above it, so just make it relative to that
                    IUIControl logicalParent = ParentNote.GetLogicalVerticalParent( this );
                    if ( logicalParent != null )
                    {
                        controlTopPos -= logicalParent.GetFrame( ).Bottom;
                    }
                    else
                    {
                        controlTopPos -= currYPos;
                    }

                    // for horizontal, it just needs to remove padding, since it'll be re-applied on load
                    controlLeftPos -= parentPadding.Left;
                    
                    string xml = "<P ";

                    string attributes = "";
                    controlLeftPos /= (ParentSize.Width - parentPadding.Left - parentPadding.Right);
                    attributes += string.Format( "Left=\"{0:#0.00}%\"", controlLeftPos * 100 );
                    

                    attributes += string.Format( " Top=\"{0}\"", controlTopPos );
                    
                    if ( string.IsNullOrWhiteSpace( ActiveUrl ) == false )
                    {
                        attributes += string.Format( " Url=\"{0}\"", HttpUtility.HtmlEncode( ActiveUrl ) );
                    }

                    xml += attributes + ">";

                    foreach( IUIControl child in ChildControls )
                    {
                        IEditableUIControl editableChild = child as IEditableUIControl;
                        if( editableChild != null )
                        {
                            // children of paragraphs cannot set their own position, so pass 0
                            xml += editableChild.Export( new RectangleF( ), 0 );
                        }
                    }

                    xml += "</P>";

                    return xml;
                }

                public void ToggleDebugRect( bool enabled )
                {
                    // set the hover appearance
                    if( enabled && OrigBackgroundColor != EditableConfig.sDebugColor )
                    {
                        OrigBackgroundColor = BorderView.BackgroundColor;
                        BorderView.BackgroundColor = EditableConfig.sDebugColor;
                    }
                    else if ( enabled == false )
                    {
                        BorderView.BackgroundColor = OrigBackgroundColor;
                    }
                }
            }
        }
    }
}
#endif
