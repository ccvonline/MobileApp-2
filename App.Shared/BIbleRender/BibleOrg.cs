using System;
using System.Drawing;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;
using RestSharp;
using MobileApp.Shared.Config;
using MobileApp.Shared.UI;
using MobileApp.Shared.Strings;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.Network;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Shared.Notes.Model;

using System.Diagnostics.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using App.Shared;


namespace App.Shared
{
    // retrieves bible content from https://bibles.org/
    public class BibleOrg : BibleService
    {
        /// <summary>
        /// The prefix for any Biblia API calls for the Bible Passage Viewer.
        /// </summary>
        public const string BibleOrg_Prefix = "bibleorg://";

        /// <summary>
        /// The URL for requesting books from a given translation and language from Bibles.ORG
        /// </summary>
        const string BiblesOrg_Books_URL = "https://bibles.org/v2/versions/eng-{0}/books.js";

        /// <summary>
        /// The URL for requesting chapters from a given translation, language and book from Bibles.ORG
        /// </summary>
        const string BiblesOrg_Chapter_URL = "https://bibles.org/v2/chapters/eng-{0}:{1}.{2}.js";

        static BibleOrg _Instance = new BibleOrg( );
        public static BibleOrg Instance { get { return _Instance; } }

        // guard against multiple requests at once
        bool RetrievingVerse { get; set; }

        public BibleOrg( )
        {
            RetrievingVerse = false;
        }

        public override void RetrieveBiblePassage( string bibleAddress, OnBibleResult onResult )
        {
            // I normally hate returning from places other than the bottom, but to prevent
            // wrapping the ENTIRE function, we will.
            if( RetrievingVerse == true )
            {
               onResult( string.Empty );
               return;
            }

            RetrievingVerse = true;
            RestRequest request = new RestRequest( Method.GET );

            // We expect the Bible URL to be written in the format "bible://[Translation]/[Book]/[Chapter]".
            string translation, book, chapter;
            FriendlyBibleUrlToParts( bibleAddress, out translation, out book, out chapter );

			// now based on the translation, get the correct book abbreviations that the API expects
			GetBookAbbreviation( translation, book, delegate ( string bookAbbrev )
            {
                if( bookAbbrev != null )
                {
                    // Build the URI for the Bible Searches API request.
                    string BibleSearchesAddress = String.Format( BiblesOrg_Chapter_URL, translation, bookAbbrev, chapter );

                    HttpRequest webRequest = new HttpRequest( );
                    webRequest.ExecuteAsync<RestResponse>( BibleSearchesAddress, request, SecuredValues.Bible_Searches_API_Key, string.Empty, delegate ( System.Net.HttpStatusCode statusCode, string statusDescription, RestResponse response )
                    {
                        string htmlStream = null;
                        if( Util.StatusInSuccessRange( statusCode ) == true )
                        {
                            string text, copyright;
                            if( TryParseChapter( response.Content, out text, out copyright ) )
                            {
                                string titleHTML = "<h2>" + book + " " + chapter + " (" + translation + ")" + "</h2><br/>";

                                string styleHeader = "<head>" +
                                	 "<style type=\"text/css\">" +
                                		 "html {-webkit-text-size-adjust:none}" +
                                		 "body {" +
                                			"font-family: Arial;" +
                                			"color: white;" +
                                			"background-color: #1C1C1C;" +
                                		  "}" +
                                		"</style>" +
                                		"</head>";

                                string copyrightHtml = "<small>" + copyright + "</small>";

                                // adds the CSS header to the HTML string
                                htmlStream = "<html>"
                                              + styleHeader
                                              + "<body>"
                                              + titleHTML
                                              + text
                                              + copyright
                                              + "</body></html>";

                                // get rid of <a href> tags in the copyright because the WebView
                                // is not set up aesthetically to show any other pages but this HTML response
                                htmlStream = Regex.Replace( htmlStream, "<a href=.*?>", String.Empty );
                                htmlStream = Regex.Replace( htmlStream, "</a>", String.Empty );
                            }
                        }

                        // return the htmlStream we built
                        RetrievingVerse = false;
                        onResult( htmlStream );
                    });
                }
                // Book Abbreviation Request failed
                else
                {
                    RetrievingVerse = false;
                    onResult( string.Empty );
                }
            });
        }

