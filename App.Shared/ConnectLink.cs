using System;
using System.Collections.Generic;

namespace App.Shared
{
    public class ConnectLink
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Url { get; set; }
        public string ImageName { get; set; }

        public ConnectLink( )
        {
        }

        public static List<ConnectLink> BuildList( )
        {
            List<ConnectLink> linkEntries = new List<ConnectLink>();

            // parse the config and see how many additional links we need.
            for ( int i = 0; i < App.Shared.Config.ConnectConfig.WebViews.Length; i += 4 )
            {
                ConnectLink link = new ConnectLink();
                linkEntries.Add( link );
                link.Title = App.Shared.Config.ConnectConfig.WebViews[ i ];
                link.SubTitle = App.Shared.Config.ConnectConfig.WebViews[ i + 1 ];
                link.Url = App.Shared.Config.ConnectConfig.WebViews[ i + 2 ];
                link.ImageName = App.Shared.Config.ConnectConfig.WebViews[ i + 3 ];
            }

            return linkEntries;
        }
    }
}

