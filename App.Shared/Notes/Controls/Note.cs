using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

using MobileApp.Shared.Notes.Styles;
using MobileApp.Shared.Notes.Model;
using Newtonsoft.Json;
using Rock.Mobile.UI;
using System.Drawing;
using System.Text;
using MobileApp.Shared.Strings;
using Rock.Mobile.IO;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// The core object that all UI Controls are children of. Does not need to derive from BaseControl.
            /// Includes utility functions for creating a Note object and initializing the styles.
            /// </summary>
            public class Note
            {
                /// <summary>
                /// Delegate for notifying the caller when a note is ready to be created via Note.Create()
                /// </summary>
                public delegate void OnPreReqsComplete( Note note, Exception e );

                public delegate void MessageBoxResult( int result );
                public delegate void DisplayMessageBoxDelegate( string title, string message, MessageBoxResult onResult );

                /// <summary>
                /// A list of all immediate child controls of the Note. (This list is hierchical and not flat)
                /// </summary>
                /// <value>The child controls.</value>
                protected List<IUIControl> ChildControls { get; set; }

                /// <summary>
                /// A seperate list of User Notes.
                /// </summary>
                /// <value>The user note controls.</value>
                protected List<UserNote> UserNoteControls { get; set; }

                /// <summary>
                /// a reference to the Anchor being touched or moved. This
                /// does NOT point to a note being edited. That's different.
                /// </summary>
                /// <value>The active user note anchor.</value>
                protected UserNote ActiveUserNoteAnchor { get; set; }

                /// <summary>
                /// The NoteScript XML. Stored for rebuilding notes on an orientation change.
                /// </summary>
                /// <value>The note xml.</value>
                public string NoteXml { get; protected set; }

                /// <summary>
                /// The style settings for the Note. Will be passed to all children.
                /// </summary>
                protected Style mStyle;

                /// <summary>
                /// The bounds (including position) of the note.
                /// </summary>
                /// <value>The bounds.</value>
                protected RectangleF Frame { get; set; }

                /// <summary>
                /// The view that actually contains all controls
                /// </summary>
                /// <value>The master view.</value>
                protected object MasterView { get; set; }

                /// <summary>
                /// The path for loading/saving user notes
                /// </summary>
                /// <value>The user note path.</value>
                protected string UserNotePath { get; set; }

                /// <summary>
                /// The height of the device, used during user note creation.
                /// </summary>
                /// <value>The height of the device.</value>
                protected float DeviceHeight { get; set; }

                /// <summary>
                /// Store the padding used so that children can know their movement restrictions
                /// </summary>
                public RectangleF Padding { get; set; }
                
                protected DisplayMessageBoxDelegate RequestDisplayMessageBox { get; set; }

                /// <summary>
                /// Attempt to download the note and its style sheet. If they are already downloaded, the completion delegate will be called immediately.
                /// </summary>
                public delegate void OnTryDownloadComplete( bool result );
                public static void TryDownloadNote( string noteUrl, string styleSheetDefaultHostDomain, bool forceDownload, OnTryDownloadComplete complete )
                {
                    // see if the note is already downloaded
                    string noteFileName = Rock.Mobile.Util.Strings.Parsers.ParseURLToFileName( noteUrl );
                    MemoryStream noteData = (MemoryStream)FileCache.Instance.LoadFile( noteFileName );

                    // if not, download it, OR, if we're forcing a download (we do this at launch to ensure it's updated)
                    if ( noteData == null || forceDownload == true )
                    {
                        try
                        {
                            Rock.Mobile.Util.Debug.WriteLine( "NO CACHED NOTES. DOWNLOADING THEM." );

                            FileCache.Instance.DownloadFileToCache( noteUrl, noteFileName, null,
                                delegate
                                {
                                    try
                                    {
                                        // good, now get the style sheet
                                        noteData = (MemoryStream)FileCache.Instance.LoadFile( noteFileName );
                                        if ( noteData != null )
                                        {
                                            string body = Encoding.UTF8.GetString( noteData.ToArray( ), 0, (int)noteData.Length );
                                            noteData.Dispose( );

                                            string styleSheetUrl = Note.GetStyleSheetUrl( body, styleSheetDefaultHostDomain );
                                            string styleFileName = Rock.Mobile.Util.Strings.Parsers.ParseURLToFileName( styleSheetUrl );

                                            FileCache.Instance.DownloadFileToCache( styleSheetUrl, styleFileName, null,
                                                delegate
                                                {
                                                    // now we can create the notes
                                                    complete( true );
                                                } );
                                        }
                                        else
                                        {
                                            complete( false );
                                        }
                                    }
                                    catch( Exception )
                                    {
                                        complete( false );
                                    }
                                } );
                        }
                        catch ( Exception )
                        {
                            complete( false );
                        }
                    }
                    else
                    {
                        noteData.Dispose( );

                        // immediately let the caller know we're done
                        complete( true );
                    }
                }

                public static string GetStyleSheetUrl( string noteXml, string styleSheetDefaultHostDomain )
                {
                    // now use a reader to get each element
                    //XmlReader reader = XmlReader.Create( new StringReader( noteXml ) );
                    XmlTextReader reader = new XmlTextReader( new StringReader( noteXml ) );

                    string styleSheetUrl = "";

                    bool finishedReading = false;
                    while( finishedReading == false && reader.Read( ) )
                    {
                        // expect the first element to be "Note"
                        switch( reader.NodeType )
                        {
                            case XmlNodeType.Element:
                            {
                                if( reader.Name == "Note" )
                                {
                                    styleSheetUrl = reader.GetAttribute( "StyleSheet" );
                                    if( styleSheetUrl == null )
                                    {
                                        throw new Exception( "Could not find attribute 'StyleSheet'. This should be a URL pointing to the style to use." );
                                    }

                                    // if the style sheet URL is relative, add the default domain (which comes from the note DB) to make it absolute
                                    if ( styleSheetUrl.Contains( "http://" ) == false )
                                    {
                                        if ( string.IsNullOrEmpty( styleSheetDefaultHostDomain ) == true )
                                        {
                                            throw new Exception( "StyleSheet URL is relative, but no absolute domain was provided." );
                                        }

                                        styleSheetUrl = styleSheetUrl.Insert( 0, styleSheetDefaultHostDomain );
                                    }
                                }
                                else
                                {
                                    throw new Exception( string.Format( "Expected root element to be <Note>. Found <{0}>", reader.Name ) );
                                }

                                finishedReading = true;
                                break;
                            }
                        }
                    }

                    return styleSheetUrl;
                }

                public Note( string noteXml, string styleXml )
                {
                    // store our XML
                    NoteXml = noteXml;

                    mStyle = new Style( );
                    mStyle.Initialize( );

                    ControlStyles.Initialize( styleXml );
                }

                public delegate void NoteSizeChanging( );
                NoteSizeChanging OnNoteSizeChanging;

                public float Create( float parentWidth, float parentHeight, object masterView, string userNoteFileName, DisplayMessageBoxDelegate displayMessageBoxDelegate, NoteSizeChanging onNoteSizeChanging )
                {
                    // setup our note timer that will wait to load our notes until AFTER the notes are created,
                    // as opposed to the same tick. This cuts down 500ms from the create time.
                    /*LoadStateTimer = new System.Timers.Timer();
                    LoadStateTimer.AutoReset = false;
                    LoadStateTimer.Interval = 25;
                    LoadStateTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                        {
                            // when the timer fires, hide the toolbar.
                            // Although the timer fires on a seperate thread, because we queue the reveal
                            // on the main (UI) thread, we don't have to worry about race conditions.
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate { LoadState( UserNotePath ); } );
                        };*/

                    OnNoteSizeChanging = onNoteSizeChanging;

                    RequestDisplayMessageBox = displayMessageBoxDelegate;

                    UserNotePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), userNoteFileName);

                    MasterView = masterView;

                    // create a child control list
                    ChildControls = new List<IUIControl>( );
                    UserNoteControls = new List<UserNote>( ); //store these seperately so we can back them up and test touch input.

                    // now use a reader to get each element
                    //XmlTextReader reader = XmlReader.Create( new StringReader( NoteXml ) ) as XmlTextReader;
                    XmlTextReader reader = new XmlTextReader( new StringReader( NoteXml ) );

                    try
                    {
                        // begin reading the xml stream
                        bool finishedReading = false;
                        while( finishedReading == false && reader.Read( ) )
                        {
                            switch( reader.NodeType )
                            {
                                case XmlNodeType.Element:
                                {
                                    if( reader.Name == "Note" )
                                    {
                                        ParseNote( reader, parentWidth, parentHeight );
                                    }
                                    else
                                    {
                                        throw new Exception( String.Format( "Expected <Note> element. Found <{0}> instead.", reader.Name ) );
                                    }

                                    finishedReading = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch( Exception e )
                    {
                        throw new Exception( e.Message + string.Format( "\nLine Number: {0}", reader.LineNumber ) );
                    }

                    // now load our note state (which will restore stuff)
                    NoteState noteState = LoadState( UserNotePath );

                    return noteState != null ? noteState.ScrollOffsetPercent : 0.00f;
                }

                public void Destroy( object obj )
                {
                    // release references to our UI objects
                    if( ChildControls != null )
                    {
                        foreach( IUIControl uiControl in ChildControls )
                        {
                            uiControl.RemoveFromView( obj );
                        }

                        // and clear our UI list
                        ChildControls.Clear( );
                    }

                    if( UserNoteControls != null )
                    {
                        // remove (but don't destroy) the notes
                        foreach( IUIControl uiControl in UserNoteControls )
                        {
                            uiControl.RemoveFromView( obj );
                        }

                        UserNoteControls.Clear();
                    }

                    NoteXml = null;
                }

                void ParseNote( XmlReader reader, float parentWidthUnits, float parentHeightUnits )
                {
                    DeviceHeight = parentHeightUnits;

                    // get the style first
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mMainNote );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    SizeF parentSize = new SizeF( parentWidthUnits, parentHeightUnits );
                    Parser.ParseBounds( reader, ref parentSize, ref bounds );

                    // Parent note doesn't support margins.

                    // PADDING
                    float leftPadding = Styles.Style.GetValueForNullable( mStyle.mPaddingLeft, parentWidthUnits, 0 );
                    float rightPadding = Styles.Style.GetValueForNullable( mStyle.mPaddingRight, parentWidthUnits, 0 );
                    float topPadding = Styles.Style.GetValueForNullable( mStyle.mPaddingTop, parentHeightUnits, 0 );
                    float bottomPadding = Styles.Style.GetValueForNullable( mStyle.mPaddingBottom, parentHeightUnits, 0 );

                    Padding = new RectangleF( leftPadding, rightPadding, topPadding, bottomPadding );

                    // now calculate the available width based on padding. (Don't actually change our width)
                    float availableWidth = parentWidthUnits - leftPadding - rightPadding;

                    // A "special" (we won't call this a hack) attribute that will enable the user
                    // to have a header container that spans the full width of the note, which allows
                    // it to be unaffected by the padding.
                    string result = reader.GetAttribute( "FullWidthHeader" );
                    if ( string.IsNullOrEmpty( result ) == false )
                    {
                        mStyle.mFullWidthHeader = bool.Parse( result );
                    }
                    
                    // begin reading the xml stream
                    bool finishedReading = false;
                    while( finishedReading == false && reader.Read( ) )
                    {
                        switch( reader.NodeType )
                        {
                            case XmlNodeType.Element:
                            {
                                float workingWidth = availableWidth;
                                if ( Header.ElementTagMatches( reader.Name ) == true && mStyle.mFullWidthHeader == true )
                                {
                                    workingWidth = parentWidthUnits;
                                }

                                IUIControl control = Parser.TryParseControl( new BaseControl.CreateParams( this, workingWidth, parentHeightUnits, ref mStyle ), reader );

                                ChildControls.Add( control );
                                break;
                            }

                            case XmlNodeType.EndElement:
                            {
                                if( reader.Name == "Note" )
                                {
                                    finishedReading = true;
                                }
                                break;
                            }
                        }
                    }

                    // lay stuff out vertically. If the notes were built by hand, like a stackPanel.
                    // if not by hand, like a canvas.
                    float noteHeight = bounds.Y + topPadding;
                    
                    foreach( IUIControl control in ChildControls )
                    {
                        RectangleF controlFrame = control.GetFrame( );
                        RectangleF controlMargin = control.GetMargin( );

                        // horizontally position the controls according to their 
                        // requested alignment
                        Alignment controlAlignment = control.GetHorzAlignment( );

                        // adjust by our position
                        float xAdjust = 0;
                        switch( controlAlignment )
                        {
                            case Alignment.Center:
                            {
                                xAdjust = bounds.X + ( ( availableWidth / 2 ) - ( controlFrame.Width / 2 ) );
                                break;
                            }
                            case Alignment.Right:
                            {
                                xAdjust = bounds.X + ( availableWidth - (controlFrame.Width + controlMargin.Width) );
                                break;
                            }
                            case Alignment.Left:
                            {
                                xAdjust = bounds.X;
                                break;
                            }
                        }


                        // place this next control at yOffset. yOffset should be the current noteHeight, which makes each control relative to the one above it.
                        float yOffset = noteHeight;

                        // if it's the header and full width is specified, don't apply padding.
                        if ( control as Header != null && mStyle.mFullWidthHeader == true )
                        {
                            control.AddOffset( xAdjust, yOffset );
                        }
                        else
                        {
                            control.AddOffset( xAdjust + leftPadding, yOffset );
                        }
                        
                        // update the note height
                        noteHeight = control.GetFrame( ).Bottom + controlMargin.Height;
                    }

                    bounds.Width = parentWidthUnits;
                    bounds.Height = ( noteHeight - bounds.Y ) + bottomPadding;
                    Frame = bounds;

                    AddControlsToView( );

                    // kick off the timer that will load the user note state
                    /*if( LoadingNoteState == false )
                    {
                        LoadingNoteState = true;
                        LoadStateTimer.Start( );
                    }*/

                }

                protected void AddControlsToView( )
                {
                    foreach( IUIControl uiControl in ChildControls )
                    {
                        uiControl.AddToView( MasterView );
                    }
                }

                public RectangleF GetFrame( )
                {
                    return Frame;
                }

                public void GetControlOfType<TControlType>( List<IUIControl> controlList ) where TControlType : class
                {
                    // let each child add itself and its children
                    foreach( IUIControl control in ChildControls )
                    {
                        control.GetControlOfType<TControlType>( controlList );
                    }
                }

                public bool HitTest( PointF touch )
                {
                    // So, see if the user is tapping on a UserNoteAnchor.
                    foreach( UserNote control in UserNoteControls )
                    {
                        // If a user note returns true, its anchor is being touched.
                        if( control.HitTest( touch ) == true )
                        {
                            return true;
                        }
                    }

                    return false;
                }

                /// <summary>
                /// Seperated from ToucheBegan so that the parent container can know if it will be allowed to pan or not.
                /// </summary>
                /// <returns><c>true</c>, if user note was touchinged, <c>false</c> otherwise.</returns>
                /// <param name="touch">Touch.</param>
                public bool TouchingUserNote( PointF touch )
                {
                    if ( ActiveUserNoteAnchor == null )
                    {
                        // We receive TouchesBegan if anything except a TextField was tapped.
                        // The only control we have that needs this is the UserNote for its Anchor.

                        // So, see if the user is tapping on a UserNoteAnchor.
                        foreach ( UserNote control in UserNoteControls )
                        {
                            // If a user note returns true, its anchor is being touched.
                            if ( control.TouchesBegan( touch ) == true )
                            {
                                // Begin tracking this anchor for movement and touchEnd
                                ActiveUserNoteAnchor = control;
                                return true;
                            }
                        }
                    }

                    // No UserNote Anchors were touched, so do not
                    // say we consumed this.
                    return false;
                }

                public bool TouchesBegan( PointF touch )
                {
                    return TouchingUserNote( touch );
                }

                public void TouchesMoved( PointF touch )
                {
                    // If a UserNote anchor was tapped in TouchesBegan,
                    // we will have an active Anchor, so notify it of the new position.
                    if( ActiveUserNoteAnchor != null )
                    {
                        ActiveUserNoteAnchor.TouchesMoved( touch );
                    }
                }

                public string TouchesEnded( PointF touch, out bool urlLaunchesExternalBrowser, out bool urlUsesRockImpersonation )
                {
                    // TouchesEnded is tricky. It's reasonable a User will have
                    // the keyboard up and decide to open/close/move another Note.
                    // This should NOT cause the keyboard to hide. The only time a keyboard
                    // should hide is if a User taps on a general area of the screen.

                    // To accomplish this, we rely on ActiveUserNoteAnchor. If it's valid in TouchesEnded,
                    // that means the touch was in an anchor and not a general part of the screen.

                    // Secondly, if a normal control was touched, it may contain an active link. If a control
                    // consumes input, we will ask it for its link and return that to the caller.
                    // An example would be a Quote. It will consume input and then return a link to the citation.
                    string activeUrl = string.Empty;
                    urlLaunchesExternalBrowser = false;
                    urlUsesRockImpersonation = false;

                    // If there's an active UserNote Anchor, notify only it.
                    if( ActiveUserNoteAnchor != null )
                    {
                        // Notify the active anchor, and clear it since the user released input.
                        ActiveUserNoteAnchor.TouchesEnded( touch );

                        // does this note want to be deleted?
                        if( ActiveUserNoteAnchor.State == UserNote.TouchState.Delete )
                        {
                            // reset its state, and we'll let the messagebox result decide
                            ActiveUserNoteAnchor.State = UserNote.TouchState.None;

                            // then store a pointer to this note
                            UserNote activeNote = ActiveUserNoteAnchor;

                            // and have the system prompt the user for confirmation
                            RequestDisplayMessageBox( MessagesStrings.UserNote_DeleteTitle, MessagesStrings.UserNote_DeleteMessage, 
                                delegate( int result )
                                {
                                    // if they said yes, do it.
                                    if ( result == 0 )
                                    {
                                        // remove it from our list.
                                        UserNoteControls.Remove( activeNote );

                                        // notify our parent
                                        UserNoteChanged( activeNote );

                                        // now clear the anchor ref, which will effectively delete the note (eligible for garbage collection)
                                        activeNote.Dispose( MasterView );
                                    }
                                } );
                        }

                        // clear the user note
                        ActiveUserNoteAnchor = null;
                    }
                    else
                    {
                        // Since a UserNote Anchor was NOT touched, we know it was a "general"
                        // area of the screen, and can allow the keyboard to hide.
                        foreach( UserNote userNote in UserNoteControls )
                        {
                            userNote.NoteTouchesCleared( );
                        }


                        // Now notify all remaining controls until we find out one consumed it.

                        // 1. Start with User Notes. They should get first priority
                        bool consumed = false;
                        foreach( IUIControl control in UserNoteControls )
                        {
                            // was it consumed?
                            IUIControl consumingControl = control.TouchesEnded( touch );
                            if( consumingControl != null )
                            {
                                consumed = true;
                                break;
                            }
                        }


                        // if no user note consumed it, notify all regular controls
                        if( consumed == false )
                        {
                            // 2. check all NON-revealed reveal boxees. 
                            // This allows them to get priority over taps on URLs and already-revealed boxes.

                            List<IUIControl> revealBoxes = new List<IUIControl>( );
                            GetControlOfType<RevealBox>( revealBoxes );
                            foreach ( IUIControl control in revealBoxes )
                            {
                                // if not revealed and consuming input, we're good.
                                if ( ( control as RevealBox ).Revealed == false && control.TouchesEnded( touch ) != null )
                                {
                                    consumed = true;
                                    break;
                                }
                            }


                            // 3. notify all remaining controls
                            if ( consumed == false )
                            {
                                foreach ( IUIControl control in ChildControls )
                                {
                                    // was it consumed?
                                    IUIControl consumingControl = control.TouchesEnded( touch );
                                    if ( consumingControl != null )
                                    {
                                        // then see if it has an active URL we should hit
                                        activeUrl = consumingControl.GetActiveUrl( out urlLaunchesExternalBrowser, out urlUsesRockImpersonation );
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    return activeUrl;
                }

                public bool DidDoubleTap(PointF touch)
                {
                    // do not allow a new note within range of another's anchor.
                    bool allowNoteCreation = true;

                    // don't let them get ridiculous with their notes
                    if ( UserNoteControls.Count < 20 )
                    {
                        foreach ( UserNote userNote in UserNoteControls )
                        {
                            if ( userNote.TouchInAnchorRange( touch ) )
                            {
                                allowNoteCreation = false;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception( MobileApp.Shared.Strings.MessagesStrings.TooManyNotes );
                    }

                    // don't let them create one on top of a reveal box.
                    //Reveal Boxes
                    List<IUIControl> revealBoxes = new List<IUIControl>( );
                    GetControlOfType<RevealBox>( revealBoxes );

                    foreach ( IUIControl control in revealBoxes )
                    {
                        // test for this touch position inside a reveal box
                        RectangleF frame = ( (RevealBox)control ).GetHitTarget( );
                        if ( frame.Contains( touch ) )
                        {
                            allowNoteCreation = false;
                        }
                    }

                    if( allowNoteCreation )
                    {
                        UserNote userNote = new UserNote( new BaseControl.CreateParams( this, Frame.Width, Frame.Height, ref mStyle ), DeviceHeight, touch, UserNoteChanged );
                        UserNoteControls.Add( userNote );

                        userNote.AddToView( MasterView );

                        userNote.OpenNote( true );
                        return true;
                    }

                    return false;
                }

                public void SaveState( float scrollOffsetPercent )
                {
                    // if we're waiting for our notes to load, don't allow saving! We'll
                    // save a blank state over our real notes!
                    //if( LoadingNoteState == false )
                    {
                        // open a stream
                        using (StreamWriter writer = new StreamWriter(UserNotePath, false))
                        {
                            NoteState noteState = new NoteState( );

                            // Scroll position
                            noteState.ScrollOffsetPercent = scrollOffsetPercent;

                            // User Notes
                            noteState.UserNoteContentList = new List<NoteState.UserNoteContent>( );
                            foreach( UserNote note in UserNoteControls )
                            {
                                noteState.UserNoteContentList.Add( note.GetContent( ) );
                            }
                            //


                            //Reveal Boxes
                            List<IUIControl> revealBoxes = new List<IUIControl>( );
                            GetControlOfType<RevealBox>( revealBoxes );

                            noteState.RevealBoxStateList = new List<NoteState.RevealBoxState>( );
                            foreach( RevealBox revealBox in revealBoxes )
                            {
                                noteState.RevealBoxStateList.Add( revealBox.GetState( ) );
                            }
                            //

                            //Text Inputs
                            List<IUIControl> textInputs = new List<IUIControl>( );
                            GetControlOfType<TextInput>( textInputs );

                            noteState.TextInputStateList = new List<NoteState.TextInputState>( );
                            foreach( TextInput textInput in textInputs )
                            {
                                noteState.TextInputStateList.Add( textInput.GetState( ) );
                            }

                            // now we can serialize this and save it.
                            string json = JsonConvert.SerializeObject( noteState );
                            writer.WriteLine( json );
                        }
                    }
                }

                protected NoteState LoadState( string filePath )
                {
                    // sanity check to make sure the notes were requested to load.
                    //if( LoadingNoteState == true )
                    {
                        NoteState noteState = null;

                        // if the file exists
                        if(System.IO.File.Exists(filePath) == true)
                        {
                            // read it
                            using (StreamReader reader = new StreamReader(filePath))
                            {
                                // grab the stream that reprents a list of all their notes
                                string json = reader.ReadLine();

                                if( json != null )
                                {
                                    noteState = JsonConvert.DeserializeObject<NoteState>( json ) as NoteState;
                                }
                            }
                        }

                        if( noteState != null )
                        {
                            // restore each user note
                            foreach( NoteState.UserNoteContent note in noteState.UserNoteContentList )
                            {
                                // create the note, add it to our list, and to the view
                                UserNote userNote = new UserNote( new BaseControl.CreateParams( this, Frame.Width, Frame.Height, ref mStyle ), DeviceHeight, note, UserNoteChanged );
                                UserNoteControls.Add( userNote );
                                userNote.AddToView( MasterView );
                            }

                            // we can assume that the states are 1:1 in the same order as the controls,
                            // because the controls are sorted right after being created, so we're guaranteed
                            // their order is known. There's no risk they were created in a different order.

                            // collect all the reveal boxes and restore them
                            List<IUIControl> revealBoxes = new List<IUIControl>( );
                            GetControlOfType<RevealBox>( revealBoxes );

                            // for the count, take whichever is less, the number of reveal boxes OR the state list,
                            // because it's possible the note was changed after the last save, and a reveal box
                            // may have been added / removed.
                            int revealBoxCount = Math.Min(revealBoxes.Count, noteState.RevealBoxStateList.Count);

                            for(int i = 0; i < revealBoxCount; i++ )
                            {
                                RevealBox revealBox = revealBoxes[ i ] as RevealBox;
                                revealBox.SetRevealed( noteState.RevealBoxStateList[ i ].Revealed );
                            }


                            // collect all the text inputs and restore them
                            List<IUIControl> textInputList = new List<IUIControl>( );
                            GetControlOfType<TextInput>( textInputList );

                            // for the count, take whichever is less, the number of text inputs OR the state list,
                            // because it's possible the note was changed after the last save, and a text input
                            // may have been added / removed.
                            int textInputCount = Math.Min(textInputList.Count, noteState.TextInputStateList.Count);

                            for(int i = 0; i < textInputCount; i++ )
                            {
                                TextInput textInput = textInputList[i] as TextInput;
                                textInput.SetText( noteState.TextInputStateList[ i ].Text );
                            }
                        }

                        return noteState;
                        //LoadingNoteState = false;
                    }
                }

                public void GetNotesForEmail( out string htmlStream, out string textStream )
                {
                    // first setup the string that will contain the notes
                    htmlStream = "<!DOCTYPE html PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n<HTML><Body>\n";
                    textStream = "";

                    // make a COPY of the user notes so the html stream generator can modify it
                    IUIControl[] userNotes = UserNoteControls.ToArray( );

                    List<IUIControl> userNoteListCopy = new List<IUIControl>( );
                    for(int i = 0; i < userNotes.Length; i++)
                    {
                        userNoteListCopy.Add( userNotes[i] );
                    }


                    // let each control recursively build its html stream.
                    // provide the user note list so it can embed user notes in the appropriate spots.
                    foreach( IUIControl control in ChildControls )
                    {
                        control.BuildHTMLContent( ref htmlStream, ref textStream, userNoteListCopy );
                        htmlStream += "<br><br>";
                        textStream += "\n\n";
                    }

                    // if there happened to be any user note that wasn't already added, drop it in at the bottom
                    if( userNoteListCopy.Count > 0)
                    {
                        foreach( UserNote note in userNoteListCopy )
                        {
                            note.BuildHTMLContent( ref htmlStream, ref textStream, null );
                        }
                    }

                    htmlStream += "</Body></HTML>";
                }

                public float GetNoteAbsoluteHeight( )
                {
                    // INCLUDING USER NOTES, this will give us the total
                    // height of the notes.
                    float height = Frame.Height;
                    foreach ( UserNote note in UserNoteControls )
                    {
                        if ( note.GetFrame( ).Bottom > height )
                        {
                            height = note.GetFrame( ).Bottom;
                        }
                    }

                    return height;
                }

                void UserNoteChanged( UserNote note )
                {
                    // our handler for ANY user note modification. This allows us to notify the
                    // parent UI so it can resize or do whatever it may need to do. (Like grow the
                    // scroll view as the note enlarges)
                    Rock.Mobile.Util.Debug.WriteLine( "User Note did change in some way." );

                    OnNoteSizeChanging( );
                }

#if __WIN__
                public IEditableUIControl HandleMouseDown( PointF position )
                {
                    // let each child add itself and its children
                    foreach( IUIControl control in ChildControls )
                    {
                        IEditableUIControl editableControl = control as IEditableUIControl;
                        if( editableControl != null )
                        {
                            IEditableUIControl targetControl = editableControl.HandleMouseDown( position );

                            if ( targetControl != null )
                            {
                                return targetControl;
                            }
                        }
                    }

                    return null;
                }

                public IEditableUIControl HandleMouseDoubleClick( PointF position )
                {
                    // let each child add itself and its children
                    foreach( IUIControl control in ChildControls )
                    {
                        IEditableUIControl editableControl = control as IEditableUIControl;
                        if( editableControl != null )
                        {
                            IEditableUIControl targetControl = editableControl.HandleMouseDoubleClick( position );

                            if ( targetControl != null )
                            {
                                return targetControl;
                            }
                        }
                    }

                    return null;
                }

                public IEditableUIControl HandleControlSnappingX( IEditableUIControl sourceControl, PointF currPos, PointF snapRange )
                {
                    // given a currPos (which is the position the control WILL move to, not the current position of the control), 
                    // find controls that fall within snapRange, and "snap" horizontally to the one closest
                    IEditableUIControl nearbyControl = null;
                    float minDeltaX = snapRange.X;
                    
                    int i;
                    for( i = 0; i < ChildControls.Count; i++ )
                    {
                        // make sure it's an editable control that isn't ourself
                        IEditableUIControl editableControl = ChildControls[ i ] as IEditableUIControl;
                        if( editableControl != null && editableControl != sourceControl )
                        {
                            // get the distance
                            float deltaX = Math.Abs( currPos.X - editableControl.GetPosition( ).X );
                            float deltaY = Math.Abs( currPos.Y - editableControl.GetPosition( ).Y );

                            // if we're within range, take it
                            if( deltaX < minDeltaX && deltaY < snapRange.Y )
                            {
                                nearbyControl = editableControl;
                                minDeltaX = deltaX;
                            }
                        }
                    }

                    return nearbyControl;
                }

                public IEditableUIControl ControlAtPosition( PointF position )
                {
                    IEditableUIControl consumingControl = null;

                    // create a position outside the canvas. As soon as we find an item we're hovering over,
                    // we'll send the rest of the controls this position to force them to discontinue their hover state
                    PointF oobPos = new PointF( -100, -100 );

                    // let each child add itself and its children
                    int i;
                    for( i = 0; i < ChildControls.Count; i++ )
                    {
                        IEditableUIControl editableControl = ChildControls[ i ] as IEditableUIControl;
                        if( editableControl != null )
                        {
                            consumingControl = editableControl.HandleMouseHover( position );
                            if( consumingControl != null )
                            {
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

                    // and return the control consuming this hover
                    return consumingControl;
                }

                public IEditableUIControl HandleMouseHover( PointF position )
                {
                    return ControlAtPosition( position );
                }

                public IEditableUIControl ContainerForControl( Type controlType, PointF mousePos )
                {
                    // this will return the deepest level control that can own controlType as a parent.
                    // Example: contrlType == Paragraph, it's going to return the deepest level Stack Panel or Canvas found.
                    foreach( IUIControl control in ChildControls )
                    {
                        IEditableUIControl editableControl = control as IEditableUIControl;
                        if( editableControl != null )
                        {
                            IEditableUIControl targetControl = editableControl.ContainerForControl( controlType, mousePos );

                            if ( targetControl != null )
                            {
                                return targetControl;
                            }
                        }
                    }

                    return null;
                }


                public void HandleCreateControl( Type controlType, PointF mousePos )
                {
                    do
                    {
                        // first, if we're creating a header, we need to make sure there isn't already one
                        if ( typeof( EditableHeader ) == controlType )
                        {
                            List<IUIControl> headerControls = new List<IUIControl>( );
                            GetControlOfType<EditableHeader>( headerControls );

                            // we found a header, so we're done.
                            if ( headerControls.Count > 0 ) break;
                        }
                    
                        // now see if any child wants to create it
                        IUIControl newControl = null;
                        foreach( IUIControl control in ChildControls )
                        {
                            IEditableUIControl editableControl = control as IEditableUIControl;
                            if ( editableControl != null )
                            {
                                IEditableUIControl containerControl = editableControl.ContainerForControl( controlType, mousePos );
                                if( containerControl != null )
                                {
                                    newControl = containerControl.HandleCreateControl( controlType, mousePos );
                                    break;
                                }
                            }
                        }

                        // if a child handled it, we're done
                        if( newControl != null ) break;

                    
                        // it wasn't a header, and a child didn't create it, so we will.
                        float availableWidth = Frame.Width - Padding.Right - Padding.Left;

                        // if the control type is a header, we want to force it to position 0
                        float workingWidth = availableWidth;
                        if ( typeof( EditableHeader ) == controlType )
                        {
                            mousePos = PointF.Empty;

                            // and if its allowed, use the full width
                            if ( mStyle.mFullWidthHeader == true )
                            {
                                workingWidth = Frame.Width;
                            }
                        }

                        newControl = Parser.CreateEditableControl( controlType, new BaseControl.CreateParams( this, workingWidth, DeviceHeight, ref mStyle ) );
                        ChildControls.Add( newControl );
                    
                        // add it to our renderable canvas
                        newControl.AddToView( MasterView );

                        // default it to where the click occurred
                        newControl.AddOffset( (float) mousePos.X, (float) mousePos.Y );

                        // if the newly created control is the lower than all others, update the note height.
                        // This lets us continue to build vertically
                        if( newControl.GetFrame( ).Bottom > Frame.Height )
                        {
                            Frame = new RectangleF( Frame.Left, Frame.Top, Frame.Width, newControl.GetFrame( ).Bottom + Padding.Bottom );
                        }
                    }
                    while( 0 != 0 );
                }

                public void HandleDeleteControl( IEditableUIControl control )
                {
                    // notify the control to delete itself. if this results in a
                    // direct child of ours being deleted, we'll receive a HandleChildDeleted call
                    control.HandleDelete( true );
                }

                public void HandleChildDeleted( IEditableUIControl childControl )
                {
                    // this allows direct children of ours to notify us they were deleted
                    // (example: a user deletes the last word of text in a paragraph - the paragraph deletes itself, but Note must remove it from its ChildControls list.)

                    foreach( IUIControl child in ChildControls )
                    {
                        if( child.Equals( childControl ) == true )
                        {
                            ChildControls.Remove( child );
                            break;
                        }
                    }
                }

                public string Export( )
                {
                    string xmlExport = "<Note StyleSheet=\"http://rock.ccv.church/content/mobileapp/xml/default_style.xml\">";

                    // first, sort all controls by Y. That way, if something was created and then moved UP, it won't
                    // have a negative value
                    ChildControls.Sort( delegate( IUIControl a, IUIControl b )
                    {
                        if( a.GetFrame( ).Top < b.GetFrame( ).Top )
                        {
                            return -1;
                        }
                        return 1;
                    });
                    
                    foreach( IUIControl child in ChildControls )
                    {
                        IEditableUIControl editableChild = child as IEditableUIControl;
                        xmlExport += editableChild.Export( new RectangleF( Padding.Left, Padding.Top, 0, 0 ), 0 );
                    }

                    xmlExport += "</Note>";
                    return xmlExport;
                }

                public IUIControl GetLogicalVerticalParent( IUIControl sourceControl )
                {
                    //// DEBUG - SORT SO WE CAN VISUALLY DRAW THINGS
                    //ChildControls.Sort( delegate( IUIControl a, IUIControl b )
                    //{
                    //    if( a.GetFrame( ).Top < b.GetFrame( ).Top )
                    //    {
                    //        return -1;
                    //    }
                    //    return 1;
                    //});
                    //

                    // Returns the control that is "truly" above the sourceControl.
                    // This means that its bottom will be ABOVE the top of the sourceControl.
                    // By making controls relative to their logical parent, the spacing remains consistent across
                    // device types
                    IUIControl nearestControl = null;
                    float currMinDeltaY = float.MaxValue;

                    // given a yPos, find the nearest control that is fully "above" this control
                    // and return its bottom position.
                    foreach( IUIControl child in ChildControls )
                    {
                        // skip the control LOOKING for its parent.
                        if ( child != sourceControl )
                        {
                            float deltaY = sourceControl.GetFrame( ).Top - child.GetFrame( ).Bottom;
                            if ( deltaY >= 0 && deltaY < currMinDeltaY )
                            {
                                currMinDeltaY = deltaY;
                                nearestControl = child;
                            }
                        }
                    }

                    return nearestControl;
                }
#endif
            }
        }
    }
}
