using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreLocation;
using MapKit;
using App.Shared.Config;
using CoreGraphics;
using System.Collections.Generic;
using App.Shared.Network;
using Rock.Mobile.Util.Strings;
using System.Collections;
using App.Shared;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using App.Shared.Strings;
using App.Shared.Analytics;
using Rock.Mobile.Animation;
using App.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared.PrivateConfig;

namespace iOS
{
	partial class GroupFinderViewController : TaskUIViewController
	{
        public GroupFinderViewController (IntPtr handle) : base (handle)
        {
        }

        public class TableSource : UITableViewSource 
        {
            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class GroupCell : UITableViewCell
            {
                public static string Identifier = "GroupCell";

                public TableSource TableSource { get; set; }

                public UILabel Title { get; set; }
                public UILabel MeetingTime { get; set; }
                public UILabel Distance { get; set; }
                public UIButton JoinButton { get; set; }

                public UIView Seperator { get; set; }

                public int RowIndex { get; set; }

                public GroupCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Title = new UILabel( );
                    Title.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Medium_FontSize );
                    Title.Layer.AnchorPoint = CGPoint.Empty;
                    Title.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Title.BackgroundColor = UIColor.Clear;
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Title );

                    MeetingTime = new UILabel( );
                    MeetingTime.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
                    MeetingTime.Layer.AnchorPoint = CGPoint.Empty;
                    MeetingTime.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    MeetingTime.BackgroundColor = UIColor.Clear;
                    AddSubview( MeetingTime );

                    Distance = new UILabel( );
                    Distance.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
                    Distance.Layer.AnchorPoint = CGPoint.Empty;
                    Distance.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Distance.BackgroundColor = UIColor.Clear;
                    AddSubview( Distance );

