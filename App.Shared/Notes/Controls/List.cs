using System;
using System.Collections.Generic;
using System.Xml;
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
            /// A container that displays children in a vertical stack.
            /// </summary>
            public class List : BaseControl
            {
                protected const string ListTypeBullet = "Bullet";
                protected const string ListTypeNumbered = "Numbered";

                protected string ListType { get; set; }

                /// <summary>
                /// Children to display
                /// </summary>
                /// <value>The child controls.</value>
                protected List<IUIControl> ChildControls { get; set; }

                /// <summary>
                /// The view representing any surrounding border for the canvas.
                /// </summary>
                /// <value>The border view.</value>
                protected PlatformView BorderView { get; set; }

                /// <summary>
                /// The bounds (including position) of the stack panel.
                /// </summary>
                /// <value>The bounds.</value>
                protected RectangleF Bounds { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    ChildControls = new List<IUIControl>( );

                    BorderView = PlatformView.Create( );
                }

                public List( CreateParams parentParams, XmlReader reader )
                {
                    Initialize( );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mList );

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

                    // convert indentation if it's a percentage
                    float listIndentation = mStyle.mListIndention.Value;
                    if( listIndentation < 1 )
                    {
                        listIndentation = parentParams.Width * listIndentation;
                    }

                    // now calculate the available width based on padding. (Don't actually change our width)
                    // also consider the indention amount of the list.
                    float availableWidth = bounds.Width - padding.Left - padding.Width - listIndentation - (borderPaddingPx * 2);


                    // parse for the desired list style. Default to Bullet if they didn't put anything.
                    ListType = reader.GetAttribute( "Type" );
                    if( string.IsNullOrEmpty( ListType ) == true)
                    {
                        ListType = ListTypeBullet;
                    }

                    // Parse Child Controls
                    int numberedCount = 1;

                    // don't force our alignment, borders, bullet style or indentation on children.
                    Style style = new Style( );
                    style = mStyle;
                    style.mAlignment = null;
                    style.mListIndention = null;
                    style.mListBullet = null;
                    style.mBorderColor = null;
                    style.mBorderRadius = null;
                    style.mBorderWidth = null;

                    bool finishedParsing = false;
                    while( finishedParsing == false && reader.Read( ) )
                    {
                        switch( reader.NodeType )
                        {
                            case XmlNodeType.Element:
                            {
                                // Create the prefix for this list item.
                                string listItemPrefixStr = mStyle.mListBullet + " ";
                                if( ListType == ListTypeNumbered )
                                {
                                    listItemPrefixStr = numberedCount.ToString() + ". ";
                                }

                                NoteText textLabel = new NoteText( new CreateParams( this, availableWidth, parentParams.Height, ref style ), listItemPrefixStr );
                                ChildControls.Add( textLabel );


                                // create our actual child, but throw an exception if it's anything but a ListItem.
                                IUIControl control = Parser.TryParseControl( new CreateParams( this, availableWidth - textLabel.GetFrame().Width, parentParams.Height, ref style ), reader );

                                ListItem listItem = control as ListItem;
                                if( listItem == null ) throw new Exception( String.Format("Only a <ListItem> may be a child of a <List>. Found element <{0}>.", control.GetType( ) ) );


                                // if it will actually use the bullet point, increment our count.
                                if( listItem.ShouldShowBulletPoint() == true )
                                {
                                    numberedCount++;
                                }
                                else
                                {
                                    // otherwise give it a blank space, and keep our count the same.
                                    textLabel.SetText("  ");
                                }

                                // and finally add the actual list item.
                                ChildControls.Add( control );
                                break;
                            }

                            case XmlNodeType.EndElement:
                            {
                                // if we hit the end of our label, we're done.
                                //if( reader.Name == "List" || reader.Name == "L" )
                                if( ElementTagMatches( reader.Name ) )
                                {
                                    finishedParsing = true;
                                }

                                break;
                            }
                        }
                    }


                    // layout all controls
                    float xAdjust = bounds.X + listIndentation; 
                    float yOffset = bounds.Y + padding.Top + borderPaddingPx; //vertically they should just stack

                    // we know each child is a NoteText followed by ListItem. So, lay them out 
                    // as: * - ListItem
                    //     * - ListItem
                    foreach( IUIControl control in ChildControls )
                    {
                        // position the control
                        control.AddOffset( xAdjust + padding.Left + borderPaddingPx, yOffset );

                        RectangleF controlFrame = control.GetFrame( );
                        RectangleF controlMargin = control.GetMargin( );

                        // is this the item prefix?
                        if( (control as NoteText) != null )
                        {
                            // and update xAdjust so the actual item starts after.
                            xAdjust += controlFrame.Width;
                        }
                        else
                        {
                            // reset the values for the next line.
                            xAdjust = bounds.X + listIndentation;
                            yOffset = controlFrame.Bottom + controlMargin.Height;
                        }
                    }

                    // we need to store our bounds. We cannot
                    // calculate them on the fly because we
                    // would lose any control defined offsets, which would throw everything off.
                    bounds.Height = ( yOffset - bounds.Y ) + padding.Height + borderPaddingPx;
                    Bounds = bounds;

                    BorderView.Frame = bounds;

                    // store our debug frame
                    SetDebugFrame( Bounds );

                    // sort everything
                    ChildControls.Sort( BaseControl.Sort );
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

                    return null;
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
                    Bounds = new RectangleF( Bounds.X + xOffset, Bounds.Y + yOffset, Bounds.Width, Bounds.Height );
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

                public override RectangleF GetFrame( )
                {
                    return Bounds;
                }

                protected override List<IUIControl> GetChildControls( )
                {
                    return ChildControls;
                }

                public override void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                    /// add the appropriate list tag
                    if( ListType == ListTypeNumbered )
                    {
                        htmlStream += "<ol>";
                    }
                    else
                    {
                        htmlStream += "<ul>";
                    }

                    foreach( IUIControl control in ChildControls )
                    {
                        // don't render the NoteTexts, those are just numbers, which the
                        // HTML will insert for us.
                        if( control as ListItem != null )
                        {
                            control.BuildHTMLContent( ref htmlStream, userNotes );
                        }
                    }

                    // handle user notes
                    EmbedIntersectingUserNotes( ref htmlStream, userNotes );

                    if( ListType == ListTypeNumbered )
                    {
                        htmlStream += "</ol>";
                    }
                    else
                    {
                        htmlStream += "</ul>";
                    }
                    // closing markup
                }

                public static bool ElementTagMatches(string elementTag)
                {
                    if ( elementTag == "L" || elementTag == "List" )
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
