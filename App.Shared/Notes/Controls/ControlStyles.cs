using System;
using System.Xml;
using System.IO;
using Rock.Mobile.Network;
using RestSharp;
using System.Text;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            namespace Styles
            {
                /// <summary>
                /// Alignment options for controls within parents.
                /// </summary>
                public enum Alignment
                {
                    Inherit,
                    Left,
                    Right,
                    Center
                }

                /// <summary>
                /// Casing options for text within controls.
                /// </summary>
                public enum TextCase
                {
                    Normal,
                    Upper,
                    Lower
                }

                /// <summary>
                /// Defines the font a control's text should use.
                /// Anything null uses values from the control's default style.
                /// </summary>
                public struct FontParams
                {
                    public string mName;
                    public float? mSize;
                    public uint? mColor;

                    public FontParams( string name, float size, uint color )
                    {
                        mName = name;
                        mSize = size;
                        mColor = color;
                    }
                }

                /// <summary>
                /// Defines common styling values for controls.
                /// </summary>
                public struct Style
                {
                    /// <summary>
                    /// The background color for a control. (Not used by all controls)
                    /// </summary>
                    public uint? mBackgroundColor;

                    /// <summary>
                    /// The alignment of the control within its parent.
                    /// </summary>
                    public Alignment? mAlignment;

                    /// <summary>
                    /// The font the control's text should use, as well as what will be passed to children.
                    /// </summary>
                    public FontParams mFont;

                    /// <summary>
                    /// Amount of padding between control's content and left edge.
                    /// </summary>
                    public float? mPaddingLeft;

                    /// <summary>
                    /// Amount of padding between control's content and top edge.
                    /// </summary>
                    public float? mPaddingTop;

                    /// <summary>
                    /// Amount of padding between control's content and right edge.
                    /// </summary>
                    public float? mPaddingRight;

                    /// <summary>
                    /// Amount of padding between control's content and bottom edge.
                    /// </summary>
                    public float? mPaddingBottom;

                    /// <summary>
                    /// Amount of margin space along the left edge of the control.
                    /// </summary>
                    public float? mMarginLeft;

                    /// <summary>
                    /// Amount of margin space along the top edge of the control.
                    /// </summary>
                    public float? mMarginTop;

                    /// <summary>
                    /// Amount of margin space along the right edge of the control.
                    /// </summary>
                    public float? mMarginRight;

                    /// <summary>
                    /// Amount of margin space along the bottom edge of the control.
                    /// </summary>
                    public float? mMarginBottom;

                    /// <summary>
                    /// The character to use for list bullet points
                    /// </summary>
                    public string mListBullet;

                    /// <summary>
                    /// The indention amount for lists
                    /// </summary>
                    public float? mListIndention;

                    /// <summary>
                    /// The casing for the text within a control. As is, Upper or Lower.
                    /// </summary>
                    public TextCase? mTextCase;

                    /// <summary>
                    /// The thickness of any border desired around the control
                    /// </summary>
                    public float? mBorderWidth;

                    /// <summary>
                    /// The color of the border
                    /// </summary>
                    public uint? mBorderColor;

                    /// <summary>
                    /// The amount of curvature for the border corners
                    /// </summary>
                    public float? mBorderRadius;

                    /// <summary>
                    /// Used only by the main Note element. If true, causes the
                    /// note container to span the entire width of the screen,
                    /// rather than be affected by the note's padding.
                    /// </summary>
                    public bool? mFullWidthHeader;

                    public void Initialize( )
                    {
                        mFont = new FontParams( );
                    }

                    // Utility function for retrieving a value without knowing if it's percent or not.
                    public static float GetValueForNullable( float? value, float valueForPerc, float nullDefault )
                    {
                        // set the default value we'll return if value is null.
                        float styleValue = nullDefault;

                        if( value.HasValue )
                        {
                            float realValue = value.Value;

                            // If it's a percent
                            if( realValue <= 1 )
                            {
                                // convert using valueForPerc as the source
                                styleValue = valueForPerc * realValue;
                            }
                            else
                            {
                                // otherwise take the value
                                styleValue = realValue;
                            }
                        }

                        //return Rock.Mobile.Graphics.Util.UnitToPx( styleValue );

                        return styleValue;
                    }

                    public static UInt32 ParseColor( string color )
                    {
                        if( color[0] != '#' ) 
                        {
                            throw new Exception( String.Format( "Colors must be in the format #RRGGBBAA. Color found: {0}", color ) );
                        }
                        return Convert.ToUInt32( color.Substring( 1 ), 16 ); //skip the first character
                    }

                    // Utility function for parsing common style attributes
                    public static void ParseStyleAttributesWithDefaults( XmlReader reader, ref Style style, ref Style defaultStyle )
                    {
                        // This builds a style with the following conditions
                        // Values from XML come first. This means the control specifically asked for this.
                        // Values already set come second. This means the parent specifically asked for this.
                        // Values from defaultStyle come last. This means no one set the style, so it should use the control default.

                        // first use the normal parsing, which will result in a style with potential null values.
                        ParseStyleAttributes( reader, ref style );

                        // Lastly, merge defaultStyle values for anything null in style 
                        MergeStyleAttributesWithDefaults( ref style, ref defaultStyle );
                    }

                    public static void ParseStyleAttributes( XmlReader reader, ref Style style )
                    {
                        // This builds a style with the following conditions
                        // Values from XML come first. This means the control specifically asked for this.
                        // Values already set come second. This means the parent specifically asked for this.
                        // Padding is an exception AND DOES NOT INHERIT
                        // Unlike the WithDefaults version, this will allow style values to remain null. Important
                        // for container controls that don't want to force styles.
                        string result = reader.GetAttribute( "FontName" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mFont.mName = result;
                        }

                        result = reader.GetAttribute( "FontSize" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mFont.mSize = float.Parse( result );
                        }

                        result = reader.GetAttribute( "FontColor" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mFont.mColor = ParseColor( result );
                        }

                        // check for alignment
                        result = reader.GetAttribute( "Alignment" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            switch( result )
                            {
                                case "Left": style.mAlignment = Styles.Alignment.Left; break;
                                case "Right": style.mAlignment = Styles.Alignment.Right; break;
                                case "Center": style.mAlignment = Styles.Alignment.Center; break;

                                default: throw new Exception( "Unknown alignment type specified." );
                            }
                        }

                        // check text casing
                        result = reader.GetAttribute( "TextCase" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            switch( result )
                            {
                                case "Normal": style.mTextCase = Styles.TextCase.Normal; break;
                                case "Upper": style.mTextCase = Styles.TextCase.Upper; break;
                                case "Lower": style.mTextCase = Styles.TextCase.Lower; break;

                                default: throw new Exception( "Unknown text case specified." );
                            }
                        }

                        // check for background color: DOES NOT INHERIT
                        result = reader.GetAttribute( "BackgroundColor" );
                        if ( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mBackgroundColor = ParseColor( result );
                        }
                        else
                        {
                            style.mBackgroundColor = null;
                        }

                        // check for full width header (note only)
                        result = reader.GetAttribute( "FullWidthHeader" );
                        if ( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mFullWidthHeader = bool.Parse( result );
                        }

                        // check for list bullet point (lists only)
                        result = reader.GetAttribute( "BulletPoint" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mListBullet = result;
                        }

                        // check for list indentation (lists only)
                        result = reader.GetAttribute( "Indentation" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            float denominator = 1.0f;
                            if( result.Contains( "%" ) )
                            {
                                result = result.Trim( '%' );
                                denominator = 100.0f;
                            }

                            style.mListIndention = float.Parse( result ) / denominator;
                        }

                        // Check for borders: DOES NOT INHERIT
                        result = reader.GetAttribute( "BorderColor" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mBorderColor = ParseColor( result );
                        }
                        else
                        {
                            style.mBorderColor = null;
                        }

                        // check for border width
                        result = reader.GetAttribute( "BorderWidth" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mBorderWidth = float.Parse( result );
                        }
                        else
                        {
                            style.mBorderWidth = null;
                        }

                        // check for border radius
                        result = reader.GetAttribute( "BorderRadius" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mBorderRadius = float.Parse( result );
                        }
                        else
                        {
                            // the control didn't request a value, so set null which will prevent
                            // inheritence and get the default value here.
                            style.mBorderRadius = null;
                        }

                        // check for padding; DOES NOT INHERIT
                        result = reader.GetAttribute( "Padding" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            // set it for all padding values
                            style.mPaddingLeft = Parser.ParsePositioningValue( result );
                            style.mPaddingTop = style.mPaddingLeft;
                            style.mPaddingRight = style.mPaddingLeft;
                            style.mPaddingBottom = style.mPaddingLeft;
                        }
                        else
                        {
                            // for anything NullOrEmpty, it means the control didn't request a value, 
                            // so set null which will prevent inheritence and get the default value here.

                            result = reader.GetAttribute( "PaddingLeft" );
                            if( string.IsNullOrEmpty( result ) == false )
                            {
                                style.mPaddingLeft = Parser.ParsePositioningValue( result );
                            }
                            else
                            {
                                style.mPaddingLeft = null;
                            }

                            result = reader.GetAttribute( "PaddingTop" );
                            if( string.IsNullOrEmpty( result ) == false )
                            {
                                style.mPaddingTop = Parser.ParsePositioningValue( result );
                            }
                            else
                            {
                                style.mPaddingTop = null;
                            }

                            result = reader.GetAttribute( "PaddingRight" );
                            if( string.IsNullOrEmpty( result ) == false )
                            {
                                style.mPaddingRight = Parser.ParsePositioningValue( result );
                            }
                            else
                            {
                                style.mPaddingRight = null;
                            }

                            result = reader.GetAttribute( "PaddingBottom" );
                            if( string.IsNullOrEmpty( result ) == false )
                            {
                                style.mPaddingBottom = Parser.ParsePositioningValue( result );
                            }
                            else
                            {
                                style.mPaddingBottom = null;
                            }
                        }

                        // check for margin; DOES NOT INHERIT
                        result = reader.GetAttribute( "Margin" );
                        if( string.IsNullOrEmpty( result ) == false )
                        {
                            style.mMarginLeft = Parser.ParsePositioningValue( result );
                            style.mMarginTop = style.mMarginLeft;
                            style.mMarginRight = style.mMarginLeft;
                            style.mMarginBottom = style.mMarginLeft;
                        }
                        else
                        {
                            // for anything NullOrEmpty, it means the control didn't request a value, 
                            // so set null which will prevent inheritence and get the default value here.

                            result = reader.GetAttribute( "MarginLeft" );
                            if( string.IsNullOrEmpty( result ) == false )
                            {
                                style.mMarginLeft = Parser.ParsePositioningValue( result );
                            }
                            else
                            {
                                style.mMarginLeft = null;
                            }

                            result = reader.GetAttribute( "MarginTop" );
                            if( string.IsNullOrEmpty( result ) == false )
                            {
                                style.mMarginTop = Parser.ParsePositioningValue( result );
                            }
                            else
                            {
                                style.mMarginTop = null;
                            }

                            result = reader.GetAttribute( "MarginRight" );
                            if( string.IsNullOrEmpty( result ) == false )
                            {
                                style.mMarginRight = Parser.ParsePositioningValue( result );
                            }
                            else
                            {
                                style.mMarginRight = null;
                            }

                            result = reader.GetAttribute( "MarginBottom" );
                            if( string.IsNullOrEmpty( result ) == false )
                            {
                                style.mMarginBottom = Parser.ParsePositioningValue( result );
                            }
                            else
                            {
                                style.mMarginBottom = null;
                            }
                        }
                    }

                    public static void MergeStyleAttributesWithDefaults( ref Style style, ref Style defaultStyle )
                    {
                        // validate everything, and for anything null, use the default provided.
                        if( style.mFont.mName == null )
                        {
                            style.mFont.mName = defaultStyle.mFont.mName;
                        }

                        if( style.mFont.mSize.HasValue == false )
                        {
                            style.mFont.mSize = defaultStyle.mFont.mSize;
                        }

                        if( style.mFont.mColor.HasValue == false )
                        {
                            style.mFont.mColor = defaultStyle.mFont.mColor;
                        }

                        // check for alignment
                        if( style.mAlignment.HasValue == false )
                        {
                            style.mAlignment = defaultStyle.mAlignment;
                        }

                        // check for text case
                        if ( style.mTextCase.HasValue == false )
                        {
                            style.mTextCase = defaultStyle.mTextCase;
                        }

                        // check for note specific
                        if ( style.mFullWidthHeader == null )
                        {
                            style.mFullWidthHeader = defaultStyle.mFullWidthHeader;
                        }

                        // check for list specifics
                        if ( style.mListBullet == null )
                        {
                            style.mListBullet = defaultStyle.mListBullet;
                        }

                        if ( style.mListIndention.HasValue == false )
                        {
                            style.mListIndention = defaultStyle.mListIndention;
                        }

                        // check for border values
                        if( style.mBorderColor.HasValue == false )
                        {
                            style.mBorderColor = defaultStyle.mBorderColor;
                        }

                        if ( style.mBorderWidth.HasValue == false )
                        {
                            style.mBorderWidth = defaultStyle.mBorderWidth;
                        }

                        if( style.mBorderRadius.HasValue == false )
                        {
                            style.mBorderRadius = defaultStyle.mBorderRadius;
                        }

                        // check for padding
                        if( style.mPaddingLeft.HasValue == false )
                        {
                            style.mPaddingLeft = defaultStyle.mPaddingLeft;
                        }

                        if( style.mPaddingTop.HasValue == false )
                        {
                            style.mPaddingTop = defaultStyle.mPaddingTop;
                        }

                        if( style.mPaddingRight.HasValue == false )
                        {
                            style.mPaddingRight = defaultStyle.mPaddingRight;
                        }

                        if( style.mPaddingBottom.HasValue == false )
                        {
                            style.mPaddingBottom = defaultStyle.mPaddingBottom;
                        }

                        // check for margin
                        if( style.mMarginLeft.HasValue == false )
                        {
                            style.mMarginLeft = defaultStyle.mMarginLeft;
                        }

                        if( style.mMarginTop.HasValue == false )
                        {
                            style.mMarginTop = defaultStyle.mMarginTop;
                        }

                        if( style.mMarginRight.HasValue == false )
                        {
                            style.mMarginRight = defaultStyle.mMarginRight;
                        }

                        if( style.mMarginBottom.HasValue == false )
                        {
                            style.mMarginBottom = defaultStyle.mMarginBottom;
                        }

                        // check for background color
                        if( style.mBackgroundColor.HasValue == false )
                        {
                            style.mBackgroundColor = defaultStyle.mBackgroundColor;
                        }
                    }
                }
            }

            //This is a static class containing all the default styles for controls
            public class ControlStyles
            {
                // These are to be referenced globally as needed
                /// <summary>
                /// Note control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mMainNote;

                /// <summary>
                /// Paragraph control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mParagraph;

                /// <summary>
                /// Stack control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mStackPanel;

                /// <summary>
                /// Canvas' default style if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mCanvas;

                /// <summary>
                /// Revealbox's control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mRevealBox;

                /// <summary>
                /// TextInput control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mTextInput;

                /// <summary>
                /// Header Container's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mHeaderContainer;

                /// <summary>
                /// Header Title control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mHeaderTitle;

                /// <summary>
                /// Header Date control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mHeaderDate;

                /// <summary>
                /// Header Speaker control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mHeaderSpeaker;

                /// <summary>
                /// Quote control's default styles if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mQuote;

                /// <summary>
                /// Text's default styles if none are specified by a parent or in NoteScript XML.
                /// Note that text is not an actual control, rather it uses Labels.
                /// </summary>
                public static Styles.Style mText;

                /// <summary>
                /// List Item's default style if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mListItem;

                /// <summary>
                /// List's default style if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mList;

                /// <summary>
                /// User note appearance's default style if none are specified by a parent or in NoteScript XML.
                /// </summary>
                public static Styles.Style mUserNote;

                static HttpRequest WebRequest { get; set; }

                static void CreateHeaderStyle()
                {
                    WebRequest = new HttpRequest();

                    // Default the header container to no padding and left alignment.
                    mHeaderContainer = new Styles.Style( );
                    mHeaderContainer.Initialize();
                    mHeaderContainer.mAlignment = Styles.Alignment.Left;

                    // Title should be large, bevan, centered and white
                    // Also, prefer an upper case title.
                    mHeaderTitle = new Styles.Style( );
                    mHeaderTitle.Initialize();
                    mHeaderTitle.mAlignment = Styles.Alignment.Center;
                    mHeaderTitle.mTextCase = Styles.TextCase.Upper;
                    mHeaderTitle.mFont.mName = "Bevan";
                    mHeaderTitle.mFont.mSize = 18;
                    mHeaderTitle.mFont.mColor = 0xFFFFFFFF;

                    // Date should be medium, verdana, left and white
                    mHeaderDate = new Styles.Style( );
                    mHeaderDate.Initialize();
                    mHeaderDate.mAlignment = Styles.Alignment.Left;
                    mHeaderDate.mTextCase = Styles.TextCase.Normal;
                    mHeaderDate.mFont.mName = "Bevan";
                    mHeaderDate.mFont.mSize = 12;
                    mHeaderDate.mFont.mColor = 0xFFFFFFFF;

                    // Speaker should be mediu, verdana, right and white
                    mHeaderSpeaker = new Styles.Style( );
                    mHeaderSpeaker.Initialize();
                    mHeaderSpeaker.mAlignment = Styles.Alignment.Right;
                    mHeaderSpeaker.mTextCase = Styles.TextCase.Normal;
                    mHeaderSpeaker.mFont.mName = "Bevan";
                    mHeaderSpeaker.mFont.mSize = 12;
                    mHeaderSpeaker.mFont.mColor = 0xFFFFFFFF;
                }

                static void CreateMainNoteStyle()
                {
                    // note should have a black background and no alignment
                    mMainNote = new Styles.Style( );
                    mMainNote.Initialize();

                    mMainNote.mBackgroundColor = 0x000000FF;
                    mMainNote.mFullWidthHeader = false;
                }

                static void CreateParagraphStyle()
                {
                    // paragraphs should have a preference for left alignment
                    mParagraph = new Styles.Style( );
                    mParagraph.Initialize();

                    mParagraph.mAlignment = Styles.Alignment.Left;
                }

                static void CreateStackPanelStyle()
                {
                    // like paragraphs, stack panels shouldn't care about anything but alignment
                    mStackPanel = new Styles.Style( );
                    mStackPanel.Initialize( );

                    mStackPanel.mAlignment = Styles.Alignment.Left;
                }

                static void CreateCanvasStyle()
                {
                    // like stacks and paragraphs, canvas shouldn't care about anything
                    // but alignment
                    mCanvas = new Styles.Style( );
                    mCanvas.Initialize( );

                    mCanvas.mAlignment = Styles.Alignment.Left;
                }

                static void CreateRevealBoxStyle()
                {
                    mRevealBox = new Styles.Style( );
                    mRevealBox.Initialize();

                    // make reveal boxes redish
                    mRevealBox.mAlignment = Styles.Alignment.Left;
                    mRevealBox.mFont.mName = "Bevan";
                    mRevealBox.mFont.mSize = 12;
                    mRevealBox.mFont.mColor = 0x6d1c1cFF;
                    mRevealBox.mTextCase = Styles.TextCase.Normal;
                }

                static void CreateTextInputStyle()
                {
                    mTextInput = new Styles.Style( );
                    mTextInput.Initialize();

                    // Make text input small by default
                    mTextInput.mAlignment = Styles.Alignment.Left;
                    mTextInput.mFont.mName = "Bevan";
                    mTextInput.mFont.mSize = 8;
                    mTextInput.mFont.mColor = 0xFFFFFFFF;
                    mTextInput.mTextCase = Styles.TextCase.Normal;

                    mTextInput.mBorderColor = 0x777777FF;
                    mTextInput.mBorderRadius = 5;
                    mTextInput.mBorderWidth = 4;
                }

                static void CreateQuoteStyle()
                {
                    mQuote = new Styles.Style( );
                    mQuote.Initialize();

                    // Make quotes redish
                    mQuote.mAlignment = Styles.Alignment.Left;
                    mQuote.mFont.mName = "Bevan";
                    mQuote.mFont.mSize = 12;
                    mQuote.mFont.mColor = 0x6d1c1cFF;
                    mQuote.mTextCase = Styles.TextCase.Normal;
                }

                static void CreateTextStyle()
                {
                    mText = new Styles.Style( );
                    mText.Initialize();

                    mText.mAlignment = Styles.Alignment.Left;
                    mText.mFont.mName = "Bevan";
                    mText.mFont.mSize = 12;
                    mText.mFont.mColor = 0xFFFFFFFF;
                    mText.mTextCase = Styles.TextCase.Normal;
                }

                static void CreateListStyle()
                {
                    mList = new Styles.Style( );
                    mList.Initialize( );

                    mList.mAlignment = Styles.Alignment.Left;
                    mList.mListBullet = "•"; //we want a nice looking bullet as default
                    mList.mListIndention = 25.0f;
                }

                static void CreateListItemStyle()
                {
                    mListItem = new Styles.Style( );
                    mListItem.Initialize( );
                }

                static void CreateUserNoteStyle()
                {
                    mUserNote = new Styles.Style( );
                    mUserNote.Initialize( );

                    mUserNote.mBorderColor = 0x777777FF;
                    mUserNote.mBorderRadius = 5;
                    mUserNote.mBorderWidth = 4;

                    mUserNote.mAlignment = Styles.Alignment.Left;
                    mUserNote.mFont.mName = "Bevan";
                    mUserNote.mFont.mSize = 8;
                    mUserNote.mFont.mColor = 0x777777FF;
                    mUserNote.mTextCase = Styles.TextCase.Normal;
                }

                public static void Initialize( string styleSheetXml )
                {
                    // Create each control's default style. And put some defaults...for the defaults.
                    //(Seriously, that way if it doesn't exist in XML we still have a value.)
                    CreateMainNoteStyle();
                    CreateParagraphStyle();
                    CreateStackPanelStyle();
                    CreateCanvasStyle();
                    CreateRevealBoxStyle();
                    CreateTextInputStyle();
                    CreateQuoteStyle();
                    CreateTextStyle();
                    CreateHeaderStyle();
                    CreateListItemStyle();
                    CreateListStyle();
                    CreateUserNoteStyle();

                    // otherwise we can directly parse the XML
                    ParseStyles( styleSheetXml );
                }

                protected static bool ParseStyles( string styleSheetXml )
                {
                    bool result = false;

                    // now use a reader to get each element
                    XmlTextReader reader = XmlReader.Create( new StringReader( styleSheetXml ) ) as XmlTextReader;
                    try
                    {
                        bool finishedParsing = false;

                        // look at each element, as they all define styles for our controls
                        while( finishedParsing == false && reader.Read( ) )
                        {
                            switch( reader.NodeType )
                            {
                            //Find the control elements
                                case XmlNodeType.Element:
                                {
                                    //most controls don't care about anything other than the basic attributes,
                                    // so we can use a common Parse function. Certain styles may need to define more specific things,
                                    // for which we can add special parse classes
                                    switch( reader.Name )
                                    {
                                        case "Note": Notes.Styles.Style.ParseStyleAttributes( reader, ref mMainNote ); break;
                                        case "Paragraph": Notes.Styles.Style.ParseStyleAttributes( reader, ref mParagraph ); break;
                                        case "RevealBox": Notes.Styles.Style.ParseStyleAttributes( reader, ref mRevealBox ); break;
                                        case "TextInput": Notes.Styles.Style.ParseStyleAttributes( reader, ref mTextInput ); break;
                                        case "Quote": Notes.Styles.Style.ParseStyleAttributes( reader, ref mQuote ); break;
                                        case "Header.Container": Notes.Styles.Style.ParseStyleAttributes( reader, ref mHeaderContainer ); break;
                                        case "Header.Speaker": Notes.Styles.Style.ParseStyleAttributes( reader, ref mHeaderSpeaker ); break;
                                        case "Header.Date": Notes.Styles.Style.ParseStyleAttributes( reader, ref mHeaderDate ); break;
                                        case "Header.Title": Notes.Styles.Style.ParseStyleAttributes( reader, ref mHeaderTitle ); break;
                                        case "Text": Notes.Styles.Style.ParseStyleAttributes( reader, ref mText ); break;
                                        case "List": Notes.Styles.Style.ParseStyleAttributes( reader, ref mList ); break;
                                        case "ListItem": Notes.Styles.Style.ParseStyleAttributes( reader, ref mListItem ); break;
                                        case "UserNote": Notes.Styles.Style.ParseStyleAttributes( reader, ref mUserNote ); break;
                                    }
                                    break;
                                }

                                case XmlNodeType.EndElement:
                                {
                                    if( reader.Name == "Styles" )
                                    {
                                        finishedParsing = true;
                                        result = true;
                                    }
                                    break;
                                }
                            }
                        }
                    } 
                    catch( Exception )
                    {
                    }

                    return result;
                }
            }
        }
    }
}
    