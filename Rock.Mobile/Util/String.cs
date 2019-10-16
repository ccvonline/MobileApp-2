using System;

namespace Rock.Mobile.Util.Strings
{
    public static class Parsers
    {
        /// <summary>
        /// Given an address formatted "street, city, state, zip" (commas optional)
        /// The broken out components will be returned.
        /// </summary>
        /// <returns><c>true</c>, if address was parsed, <c>false</c> otherwise.</returns>
        public static bool ParseAddress( string address, ref string street, ref string city, ref string state, ref string zip )
        {
            bool result = false;

            // we parse by working backwards.
            do
            {
                // first parse off the zip code, which comes last.
                int zipCodeIndex = address.LastIndexOf( ' ' );
                if( zipCodeIndex == -1 ) break;
                string workingStr = address.Substring( zipCodeIndex );

                if( workingStr == null ) break;
                zip = workingStr.Trim( new char[] { ',' } );

                // make sure it contains at least 1 digit, or it's obviously not a zip code.
                if( zip.AsNumeric( ).Length == 0 ) break;

                // truncate at the zipcode
                address = address.Remove( zipCodeIndex );


                // next comes the state
                int stateIndex = address.LastIndexOf( ' ' );
                if( stateIndex == -1 ) break;

                state = address.Substring( stateIndex );
                if( state == null ) break;

                state = state.Trim( new char[] { ',' } );

                // truncate at the state
                address = address.Remove( stateIndex );


                // city
                int cityIndex = address.LastIndexOf( ' ' );
                if( cityIndex == -1 ) break;

                city = address.Substring( cityIndex );
                if( city == null ) break;

                city = city.Trim( new char[] { ',' } );

                // truncate at the city
                address = address.Remove( cityIndex );


                // street is the remaning string
                if( address == null ) break;
                street = address.Trim( new char[] { ',' } );

                result = true;
            }
            while( 0 != 1 );

            return result;
        }

        public static string ParseURLToFileName( string url )
        {
            int lastSlashIndex = url.LastIndexOf( "/" ) + 1;
            return url.Substring( lastSlashIndex );
        }

        public static string AddParamToURL( string url, string parameter )
        {
            string fullUrl = url;

            // if the URL contains a ? already, just add the parameter as an additional
            if ( url.Contains( "?" ) )
            {
                fullUrl += "&";
            }
            else
            {
                // otherwise add the ?
                fullUrl += "?";
            }

            return fullUrl + parameter;
        }
    }

    public static class StringExtensions
    {
        /// <summary>
        /// Returns a new string that contains only digits
        /// </summary>
        /// <returns>The non numeric.</returns>
        /// <param name="source">Source.</param>
        public static string AsNumeric( this string source )
        {
            if ( string.IsNullOrWhiteSpace( source ) == false )
            {
                string numericString = "";

                for ( int i = 0; i < source.Length; i++ )
                {
                    if ( source[ i ] >= '0' && source[ i ] <= '9' )
                    {
                        numericString += source[ i ];
                    }
                }

                return numericString;
            }

            return source;
        }

