using System;
using System.Xml;

using App.Shared.Notes.Styles;
using Rock.Mobile.UI;
using System.Collections.Generic;
using System.Drawing;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Contains common properties and methods used by all UI Controls.
            /// </summary>
            public abstract class BaseControl : IUIControl
            {
                #if DEBUG
                /// <summary>
                /// Layer used for the debug frame.
                /// </summary>
                protected PlatformLabel DebugFrameView { get; set; }
                #endif

                #if DEBUG
                /// <summary>
                /// If true, displays a debug frame around the bounds of the control.
                /// </summary>
                protected bool ShowDebugFrame { get; set; }
                #endif

                /// <summary>
                /// Defines the style that this control should use.
                /// </summary>
                protected Style mStyle;

                /// <summary>
                /// The margin, needed when the parent aligns this control.
                /// </summary>
                /// <value>The margin.</value>
                protected RectangleF Margin { get; set; }

                /// <summary>
                /// Used to pass creation params from parent to child.
                /// </summary>
                public class CreateParams
                {
                    public object Parent { get; set; }

                    public float Height { get; set; }

                    public float Width { get; set; }

                    public Style Style { get; set; }

                    public CreateParams( object parent, float width, float height, ref Style style )
                    {
                        Parent = parent;
                        Height = height;
                        Width = width;
                        Style = style;
                    }
                }

                public static int Sort(IUIControl x, IUIControl y) 
                {
                    RectangleF xFrame = x.GetFrame( );
                    RectangleF yFrame = y.GetFrame( );

                    // take the absolute deltas so that when comparing for "exact",
                    // we can allow an error-margin. The reason is certain words might be centered
                    // within the line height, making them a pixel or two off, but they're still effectively
                    // equal with their sibling
                    float deltaY = Math.Abs( yFrame.Y - xFrame.Y );
                    float deltaX = Math.Abs( yFrame.X - xFrame.X );

                    // if Y is the same, check X.
                    if( deltaY < 2 )
                    {
                        if( deltaX < 2 )
                        {
                            return 0;
                        }
                        return yFrame.X > xFrame.X ? -1 : 1;
                    }
                    return yFrame.Y > xFrame.Y ? -1 : 1;
                }

                protected virtual void Initialize( )
                {
                    mStyle = new Style( );
                    mStyle.Initialize( );
                    mStyle.mAlignment = Alignment.Inherit;

                    //Debugging - show the grid frames
                    #if DEBUG
                    DebugFrameView = PlatformLabel.Create( );
                    DebugFrameView.Opacity = .50f;
                    DebugFrameView.BackgroundColor = 0x0000FFFF;
                    DebugFrameView.ZPosition = 100;
                    #endif
                    //Debugging
                }

                public BaseControl( )
                {
                }

                protected void ParseCommonAttribs( XmlReader reader, ref SizeF parentSize, ref RectangleF bounds )
                {
                    // check for positioning attribs
                    Parser.ParseBounds( reader, ref parentSize, ref bounds );

                    // check for a debug frame
                    #if DEBUG
                    string result = reader.GetAttribute( "Debug" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        ShowDebugFrame = bool.Parse( result );
                    }
                    else
                    {
                        ShowDebugFrame = false;
                    }
                    #endif
                }

                protected void GetMarginsAndPadding( ref Style style, ref SizeF parentSize, ref RectangleF bounds, out RectangleF margin, out RectangleF padding )
                {
                    padding = new RectangleF( );
                    margin = new RectangleF( );

                    // get padding values (this will ensure we get 0 if padding is null)
                    padding.X = Styles.Style.GetValueForNullable( style.mPaddingLeft, parentSize.Width, 0 );
                    padding.Width = Styles.Style.GetValueForNullable( style.mPaddingRight, parentSize.Width, 0 );
                    padding.Y = Styles.Style.GetValueForNullable( style.mPaddingTop, parentSize.Height, 0 );
                    padding.Height = Styles.Style.GetValueForNullable( style.mPaddingBottom, parentSize.Height, 0 );

                    // get margin values (this will ensure we get 0 if padding is null)
                    margin.X = Styles.Style.GetValueForNullable( style.mMarginLeft, parentSize.Width, 0 );

                    // width needs to be the reciprocal (where 10% becomes 90%)
                    margin.Width = Styles.Style.GetValueForNullable( style.mMarginRight, parentSize.Width, 0 );

                    float remainingWidth = parentSize.Width - bounds.X;
                    margin.Width = remainingWidth - (remainingWidth - margin.Width);

                    margin.Y = Styles.Style.GetValueForNullable( style.mMarginTop, parentSize.Height, 0 );
                    margin.Height = Styles.Style.GetValueForNullable( style.mMarginBottom, parentSize.Height, 0 );
                }

                protected void ApplyImmediateMargins( ref RectangleF bounds, ref RectangleF margin, ref SizeF parentSize )
                {
                    // this will apply the margins to our bounds that we can apply.
                    // Bottom margin can only be applied by our parent, because we don't know where the
                    // next control will go.
                    bounds.X += margin.Left;
                    bounds.Y += margin.Top;
                    bounds.Width = Math.Min( bounds.Width, (parentSize.Width - margin.Width) - margin.Left );
                }

                public void SetDebugFrame( RectangleF frame )
                {
                    #if DEBUG
                    DebugFrameView.Frame = frame;
                    #endif
                }

                public virtual void AddOffset( float xOffset, float yOffset )
                {
                    #if DEBUG
                    if( ShowDebugFrame )
                    {
                        DebugFrameView.Position = new PointF( DebugFrameView.Position.X + xOffset, 
                            DebugFrameView.Position.Y + yOffset );
                    }
                    #endif
                }

                public virtual void AddToView( object obj )
                {
                }

                public virtual void RemoveFromView( object obj )
                {
                }

                public void EmbedIntersectingUserNotes( ref string htmlStream, List<IUIControl> userNotes )
                {
                    // this will test to see if any notes in the userNotes list intersect
                    // the Y region of our bounding box. If any do, they will be embedded in the htmlStrea
                    // and then removed from the list so other controls don't embed then.

                    RectangleF controlFrame = GetFrame( );

                    // expand the rect by half its width on all sides in case the note falls between two controls
                    float halfWidth = (controlFrame.Width / 2);
                    RectangleF expandedFrame = new RectangleF( controlFrame.X - halfWidth, controlFrame.Y - halfWidth, 
                                                               controlFrame.Width + halfWidth, controlFrame.Height + halfWidth );

                    RectangleF userNoteFrame;

                    for( int i = userNotes.Count - 1; i >= 0; i-- )
                    {
                        UserNote note = (UserNote) userNotes[ i ];
                        userNoteFrame = note.GetFrame( );

                        // if the user note is within the bounding vertical box of this control, embed it here
                        if( userNoteFrame.Y >= expandedFrame.Y && userNoteFrame.Y <= expandedFrame.Bottom )
                        {
                            note.BuildHTMLContent( ref htmlStream, null );

                            // this note has been placed, so remove it from the list.
                            userNotes.Remove( userNotes[ i ] );
                        }
                    }
                }

                public void TryAddDebugLayer( object obj )
                {
                    // call this at the _end_ so it is the highest level 
                    // view on Android.
                    #if DEBUG
                    if( ShowDebugFrame )
                    {
                        DebugFrameView.AddAsSubview( obj );
                    }
                    #endif
                }

                public void TryRemoveDebugLayer( object obj )
                {
                    // call this at the _end_ so it is the highest level 
                    // view on Android.
                    #if DEBUG
                    if( ShowDebugFrame )
                    {
                        DebugFrameView.RemoveAsSubview( obj );
                    }
                    #endif
                }

                public virtual bool TouchesBegan( PointF touch )
                {
                    return false;
                }

                public virtual void TouchesMoved( PointF touch )
                {
                }

                public virtual IUIControl TouchesEnded( PointF touch )
                {
                    return null;
                }

                public Alignment GetHorzAlignment( )
                {
                    return mStyle.mAlignment.Value;
                }

                public void GetControlOfType<TControlType>( List<IUIControl> controlList ) where TControlType : class
                {
                    // if we're what is being looked for, add ourselves
                    if( (this as TControlType) != null )
                    {
                        controlList.Add( this );
                    }

                    // let each child do the same thing
                    List<IUIControl> childControls = GetChildControls( );
                    if( childControls != null )
                    {
                        foreach( IUIControl control in childControls )
                        {
                            control.GetControlOfType<TControlType>( controlList );
                        }
                    }
                }

                protected virtual List<IUIControl> GetChildControls( )
                {
                    return null;
                }

                public virtual void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                }

                public virtual PlatformBaseUI GetPlatformControl( )
                {
                    return null;
                }
                
                public virtual RectangleF GetFrame( )
                {
                    return new RectangleF( );
                }

                public RectangleF GetMargin( )
                {
                    return Margin;
                }

                public virtual string GetActiveUrl( )
                {
                    return string.Empty;
                }

                public virtual bool ShouldShowBulletPoint( )
                {
                    //the default behavior is that we should want a bullet point
                    return true;
                }
            }
        }
    }
}