        // TODO: We should desperately add caching for this.
        delegate void OnGetBookResult( string bookAbbrev );
        void GetBookAbbreviation( string translation, string bookName, OnGetBookResult onResult )
        {
           	string bibleBooksAddress = String.Format( BiblesOrg_Books_URL, translation );

           	RestRequest request = new RestRequest( Method.GET );
           	HttpRequest webRequest = new HttpRequest( );
           	webRequest.ExecuteAsync<RestResponse>( bibleBooksAddress, request, SecuredValues.Bible_Searches_API_Key, string.Empty, delegate ( System.Net.HttpStatusCode statusCode, string statusDescription, RestResponse response )
           	{
   				// this will be either null if we failed ,or the book abbreviation
   				string bookAbbrev = null;

           		if( Util.StatusInSuccessRange( statusCode ) == true )
           		{
   					// first get the response as json
   					var jsonResponse = JObject.Parse( response.Content );
           			if( jsonResponse != null )
           			{
   						// grab the response object
   						var responseObj = jsonResponse[ "response" ];
           				if( responseObj != null )
           				{
   							// grab the books object (which is a list of books)
   							var booksListObj = responseObj[ "books" ];
           					if( booksListObj != null )
           					{
   								// get the individual books as a list
   								List<JToken> booksList = booksListObj.Children( ).ToList( );
           						if( booksList != null )
           						{
   									// get the book object by book name
   									var bookObj = booksList.Where( b => b[ "name" ].ToString( ) == bookName ).FirstOrDefault( );
           							if( bookObj != null )
           							{
   										// get the abbreviation object
   										var bookAbbrvObj = bookObj[ "abbr" ];
           								if( bookAbbrvObj != null )
           								{
           									bookAbbrev = bookAbbrvObj.ToString( );
           								}
           							}
           						}
           					}
           				}
           			}
           		}

        		   onResult( bookAbbrev );  
            } );
        }

        bool TryParseChapter( string responseString, out string text, out string copyright )
        {
            text = string.Empty;
            copyright = string.Empty;

            bool success = false;

            // first get the response as json
            var jsonResponse = JObject.Parse( responseString );
            if( jsonResponse != null )
            {
                // grab the response object
                var responseObj = jsonResponse[ "response" ];
                if( responseObj != null )
                {
                    // grab the chapters object (which is a list of chapters)
                    var chaptersListObj = responseObj[ "chapters" ];
                    if( chaptersListObj != null )
                    {
                        // get the individual chapters as a list (it will be a list of 1)
                        List<JToken> chaptersList = chaptersListObj.Children( ).ToList( );
                        if( chaptersList != null )
                        {
                            // get the first chapter, since we requested only one chapter.
                            var chapterObj = chaptersList.FirstOrDefault( );
                            if( chapterObj != null )
                            {
                                var textObj = chapterObj[ "text" ];
                                var copyrightObj = chapterObj[ "copyright" ];
                                                 
                                if( textObj != null && copyrightObj != null )
                                {
                                    text = textObj.ToString( );
                                    copyright = copyrightObj.ToString( );

                                    success = true;
                                }
                            }
                        }
                    }
                }
            }

            return success;
        }

        // converts an end-user friendly bible url (bibleorg://[Translation]/[Book]/[Chapter]) (ex: bibleorg://NASB/1 Timothy/6) to its parts
        bool FriendlyBibleUrlToParts( string friendlyBibleUrl, out string translation, out string book, out string chapter )
        {
            translation = string.Empty;
            book = string.Empty;
            chapter = string.Empty;
            
            string bookChapterStr = friendlyBibleUrl.Substring( BibleOrg_Prefix.Length );
            if( string.IsNullOrWhiteSpace( bookChapterStr ) == false )
            {
               string[ ] parts = bookChapterStr.Split( '/' );
               if( parts.Length == 3 )
               {
                  translation = parts[ 0 ];
                  book = parts[ 1 ];
                  chapter = parts[ 2 ];

                  if( string.IsNullOrEmpty( translation ) == false && string.IsNullOrWhiteSpace( book ) == false && string.IsNullOrWhiteSpace( chapter ) == false )
                  {
                     return true;
                  }
               }
            }

            return false;
        }
    }
}
