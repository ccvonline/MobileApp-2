using System;
using System.Collections.Generic;

namespace MobileApp.Shared
{
    public class ConnectLink
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Url { get; set; }
        public string ImageName { get; set; }
        public string Command_Keyword { get; set; }

        public ConnectLink( )
        {
        }

        public static List<ConnectLink> BuildGetEngagedList( )
        {
            List<ConnectLink> linkEntries = new List<ConnectLink>();

            // parse the config and see how many additional links we need.
            for ( int i = 0; i < MobileApp.Shared.Config.ConnectConfig.GetEngagedList.Length; i++ )
            {
                ConnectLink link = new ConnectLink();
                linkEntries.Add( link );

                string[] engagedEntry = MobileApp.Shared.Config.ConnectConfig.GetEngagedList[ i ].GetEntry( MobileApp.Shared.Network.RockMobileUser.Instance );

                // use the "positive" list, which has the data for them BEING in the group.
                link.Title = engagedEntry[ 0 ];
                link.SubTitle = engagedEntry[ 1 ];
                link.Url = engagedEntry[ 2 ];
                link.ImageName = engagedEntry[ 3 ];
                link.Command_Keyword = engagedEntry[ 4 ];
            }

            return linkEntries;
        }
    }
}

