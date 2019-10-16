using System;
using System.IO;

namespace Rock.Mobile.Util
{
    public static class Debug
    {
        public static void WriteLine( string output )
        {
            #if DEBUG
            Console.WriteLine( output );
            #endif
        }

        public static void WriteToLog( string message )
        {
            Console.WriteLine( message );

            using( FileStream writer = new FileStream( "sdcard" + "/" + "locLog.txt", FileMode.Append ) )
            {
                StreamWriter stringWriter = new StreamWriter( writer );
                stringWriter.Write( DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ": " + message + "\n" );

                stringWriter.Close( );

                writer.Close( );
            }
        }

        public static void DisplayError( string errorTitle, string errorMessage )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    #if __IOS__
                    UIKit.UIAlertView alert = new UIKit.UIAlertView();
                    alert.Title = errorTitle;
                    alert.Message = errorMessage;
                    alert.AddButton( "Ok" );
                    alert.Show( ); 
                    #elif __ANDROID__
                    #endif
                } );
        }
    }
}

