
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;

namespace App.Shared
{
   public class Verse
   {
      public int Number { get; set; }
      public string Text { get; set; }
   }
   
   public class Chapter
   {
      public Chapter( )
      {
         Verses = new List<Verse>( );
      }

      public string GetHTML( )
      {
         string htmlString = string.Empty;

         foreach( Verse verse in Verses )
         {
            htmlString += "<p><strong>" + verse.Number.ToString( ) + " " + "</strong>" + verse.Text + "</p>";
         }

         return htmlString;
      }

      public int Number { get; set; }
      public List<Verse> Verses { get; set; }
   }

   public class Book
   {
      public bool IsValid( )
      {
         if( Chapters.Count > 0 && string.IsNullOrWhiteSpace( Name ) == false )
         {
            return true;
         }

         return false;
      }
      
      public Book( )
      {
         Chapters = new List<Chapter>( );
      }

      public string GetHTML( int chapterNum )
      {
         string htmlString = string.Empty;

         // if 0, do everything, otherwise, a specific chapter
         if( chapterNum > 0 )
         {
            Chapter chapter = Chapters.Where( c => c.Number == chapterNum ).FirstOrDefault( );
            if( chapter != null )
            {
               htmlString = chapter.GetHTML( );
            }
         }
         else
         {
            foreach( Chapter chapter in Chapters )
            {
               htmlString += chapter.GetHTML( );
            }
         }

         return htmlString;
      }
      
      public string Name { get; set; }
      public List<Chapter> Chapters { get; set; }
   }

   public class BibleNIV : BibleService
   {
      public const string BibleNIV_Prefix = "niv://";

      const string CopyrightText = "Biblica provides accurate, readable translations of Scripture. We offer free study tools and community reading experiences to help you engage God’s Word. NIV® Copyright 2011 by Biblica, Inc.®";

      static BibleNIV _Instance = new BibleNIV( );
      public static BibleNIV Instance { get { return _Instance; } }

      private BibleNIV( )
      {
      }

      bool FriendlyBibleUrlToParts( string friendlyBibleUrl, out string book, out string chapter )
      {
         book = string.Empty;
         chapter = string.Empty;
         
         string bookChapterStr = friendlyBibleUrl.Substring( BibleNIV_Prefix.Length );
         if( string.IsNullOrWhiteSpace( bookChapterStr ) == false )
         {
            string[ ] parts = bookChapterStr.Split( '/' );
            if( parts.Length == 2 )
            {
               book = parts[ 0 ];
               chapter = parts[ 1 ];

               if( string.IsNullOrWhiteSpace( book ) == false && string.IsNullOrWhiteSpace( chapter ) == false )
               {
                  return true;
               }
            }
         }

         return false;
      }

      public override void RetrieveBiblePassage( string bibleAddress, OnBibleResult onResult )
      {
         string htmlStream = string.Empty;

         // first, convert what the user typed in into fragments
         string bookStr, chapterStr;
         if( FriendlyBibleUrlToParts( bibleAddress, out bookStr, out chapterStr ) )
         {
            Book book = new Book( );

#if __IOS__
            using( StreamReader sr = new StreamReader( Foundation.NSBundle.MainBundle.BundlePath + "/" + "bible_niv_xml.xml" ) )

#elif __ANDROID__

            using( StreamReader sr = new StreamReader( Rock.Mobile.PlatformSpecific.Android.Core.Context.Assets.Open( "bible_niv_xml.xml" ) ) )
#elif __WIN__ //NOTE: Not supported or needed on Windows platform
            using( StreamReader sr = new StreamReader( "" ) )
#endif
            {
               using( XmlTextReader reader = new XmlTextReader( sr ) )
               {
                  while( reader.Read( ) )
                  {
                     switch( reader.NodeType )
                     {
                        case XmlNodeType.Element:
                        {
                           if( reader.Name == "b" )
                           {
                              string bookName = reader.GetAttribute( "n" );

                              // if this is the book we want, parse it.
                              if( bookName.ToLower( ) == bookStr.ToLower( ) )
                              {
                                 List<Chapter> chapterList = ParseBook( reader );

                                 book.Name = bookName;
                                 book.Chapters = chapterList;
                              }
                           }

                           break;
                        }
                     }
                  }

                  if( book.IsValid( ) )
                  {
                     // get the chapter they want
                     int chapterNum = int.Parse( chapterStr );
                     string textBody = book.GetHTML( chapterNum );

                     // cool, build html
                     string titleHTML = "<h2>" + book.Name + " " + chapterStr + " (" + "NIV" + ")" + "</h2>";

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

                     string copyrightHtml = "<p style=\"text-align: center\"><small>" + CopyrightText + "</small></p>";

                     // adds the CSS header to the HTML string
                     htmlStream = "<html>"
                                        + styleHeader
                                        + "<body>"
                                        + titleHTML
                                        + textBody
                                        + copyrightHtml
                                        + "</body></html>";
                  }
               }
            }
         }

         onResult( htmlStream );
      }

      List<Chapter> ParseBook( XmlTextReader reader )
      {
         bool finishedReading = false;

         List<Chapter> chapterList = new List<Chapter>( );

         // find all chapters and verses
         while( reader.Read( ) && finishedReading == false )
         {
            switch( reader.NodeType )
            {
               case XmlNodeType.Element:
               {
                  // parse the chapter
                  if( reader.Name == "c" )
                  {
                     Chapter chapter = ParseChapter( reader );

                     chapterList.Add( chapter );
                  }
                  break;
               }

               case XmlNodeType.EndElement:
               {
                  finishedReading = true;
                  break;
               }
            }
         }

         return chapterList;
      }

      Chapter ParseChapter( XmlTextReader reader )
      {
         bool finishedReading = false;

         Chapter chapter = new Chapter( );

         // get the chapter number
         string chapterNum = reader.GetAttribute( "n" );
         chapter.Number = int.Parse( chapterNum );

         while( reader.Read( ) && finishedReading == false )
         {
            switch( reader.NodeType )
            {
               case XmlNodeType.Element:
               {
                  if( reader.Name == "v" )
                  {
                     Verse verse = ParseVerse( reader );

                     chapter.Verses.Add( verse );
                  }
                  break;
               }

               case XmlNodeType.EndElement:
               {
                  finishedReading = true;
                  break;
               }
            }
         }

         return chapter;
      }

      Verse ParseVerse( XmlTextReader reader )
      {
         bool finishedReading = false;

         Verse verse = new Verse( );
         
         // get the verse number
         string verseNum = reader.GetAttribute( "n" );
         verse.Number = int.Parse( verseNum );

      	while( reader.Read( ) && finishedReading == false )
      	{
      		switch( reader.NodeType )
      		{
               case XmlNodeType.Text:
               {
                  verse.Text = reader.Value;
                  break;
               }

               // find the close tag so we can end
               case XmlNodeType.EndElement:
   				{
                  finishedReading = true;
   					break;
   				}
      		}
      	}

      	return verse;
      }
   }
}
