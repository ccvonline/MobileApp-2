using System;
namespace App.Shared
{
   public static class BibleRenderer
   {
      const string Legacy_Prefix = "bible://";

      public static bool IsBiblePrefix( string url )
      {
         if( url.ToLower( ).StartsWith( Legacy_Prefix ) )
         {
            return true;
         }
         else if ( url.ToLower( ).StartsWith( BibleOrg.BibleOrg_Prefix ) )
         {
            return true;
         }
         else if ( url.ToLower( ).StartsWith( BibleNIV.BibleNIV_Prefix ) )
         {
            return true;
         }

         return false;
      }

      public static void RetrieveBiblePassage( string bibleAddress, BibleService.OnBibleResult onResult )
      {
         // simply check the type, and call the appropriate implementation

         bool useDefault = true;

         // first test for the BibleOrg service
         if( bibleAddress.ToLower( ).StartsWith( BibleOrg.BibleOrg_Prefix ) == true )
         {
            useDefault = false;
            BibleOrg.Instance.RetrieveBiblePassage( bibleAddress, onResult );
         }
         // TODO: In like 6 months, we can drop this. for now, we need to route it to NIV
         // TODO: To drop it, we'll need to go thru the old notes (May - June 17) and update them
         else if ( bibleAddress.ToLower( ).StartsWith( Legacy_Prefix ) == true )
         {
            // we know we'll need to use NIV. So now reformat it correctly
            string bookString = bibleAddress.Substring( Legacy_Prefix.Length );

            // if the bookString starts with a number, we need to add a space
            if( bookString[0] >= '0' && bookString[0] <= '9' )
            {
               bookString = bookString.Insert( 1, " " );
            }

            // the bookString WILL _END_ with a number
            // find where the number starts, and put a / before it.
            int i = 0;
            for( i = bookString.Length - 1; i >= 0; i-- )
            {
               if( bookString[ i ] < '0' || bookString[ i ] > '9' )
               {
                  bookString = bookString.Insert( i + 1, "/" );
                  break;
               }
            }

            bibleAddress = BibleNIV.BibleNIV_Prefix + bookString;
         }

         // let NIV be the default. If another is used above, it should set useDefault to false
         if( useDefault )
         {
            BibleNIV.Instance.RetrieveBiblePassage( bibleAddress, onResult );
         }
      }
   }
}
