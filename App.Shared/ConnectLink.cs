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

        public static List<ConnectLink> BuildGetStartedList( )
        {
            List<ConnectLink> linkEntries = new List<ConnectLink>();

            // parse the config and see how many additional links we need.
            for ( int i = 0; i < App.Shared.Config.ConnectConfig.GetStartedList.Length; i += 4 )
            {
                ConnectLink link = new ConnectLink();
                linkEntries.Add( link );
                link.Title = App.Shared.Config.ConnectConfig.GetStartedList[ i ];
                link.SubTitle = App.Shared.Config.ConnectConfig.GetStartedList[ i + 1 ];
                link.Url = App.Shared.Config.ConnectConfig.GetStartedList[ i + 2 ];
                link.ImageName = App.Shared.Config.ConnectConfig.GetStartedList[ i + 3 ];
            }

            return linkEntries;
        }

        public static List<ConnectLink> BuildGetEngagedList( )
        {
            List<ConnectLink> linkEntries = new List<ConnectLink>();

            // parse the config and see how many additional links we need.
            for ( int i = 0; i < App.Shared.Config.ConnectConfig.GetEngagedList.Length; i += 4 )
            {
                ConnectLink link = new ConnectLink();
                linkEntries.Add( link );
                link.Title = App.Shared.Config.ConnectConfig.GetEngagedList[ i ];
                link.SubTitle = App.Shared.Config.ConnectConfig.GetEngagedList[ i + 1 ];
                link.Url = App.Shared.Config.ConnectConfig.GetEngagedList[ i + 2 ];
                link.ImageName = App.Shared.Config.ConnectConfig.GetEngagedList[ i + 3 ];
            }

            return linkEntries;
        }
    }
}

