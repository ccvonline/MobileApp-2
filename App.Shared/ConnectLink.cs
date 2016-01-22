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

        public static List<ConnectLink> BuildGetEngagedList( )
        {
            List<ConnectLink> linkEntries = new List<ConnectLink>();

            // parse the config and see how many additional links we need.
            for ( int i = 0; i < App.Shared.Config.ConnectConfig.GetEngagedList.Length; i++ )
            {
                ConnectLink link = new ConnectLink();
                linkEntries.Add( link );

                string[] engagedEntry = App.Shared.Config.ConnectConfig.GetEngagedList[ i ].GetEntry( App.Shared.Network.RockMobileUser.Instance.Groups );

                // use the "positive" list, which has the data for them BEING in the group.
                link.Title = engagedEntry[ 0 ];
                link.SubTitle = engagedEntry[ 1 ];
                link.Url = engagedEntry[ 2 ];
                link.ImageName = engagedEntry[ 3 ];
            }

            return linkEntries;
        }
    }
}

