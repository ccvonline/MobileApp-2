using System;
using System.Xml;
using Rock.Mobile.UI;
using System.Threading;
using System.Collections.Generic;
using App.Shared.Config;
using App.Shared.Strings;
using System.Drawing;
using Rock.Mobile.Animation;
using App.Shared.PrivateConfig;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// A label displaying Placeholder text. When tapped, allows
            /// a user to enter text via keyboard.
            /// </summary>
            public class UserNote : BaseControl
            {
                /// <summary>
                /// Actual TextView object.
                /// </summary>
                protected PlatformTextView TextView { get; set; }

                /// <summary>
                /// The view representing the note's "Anchor"
                /// </summary>
                protected PlatformCircleView Anchor { get; set; }
                protected PlatformLabel NoteIcon { get; set; }
                protected RectangleF AnchorFrame { get; set; }

                /// <summary>
                /// Delete button
                /// </summary>
                /// <value>The anchor.</value>
                protected PlatformView UtilityLayer { get; set; }
                protected PlatformLabel DeleteButton { get; set; }
                protected PlatformLabel CloseButton { get; set; }

                /// <summary>
                /// Tracks the movement of the note as a user repositions it.
                /// </summary>
                protected PointF TrackingLastPos { get; set; }

                /// <summary>
                /// True if the note was moved after being tapped.
                /// </summary>
                protected bool DidMoveNote { get; set; }

                /// <summary>
                /// The furthest on X a note is allowed to be moved.
                /// </summary>
                protected float MaxAllowedX { get; set; }

                /// <summary>
                /// The furthest on Y a note is allowed to be moved.
                /// </summary>
                protected float MaxAllowedY { get; set; }

                /// <summary>
                /// The maximum width of the note.
                /// </summary>
                protected float MaxNoteWidth { get; set; }

                /// <summary>
                /// The minimum width of the note.
                /// </summary>
                protected float MinNoteWidth { get; set; }

                /// <summary>
                /// The timer monitoring whether the user held long enough to
                /// enable deleting.
                /// </summary>
                /// <value>The delete timer.</value>
                protected System.Timers.Timer DeleteTimer { get; set; }

                /// <summary>
                /// Manages the state of the note. 
                /// None - means it isn't being interacted with
                /// Hold - The user is holding their finger on it
                /// Moving - The user is dragging it around
                /// Delete - The note should be deleted.
                /// </summary>
                public enum TouchState
                {
                    None,
                    Hold,
                    Moving,
                    Delete,
                    Close,
                };
                public TouchState State { get; set; }

                /// <summary>
                /// True when a note is eligible for delete. Tapping on it while this is true will delete it.
                /// </summary>
                /// <value><c>true</c> if delete enabled; otherwise, <c>false</c>.</value>
                bool DeleteEnabled { get; set; }

                /// <summary>
                /// The maximum you can be from an anchor to be considered touching it.
                /// </summary>
                /// <value>The anchor touch range.</value>
                float AnchorTouchMaxDist { get; set; }

                /// <summary>
                /// the width of the screen so we know
                /// what the remaining width is when moving the note around.
                /// </summary>
                /// <value>The width of the screen.</value>
                float ScreenWidth { get; set; }

                /// <summary>
                /// The value to scale the positions by to get percentage and back
                /// </summary>
                /// <value>The position scalar.</value>
                PointF PositionTransform { get; set; }

                SizeF NoteIconOpenSize { get; set; }
                SizeF NoteIconClosedSize { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    TextView = PlatformTextView.Create( );
                    Anchor = PlatformCircleView.Create( );
                    NoteIcon = PlatformLabel.Create( );
                    UtilityLayer = PlatformView.Create( );
                    DeleteButton = PlatformLabel.Create( );
                    CloseButton = PlatformLabel.Create( );
                }

                public UserNote( BaseControl.CreateParams createParams, float deviceHeight, Model.NoteState.UserNoteContent userNoteContent, UserNoteChanged onUserNoteChanged )
                {
                    PositionTransform = new PointF( createParams.Width, createParams.Height );

                    PointF startPos = new PointF( userNoteContent.PositionPercX * PositionTransform.X, userNoteContent.PositionPercY * PositionTransform.Y );
                    Create( createParams, deviceHeight, startPos, userNoteContent.Text, onUserNoteChanged );

                    // since we're restoring an existing user note,
                    // we want to turn off scaling so we can adjust the height 
                    // for all the text
                    TextView.ScaleHeightForText = false;

                    TextView.SizeToFit( );

                    // a small hack, but calling SizeToFit breaks
                    // the note width, so this will restore it.
                    ValidateBounds( );

                    // now we can turn it back on so that if they continue to edit,
                    // it will grow.
                    TextView.ScaleHeightForText = true;

                    // new notes are open by default. So if we're restoring one that was closed,
                    // keep it closed.
                    if ( userNoteContent.WasOpen == false )
                    {
                        CloseNote( );
                    }
                    else
                    {
                        // open up the text field WITHOUT animating
                        // the textView
                        TextView.Hidden = false;
                        AnimateNoteIcon( true );
                        AnimateUtilityView( true );
                    }
                }

                /// <summary>
                /// User notes are tricky and can require big changes to the layout of the NoteView.
                /// So, each one will accept a callback so it can notify the parent Note when something happens to it.
                /// </summary>
                public delegate void UserNoteChanged( UserNote note );
                UserNoteChanged OnUserNoteChanged;

                void InvokeChangedCallback( )
                {
                    if( OnUserNoteChanged != null )
                    {
                        OnUserNoteChanged( this );
                    }
                }

                public UserNote( CreateParams parentParams, float deviceHeight, PointF startPos, UserNoteChanged onUserNoteChanged )
                {
                    Create( parentParams, deviceHeight, startPos, null, onUserNoteChanged );
                }

                const float UtilityLayerHeight = 40;

                void Create( CreateParams parentParams, float deviceHeight, PointF startPos, string startingText, UserNoteChanged onUserNoteChanged )
                {
                    Initialize( );

                    OnUserNoteChanged = onUserNoteChanged;

                    PositionTransform = new PointF( parentParams.Width, parentParams.Height );

                    //setup our timer for allowing movement/
                    /*DeleteTimer = new System.Timers.Timer();
                    DeleteTimer.Interval = 1000;
                    DeleteTimer.Elapsed += DeleteTimerDidFire;
                    DeleteTimer.AutoReset = false;*/

                    // take the UserNote default style, because they aren't part of the XML stream and have nothing to inherit.
                    mStyle = ControlStyles.mUserNote;

                    // flag that we want this text field to grow as more text is added
                    TextView.ScaleHeightForText = true;

                    // Setup the font
                    TextView.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    TextView.TextColor = mStyle.mFont.mColor.Value;
                    TextView.Placeholder = MessagesStrings.UserNote_Placeholder;
                    TextView.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
                    TextView.KeyboardAppearance = GeneralConfig.iOSPlatformUIKeyboardAppearance;
                    TextView.SetOnEditCallback( 
                        delegate(PlatformTextView textView )
                        {
                            InvokeChangedCallback( );
                        } );
                     
                    // check for border styling
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        TextView.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        TextView.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        TextView.BorderWidth = mStyle.mBorderWidth.Value;
                    }

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        TextView.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }

                    // Setup the anchor BG
                    int area = (int) Rock.Mobile.Graphics.Util.UnitToPx( 50 );
                    Anchor.BackgroundColor = NoteConfig.UserNote_AnchorColor;
                    Anchor.Bounds = new RectangleF( 0, 0, area, area );

                    // Setup the anchor color
                    NoteIcon.Text = PrivateNoteConfig.UserNote_Icon;
                    NoteIcon.TextColor = mStyle.mFont.mColor.Value;//NoteConfig.UserNote_IconColor;

                    // get the small and large sizes for the note icon, so we can animate correctly
                    NoteIcon.SetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.UserNote_IconOpenSize );
                    NoteIcon.Bounds = new RectangleF( 0, 0, area, 0 );
                    NoteIcon.SizeToFit();
                    NoteIconOpenSize = NoteIcon.Bounds.Size;

                    NoteIcon.SetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.UserNote_IconClosedSize );
                    NoteIcon.Bounds = new RectangleF( 0, 0, area, 0 );
                    NoteIcon.SizeToFit( );
                    NoteIconClosedSize = NoteIcon.Bounds.Size;
                    ////


                    // store the width of the screen so we know
                    // what the remaining width is when moving the note around.
                    ScreenWidth = parentParams.Width * .95f;

                    // Don't let the note's width be less than twice the anchor width. Any less
                    // and we end up with text clipping.
                    MinNoteWidth = (Anchor.Bounds.Width * 2);

                    // Dont let the note be any wider than the screen - twice the min width. This allows a little
                    // free play so it doesn't feel like the note is always attached to the right edge.
                    MaxNoteWidth = Math.Min( ScreenWidth - MinNoteWidth, (MinNoteWidth * 6) );

                    // set the allowed X/Y so we don't let the user move the note off-screen.
                    MaxAllowedX = ( ScreenWidth - MinNoteWidth - Anchor.Bounds.Width );
                    MaxAllowedY = ( parentParams.Height - Anchor.Bounds.Height );
                    MaxAllowedY *= 1.05f;

                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, MaxAllowedX - startPos.X ) );
                    TextView.Bounds = new RectangleF( 0, 0, width, 0 );

                    UtilityLayer.BackgroundColor = TextView.BackgroundColor;
                    UtilityLayer.CornerRadius = TextView.CornerRadius;
                    UtilityLayer.BorderWidth = TextView.BorderWidth;
                    UtilityLayer.BorderColor = TextView.BorderColor;
                    UtilityLayer.Bounds = new RectangleF( 0, 0, MinNoteWidth, 0 );

                    // setup the delete button
                    DeleteButton.Text = PrivateNoteConfig.UserNote_DeleteIcon;
                    DeleteButton.TextColor = mStyle.mFont.mColor.Value;//NoteConfig.UserNote_IconColor;
                    DeleteButton.SetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.UserNote_DeleteIconSize );
                    DeleteButton.SizeToFit( );

                    // setup the close button
                    CloseButton.Text = PrivateNoteConfig.UserNote_CloseIcon;
                    CloseButton.TextColor = mStyle.mFont.mColor.Value;//NoteConfig.UserNote_IconColor;
                    CloseButton.SetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateNoteConfig.UserNote_CloseIconSize );
                    CloseButton.SizeToFit( );



                    // Setup the initial positions
                    Anchor.Position = startPos;
                    AnchorFrame = Anchor.Frame;

                    AnchorTouchMaxDist = AnchorFrame.Width / 2;
                    AnchorTouchMaxDist *= AnchorTouchMaxDist;

                    NoteIcon.Position = new PointF( Anchor.Frame.Left + (Anchor.Frame.Width - NoteIconClosedSize.Width) / 2, 
                        Anchor.Frame.Top + (Anchor.Frame.Height - NoteIconClosedSize.Height) / 2 );

                    // set the actual note TextView relative to the anchor
                    TextView.Position = new PointF( AnchorFrame.Left + AnchorFrame.Width / 2, 
                        AnchorFrame.Top + AnchorFrame.Height / 2 );
                    
                    UtilityLayer.Position = new PointF( (TextView.Position.X + width) - UtilityLayer.Bounds.Width, 
                        TextView.Position.Y - Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) );

                    // set the position for the delete button
                    DeleteButton.Position = new PointF( UtilityLayer.Position.X + DeleteButton.Bounds.Width / 2, 
                        UtilityLayer.Position.Y + (Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) - DeleteButton.Bounds.Height) / 2 );

                    CloseButton.Position = new PointF( UtilityLayer.Frame.Right - (CloseButton.Bounds.Width + (CloseButton.Bounds.Width / 2)), 
                        UtilityLayer.Position.Y + (Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) - CloseButton.Bounds.Height) / 2 );

                    // validate its bounds
                    ValidateBounds( );


                    // set the starting text if it was provided
                    if( startingText != null )
                    {
                        TextView.Text = startingText;
                    }

                    TextView.Hidden = true;

                    SetDebugFrame( TextView.Frame );
                }

                public bool TouchInCloseButtonRange( PointF touch )
                {
                    // create a vector from the note anchor's center to the touch
                    PointF labelToTouch = new PointF( touch.X - (CloseButton.Frame.X + CloseButton.Frame.Width / 2), 
                                                      touch.Y - (CloseButton.Frame.Y + CloseButton.Frame.Height / 2));

                    float distSquared = Rock.Mobile.Math.Util.MagnitudeSquared( labelToTouch );
                    if( distSquared < AnchorTouchMaxDist )
                    {
                        return true;
                    }

                    return false;
                }

                public bool TouchInDeleteButtonRange( PointF touch )
                {
                    // create a vector from the note anchor's center to the touch
                    PointF labelToTouch = new PointF( touch.X - (DeleteButton.Frame.X + DeleteButton.Frame.Width / 2), 
                                                      touch.Y - (DeleteButton.Frame.Y + DeleteButton.Frame.Height / 2));

                    float distSquared = Rock.Mobile.Math.Util.MagnitudeSquared( labelToTouch );
                    if( distSquared < AnchorTouchMaxDist )
                    {
                        return true;
                    }

                    return false;
                }

                public bool TouchInAnchorRange( PointF touch )
                {
                    // create a vector from the note anchor's center to the touch
                    PointF labelToTouch = new PointF( touch.X - (AnchorFrame.X + AnchorFrame.Width / 2), 
                                                      touch.Y - (AnchorFrame.Y + AnchorFrame.Height / 2));

                    float distSquared = Rock.Mobile.Math.Util.MagnitudeSquared( labelToTouch );
                    if( distSquared < AnchorTouchMaxDist )
                    {
                        return true;
                    }

                    return false;
                }

                public bool HitTest( PointF touch )
                {
                    if( TouchInAnchorRange( touch ) )
                    {
                        return true;
                    }

                    return false;
                }

                public override bool TouchesBegan( PointF touch )
                {
                    // is a user wanting to interact?
                    bool consumed = false;

                    // if delete is enabled, see if they tapped within range of the delete button
                    /*if( DeleteEnabled )
                    {
                        if( TouchInDeleteButtonRange( touch ) )
                        {
                            // if they did, we consume this and bye bye note.
                            consumed = true;

                            State = TouchState.Delete;
                        }
                    }*/
                    if ( DeleteButton.Hidden == false && TouchInDeleteButtonRange( touch ) )
                    {
                        // if they did, we consume this and bye bye note.
                        consumed = true;

                        State = TouchState.Delete;
                    }
                    else if ( CloseButton.Hidden == false && TouchInCloseButtonRange( touch ) )
                    {
                        consumed = true;

                        State = TouchState.Close;
                    }
                    // if the touch is in our region, begin tracking
                    else if( TouchInAnchorRange( touch ) )
                    {
                        consumed = true;

                        if( State == TouchState.None )
                        {
                            // Enter the hold state
                            State = TouchState.Hold;

                            // Store our starting touch and kick off our delete timer
                            TrackingLastPos = touch;
                            //DeleteTimer.Start();
                            Rock.Mobile.Util.Debug.WriteLine( "UserNote Hold" );
                        }
                    }

                    return consumed;
                }

                // By design, this will only be called on the UserNote that received a TouchesBegan IN ANCHOR RANGE.
                static float sMinDistForMove = 625;
                public override void TouchesMoved( PointF touch )
                {
                    // We would be in the hold state if this is the first TouchesMoved 
                    // after TouchesBegan.
                    //if( DeleteEnabled == false )
                    {
                        // if we're moving, update by the amount we moved.
                        PointF delta = new PointF( touch.X - TrackingLastPos.X, touch.Y - TrackingLastPos.Y );

                        // if we're in the hold state, require a small amount of moving before committing to movement.
                        if( State == TouchState.Hold )
                        {
                            float magSquared = Rock.Mobile.Math.Util.MagnitudeSquared( delta );
                            if( magSquared > sMinDistForMove )
                            {
                                // stamp our position as the new starting position so we don't
                                // get a "pop" in movement.
                                TrackingLastPos = touch;

                                State = TouchState.Moving;
                                Rock.Mobile.Util.Debug.WriteLine( "UserNote MOVING" );
                            }
                        }
                        else if( State == TouchState.Moving )
                        {
                            AddOffset( delta.X, delta.Y );

                            // stamp our position
                            TrackingLastPos = touch;
                        }
                    }
                }

                // By design, this will only be called on the UserNote that received a TouchesBegan IN ANCHOR RANGE.
                public override IUIControl TouchesEnded( PointF touch )
                {
                    bool consumed = false;

                    switch( State )
                    {
                        case TouchState.None:
                        {
                            // don't do anything if our state is none
                            break;
                        }

                        case TouchState.Moving:
                        {
                            // if we were moving, don't do anything except exit the movement state.
                            consumed = true;
                            State = TouchState.None;

                            Rock.Mobile.Util.Debug.WriteLine( "UserNote Finished Moving" );
                            break;
                        }

                        case TouchState.Hold:
                        {
                            consumed = true;
                            State = TouchState.None;

                            // if delete enabled was turned on while holding
                            // (which would happen if a timer fired while holding)
                            // then don't toggle, they are deciding what to delete.
                            //if( DeleteEnabled == false )
                            {
                                // if it's open and they tapped in the note anchor, close it.
                                /*if( TextView.Hidden == false )
                                {
                                    CloseNote();
                                }
                                // if it's closed and they tapped in the note anchor, open it
                                else
                                {
                                    OpenNote( );
                                }*/

                                // JHM 4-29-15 In support of the utility bar, only let a tap on the anchor OPEN the note.
                                // Require them to use the close button to close it
                                if ( TextView.Hidden == true )
                                {
                                    OpenNote( false );
                                }
                            }
                            break;
                        }

                        case TouchState.Close:
                        {
                            consumed = true;
                            State = TouchState.None;

                            CloseNote();
                            break;
                        }

                        case TouchState.Delete:
                        {
                            Rock.Mobile.Util.Debug.WriteLine( "User Wants to delete note" );
                            break;
                        }
                    }

                    //DeleteTimer.Stop();

                    return consumed == true ? this : null;
                }

                /*protected void DeleteTimerDidFire(object sender, System.Timers.ElapsedEventArgs e)
                {
                    // if they're still in range and haven't moved the note yet, activate delete mode.
                    if ( TouchInAnchorRange( TrackingLastPos ) && State == TouchState.Hold )
                    {
                        // reveal the delete button
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate {  ShowDeleteUI( ); } );
                        DeleteEnabled = true;
                    }
                }*/

                /*void ShowDeleteUI( )
                {
                    DeleteButton.Hidden = false;
                    TextView.UserInteractionEnabled = false;
                    TextView.ResignFirstResponder( );

                    SimpleAnimator_Color colorAnimator = new SimpleAnimator_Color( NoteConfig.UserNote_AnchorColor, NoteConfig.UserNote_DeleteAnchorColor, .15f, delegate(float percent, object value )
                        {
                            Anchor.BackgroundColor = (uint)value;
                        }
                        ,
                        delegate
                        {
                        } );
                    colorAnimator.Start( );
                }*/

                public void Dispose( object masterView )
                {
                    // remove it from the UI
                    RemoveFromView( masterView );

                    // todo: do something here to fix android's focus issue
                }

                public void NoteTouchesCleared( )
                {
                    // This is called by our parent when we can safely assume NO NOTE
                    // was touched in the latest OnTouch/HoldTouch/EndTouch.

                    // This is important because to exit delete mode, hide a keyboard, etc.,
                    // we only want to do that when no other note is touched.
                    TextView.ResignFirstResponder( );

                    /*if( DeleteEnabled == true )
                    {
                        DeleteEnabled = false;
                        Rock.Mobile.Util.Debug.WriteLine( "Clearing Delete Mode" );
                        TextView.UserInteractionEnabled = true;

                        DeleteButton.Hidden = true;

                        SimpleAnimator_Color colorAnimator = new SimpleAnimator_Color( NoteConfig.UserNote_DeleteAnchorColor, NoteConfig.UserNote_AnchorColor, .15f, delegate(float percent, object value )
                            {
                                Anchor.BackgroundColor = (uint)value;
                            }
                            ,
                            delegate
                            {
                            } );
                        colorAnimator.Start( );
                    }*/
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    // clamp X & Y movement to within margin of the screen
                    if( Anchor.Position.X + xOffset < 0 )
                    {
                        // watch the left side
                        xOffset += 0 - (Anchor.Position.X + xOffset);
                    }
                    else if( Anchor.Position.X + xOffset > MaxAllowedX )
                    {
                        // and the right
                        xOffset -= (Anchor.Position.X + xOffset) - MaxAllowedX;
                    }

                    // Check Y...
                    if( Anchor.Position.Y + yOffset < 0 )
                    {
                        yOffset += -(Anchor.Position.Y + yOffset);
                    }
                    else if (Anchor.Position.Y + yOffset > MaxAllowedY )
                    {
                        yOffset -= (Anchor.Position.Y + yOffset) - MaxAllowedY;
                    }

                    // Now that offsets have been clamped, reposition the note
                    base.AddOffset( xOffset, yOffset );

                    TextView.Position = new PointF( TextView.Position.X + xOffset, 
                        TextView.Position.Y + yOffset );

                    Anchor.Position = new PointF( Anchor.Position.X + xOffset,
                                                  Anchor.Position.Y + yOffset );

                    NoteIcon.Position = new PointF( NoteIcon.Position.X + xOffset,
                                                    NoteIcon.Position.Y + yOffset );
                    
                    AnchorFrame = Anchor.Frame;


                    // Scale the TextView to no larger than the remaining width of the screen 
                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, ScreenWidth - (AnchorFrame.X + (AnchorFrame.Width / 2)) ) );
                    TextView.Bounds = new RectangleF( 0, 0, width, TextView.Bounds.Height);

                    UtilityLayer.Position = new PointF( (TextView.Position.X + width) - UtilityLayer.Bounds.Width, 
                        TextView.Position.Y - Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) );

                    // set the position for the delete button
                    DeleteButton.Position = new PointF( UtilityLayer.Position.X, 
                        UtilityLayer.Position.Y + (Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) - DeleteButton.Bounds.Height) / 2 );

                    CloseButton.Position = new PointF( UtilityLayer.Frame.Right - CloseButton.Bounds.Width, 
                        UtilityLayer.Position.Y + (Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) - CloseButton.Bounds.Height) / 2 );

                    // let the parent know we're moving, which they may care about
                    InvokeChangedCallback( );
                }

                void ValidateBounds()
                {
                    // clamp X & Y movement to within margin of the screen
                    float xPos = Math.Max( Math.Min( AnchorFrame.X, MaxAllowedX ), 0 );

                    float yPos = Math.Max( Math.Min( AnchorFrame.Y, MaxAllowedY ), 0 );

                    Anchor.Position = new PointF( xPos, yPos );
                    AnchorFrame = Anchor.Frame;

                    TextView.Position = new PointF( AnchorFrame.Left + AnchorFrame.Width / 2, 
                        AnchorFrame.Top + AnchorFrame.Height / 2 );

                    NoteIcon.Position = new PointF( Anchor.Frame.Left + (Anchor.Frame.Width - NoteIconClosedSize.Width) / 2, 
                        Anchor.Frame.Top + (Anchor.Frame.Height - NoteIconClosedSize.Height) / 2 );

                    // Scale the TextView to no larger than the remaining width of the screen 
                    float width = Math.Max( MinNoteWidth, Math.Min( MaxNoteWidth, ScreenWidth - (AnchorFrame.X + (AnchorFrame.Width / 2)) ) );
                    TextView.Bounds = new RectangleF( 0, 0, width, TextView.Bounds.Height);

                    UtilityLayer.Position = new PointF( (TextView.Position.X + width) - UtilityLayer.Bounds.Width, 
                        TextView.Position.Y - Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) );

                    // set the position for the delete button
                    DeleteButton.Position = new PointF( UtilityLayer.Position.X, 
                        UtilityLayer.Position.Y + (Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) - DeleteButton.Bounds.Height) / 2 );

                    CloseButton.Position = new PointF( UtilityLayer.Frame.Right - CloseButton.Bounds.Width, 
                        UtilityLayer.Position.Y + (Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) - CloseButton.Bounds.Height) / 2 );
                }

                public override void AddToView( object obj )
                {
                    Anchor.AddAsSubview( obj );
                    TextView.AddAsSubview( obj );
                    NoteIcon.AddAsSubview( obj );
                    UtilityLayer.AddAsSubview( obj );
                    DeleteButton.AddAsSubview( obj );
                    CloseButton.AddAsSubview( obj );

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    Anchor.RemoveAsSubview( obj );
                    TextView.RemoveAsSubview( obj );
                    NoteIcon.RemoveAsSubview( obj );
                    UtilityLayer.RemoveAsSubview( obj );
                    DeleteButton.RemoveAsSubview( obj );
                    CloseButton.RemoveAsSubview( obj );

                    TryRemoveDebugLayer( obj );
                }

                bool Animating { get; set; }
                bool AnimatingUtilityView { get; set; }
                public void OpenNote( bool becomeFirstResponder )
                {
                    AnimateNoteIcon( true );
                    AnimateUtilityView( true );

                    // open the text field
                    TextView.AnimateOpen( becomeFirstResponder );
                    Rock.Mobile.Util.Debug.WriteLine( "Opening Note" );
                }

                public void CloseNote()
                {
                    AnimateNoteIcon( false );
                    AnimateUtilityView( false );

                    // close the text field
                    TextView.AnimateClosed( );
                    Rock.Mobile.Util.Debug.WriteLine( "Closing Note" );

                    TextView.ResignFirstResponder( );
                }

                PointF GetNoteIconPos( bool open )
                {
                    if ( open == true )
                    {
                        // the / 4 is a bit of a hack. We don't actually resize the icon, so knowing that it's smaller, this
                        // correctly positions it within the anchor. It'll have to change if the sizing of the icons change.
                        //return new PointF( Anchor.Frame.Left + ( Anchor.Frame.Width - NoteIconOpenSize.Width ) / 5, 
                          //  Anchor.Frame.Top - NoteIconOpenSize.Height / 4 );

                        return new PointF( Anchor.Frame.Left + ( Anchor.Frame.Width - NoteIconOpenSize.Width ) / 2, 
                                          Anchor.Frame.Top );
                    }
                    else
                    {
                        return new PointF( Anchor.Frame.Left + (Anchor.Frame.Width - NoteIconClosedSize.Width) / 2, 
                            Anchor.Frame.Top + (Anchor.Frame.Height - NoteIconClosedSize.Height) / 2 );
                    }
                }

                void AnimateNoteIcon( bool open )
                {
                    if ( Animating == false )
                    {
                        Animating = true;

                        SizeF startSize = NoteIcon.Bounds.Size;
                        SizeF endSize;

                        PointF startPos = NoteIcon.Position;
                        PointF endPos = GetNoteIconPos( open );

                        float startTypeSize;
                        float endTypeSize;

                        float animTime = .2f;

                        // the text must always be smaller than the bounding box,
                        // so we'll scale the typeSize anim time to be FASTER when opening 
                        // and SLOWER when closing.
                        float sizeAnimTimeScalar;

                        // setup the target values based on whether we're opening or closing
                        if ( open == true )
                        {
                            endSize = NoteIconOpenSize;

                            startTypeSize = PrivateNoteConfig.UserNote_IconClosedSize;
                            endTypeSize = PrivateNoteConfig.UserNote_IconOpenSize;

                            sizeAnimTimeScalar = .95f;
                        }
                        else
                        {
                            endSize = NoteIconClosedSize;

                            startTypeSize = PrivateNoteConfig.UserNote_IconOpenSize;
                            endTypeSize = PrivateNoteConfig.UserNote_IconClosedSize;

                            sizeAnimTimeScalar = 1.05f;
                        }

                        // size...
                        SimpleAnimator_SizeF sizeAnimator = new SimpleAnimator_SizeF( startSize, endSize, animTime, 
                                                                delegate(float percent, object value )
                            {
                                SizeF currSize = (SizeF)value;
                                NoteIcon.Bounds = new RectangleF( 0, 0, currSize.Width, currSize.Height );
                            }, null );

                        sizeAnimator.Start( );

                        // pos...
                        SimpleAnimator_PointF posAnimator = new SimpleAnimator_PointF( startPos, endPos, animTime, 
                                                                delegate(float percent, object value )
                            {
                                NoteIcon.Position = (PointF)value;
                            }, null );
                        posAnimator.Start( );

                        // font typesize...
                        SimpleAnimator_Float floatAnimator = new SimpleAnimator_Float( startTypeSize, endTypeSize, animTime * sizeAnimTimeScalar, 
                                                                 delegate(float percent, object value )
                            {
                                NoteIcon.SetFont( PrivateControlStylingConfig.Icon_Font_Secondary, (float)value );
                            }, delegate { Animating = false; } );
                        floatAnimator.Start( );
                    }
                }

                void AnimateUtilityView( bool open )
                {
                    if ( AnimatingUtilityView == false )
                    {
                        AnimatingUtilityView = true;

                        SizeF startSize = UtilityLayer.Bounds.Size;
                        SizeF endSize;

                        float animTime = .2f;

                        // setup the target values based on whether we're opening or closing
                        if ( open == true )
                        {
                            UtilityLayer.Hidden = false;
                            endSize = new SizeF( MinNoteWidth, Rock.Mobile.Graphics.Util.UnitToPx( UtilityLayerHeight ) );
                        }
                        else
                        {
                            DeleteButton.Hidden = true;
                            CloseButton.Hidden = true;
                            endSize = new SizeF( MinNoteWidth, 0 );
                        }

                        // size...
                        SimpleAnimator_SizeF sizeAnimator = new SimpleAnimator_SizeF( startSize, endSize, animTime, 
                            delegate(float percent, object value )
                            {
                                SizeF currSize = (SizeF)value;
                                UtilityLayer.Bounds = new RectangleF( 0, 0, currSize.Width, currSize.Height );
                            }, 
                            delegate
                            { 
                                // if we CLOSED, hide the utility layer
                                if ( open == false )
                                {
                                    UtilityLayer.Hidden = true;
                                }
                                // and if we OPENED, unhide the DELETE button
                                else
                                {
                                    // 
                                    DeleteButton.Hidden = false;
                                    CloseButton.Hidden = false;
                                }
                                AnimatingUtilityView = false; 
                            } );

                        sizeAnimator.Start( );
                    }
                }

                public override RectangleF GetFrame( )
                {
                    // Turn off scaling and size the field, which will get us its actual width / height.
                    TextView.ScaleHeightForText = false;

                    // measure
                    TextView.SizeToFit( );

                    // a small hack, but calling SizeToFit breaks
                    // the note width, so this will restore it.
                    ValidateBounds( );

                    // store it
                    RectangleF fullFrame = TextView.Frame;

                    // then restore scaling (which will alter the Frame, but that's ok)
                    TextView.ScaleHeightForText = true;

                    // and return the full frame.
                    return fullFrame;
                }

                public override void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                    if ( string.IsNullOrEmpty( TextView.Text ) == false )
                    {
                        htmlStream += string.Format( "<br><p><b>{0}", MessagesStrings.UserNote_Prefix ) + TextView.Text + "</b></p>";
                    }
                }

                public Notes.Model.NoteState.UserNoteContent GetContent( )
                {
                    return new Notes.Model.NoteState.UserNoteContent( AnchorFrame.X / PositionTransform.X, AnchorFrame.Y / PositionTransform.Y, TextView.Text, !TextView.Hidden );
                }
            }
        }
    }
}
