using System;

namespace Rock.Mobile
{
    namespace Media
    {
        public abstract class PlatformImagePicker
        {
#if __IOS__
            static PlatformImagePicker _Instance = new iOSImagePicker( );
#elif __ANDROID__
            static PlatformImagePicker _Instance = new DroidImagePicker( );
#elif __WIN__
            static PlatformImagePicker _Instance = null;
#endif

            public static PlatformImagePicker Instance { get { return _Instance; } }


            //Implements ImagePickEventArgs, used for notifying after the user has captured an image
            public class ImagePickEventArgs
            {
                public bool Result { get; private set; }

                /// <summary>
                /// On iOS, this will be the actual image. On Android, a path TO the image.
                /// </summary>
                /// <value>The image.</value>
                public object Image { get; private set; }

                public ImagePickEventArgs( bool result, object image )
                {
                    Result = result;
                    Image = image;
                }
            }
            public delegate void ImagePickEvent( object s, ImagePickEventArgs args );

            public abstract void PickImage( object context, ImagePickEvent callback );
        }
    }
}