        /// <summary>
        /// Returns true if the string contains only numbers
        /// </summary>
        public static bool IsNumeric( this string source )
        {
            if ( string.IsNullOrWhiteSpace( source ) == false )
            {
                for ( int i = 0; i < source.Length; i++ )
                {
                    if ( source[ i ] < '0' || source[ i ] > '9' )
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool IsEmailFormat( this string source )
        {
            if ( string.IsNullOrWhiteSpace( source ) == false )
            {
                // email format is x@x.x

                // this does a VERY basic format check. It's by no means completely bullet proof

                // is the @ symbol in a safe place? (can't be at the start or end)
                int atSymbolIndex = source.IndexOf( '@' );
                if ( atSymbolIndex > 0 && atSymbolIndex < source.Length - 1 )
                {
                    // make sure there's at least one . after the @ symbol
                    int dotSymbolIndex = source.LastIndexOf( '.' );
                    if ( dotSymbolIndex > atSymbolIndex && dotSymbolIndex < source.Length - 1 )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string ToUpperWords( this string source )
        {
            if ( string.IsNullOrWhiteSpace( source ) == false )
            {
                string fixedString = "";
                
                string[] words = source.Split( ' ' );
                foreach ( string word in words )
                {
                    // it's possible this word is null (ex: they pass a string
                    // like " family")
                    if ( string.IsNullOrWhiteSpace( word ) == false )
                    {
                        fixedString += char.ToUpper( word[ 0 ] );

                        if ( word.Length > 1 )
                        {
                            fixedString += word.Substring( 1 );
                        }
                    }

                    fixedString += " ";
                }

                return fixedString.TrimEnd( ' ' );
            }

            return source;
        }

        /// <summary>
        /// Remove all characters that wouldn't be appropriate for a filename
        /// </summary>
        /// <returns>The sanitized.</returns>
        /// <param name="source">Source.</param>
        public static string AsLegalFilename( this string source )
        {
            if ( string.IsNullOrWhiteSpace( source ) == false )
            {
                string lowerSource = source.ToLower( );

                // strip all non-alpha numeric values
                string sanitizedString = "";

                for ( int i = 0; i < lowerSource.Length; i++ )
                {
                    // verify it a space. If iti s, replace it with an underscore
                    if ( lowerSource[ i ] == ' ' )
                    {
                        sanitizedString += "_";
                    }
                    // then, verify it isn't a reserved character (only allow alpha-numeric)
                    else if ( ( lowerSource[ i ] >= 'a' && lowerSource[ i ] <= 'z' ) ||
                             ( lowerSource[ i ] >= '0' && lowerSource[ i ] <= '9' ) )
                    {
                        sanitizedString += lowerSource[ i ];
                    }
                }

                return sanitizedString;
            }

            return source;
        }

        public static bool NeedsTrim( this string source )
        {
            // if it starts or ends with white space, then it needs trimming.
            if ( source.StartsWith( " ", StringComparison.CurrentCulture ) || source.EndsWith( " ", StringComparison.CurrentCulture ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Takes a string assumed to be a URL and ensures it can be parsed as a URI
        /// </summary>
        public static bool IsValidURL( this string url )
        {
            // simply wrap .net's url parser in an exception handler
            try
            {
                new System.Uri( url );
                return true;
            }
            catch
            {
            }

            // failed, so return false
            return false;
        }

        /// <summary>
        /// Takes a string assumed to be only digits and formats it as a phone number.
        /// </summary>
        public static string AsPhoneNumber( this string number )
        {
            if ( string.IsNullOrWhiteSpace( number ) == false )
            {
                // nothing to do if it's less than four digits
                if ( number.Length < 4 )
                {
                    return number;
                }
                // We know it has at least enough for a local exchange and subscriber number
                else if ( number.Length < 8 )
                {
                    return number.Substring( 0, 3 ) + "-" + number.Substring( 3 );
                }
                else
                {
                    // We know it has at least enough for an area code and local exchange
                    // Area Code
                    // Local Exchange
                    // Subscriber Number
                    string areaCode = number.Substring( 0, 3 );

                    string localExchange = number.Substring( 3, 3 );

                    // for the subscriber nubmer, take the remaining four digits, but no more.
                    string subscriberNumber = number.Substring( 6, System.Math.Min( number.Length - 6, 4 ) ); 

                    return "(" + areaCode + ")" + " " + localExchange + "-" + subscriberNumber;
                }
            }

            return number;
        }

        /// <summary>
        /// Takes a string assumed to be only digits and formats it as a birthdate [mm-dd-yyyy].
        /// </summary>
        public static string AsBirthDate( this string number )
        {
            if ( string.IsNullOrWhiteSpace( number ) == false )
            {
                // nothing to do if it's less than three digits
                if ( number.Length < 3 )
                {
                    return number;
                }
                // We know it has at least enough for a month and day
                else if ( number.Length < 5 )
                {
                    return number.Substring( 0, 2 ) + "-" + number.Substring( 2 );
                }
                else
                {
                    // We know it has at least enough for a month and day
                    string month = number.Substring( 0, 2 );

                    string day = number.Substring( 2, 2 );

                    // for the year, take the remaining four digits, but no more.
                    string year = number.Substring( 4, System.Math.Min( number.Length - 4, 4 ) );

                    return month + "-" + day + "-" + year;
                }
            }

            return number;
        }
    }
}
