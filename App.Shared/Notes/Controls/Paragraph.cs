using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

using App.Shared.Notes.Styles;
using Rock.Mobile.UI;
using App.Shared.Config;
using System.Drawing;
using App.Shared.PrivateConfig;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Container for Text, RevealText and InputText. Manages wrapping 
            /// and alignment of children.
            /// </summary>
            public class Paragraph : BaseControl
            {
                /// <summary>
                /// Text, RevealText and InputText children.
                /// </summary>
                /// <value>The child controls.</value>
                protected List<IUIControl> ChildControls { get; set; }

                /// <summary>
                /// The alignment that children should have within the paragraph container.
                /// Example: The Paragraph container might be centered, but ChildControls can be LEFT
                /// aligned within the container.
                /// </summary>
                /// <value>The child horz alignment.</value>
                protected Alignment ChildHorzAlignment { get; set; }

                /// <summary>
                /// The view representing any surrounding border for the quote.
                /// </summary>
                /// <value>The border view.</value>
                protected PlatformView BorderView { get; set; }

                /// <summary>
                /// The actual bounds (including position) of the paragraph.
                /// </summary>
                /// <value>The bounds.</value>
                protected RectangleF Frame { get; set; }

                /// <summary>
                /// Url in case this label should send the user to a website
                /// </summary>
                /// <value>The active URL.</value>
                protected string ActiveUrl { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    ChildControls = new List<IUIControl>( );

                    ChildHorzAlignment = Alignment.Inherit;

                    BorderView = PlatformView.Create( );
                }

                public Paragraph( CreateParams parentParams, XmlReader reader )
                {
                    Initialize( );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mParagraph );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    SizeF parentSize = new SizeF( parentParams.Width, parentParams.Height );
                    ParseCommonAttribs( reader, ref parentSize, ref bounds );

                    // Get margins and padding
                    RectangleF padding;
                    RectangleF margin;
                    GetMarginsAndPadding( ref mStyle, ref parentSize, ref bounds, out margin, out padding );

                    // apply margins to as much of the bounds as we can (bottom must be done by our parent container)
                    ApplyImmediateMargins( ref bounds, ref margin, ref parentSize );
                    Margin = margin;

                    // check for border styling
                    int borderPaddingPx = 0;
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        BorderView.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        BorderView.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        BorderView.BorderWidth = mStyle.mBorderWidth.Value;
                        borderPaddingPx = (int)Rock.Mobile.Graphics.Util.UnitToPx( mStyle.mBorderWidth.Value + PrivateNoteConfig.BorderPadding );
                    }

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        BorderView.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }
                    //

                    // now calculate the available width based on padding. (Don't actually change our width)
                    float availableWidth = bounds.Width - padding.Left - padding.Width - (borderPaddingPx * 2);

                    // see if there's a URL we should care about
                    ActiveUrl = reader.GetAttribute( "Url" );


                    // now read what our children's alignment should be
                    // check for alignment
                    string result = reader.GetAttribute( "ChildAlignment" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        switch( result )
                        {
                            case "Left":
                            {
                                ChildHorzAlignment = Alignment.Left;
                                break;
                            }
                            case "Right":
                            {
                                ChildHorzAlignment = Alignment.Right;
                                break;
                            }
                            case "Center":
                            {
                                ChildHorzAlignment = Alignment.Center;
                                break;
                            }
                            default:
                            {
                                ChildHorzAlignment = mStyle.mAlignment.Value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // if it wasn't specified, use LEFT alignment.
                        ChildHorzAlignment = Alignment.Left;
                    }

                    bool removedLeadingWhitespace = false;
                    bool lastControlWasReveal = false;

                    bool finishedReading = false;
                    while( finishedReading == false && reader.Read( ) )
                    {
                        switch( reader.NodeType )
                        {
                            case XmlNodeType.Element:
                            {
                                IUIControl control = Parser.TryParseControl( new CreateParams( this, availableWidth, parentParams.Height, ref mStyle ), reader );
                                if( control != null )
                                {
                                    // if the last control was a reveal, then we have two in a row. So place a space between them!
                                    if ( lastControlWasReveal )
                                    {
                                        NoteText textLabel = new NoteText( new CreateParams( this, availableWidth, parentParams.Height, ref mStyle ), " " );
                                        ChildControls.Add( textLabel );
                                    }

                                    // only allow RevealBoxes as children.
                                    if( control as RevealBox == null )
                                    {
                                        throw new Exception( String.Format("Paragraph only supports children of type <RevealBox>. Found <{0}>", control.GetType()) );
                                    }
                                    ChildControls.Add( control );

                                    // flag that whitespace is removed, because either
                                    // this was the first control and we didn't want to, or
                                    // it was removed by the first text we created.
                                    removedLeadingWhitespace = true;

                                    // flag that the last control placed was a reveal, so that
                                    // should we come across another one immediately, we know to insert a space
                                    // so they don't render concatenated.
                                    lastControlWasReveal = true;
                                }
                                break;
                            }

                            case XmlNodeType.Text:
                            {
                                // give the text a style that doesn't include things it shouldn't inherit
                                Styles.Style textStyle = mStyle;
                                textStyle.mBorderColor = null;
                                textStyle.mBorderRadius = null;
                                textStyle.mBorderWidth = null;

                                // grab the text. remove any weird characters
                                string text = Regex.Replace( reader.Value, @"\t|\n|\r", "" );

                                if( removedLeadingWhitespace == false )
                                {
                                    removedLeadingWhitespace = true;
                                    text = text.TrimStart( ' ' );
                                }

                                // now break it into words so we can do word wrapping
                                string[] words = text.Split( ' ' );
                                foreach( string word in words )
                                {
                                    // create labels out of each one
                                    if( string.IsNullOrEmpty( word ) == false )
                                    {
                                        // if the last thing we added was a special control like a reveal box, we 
                                        // need the first label after that to have a leading space so it doesn't bunch up against
                                        // the control
                                        string nextWord = word;
                                        if( lastControlWasReveal )
                                        {
                                            nextWord = word.Insert(0, " ");
                                            lastControlWasReveal = false;
                                        }

                                        NoteText wordLabel = new NoteText( new CreateParams( this, availableWidth, parentParams.Height, ref textStyle ), nextWord + " " );

                                        ChildControls.Add( wordLabel );
                                    }
                                }

                                lastControlWasReveal = false;


                                // Note - Treating the entire block of text as a single NoteText has an advantage and disadvantage.
                                // The advantage is it's extremely fast. It causes note creation to go from 1500ms to about 800ms in debug.
                                // The disadvantage is we can't have quite as precise word wrapping. So, if the ENTIRE block of text doesn't fit on the line,
                                // it will be forced to the line below. This is extremely rare tho, and I've never seen it in normal notes.
                                // If it does become an issue, we can revert to the slower code below.
                                //NoteText textLabel = new NoteText( new CreateParams( this, availableWidth, parentParams.Height, ref textStyle ), text );
                                //ChildControls.Add( textLabel );

                                break;
                            }

                            case XmlNodeType.EndElement:
                            {
                                // if we hit the end of our label, we're done.
                                //if( reader.Name == "Paragraph" || reader.Name == "P" )
                                if( ElementTagMatches( reader.Name ) )
                                {
                                    finishedReading = true;
                                }
                                break;
                            }
                        }
                    }


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

                    foreach( IUIControl control in ChildControls )
                    {
                        RectangleF controlFrame = control.GetFrame( );

                        // if there is NOT enough room on this row for the next control
                        if( rowRemainingWidth < controlFrame.Width )
                        {
                            // since we're advancing to the next row, trim leading white space, which, if we weren't wrapping,
                            // would be a space between words.
                            // note: we can safely cast to a NoteText because that's the only child type we allow.
                            string text = ( (NoteText)control ).GetText( ).TrimStart( ' ' );
                            ( (NoteText)control ).SetText( text );

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


                    // Build our final frame that determines our dimensions
                    RectangleF frame = new RectangleF( 65000, 65000, -65000, -65000 );

                    // for each child control
                    foreach( IUIControl control in ChildControls )
                    {
                        // enlarge our frame by the current frame and the next child
                        frame = Parser.CalcBoundingFrame( frame, control.GetFrame( ) );
                    }

                    frame.Y = bounds.Y;
                    frame.X = bounds.X;
                    frame.Height += padding.Height + padding.Top + (borderPaddingPx * 2); //add in padding
                    frame.Width = bounds.Width;



                    // setup our bounding rect for the border
                    frame = new RectangleF( frame.X, 
                                            frame.Y,
                                            frame.Width, 
                                            frame.Height );

                    // and store that as our bounds
                    BorderView.Frame = frame;

                    Frame = frame;
                    SetDebugFrame( Frame );

                    // sort everything
                    ChildControls.Sort( BaseControl.Sort );
                }

                void AlignRow( RectangleF bounds, List<IUIControl> currentRow, float maxWidth )
                {
                    // Determine the row's width and height (Height is defined as the tallest control on this line)
                    float rowHeight = 0;
                    float rowWidth = 0;

                    foreach( IUIControl rowControl in currentRow )
                    {
                        RectangleF controlFrame = rowControl.GetFrame( );

                        rowWidth += controlFrame.Width;
                        rowHeight = rowHeight > controlFrame.Height ? rowHeight : controlFrame.Height;
                    }

                    // the amount each control in the row should adjust is the 
                    // difference of paragraph width (which is defined by the max row width)
                    // and this row's width.
                    float xRowAdjust = 0;
                    switch( ChildHorzAlignment )
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
                    foreach( IUIControl rowControl in currentRow )
                    {
                        // vertically center all items within the row.
                        float yAdjust = rowHeight / 2 - ( rowControl.GetFrame( ).Height / 2 );

                        // set their correct X offset
                        rowControl.AddOffset( xRowAdjust, yAdjust );
                    }
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    base.AddOffset( xOffset, yOffset );

                    // position each interactive label relative to ourselves
                    foreach( IUIControl control in ChildControls )
                    {
                        control.AddOffset( xOffset, yOffset );
                    }

                    BorderView.Position = new PointF( BorderView.Position.X + xOffset,
                                                      BorderView.Position.Y + yOffset );

                    // update our bounds by the new offsets.
                    Frame = new RectangleF( Frame.X + xOffset, Frame.Y + yOffset, Frame.Width, Frame.Height );
                }

                public override IUIControl TouchesEnded( PointF touch )
                {
                    // let each child handle it
                    foreach( IUIControl control in ChildControls )
                    {
                        // if a child consumes it, stop and report it was consumed.
                        IUIControl consumingControl = control.TouchesEnded( touch );
                        if( consumingControl != null)
                        {
                            return consumingControl;
                        }
                    }

                    // if our controls above didn't care, we should consume IF we 
                    // have a url to launch
                    if ( string.IsNullOrEmpty( ActiveUrl ) == false && Frame.Contains( touch ) )
                    {
                        return this;
                    }

                    return null;
                }

                public override string GetActiveUrl()
                {
                    return ActiveUrl;
                }

                public override void AddToView( object obj )
                {
                    BorderView.AddAsSubview( obj );

                    // let each child do the same thing
                    foreach( IUIControl control in ChildControls )
                    {
                        control.AddToView( obj );
                    }

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    BorderView.RemoveAsSubview( obj );

                    // let each child do the same thing
                    foreach( IUIControl control in ChildControls )
                    {
                        control.RemoveFromView( obj );
                    }

                    TryRemoveDebugLayer( obj );
                }

                public override void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                    foreach( IUIControl control in ChildControls )
                    {
                        control.BuildHTMLContent( ref htmlStream, userNotes );
                    }

                    // handle user notes
                    EmbedIntersectingUserNotes( ref htmlStream, userNotes );
                }

                public override RectangleF GetFrame( )
                {
                    return Frame;
                }

                public static bool ElementTagMatches(string elementTag)
                {
                    if ( elementTag == "P" || elementTag == "Paragraph" )
                    {
                        return true;
                    }
                    return false;
                }

                protected override List<IUIControl> GetChildControls( )
                {
                    return ChildControls;
                }
            }
        }
    }
}
