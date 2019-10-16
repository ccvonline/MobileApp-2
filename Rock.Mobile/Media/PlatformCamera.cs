using System;

namespace Rock.Mobile
{
    namespace Media
    {
        public abstract class PlatformCamera
        {
#if __IOS__
            static PlatformCamera _Instance = new iOSCamera( );
#elif __ANDROID__
            static PlatformCamera _Instance = new DroidCamera( );
#elif __WIN__
            static PlatformCamera _Instance = null;
#endif

            public static PlatformCamera Instance { get { return _Instance; } }


            //Implements CaptureImageArgs, used for notifying after the user has captured an image
            public class CaptureImageEventArgs
            {
                public bool Result { get; private set; }
                public string ImagePath { get; private set; }

                public CaptureImageEventArgs( bool result, string imagePath )
                {
                    Result = result;
                    ImagePath = imagePath;
                }
            }
            public delegate void CaptureImageEvent( object s, CaptureImageEventArgs args );

            public abstract bool IsAvailable( );
            public abstract void CaptureImage( object imageDest, object context, CaptureImageEvent callback );
        }
    }
}
