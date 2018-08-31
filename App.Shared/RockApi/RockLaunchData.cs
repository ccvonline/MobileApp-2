using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;
using MobileApp.Shared.Notes.Model;
using RestSharp;
using MobileApp.Shared.Config;
using Rock.Mobile.IO;
using MobileApp;
using Rock.Mobile.Util.Strings;
using Newtonsoft.Json.Linq;

namespace MobileApp
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Stores data that can safely be used as placeholders for
            /// areas of the app that normally require a network connection.
            /// </summary>
            public sealed class RockLaunchData
            {
                private static RockLaunchData _Instance = new RockLaunchData( );
                public static RockLaunchData Instance { get { return _Instance; } }

                const string LAUNCH_DATA_FILENAME = "mobilelaunchdata.dat";

                // wrapper for managing the data obtained at launch
                public class LaunchData
                {
                    public LaunchData( )
                    {
                        //ALWAYS INCREMENT THIS IF UPDATING THE MODEL
                        ClientModelVersion = 3;
                        //

                        Campuses = new List<Rock.Client.Campus>( );
                        PrayerCategories = new List< KeyValuePair<string, int> >( );

                        Genders = new List<string>( );
                        Genders.Add( "Unknown" );
                        Genders.Add( "Male" );
                        Genders.Add( "Female" );

                        // in debug builds, turn developer mode on by default
                        #if DEBUG
                        DeveloperModeEnabled = true;
                        #endif

                        News = new List<RockNews>( );
                        NoteDB = new NoteDB( );

                        // for the hardcoded news, leave OFF the image extensions, so that we can add them with scaling for iOS.
                        UpgradeNewsItem = new RockNews( 
                            NewsConfig.UpgradeNews[ 0 ], 

                            NewsConfig.UpgradeNews[ 1 ],

                            NewsConfig.UpgradeNews[ 2 ],

                            true,
                            false,
                            false,

                            "",
                            NewsConfig.UpgradeNews[ 3 ],

                            new List<System.Guid>( ) );
                    }

                    /// <summary>
                    /// Copies the embedded upgrade images into the file cache so that they can be loaded like normal news item images.
                    /// </summary>
                    public void TryCacheUpgradeNewsImages( )
                    {
                        // cache the compiled in main and header images so the News system can get them transparently
                        #if __IOS__
                        string mainImageName;
                        if( UIKit.UIScreen.MainScreen.Scale > 1 )
                        {
                            mainImageName = string.Format( "{0}/{1}@{2}x.png", Foundation.NSBundle.MainBundle.BundlePath, UpgradeNewsItem.ImageName, UIKit.UIScreen.MainScreen.Scale );
                        }
                        else
                        {
                            mainImageName = string.Format( "{0}/{1}.png", Foundation.NSBundle.MainBundle.BundlePath, UpgradeNewsItem.ImageName );
                        }

                        #elif __ANDROID__
                        string mainImageName = UpgradeNewsItem.ImageName + ".png";
                        #else
                        string mainImageName = string.Empty;
                        string headerImageName = string.Empty;
                        #endif

                        // cache the main image if it's not already there
                        if( FileCache.Instance.FileExists( UpgradeNewsItem.ImageName ) == false )
                        {
                            MemoryStream stream = Rock.Mobile.IO.AssetConvert.AssetToStream( mainImageName );
                            stream.Position = 0;
                            FileCache.Instance.SaveFile( stream, UpgradeNewsItem.ImageName, FileCache.CacheFileNoExpiration );
                            stream.Dispose( );
                        }
                    }

                    /// <summary>
                    /// Private to the client, this should be updated if the model
                    /// changes at all, so that we don't attempt to load an older one when upgrading the app.
                    /// </summary>
                    [JsonProperty]
                    public int ClientModelVersion { get; protected set; }

                    public Rock.Client.Campus CampusFromId( int campusId )
                    {
                        return Campuses.Find( c => c.Id == campusId );
                    }

                    /// <summary>
                    /// Helper method for converting a campus' id to its name
                    /// </summary>
                    /// <returns>The identifier to name.</returns>
                    public string CampusIdToName( int campusId )
                    {
                        // guard against old, bad values.
                        Rock.Client.Campus campusObj = Campuses.Find( c => c.Id == campusId );
                        return campusObj != null ? campusObj.Name : "";
                    }

                    /// <summary>
                    /// Helper method for converting a campus' guid to its name
                    /// </summary>
                    /// <returns>The identifier to name.</returns>
                    public string CampusGuidToName( Guid campusGuid )
                    {
                        // guard against old, bad values.
                        Rock.Client.Campus campusObj = Campuses.Find( c => c.Guid == campusGuid );
                        return campusObj != null ? campusObj.Name : "";
                    }

                    /// <summary>
                    /// Helper method for converting a campus' name to its ID
                    /// </summary>
                    /// <returns>The name to identifier.</returns>
                    public int CampusNameToId( string campusName )
                    {
                        Rock.Client.Campus campusObj = Campuses.Find( c => c.Name == campusName );
                        return campusObj != null ? campusObj.Id : 0;
                    }

                    /// <summary>
                    /// Helper method for converting a prayer category ID to its name
                    /// </summary>
                    public string PrayerIdToCategory( int categoryId )
                    {
                        // guard against old, bad values.
                        KeyValuePair<string, int> categoryObj = PrayerCategories.Find( c => c.Value == categoryId );
                        return string.IsNullOrWhiteSpace( categoryObj.Key ) == false ? categoryObj.Key : string.Empty;
                    }

                    /// <summary>
                    /// Helper method for converting a prayer category name to its ID
                    /// </summary>
                    public int PrayerCategoryToId( string categoryName )
                    {
                        KeyValuePair<string, int> categoryObj = PrayerCategories.Find( c => c.Key == categoryName );
                        return string.IsNullOrWhiteSpace( categoryObj.Key ) == false ? categoryObj.Value : -1;
                    }

                    /// <summary>
                    /// Default news to display when there's no connection available
                    /// </summary>
                    /// <value>The news.</value>
                    public List<RockNews> News { get; set; }

                    /// <summary>
                    /// The campaign to display to a user based on their persona
                    /// </summary>
                    public RockNews PECampaign { get; set; }

                    /// <summary>
                    /// The core object that stores info about the sermon notes.
                    /// </summary>
                    public NoteDB NoteDB { get; set; }

                    /// <summary>
                    /// The last time the noteDB was downloaded. This helps us know whether to
                    /// update it or not, in case the user hasn't quit the app in days.
                    /// </summary>
                    public DateTime NoteDBTimeStamp { get; set; }

                    /// <summary>
                    // The "Please Upgrade" news item we'll show if they're out of date.
                    /// </summary>
                    public RockNews UpgradeNewsItem { get; set; }

                    /// <summary>
                    /// List of all available campuses to choose from.
                    /// </summary>
                    /// <value>The campuses.</value>
                    public List<Rock.Client.Campus> Campuses { get; set; }

                    /// <summary>
                    /// List of genders
                    /// </summary>
                    /// <value>The genders.</value>
                    public List<string> Genders { get; set; }

                    /// <summary>
                    /// Default list of prayer categories supported
                    /// </summary>
                    public List<KeyValuePair<string, int>> PrayerCategories { get; set; }

                    /// <summary>
                    /// True if this version of the app is out-of-date. We can know this
                    /// by comparing what we get in LaunchData with the version that's harded coded in the app. Yeeeah!
                    /// </summary>
                    public bool NeedsUpgrade { get; set; }

                    /// <summary>
                    /// Enables debug features like note refreshing.
                    /// </summary>
                    public bool DeveloperModeEnabled { get; set; }
                }
                public LaunchData Data { get; set; }

                /// <summary>
                /// Ensures that the images for "built in" news items are in the file cache.
                /// They need to be here so the news system can find / load them like 'normal' news items.
                /// </summary>
                public void TryCacheEmbeddedNewsImages( )
                {
                    Data.TryCacheUpgradeNewsImages( );
                }

                /// <summary>
                /// True if the notedb.xml is in the process of being downloaded. This is so that
                /// if the user visits Messages WHILE we're downloading, we can wait instead of requesting it.
                /// </summary>
                public bool RequestingNoteDB { get; private set; }

                public RockLaunchData( )
                {
                    Data = new LaunchData( );
                }

                /// <summary>
                /// The news UI should immediatley hook into this on launch so we can notify when news is ready for display.
                /// NOT CURRENTLY USING IT. ONLY NEEDED IF WE WANT TO UPDATE THE NEWS _WHILE_ THE USER IS SITTING ON THE NEWS PAGE.
                /// </summary>
                public delegate void NewsItemsDownloaded( );
                public NewsItemsDownloaded NewsItemsDownloadedCallback { get; set; }

                /// <summary>
                /// Wrapper function for getting the basic things we need at launch (campuses, prayer categories, news, notes, etc.)
                /// If for some reason one of these fails, they will be called independantly by the appropriate systems
                /// (So if NoteDB fails, GetNoteDB will be called by Messages when the user taps on it)
                /// </summary>
                public void GetLaunchData( int? personId, HttpRequest.RequestResult launchDataResult )
                {
                    Rock.Mobile.Util.Debug.WriteLine( "Get LaunchData" );

                    MobileAppApi.Get_LaunchData( 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, MobileAppApi.LaunchData launchData )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true && launchData != null )
                            {
                                Data.Campuses = launchData.Campuses;
                                Data.PrayerCategories = launchData.PrayerCategories;
                                
                                // is our version up-to-date? 
                                if( GeneralConfig.Version < launchData.MobileAppVersion )
                                {
                                    // nope, so flag that and we can remind people to upgrade.
                                    Data.NeedsUpgrade = true;
                                }
                                else
                                {
                                    Data.NeedsUpgrade = false;
                                }

                                // now get the news.
                                GetNews( delegate 
                                {
                                    // now get the campaign for the user
                                    GetPECampaign( personId, delegate
                                    {
                                        // chain any other required launch data actions here.
                                        Rock.Mobile.Util.Debug.WriteLine( "Get LaunchData DONE" );

                                        // notify the caller now that we're done
                                        if( launchDataResult != null )
                                        {
                                            // send OK, because whether we failed or not, the caller doessn't need to care.
                                            launchDataResult( System.Net.HttpStatusCode.OK, "" );
                                        }
                                    });
                                });
                            }
                            else
                            {
                                // notify the caller now that we're done
                                if( launchDataResult != null )
                                {
                                    // send failed, and we are not gonna move on.
                                    launchDataResult( System.Net.HttpStatusCode.BadGateway, "" );
                                }
                            }
                        });   
                }

                void GetPECampaign( int? personId, HttpRequest.RequestResult resultCallback )
                {
                    MobileAppApi.GetPECampaign( personId,
                           delegate( System.Net.HttpStatusCode statusCode, string statusDescription, JArray responseBlob )
                           {
                               if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                               {
                                   //attempt to parse the response into a single item. If this fails, we'll stop and return nothing. 
                                   try
                                   {

                                       JObject campaignBlob = responseBlob.First?.ToObject<JObject>( );
                                       JObject contentBlob = JObject.Parse( campaignBlob["ContentJson"].ToString( ) );

                                       // check first for mobile specific versions of the content 
                                       // (note the use of the ? conditional member access)
                                       string title = contentBlob[ "mobile-app-title" ]?.ToString( );
                                       if ( String.IsNullOrWhiteSpace( title ) == true )
                                       {
                                           title = contentBlob["title"]?.ToString( );
                                       }

                                       string body = contentBlob[ "mobile-app-body" ]?.ToString( );
                                       if( String.IsNullOrWhiteSpace( body ) == true )
                                       {
                                           body = contentBlob[ "body" ]?.ToString( );
                                       }

                                       
                                       string linkUrl = contentBlob[ "mobile-app-link" ]?.ToString( );
                                       if ( String.IsNullOrWhiteSpace( linkUrl ) == true )
                                       {
                                           linkUrl = contentBlob[ "link" ]?.ToString( );
                                       }

                                       // make sure the detail url has a valid scheme / domain, and isn't just a relative url
                                       if( linkUrl?.StartsWith( "/", StringComparison.CurrentCulture ) == true )
                                       {
                                           linkUrl = GeneralConfig.RockBaseUrl + linkUrl;
                                       }

                                       // get the url for the image
                                       string imgUrl = contentBlob[ "mobile-app-img"]?.ToString( );
                                       string imageUrl = GeneralConfig.RockBaseUrl + imgUrl;


                                       // For the image, we'll cache it using the campaign's title as the filename, plus a version number.

                                       // strip the query param for the imageUrl, and use the version argument to control the versioning of the file.
                                       // This way, we can cache the image forever, and only update the image when it's actually changed on the server.
                                       // Example: https://rock.ccv.church/images/share.jpg?v=0 will save to the device as "share0.bin"
                                       // If v=0 becomes v=1, that will then turn into a new filename on the device and cause it to update.
                                       Uri imageUri = new Uri( imageUrl );                                    
                                       var queryParams = System.Web.HttpUtility.ParseQueryString( imageUri.Query );
                                       string imageVersion = queryParams.Get( "v" ) ?? "0";

                                       // build the image filename
                                       string imageCacheFileName = (title ?? "campaign-img") + imageVersion + ".bin";
                                       imageCacheFileName = imageCacheFileName.Replace( " ", "" ).ToLower( );

                                       bool detailUrlLaunchesBrowser = false;
                                       bool includeImpersonationToken = true;
                                       bool mobileAppSkipDetailsPage = false;

                                       RockNews newsItem = new RockNews( title,
                                                                         body,
                                                                         linkUrl,
                                                                         mobileAppSkipDetailsPage,
                                                                         detailUrlLaunchesBrowser,
                                                                         includeImpersonationToken,
                                                                         imageUrl,
                                                                         imageCacheFileName,
                                                                         new List<Guid>( ) ); //support all campuses


                                       Data.PECampaign = newsItem;
                                    }
                                    catch
                                    {
                                       //something about the response was bad. Rather than crash the entire app, let's just fail here.
                                       Rock.Mobile.Util.Debug.WriteLine( statusDescription = string.Format( "Getting PE campaigned failed: {0}", statusCode ) );
                                       statusCode = System.Net.HttpStatusCode.InternalServerError;
                                   }
                                   
                                   if ( resultCallback != null )
                                   {
                                      resultCallback( statusCode, statusDescription );
                                   }
                               }
                           });
                }

                void GetNews( HttpRequest.RequestResult resultCallback )
                {
                    MobileAppApi.GetNews( 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.ContentChannelItem> model )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                Rock.Mobile.Util.Debug.WriteLine( "Got news from Rock." );

                                // before comitting to this news, make sure there's at least one valid news item.
                                if( model.Count > 0 && model[ 0 ].AttributeValues != null )
                                {
                                    // sort it by priority
                                    model.Sort( delegate(Rock.Client.ContentChannelItem x, Rock.Client.ContentChannelItem y )
                                        {
                                            return x.Priority < y.Priority ? -1 : 1;
                                        } );
                                    
                                    // clear existing news
                                    Data.News.Clear( );

                                    // parse and take the new items
                                    foreach( Rock.Client.ContentChannelItem item in model )
                                    {
                                        // it's possible rock sent us bad data, so guard against any incomplete news items
                                        if( item.AttributeValues != null )
                                        {
                                            // we do this so we can store it on the stack and print it out if there's an exception.
                                            string currKey = "";

                                            try
                                            {
                                                currKey = "FeatureImage";
                                                string featuredGuid = item.AttributeValues[ currKey ].Value;
                                                string imageUrl = GeneralConfig.RockBaseUrl + "GetImage.ashx?Guid=" + featuredGuid;

                                                currKey = "DetailsURL";
                                                string detailUrl = item.AttributeValues[ currKey ].Value;

                                                currKey = "DetailsURLLaunchesBrowser";
                                                bool detailUrlLaunchesBrowser = bool.Parse( item.AttributeValues[ currKey ].Value );

                                                currKey = "IncludeImpersonationToken";
                                                bool includeImpersonationToken = bool.Parse( item.AttributeValues[ currKey ].Value );

                                                currKey = "MobileAppSkipDetailsPage";
                                                bool mobileAppSkipDetailsPage = bool.Parse( item.AttributeValues[ currKey ].Value );

                                                // take a list of the campuses that this news item should display for
                                                // (if the list is blank, we'll show it for all campuses)
                                                currKey = "Campuses";

                                                List<Guid> campusGuids = new List<Guid>( );
                                                if( item.AttributeValues[ currKey ] != null && string.IsNullOrEmpty( item.AttributeValues[ currKey ].Value ) == false )
                                                {
                                                    // this will be a comma-dilimited list of campuses to use for the news
                                                    string[] campusGuidList = item.AttributeValues[ currKey ].Value.Split( ',' );
                                                    foreach( string campusGuid in campusGuidList )
                                                    {
                                                        campusGuids.Add( Guid.Parse( campusGuid ) );
                                                    }
                                                }

                                                // Use the image guids, rather than news title, for the image.
                                                // This will ensure the image updates anytime it's changed in Rock!
                                                RockNews newsItem = new RockNews( item.Title, 
                                                                                  item.Content, 
                                                                                  detailUrl, 
                                                                                  mobileAppSkipDetailsPage,
                                                                                  detailUrlLaunchesBrowser,
                                                                                  includeImpersonationToken,
                                                                                  imageUrl, 
                                                                                  featuredGuid.AsLegalFilename( ) + ".bin",
                                                                                  campusGuids );


                                                // handle developer fields

                                                // do a quick check and see if this should be flagged 'private'
                                                bool newsPublic = IsNewsPublic( item );
                                                newsItem.Developer_Private = !newsPublic;

                                                newsItem.Developer_StartTime = item.StartDateTime;
                                                newsItem.Developer_EndTime = item.ExpireDateTime;
                                                newsItem.Developer_ItemStatus = item.Status;

                                                Data.News.Add( newsItem );
                                            }
                                            catch( Exception e )
                                            {
                                                // one of the attribute values we wanted wasn't there. Package up what WAS there and report
                                                // the error. We can then use process of elimination to fix it.
                                                Rock.Mobile.Util.Debug.WriteLine( string.Format( "News Item Exception. Attribute Value not found is: {0}. Full Exception {1}", currKey, e ) );

                                                // Xamarin Insights was able to report on exceptions. HockeyApp cannot as of Mar 2017.
                                                // When this functionality becomes available, we should implement it here in just Release Mode.
                                                // - CG

                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Rock.Mobile.Util.Debug.WriteLine( "News request failed." );
                            }

                            if ( resultCallback != null )
                            {
                                resultCallback( statusCode, statusDescription );
                            }
                        } );
                }

                bool IsNewsPublic( Rock.Client.ContentChannelItem newsItem )
                {
                    // if the start time is valid
                    if( newsItem.StartDateTime <= DateTime.Now )
                    {
                        // and its approvated
                        if( newsItem.Status == Rock.Client.Enums.ContentChannelItemStatus.Approved )
                        {
                            return true;
                        }
                    }

                    return false;
                }

                // jhm hack: store the error so I can debug and figure this out.
                public static string HackNotesErrorCheck = "";

                public void GetNoteDB( HttpRequest.RequestResult resultCallback )
                {
                    RequestingNoteDB = true;

                    Rock.Mobile.Network.HttpRequest request = new HttpRequest();
                    RestRequest restRequest = new RestRequest( Method.GET );
                    restRequest.RequestFormat = DataFormat.Xml;

                    request.ExecuteAsync<NoteDB>( GeneralConfig.NoteBaseURL, restRequest, 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, NoteDB noteModel )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true && noteModel != null && noteModel.SeriesList.Count > 0 )
                            {
                                Rock.Mobile.Util.Debug.WriteLine( "Got NoteDB info." );
                                Data.NoteDB = noteModel;
                                Data.NoteDB.ProcessPrivateNotes( Instance.Data.DeveloperModeEnabled );
                                Data.NoteDB.MakeURLsAbsolute( );
                                Data.NoteDBTimeStamp = DateTime.Now;

                                // download the first note so the user can immediately access it without having to wait
                                // for other crap.
                                if( Data.NoteDB.SeriesList[ 0 ].Messages.Count > 0 && 
                                    string.IsNullOrEmpty( Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].NoteUrl ) == false )
                                {
                                    MobileApp.Shared.Notes.Note.TryDownloadNote( Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].NoteUrl, Data.NoteDB.HostDomain, true, delegate
                                        {
                                            RequestingNoteDB = false;

                                            if ( resultCallback != null )
                                            {
                                                resultCallback( statusCode, statusDescription );
                                            }
                                        });
                                }
                                else
                                {
                                    Rock.Mobile.Util.Debug.WriteLine( "No note for latest message." );

                                    RequestingNoteDB = false;

                                    if ( resultCallback != null )
                                    {
                                        resultCallback( statusCode, statusDescription );
                                    }
                                }
                            }
                            else if ( noteModel == null || noteModel.SeriesList.Count == 0 )
                            {
                                statusDescription = "NoteDB downloaded but failed parsing.";
                                statusCode = System.Net.HttpStatusCode.BadRequest;
                                Rock.Mobile.Util.Debug.WriteLine( statusDescription );

                                RequestingNoteDB = false;

                                if ( resultCallback != null )
                                {
                                    resultCallback( statusCode, statusDescription );
                                }

                                // jhm hack: store the error so I can debug and figure this out.
                                if( noteModel == null )
                                {
                                    HackNotesErrorCheck = "Code 1";
                                }
                                else if ( noteModel.SeriesList.Count == 0 )
                                {
                                    HackNotesErrorCheck = "Code 2";
                                }
                            }
                            else
                            {
                                // jhm hack: store the error so I can debug and figure this out.
                                HackNotesErrorCheck = "Code 3: " + statusCode;
                                
                                Rock.Mobile.Util.Debug.WriteLine( "NoteDB request failed." );
                                RequestingNoteDB = false;

                                if ( resultCallback != null )
                                {
                                    resultCallback( statusCode, statusDescription );
                                }
                            }
                        } );
                }

                /// <summary>
                /// Returns true if there ARE no series in the note DB, or if the last time the noteDB
                /// was downloaded was too long ago.
                /// </summary>
                public bool NeedSeriesDownload( )
                {
                    // if the series hasn't been downloaded yet, or it's older than a day, redownload it.
                    TimeSpan seriesDelta = DateTime.Now - Data.NoteDBTimeStamp;
                    if ( Data.NoteDB.SeriesList.Count == 0 || seriesDelta.TotalDays >= 1 )
                    {
                        return true;
                    }

                    return false;
                }

                public void SaveToDevice( )
                {
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), LAUNCH_DATA_FILENAME);

                    // open a stream
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        string json = JsonConvert.SerializeObject( Data );
                        writer.WriteLine( json );
                    }
                }

                public void LoadFromDevice(  )
                {
                    // at startup, this should be called to allow current objects to be restored.
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), LAUNCH_DATA_FILENAME);

                    // if the file exists
                    if ( System.IO.File.Exists( filePath ) == true )
                    {
                        // read it
                        using ( StreamReader reader = new StreamReader( filePath ) )
                        {
                            string json = reader.ReadLine( );

                            try
                            {
                                // guard against the LaunchData changing and the user having old data.
                                LaunchData loadedData = JsonConvert.DeserializeObject<LaunchData>( json ) as LaunchData;
                                if( loadedData.ClientModelVersion == Data.ClientModelVersion )
                                {
                                    Data = loadedData;
                                }
                            }
                            catch( Exception )
                            {
                            }
                        }
                    }
                }
            }
        }
    }
}