                    JoinButton = UIButton.FromType( UIButtonType.Custom );
                    JoinButton.TouchUpInside += (object sender, EventArgs e) => { TableSource.RowButtonClicked( RowIndex ); };
                    JoinButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateConnectConfig.GroupFinder_Join_IconSize );
                    JoinButton.SetTitle( PrivateConnectConfig.GroupFinder_JoinIcon, UIControlState.Normal );
                    JoinButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
                    JoinButton.Layer.AnchorPoint = CGPoint.Empty;
                    JoinButton.BackgroundColor = UIColor.Clear;
                    JoinButton.SizeToFit( );
                    AddSubview( JoinButton );

                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                }
            }

            GroupFinderViewController Parent { get; set; }

            nfloat PendingCellHeight { get; set; }

            public int SelectedIndex { get; set; }

            public TableSource (GroupFinderViewController parent )
            {
                Parent = parent;
                SelectedIndex = -1;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return Parent.GroupEntries.Count + 1;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return GetCachedRowHeight( tableView, indexPath );
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                return GetCachedRowHeight( tableView, indexPath );
            }

            nfloat GetCachedRowHeight( UITableView tableView, NSIndexPath indexPath )
            {
                if ( PendingCellHeight > 0 )
                {
                    return PendingCellHeight;
                }

                // If we don't have the cell's height yet (first render), return the table's height
                return tableView.Frame.Height;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                if ( indexPath.Row < Parent.GroupEntries.Count )
                {
                    GroupCell cell = tableView.DequeueReusableCell( GroupCell.Identifier ) as GroupCell;

                    // if there are no cells to reuse, create a new one
                    if (cell == null)
                    {
                        cell = new GroupCell( UITableViewCellStyle.Default, GroupCell.Identifier );
                        cell.TableSource = this;

                        // take the parent table's width so we inherit its width constraint
                        cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                        // remove the selection highlight
                        cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                        cell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
                    }

                    // if it's the group nearest the user, color it different. (we always sort by distance)
                    if ( SelectedIndex == indexPath.Row )
                    {
                        cell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                    }
                    else
                    {
                        cell.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
                    }

                    cell.RowIndex = indexPath.Row;

                    // Create the title
                    cell.Title.Text = Parent.GroupEntries[ indexPath.Row ].Title;
                    cell.Title.SizeToFit( );

                    // Meeting time - If it isn't set, just blank it out and we wont' show anything for that row.
                    if ( string.IsNullOrEmpty( Parent.GroupEntries[ indexPath.Row ].MeetingTime ) == false )
                    {
                        cell.MeetingTime.Text = Parent.GroupEntries[ indexPath.Row ].MeetingTime;
                    }
                    else
                    {
                        cell.MeetingTime.Text = ConnectStrings.GroupFinder_ContactForTime;
                    }
                    cell.MeetingTime.SizeToFit( );

                    // Distance
                    cell.Distance.Text = string.Format( "{0:##.0} {1}", Parent.GroupEntries[ indexPath.Row ].Distance, ConnectStrings.GroupFinder_MilesSuffix );
                    if ( indexPath.Row == 0 )
                    {
                        cell.Distance.Text += " " + ConnectStrings.GroupFinder_ClosestTag;
                    }
                    cell.Distance.SizeToFit( );

                    // Position the Title & Address in the center to the right of the image
                    cell.Title.Frame = new CGRect( 10, 5, cell.Frame.Width - 5, cell.Title.Frame.Height );
                    cell.MeetingTime.Frame = new CGRect( 10, cell.Title.Frame.Bottom, cell.Frame.Width - 5, cell.MeetingTime.Frame.Height + 5 );
                    cell.Distance.Frame = new CGRect( 10, cell.MeetingTime.Frame.Bottom - 6, cell.Frame.Width - 5, cell.Distance.Frame.Height + 5 );

                    // add the seperator to the bottom
                    cell.Seperator.Frame = new CGRect( 0, cell.Distance.Frame.Bottom + 5, cell.Bounds.Width, 1 );

                    PendingCellHeight = cell.Seperator.Frame.Bottom;

                    cell.JoinButton.Frame = new CGRect( cell.Bounds.Width - cell.JoinButton.Bounds.Width, 
                        ( PendingCellHeight - cell.JoinButton.Bounds.Height ) / 2, 
                        cell.JoinButton.Bounds.Width, 
                        cell.JoinButton.Bounds.Height );

                    return cell;
                }
                else
                {
                    // simply create a dummy cell that acts as padding
                    UITableViewCell cell = tableView.DequeueReusableCell( "dummy" ) as GroupCell;

                    // if there are no cells to reuse, create a new one
                    if (cell == null)
                    {
                        cell = new UITableViewCell( UITableViewCellStyle.Default, "dummy" );

                        // take the parent table's width so we inherit its width constraint
                        cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, 44 );

                        // remove the selection highlight
                        cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                        cell.BackgroundColor = UIColor.Clear;
                    }

                    return cell;
                }
            }

            public void RowButtonClicked( int row )
            {
                Parent.RowButtonClicked( row );
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                // if they clicked a valid row (and not the dummy)
                if ( indexPath.Row < Parent.GroupEntries.Count )
                {
                    SetSelectedRow( tableView, indexPath.Row );

                    // notify the parent
                    Parent.RowClicked( indexPath.Row );
                }
            }

            public void SetSelectedRow( UITableView tableView, int row )
            {
                if ( row != SelectedIndex )
                {
                    tableView.BeginUpdates( );

                    // setup a list with the rows that need to be redrawn
                    List<NSIndexPath> rowIndices = new List<NSIndexPath>();

                    // if there was previously a row selected, add it to our list
                    // so it'll be deselected
                    if ( SelectedIndex > -1 )
                    {
                        rowIndices.Add( NSIndexPath.FromRowSection( SelectedIndex, 0 ) );
                    }


                    // setup the newly selected index
                    SelectedIndex = row;
                    NSIndexPath activeIndex = NSIndexPath.FromRowSection( SelectedIndex, 0 );
                    rowIndices.Add( activeIndex );

                    // force a redraw on the row(s) so their selection state is updated
                    tableView.ReloadRows( rowIndices.ToArray( ), UITableViewRowAnimation.Fade );

                    tableView.EndUpdates( );


                    // make sure the newly selected row comes fully into view
                    tableView.ScrollToRow( activeIndex, UITableViewScrollPosition.Top, true );
                }
            }
        }

        List<GroupFinder.GroupEntry> GroupEntries { get; set; }
        GroupFinder.GroupEntry SourceLocation { get; set; }

        public bool GroupListUpdated { get; set; }

        UITableView GroupFinderTableView { get; set; }
        GroupFinderViewController.TableSource GroupTableSource { get; set; }

        UIBlockerView BlockerView { get; set; }

        public MKMapView MapView { get; set; }

        public UIView SearchResultsBGLayer { get; set; }
        public UILabel SearchResultsPrefix { get; set; }
        public UILabel SearchResultsNeighborhood { get; set; }
        public UIView Seperator { get; set; }

        UIButton SearchAddressButton { get; set; }

        UIGroupFinderSearch SearchPage { get; set; }

        bool Searching { get; set; }

        // store the values they type in so that if they leave the page and return, we can re-populate them.
        string StreetValue { get; set; }
        string CityValue { get; set; }
        string StateValue { get; set; }
        string ZipValue { get; set; }

        class MapViewDelegate : MKMapViewDelegate
        {
            public GroupFinderViewController Parent { get; set; }

            public override void DidSelectAnnotationView(MKMapView mapView, MKAnnotationView view)
            {
                Parent.AnnotationSelected( view );
            }

            static string AnnotationID = "pinID";
            public override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
            {
                MKPinAnnotationView pinView = (MKPinAnnotationView) mapView.DequeueReusableAnnotation( AnnotationID );
                if ( pinView == null )
                {
                    pinView = new MKPinAnnotationView( annotation, AnnotationID );
                    pinView.CanShowCallout = true;
                }

                // are we rendering the source location?
                if ( annotation.Coordinate.Latitude == Parent.SourceLocation.Latitude &&
                     annotation.Coordinate.Longitude == Parent.SourceLocation.Longitude )
                {
                    pinView.PinColor = MKPinAnnotationColor.Green;
                }
                else
                {
                    pinView.PinColor = MKPinAnnotationColor.Red;
                }

                return pinView;
            }

            public override void DidAddAnnotationViews(MKMapView mapView, MKAnnotationView[] views)
            {
                Rock.Mobile.Util.Debug.WriteLine( "Done" );
            }

            public override void DidFinishRenderingMap(MKMapView mapView, bool fullyRendered)
            {
                Rock.Mobile.Util.Debug.WriteLine( "Done" );
            }

            public override void RegionChanged(MKMapView mapView, bool animated)
            {
                Parent.RegionChanged( );
            }
        }

        /// <summary>
        /// Simple class to inset the text of our text fields.
        /// </summary>
        public class UIInsetTextField : UITextField
        {
            public override CGRect TextRect(CGRect forBounds)
            {
                return new CGRect( forBounds.X + 5, forBounds.Y, forBounds.Width - 5, forBounds.Height );
            }

            public override CGRect EditingRect(CGRect forBounds)
            {
                return new CGRect( forBounds.X + 5, forBounds.Y, forBounds.Width - 5, forBounds.Height );
            }
        }

        /// <summary>
        /// Delegate for our address field. When returning, notify the primary cell's parent that this was clicked.
        /// </summary>
        class AddressDelegate : UITextFieldDelegate
        {
            public GroupFinderViewController Parent { get; set; }

            public override bool ShouldReturn(UITextField textField)
            {
                Parent.ShouldReturn( );

                textField.ResignFirstResponder( );
                return true;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( App.Shared.Config.ControlStylingConfig.BackgroundColor );

            // setup everything except positioning, which will happen in LayoutChanged()
            SourceLocation = null;
            GroupEntries = new List<GroupFinder.GroupEntry>();

            SearchAddressButton = UIButton.FromType( UIButtonType.System );
            View.AddSubview( SearchAddressButton );
            SearchAddressButton.Layer.AnchorPoint = CGPoint.Empty;
            ControlStyling.StyleButton( SearchAddressButton, ConnectStrings.GroupFinder_SearchButtonLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            SearchAddressButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    SearchPage.Show( );
                    Task.NavToolbar.Reveal( false );
                };


            MapView = new MKMapView( );
            View.AddSubview( MapView );

            // set the default position for the map to whatever specified area.
            MKCoordinateRegion region = MKCoordinateRegion.FromDistance( new CLLocationCoordinate2D( 
                ConnectConfig.GroupFinder_DefaultLatitude, 
                ConnectConfig.GroupFinder_DefaultLongitude ), 
                ConnectConfig.GroupFinder_DefaultScale_iOS, 
                ConnectConfig.GroupFinder_DefaultScale_iOS );
            MapView.SetRegion( region, true );

            MapView.Layer.AnchorPoint = new CGPoint( 0, 0 );
            MapView.Delegate = new MapViewDelegate() { Parent = this };

            SearchResultsBGLayer = new UIView();
            View.AddSubview( SearchResultsBGLayer );
            SearchResultsBGLayer.Layer.AnchorPoint = new CGPoint( 0, 0 );
            SearchResultsBGLayer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

            SearchResultsPrefix = new UILabel( );
            View.AddSubview( SearchResultsPrefix );
            SearchResultsPrefix.Layer.AnchorPoint = new CGPoint( 0, 0 );
            SearchResultsPrefix.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            SearchResultsPrefix.Text = ConnectStrings.GroupFinder_NoGroupsFound;
            SearchResultsPrefix.SizeToFit( );
            SearchResultsPrefix.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            SearchResultsPrefix.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            SearchResultsPrefix.TextAlignment = UITextAlignment.Center;

            SearchResultsNeighborhood = new UILabel( );
            View.AddSubview( SearchResultsNeighborhood );
            SearchResultsNeighborhood.Layer.AnchorPoint = new CGPoint( 0, 0 );
            SearchResultsNeighborhood.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            SearchResultsNeighborhood.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            SearchResultsNeighborhood.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            SearchResultsNeighborhood.TextAlignment = UITextAlignment.Center;


            Seperator = new UIView( );
            View.AddSubview( Seperator );
            Seperator.Layer.BorderWidth = 1;
            Seperator.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ).CGColor;


            GroupFinderTableView = new UITableView();
            View.AddSubview( GroupFinderTableView );
            GroupTableSource = new GroupFinderViewController.TableSource( this );

            // add the table view and source
            GroupFinderTableView.BackgroundColor = UIColor.Clear;//Rock.Mobile.UI.Util.GetUIColor( App.Shared.Config.ControlStylingConfig.Table_Footer_Color );
            //GroupFinderTableView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( App.Shared.Config.ControlStylingConfig.BG_Layer_Color );
            GroupFinderTableView.Source = GroupTableSource;
            GroupFinderTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            BlockerView = new UIBlockerView( View, View.Frame.ToRectF( ) );

            SearchPage = new UIGroupFinderSearch();

            SearchPage.Create( View, View.Frame.ToRectF( ), 
                delegate
                {
                    SearchPage.Hide( true );
                    GetGroups( SearchPage.Street.Text, SearchPage.City.Text, SearchPage.State.Text, SearchPage.ZipCode.Text );
                    Task.NavToolbar.Reveal( true );
                } );
            SearchPage.SetTitle( ConnectStrings.GroupFinder_SearchPageHeader, ConnectStrings.GroupFinder_SearchPageDetails );
            SearchPage.Hide( false );

            // don't allow them to tap the address button until we reveal the search page.
            SearchAddressButton.Enabled = false;

            // wait a couple seconds before revealing the search page.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = false;
            timer.Interval = 1000;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                {
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            SearchAddressButton.Enabled = true;
                            SearchPage.Show( );                
                        } );
                };
            timer.Start( );

            // hook in delegates so we can handle return
            ((UITextField)SearchPage.Street.PlatformNativeObject).Delegate = new AddressDelegate( ) { Parent = this };
            ((UITextField)SearchPage.City.PlatformNativeObject).Delegate = new AddressDelegate( ) { Parent = this };
            ((UITextField)SearchPage.State.PlatformNativeObject).Delegate = new AddressDelegate( ) { Parent = this };
            ((UITextField)SearchPage.ZipCode.PlatformNativeObject).Delegate = new AddressDelegate( ) { Parent = this };
        }

        public void ShouldReturn( )
        {
            SearchPage.ShouldReturn( );   
        }

        public override void LayoutChanged( )
        {
            base.LayoutChanged( );

            SearchPage.LayoutChanged( View.Frame.ToRectF( ) );

            // Map
            MapView.Frame = new CGRect( 0, 0, View.Frame.Width, View.Frame.Height * .40f );

            SearchAddressButton.Frame = new CGRect( 0, MapView.Frame.Bottom, View.Frame.Width, 33 );

            // Search Results Banner
            UpdateResultsBanner( );

            // add the seperator to the bottom
            Seperator.Frame = new CGRect( 0, SearchResultsPrefix.Frame.Bottom - 1, View.Bounds.Width, 1 );

            // wait to layout the table view until all subviews have been laid out. Fixes an issue where the table gets more height than it should,
            // and the last row doesn't fit on screen.
            GroupFinderTableView.Frame = new CGRect( 0, Seperator.Frame.Bottom, View.Bounds.Width, View.Bounds.Height - Seperator.Frame.Bottom );
            GroupFinderTableView.ReloadData( );

            BlockerView.SetBounds( View.Frame.ToRectF( ) );
        }

        public void UpdateMap( bool result )
        {
            GroupTableSource.SelectedIndex = -1;

            // remove existing annotations
            MapView.RemoveAnnotations( MapView.Annotations );

            // set the search results banner appropriately
            if ( GroupEntries.Count > 0 )
            {
                SearchResultsPrefix.Text = ConnectStrings.GroupFinder_Neighborhood;
                SearchResultsNeighborhood.Text = GroupEntries[ 0 ].NeighborhoodArea;

                // add an annotation for each position found in the group
                List<IMKAnnotation> annotations = new List<IMKAnnotation>();

                // add an annotation for the source
                MKPointAnnotation sourceAnnotation = new MKPointAnnotation();
                sourceAnnotation.SetCoordinate( new CLLocationCoordinate2D( SourceLocation.Latitude, SourceLocation.Longitude ) );
                sourceAnnotation.Title = "";
                sourceAnnotation.Subtitle = "";
                annotations.Add( sourceAnnotation );

                foreach ( GroupFinder.GroupEntry entry in GroupEntries )
                {
                    MKPointAnnotation annotation = new MKPointAnnotation();
                    annotation.SetCoordinate( new CLLocationCoordinate2D( entry.Latitude, entry.Longitude ) );
                    annotation.Title = entry.Title;
                    annotation.Subtitle = string.Format( "{0:##.0} {1}", entry.Distance, ConnectStrings.GroupFinder_MilesSuffix );
                    annotations.Add( annotation );
                }
                MapView.ShowAnnotations( annotations.ToArray( ), true );
            }
            else
            {
                if ( result == true )
                {
                    SearchResultsPrefix.Text = ConnectStrings.GroupFinder_NoGroupsFound;
                    SearchResultsNeighborhood.Text = string.Empty;

                    // since there were no groups, revert the map to whatever specified area
                    MKCoordinateRegion region = MKCoordinateRegion.FromDistance( new CLLocationCoordinate2D( 
                                                        ConnectConfig.GroupFinder_DefaultLatitude, 
                                                        ConnectConfig.GroupFinder_DefaultLongitude ), 
                                                    ConnectConfig.GroupFinder_DefaultScale_iOS, 
                                                    ConnectConfig.GroupFinder_DefaultScale_iOS );
                    MapView.SetRegion( region, true );
                }
                else
                {
                    SearchResultsPrefix.Text = ConnectStrings.GroupFinder_NetworkError;
                    SearchResultsNeighborhood.Text = string.Empty;
                }
            }

            UpdateResultsBanner( );
        }

        void UpdateResultsBanner( )
        {
            // Search Results Banner
            SearchResultsPrefix.SizeToFit( );
            SearchResultsNeighborhood.SizeToFit( );

            // now center the search result / neighborhood
            nfloat resultTotalWidth = SearchResultsPrefix.Bounds.Width + SearchResultsNeighborhood.Bounds.Width;
            nfloat xStartPos = ( View.Bounds.Width - resultTotalWidth ) / 2;

            SearchResultsBGLayer.Frame = new CGRect( 0, SearchAddressButton.Frame.Bottom, View.Frame.Width, SearchResultsPrefix.Frame.Height );
            SearchResultsPrefix.Frame = new CGRect( xStartPos, SearchAddressButton.Frame.Bottom, SearchResultsPrefix.Frame.Width, SearchResultsPrefix.Frame.Height );
            SearchResultsNeighborhood.Frame = new CGRect( SearchResultsPrefix.Frame.Right, SearchAddressButton.Frame.Bottom, SearchResultsNeighborhood.Frame.Width, SearchResultsNeighborhood.Frame.Height );
        }

        public void RowButtonClicked( int row )
        {
            // create the view controller
            GroupFinderJoinViewController joinController = new GroupFinderJoinViewController();

            // set the group info
            GroupFinder.GroupEntry currGroup = GroupEntries[ row ];
            joinController.GroupTitle = currGroup.Title;
            joinController.MeetingTime = string.IsNullOrEmpty( currGroup.MeetingTime ) == false ? currGroup.MeetingTime : ConnectStrings.GroupFinder_ContactForTime;
            joinController.GroupID = currGroup.Id;

            joinController.Distance = string.Format( "{0:##.0} {1}", currGroup.Distance, ConnectStrings.GroupFinder_MilesSuffix );
            if ( row == 0 )
            {
                joinController.Distance += " " + ConnectStrings.GroupFinder_ClosestTag;
            }

            // launch the view
            Task.PerformSegue( this, joinController );
        }

        public void RowClicked( int row )
        {
            // if they selected a group in the list, center it on the map.
            if ( MapView.Annotations.Length > 0 )
            {
                // use the row index to get the matching annotation in the map
                IMKAnnotation marker = TableRowToMapAnnotation( row );

                // select it and set it as the center coordinate
                MapView.SelectedAnnotations = new IMKAnnotation[1] { marker };
                MapView.SetCenterCoordinate( marker.Coordinate, true );
            }
        }

        IMKAnnotation TableRowToMapAnnotation( int row )
        {
            GroupFinder.GroupEntry selectedGroup = GroupEntries[ row ];
            CLLocationCoordinate2D selectedCoord = new CLLocationCoordinate2D( selectedGroup.Latitude, selectedGroup.Longitude );

            // given the row index of a group entry, return its associated annotation
            // select the matching marker
            foreach ( IMKAnnotation marker in MapView.Annotations )
            {
                if ( marker.Coordinate.Latitude == selectedCoord.Latitude || 
                     marker.Coordinate.Longitude == selectedCoord.Longitude )
                {
                    return marker;
                }
            }

            return null;
        }

        int MapAnnotationToTableRow( IMKAnnotation marker )
        {
            // given a map annotation, we'll go thru each group entry and compare the coordinates
            // to find the matching location.

            for ( int i = 0; i < GroupEntries.Count; i++ )
            {
                // find the row index by matching coordinates
                GroupFinder.GroupEntry currGroup = GroupEntries[ i ];
                CLLocationCoordinate2D currCoord = new CLLocationCoordinate2D( currGroup.Latitude, currGroup.Longitude );

                if ( marker.Coordinate.Latitude == currCoord.Latitude &&
                     marker.Coordinate.Longitude == currCoord.Longitude )
                {
                    return i;
                }
            }

            return -1;
        }

        public void AnnotationSelected( MKAnnotationView annotationView )
        {
            // first select (center) the annotation)
            MapView.SetCenterCoordinate( annotationView.Annotation.Coordinate, true );

            // now determine where it is in the group list
            int rowIndex = MapAnnotationToTableRow( annotationView.Annotation );

            // and select that row
            GroupTableSource.SetSelectedRow( GroupFinderTableView, rowIndex );
        }

        public void RegionChanged( )
        {
            // called when we're done focusing on a new area
            if ( GroupListUpdated == true )
            {
                GroupListUpdated = false;
                RowClicked( 0 );
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            //base.TouchesEnded(touches, evt);

            SearchPage.TouchesEnded( );
        } 

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // store the values they type in so that if they leave the page and return, we can re-populate them.
            StreetValue = SearchPage.Street.Text;
            CityValue = SearchPage.City.Text;
            StateValue = SearchPage.State.Text;
            ZipValue = SearchPage.ZipCode.Text;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // see if there's an address for this person that we can automatically use.
            if ( RockMobileUser.Instance.HasFullAddress( ) == true )
            {
                // only use it if a last value wasn't provided.
                if ( string.IsNullOrEmpty( StreetValue ) == true &&
                     string.IsNullOrEmpty( CityValue ) == true &&
                     string.IsNullOrEmpty( StateValue ) == true &&
                     string.IsNullOrEmpty( ZipValue ) == true )
                {
                    SearchPage.SetAddress( RockMobileUser.Instance.Street1( ), RockMobileUser.Instance.City( ), RockMobileUser.Instance.State( ), RockMobileUser.Instance.Zip( ) );
                }
                else
                {
                    // otherwise use whatever they last entered (and for state, use the default if nothing was entered
                    SearchPage.SetAddress( StreetValue, CityValue, string.IsNullOrEmpty( StateValue ) ? ConnectStrings.GroupFinder_DefaultState : StateValue, ZipValue );
                }
            }
            else
            {
                // populate with the last used values. If state is empty, use the default.
                SearchPage.SetAddress( StreetValue, CityValue, string.IsNullOrEmpty( StateValue ) ? ConnectStrings.GroupFinder_DefaultState : StateValue, ZipValue );
            }
        }

        void GetGroups( string street, string city, string state, string zip )
        {
            if ( string.IsNullOrEmpty( street ) == false &&
                 string.IsNullOrEmpty( city ) == false &&
                 string.IsNullOrEmpty( state ) == false &&
                 string.IsNullOrEmpty( zip ) == false )
            {
                if ( Searching == false )
                {
                    Searching = true;

                    BlockerView.Show( delegate
                        {
                            GroupFinder.GetGroups( street, city, state, zip, 
                                delegate( GroupFinder.GroupEntry sourceLocation, List<GroupFinder.GroupEntry> groupEntries, bool result )
                                {
                                    BlockerView.Hide( delegate
                                        {
                                            Searching = false;

                                            groupEntries.Sort( delegate(GroupFinder.GroupEntry x, GroupFinder.GroupEntry y )
                                                {
                                                    return x.Distance < y.Distance ? -1 : 1;
                                                } );

                                            SourceLocation = sourceLocation;
                                            GroupEntries = groupEntries;
                                            UpdateMap( result );
                                            GroupFinderTableView.ReloadData( );

                                            // flag that our group list was updated so that
                                            // on the region updated callback from the map, we 
                                            // can select the appropriate group
                                            GroupListUpdated = true;

                                            // and record an analytic for the neighborhood that this location was apart of. This helps us know
                                            // which neighborhoods get the most hits.
                                            string address = street + " " + city + ", " + state + ", " + zip;

                                            // send an analytic if the request went thru ok
                                            if( result )
                                            {
                                                if ( groupEntries.Count > 0 )
                                                {
                                                    // record an analytic that they searched
                                                    GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.Location, address );

                                                    GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.Neighborhood, groupEntries[ 0 ].NeighborhoodArea );
                                                }
                                                else
                                                {
                                                    // record that this address failed
                                                    GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.OutOfBounds, address );
                                                }
                                            }
                                        } );
                                } );
                        } );
                }
            }
        }
	}
}
