using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rock.Mobile.Util.URL
{
    // This defines our App URL Override system. Before processing any URL, the system will check this 
    // dictionary. If any of these keys are contained in the URL being processed, then the URL will be
    // updated with the value.

    // Example: 
    // { "ccv.church/give", ExternalUrlToken + "{0}" } 
    // would result in:
    // http://ccv.church/give will become "external:http://ccv.church/give"
    public static class Override
    {
        // The app must defined the list of url keys it wants overidden. (Or just an empty dictionary if it doesn't want anything)
        static Dictionary<string, string> App_URL_Override = new Dictionary<string, string>( );

        public static void SetAppUrlOverrides( Dictionary<string, string> appUrlOverrides )
        {
            App_URL_Override = appUrlOverrides;
        }

        public static string ProcessURLOverrides( string requestUrl )
        {
            // default to using the same URL
            string processedUrl = requestUrl;

            // check to see if any of these overrides exist in the URL. We support only ONE at a time,
            // so the first found is the one used.
            foreach( KeyValuePair<string, string> urlOverride in App_URL_Override )
            {
                if( requestUrl.Contains( urlOverride.Key ) )
                {
                    // update it
                    processedUrl = string.Format( urlOverride.Value, requestUrl);
                    break;
                }
            }

            return processedUrl;
        }
    }
}

