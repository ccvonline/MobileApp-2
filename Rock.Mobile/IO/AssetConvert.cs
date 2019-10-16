using System;
using System.IO;

namespace Rock.Mobile.IO
{
    public static class AssetConvert
    {
        /// <summary>
        /// Takes a bundled asset (like a .png file), converts it to a c# memory stream and returns it.
        /// </summary>
        /// <returns>The to stream.</returns>
        /// <param name="assetPath">Asset path.</param>
        public static MemoryStream AssetToStream( string assetPath )
        {
#if __IOS__
            Foundation.NSData data = Foundation.NSData.FromFile( assetPath );

            byte[] dataBytes = new byte[ data.Length ];
            System.Runtime.InteropServices.Marshal.Copy( data.Bytes, dataBytes, 0, Convert.ToInt32( data.Length ) );

            return new MemoryStream( dataBytes );
#endif

#if __ANDROID__
            try
            {
                System.IO.Stream stream = Rock.Mobile.PlatformSpecific.Android.Core.Context.Assets.Open( assetPath );
            
                MemoryStream memStream = new MemoryStream( );
                stream.CopyTo( memStream );

                // reset the memstream position
                memStream.Position = 0;

                stream.Dispose( );
                stream = null;

                return memStream;
            }
            catch
            {
                return null;
            }
#endif

#if __WIN__
            return null;
#endif
        }
    }
}

