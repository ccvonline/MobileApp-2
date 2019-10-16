#if __IOS__
using System;
using UIKit;
using Foundation;
using CoreGraphics;
using CoreText;

namespace Rock.Mobile.PlatformSpecific.iOS.Graphics
{
    public class FontManager
    {
        public static UIFont GetFont( String name, float fontSize )
        {
            // first attempt to simpy load it (it may be loaded already)
            UIFont uiFont = UIFont.FromName(name, fontSize );

            // failed, so attempt to load it dynamically
            if( uiFont == null )
            {
                // get a path to our custom fonts folder
                String fontPath = NSBundle.MainBundle.BundlePath + "/Fonts/" + name + ".ttf";

                // build a data model for the font
                CGDataProvider fontProvider = CoreGraphics.CGDataProvider.FromFile( fontPath );

                // create a renderable font out of it
                CGFont newFont = CoreGraphics.CGFont.CreateFromProvider( fontProvider );

                // get the legal loadable font name
                String fontScriptName = newFont.PostScriptName;

                // register the font with the CoreText / UIFont system.
                NSError error = null;
                bool result = CTFontManager.RegisterGraphicsFont( newFont, out error );
                if ( result == false )
                    throw new NSErrorException( error );

                uiFont = UIFont.FromName( fontScriptName, fontSize );

                // release the CT reference to the font, leaving only the UI manager's referene.
                result = CTFontManager.UnregisterGraphicsFont( newFont, out error );
                if ( result == false )
                    throw new NSErrorException( error );
            }

            return uiFont;
        }
    }
}
#endif
