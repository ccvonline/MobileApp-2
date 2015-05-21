using System;
using System.Xml;
using Rock.Mobile.UI;
using System.Collections.Generic;
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
            /// A header describes a Title, Speaker and Date with a default layout that can 
            /// be overridden via NoteScript or Style.
            /// </summary>
            public class Header : BaseControl
            {
                /// <summary>
                /// The Title control for the header.
                /// </summary>
                protected PlatformLabel mTitle;

                /// <summary>
                /// The Date control for the header.
                /// </summary>
                protected const float DEFAULT_DATE_Y_OFFSET = .15f;
                protected PlatformLabel mDate;

                /// <summary>
                /// The speaker control for the header
                /// </summary>
                protected const float DEFAULT_SPEAKER_Y_OFFSET = .15f;
                protected PlatformLabel mSpeaker;

                /// <summary>
                /// The bounds (including position) of the header.
                /// </summary>
                /// <value>The frame.</value>
                protected RectangleF Frame { get; set; }

                /// <summary>
                /// The view representing any surrounding border for the quote.
                /// </summary>
                /// <value>The border view.</value>
                protected PlatformView BorderView { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    mTitle = null;
                    mDate = null;
                    mSpeaker = null;

                    BorderView = PlatformView.Create( );
                }

                public Header( CreateParams parentParams, XmlReader reader )
                {
                    Initialize( );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mHeaderContainer );

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


                    bool finishedHeader = false;
                    while( finishedHeader == false && reader.Read( ) )
                    {
                        // look for the next tag type
                        switch( reader.NodeType )
                        {
                            // we expect elements
                            case XmlNodeType.Element:
                            {
                                // determine which element it is and setup appropriately
                                switch( reader.Name )
                                {
                                    case "Title":
                                    {   
                                        // check for attributes we support
                                        RectangleF elementBounds = new RectangleF( 0, 0, availableWidth, parentParams.Height );
                                        Parser.ParseBounds( reader, ref parentSize, ref elementBounds );

                                        ParseHeaderElement( reader, availableWidth, parentParams.Height, mStyle.mBackgroundColor, out mTitle, ref elementBounds, ref ControlStyles.mHeaderTitle );
                                        break;
                                    }

                                    case "Date":
                                    {
                                        // check for attributes we support
                                        RectangleF elementBounds = new RectangleF( 0, 0, availableWidth, parentParams.Height );
                                        Parser.ParseBounds( reader, ref parentSize, ref elementBounds );

                                        ParseHeaderElement( reader, availableWidth, parentParams.Height, mStyle.mBackgroundColor, out mDate, ref elementBounds, ref ControlStyles.mHeaderDate );
                                        break;
                                    }

                                    case "Speaker":
                                    {
                                        // check for attributes we support
                                        RectangleF elementBounds = new RectangleF( 0, 0, availableWidth, parentParams.Height );
                                        Parser.ParseBounds( reader, ref parentSize, ref elementBounds );

                                        ParseHeaderElement( reader, availableWidth, parentParams.Height, mStyle.mBackgroundColor, out mSpeaker, ref elementBounds, ref ControlStyles.mHeaderSpeaker );
                                        break;
                                    }
                                }
                                break;
                            }

                            case XmlNodeType.EndElement:
                            {
                                //if( reader.Name == "Header" || reader.Name == "H" )
                                if( ElementTagMatches( reader.Name ) )
                                {
                                    // flag that we're done reading the header
                                    finishedHeader = true;
                                }
                                break;
                            }
                        }
                    }

                    // offset the controls according to our layout
                    mTitle.Position = new PointF( mTitle.Position.X + bounds.X + padding.Left + borderPaddingPx, 
                                                  mTitle.Position.Y + bounds.Y + padding.Top + borderPaddingPx );

                    // guarantee date and speaker are below title.
                    mDate.Position = new PointF( mDate.Position.X + bounds.X + padding.Left + borderPaddingPx, 
                                                 mTitle.Frame.Bottom + mDate.Position.Y + bounds.Y + padding.Top );

                    mSpeaker.Position = new PointF( mSpeaker.Position.X + bounds.X + padding.Left + borderPaddingPx, 
                                                    mTitle.Frame.Bottom + mSpeaker.Position.Y + bounds.Y + padding.Top );


                    // verify that the speaker won't overlap date. if it will, left justify them under each other beneath the title.
                    if ( mSpeaker.Position.X < mDate.Frame.Right )
                    {
                        mDate.Position = new PointF( mTitle.Position.X, mTitle.Frame.Bottom );
                        
                        mSpeaker.Position = new PointF( mTitle.Position.X, mDate.Frame.Bottom );
                    }

                    // determine the lowest control
                    float bottomY = mSpeaker.Frame.Bottom > mTitle.Frame.Bottom ? mSpeaker.Frame.Bottom : mTitle.Frame.Bottom;
                    bottomY = bottomY > mDate.Frame.Bottom ? bottomY : mDate.Frame.Bottom;

                    // set our bounds
                    Frame = new RectangleF( bounds.X, bounds.Y, bounds.Width, (bottomY + padding.Height + borderPaddingPx) - bounds.Y);

                    BorderView.Frame = Frame;

                    SetDebugFrame( Frame );
                }

                void ParseHeaderElement( XmlReader reader, float parentWidth, float parentHeight, uint? parentBGColor, out PlatformLabel element, ref RectangleF elementBounds, ref Styles.Style defaultStyle )
                {
                    element = PlatformLabel.Create( );

                    // header elements are weird with styles. We don't want any of our parent's styles,
                    // so we create our own and mix that with our defaults
                    Styles.Style elementStyle = new Styles.Style( );
                    elementStyle.Initialize( );
                    elementStyle.mBackgroundColor = parentBGColor.HasValue ? parentBGColor.Value : 0; //one exception is background color. We do want to inherit that.
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref elementStyle, ref defaultStyle );

                    // Note: Margins and padding are not supported by the individual elements of the header.

                    element.SetFont( elementStyle.mFont.mName, elementStyle.mFont.mSize.Value );
                    element.TextColor = elementStyle.mFont.mColor.Value;

                    if( elementStyle.mBackgroundColor.HasValue )
                    {
                        element.BackgroundColor = elementStyle.mBackgroundColor.Value;
                    }

                    element.Bounds = elementBounds;

                    // get text
                    switch( elementStyle.mTextCase )
                    {
                        case Styles.TextCase.Upper:
                        {
                            element.Text = reader.ReadElementContentAsString( ).ToUpper( );
                            break;
                        }

                        case Styles.TextCase.Lower:
                        {
                            element.Text = reader.ReadElementContentAsString( ).ToLower( );
                            break;
                        }

                        case Styles.TextCase.Normal:
                        {
                            element.Text = reader.ReadElementContentAsString( );
                            break;
                        }
                    }
                    element.SizeToFit( );


                    // horizontally position the controls according to their 
                    // requested alignment
                    Styles.Alignment controlAlignment = elementStyle.mAlignment.Value;

                    // adjust by our position
                    float xAdjust = 0;
                    switch( controlAlignment )
                    {
                        case Styles.Alignment.Center:
                        {
                            xAdjust = elementBounds.X + ( ( parentWidth / 2 ) - ( element.Bounds.Width / 2 ) );
                            element.TextAlignment = TextAlignment.Center;
                            break;
                        }
                        case Styles.Alignment.Right:
                        {
                            xAdjust = elementBounds.X + ( parentWidth - element.Bounds.Width );
                            element.TextAlignment = TextAlignment.Right;
                            break;
                        }
                        case Styles.Alignment.Left:
                        {
                            xAdjust = elementBounds.X;
                            element.TextAlignment = TextAlignment.Left;
                            break;
                        }
                    }

                    // adjust position
                    element.Position = new PointF( elementBounds.X + xAdjust, elementBounds.Y );
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    base.AddOffset( xOffset, yOffset );

                    mTitle.Position = new PointF( mTitle.Position.X + xOffset, 
                                                  mTitle.Position.Y + yOffset );

                    mDate.Position = new PointF( mDate.Position.X + xOffset, 
                                                 mDate.Position.Y + yOffset );

                    mSpeaker.Position = new PointF( mSpeaker.Position.X + xOffset, 
                                                    mSpeaker.Position.Y + yOffset );

                    BorderView.Position = new PointF( BorderView.Position.X + xOffset,
                                                      BorderView.Position.Y + yOffset );

                    // update our bounds by the new offsets.
                    Frame = new RectangleF( Frame.X + xOffset, Frame.Y + yOffset, Frame.Width, Frame.Height );
                }

                public override void AddToView( object obj )
                {
                    BorderView.AddAsSubview( obj );

                    mTitle.AddAsSubview( obj );
                    mDate.AddAsSubview( obj );
                    mSpeaker.AddAsSubview( obj );

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    BorderView.RemoveAsSubview( obj );

                    mTitle.RemoveAsSubview( obj );
                    mDate.RemoveAsSubview( obj );
                    mSpeaker.RemoveAsSubview( obj );

                    TryRemoveDebugLayer( obj );
                }

                public override RectangleF GetFrame( )
                {
                    return Frame;
                }

                public override void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                    htmlStream += "<h1>" + mTitle.Text + "</h1>" + 
                                  "<h3>" + mSpeaker.Text + "&nbsp;&nbsp;&nbsp;" + mDate.Text + "</h3>";


                    // handle user notes
                    EmbedIntersectingUserNotes( ref htmlStream, userNotes );
                }

                public static bool ElementTagMatches(string elementTag)
                {
                    if ( elementTag == "H" || elementTag == "Header" )
                    {
                        return true;
                    }
                    return false;
                }

                public override bool ShouldShowBulletPoint( )
                {
                    // as a container, it wouldn't really make sense to show a bullet point.
                    return false;
                }
            }
        }
    }
}
