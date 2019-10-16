#if __WIN__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Rock.Mobile.PlatformSpecific.Win.Graphics
{
    public class FontManager
    {
        static FontManager _Instance = new FontManager( );
        public static FontManager Instance { get { return _Instance; } }

        List<FontFamily> FontFamilies { get; set; }

        public FontManager( )
        {
            // load all available fonts
            FontFamilies = Fonts.GetFontFamilies( new Uri("pack://application:,,,/"), "./Resources/Fonts/" ).ToList( );
        }

        public List<string> GetAvailableFontFiles( )
        {
            List<string> fontNames = new List<string>( );

            // for each loaded font family
            foreach ( FontFamily family in FontFamilies )
            {
                // get each typeface within it
                foreach ( Typeface typeface in family.GetTypefaces( ) )
                {
                    GlyphTypeface glyphTypeface = null;
                    typeface.TryGetGlyphTypeface( out glyphTypeface );

                    if ( glyphTypeface != null && glyphTypeface.Win32FamilyNames.Values.Count > 0 )
                    {
                        // the font name should be in the format "FamilyName-FaceName" with spaces removed
                        string fullFontName = glyphTypeface.Win32FamilyNames.Values.First( );

                        if( glyphTypeface.Win32FaceNames.Values.Count > 0 )
                        {
                            string faceName = glyphTypeface.Win32FaceNames.Values.First( );
                            fullFontName += "-" + faceName;
                        }
                        fullFontName = fullFontName.Replace( " ", String.Empty );

                        if( !fontNames.Exists( e => e == fullFontName ) )
                        {
                            fontNames.Add( fullFontName );
                        }
                    }
                }
            }

            return fontNames;
        }
        
        public void GetFont( String fileName, out System.Windows.Media.FontFamily fontFamily, out GlyphTypeface fontTypeface )
        {
            // iOS and Android get fonts by filename. Windows discourages this, and wants us to use
            // Font Family names. So, we'll go thru each font family it loaded, and look for one
            // that, at its core, was loaded from the filename provided.
            //
            // This is using Windows' design upside down, but allows us to maintain compatibility
            // with the actual production platforms: iOS and Android.
            fontFamily = null;
            fontTypeface = null;

            // first lowercase the name for faster searching
            string lowerFilename = fileName.ToLower( );

            // for each loaded font family
            foreach( FontFamily family in FontFamilies )
            {
                // get each typeface within it
                foreach ( Typeface typeface in family.GetTypefaces( ) )
                {
                    // now get the core glyph typeface, which will contain the filename the typeface came from
                    GlyphTypeface glyphTypeface = null;
                    typeface.TryGetGlyphTypeface( out glyphTypeface );
                        
                    // if we were able to load it, and the name matches, we found it.
                    if ( glyphTypeface != null && glyphTypeface.FontUri.AbsolutePath.ToLower( ).Contains( lowerFilename ) )
                    {
                        fontFamily = family;
                        fontTypeface = glyphTypeface;
                        break;
                    }
                }
            }
        }
    }
}
#endif
