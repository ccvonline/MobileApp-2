using System;
using System.Collections.Generic;
using App.Shared.PrivateConfig;
using MobileApp;
using App.Shared.Config;
using App.Shared.Strings;

namespace App.Shared
{
    /// <summary>
    /// The Engagement system implements a logic based list item for each entry in the "Next Steps->Get Engaged" section of the app.
    /// To the user, it's a simple list of options for engagement, but because what displays is conditional, they are backed by this class.
    /// </summary>
    public interface IEngagement
    {
        // get entry will return a string array in the same format as the above "GetStartedList".
        // The difference is, these are actual classes so a function can figure out
        // what type of entry to return.

        // The string array order is: Title, Subtitle, URL, Icon
        string[] GetEntry( List<Rock.Client.Group> groups );
    }

    public class StartingPoint_Engagement : IEngagement
    {
        string [] StartingPoint_Entry = { "Starting Point", "Begin your journey at CCV", "http://ccv.church/ma-startingpoint", "starting_point_thumb.png" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return StartingPoint_Entry;
        }
    }

    public class Baptism_Engagement : IEngagement
    {
        string [] Baptism_Entry = { "Baptisms", "Make your faith public", "http://ccv.church/ma-baptism", "baptism_thumb.png" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Baptism_Entry;
        }
    }

    public class Worship_Engagement : IEngagement
    {
        string [] Worship_Entry = { "Worship", "Worship God with others", AboutConfig.Url, "worship_thumb.png" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Worship_Entry;
        }
    }

    public class Connect_Engagement : IEngagement
    {
        string [] Connect_Entry = { ConnectStrings.Main_Connect_GroupFinder, ConnectStrings.Main_Connect_GroupFinder_SubTitle, "", PrivateConnectConfig.GroupFinder_IconImage };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Connect_Entry;
        }
    }

    public class Serve_Engagement : IEngagement
    {
        string [] Serve_Entry = { "Serve", "Impact lives by serving", "http://ccv.church/ma-serve", "serve_thumb.png" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Serve_Entry;
        }
    }

    public class Give_Engagement : IEngagement
    {
        string [] Give_Entry = { "Give", "Trust God financially", "external:" + GiveConfig.GiveUrl, "give_thumb.png" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Give_Entry;
        }
    }

    public class Share_Engagement : IEngagement
    {
        string [] Share_Entry = { "Share", "Share your story", "http://ccv.church/mystory", "share_thumb.png" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Share_Entry;
        }
    }

    /// <summary>
    /// Coach Engagement is the entry in the list for Becoming a Next Steps Coach, a Neighborhood Coach, or requesting to become or receieve a coach.
    /// </summary>
    public class Coach_Engagement : IEngagement
    {
        // Next Steps Coach
        const int GroupType_NextSteps = 78;
        const int GroupTypeRole_Coach = 114;
        string [] NextStepsCoach_Entry = { "Coach", "Access your toolbox", "http://ccv.church/page/985?GroupId={0}", "coach_thumb.png" };

        // Neighborhood Group Coach
        string [] GroupCoach_Entry = { "Coach", "Access your toolbox", "http://ccv.church/page/490?GroupId={0}", "coach_thumb.png" };

        // NotCoach 
        string[] NotCoach_Entry = { "Coaches", "Learn about coaching", "http://ccv.church/ma-request-coach", "coach_thumb.png" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            // first, are they a Next Steps Coach?
            int groupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, GroupType_NextSteps, GroupTypeRole_Coach );
            if( groupId > -1 )
            {
                // update the entry with the groupId
                NextStepsCoach_Entry[ 2 ] = string.Format( NextStepsCoach_Entry[ 2 ], groupId );

                // yup.
                return NextStepsCoach_Entry;
            }

            // then are they a Neighborhood Group Coach?
            groupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.NeighborhoodGroupValueId, MobileAppApi.GroupMemberRole_Leader );
            if( groupId > -1 )
            {
                // update the entry with the groupId
                GroupCoach_Entry[ 2 ] = string.Format( GroupCoach_Entry[ 2 ], groupId );

                // yup.
                return GroupCoach_Entry;
            }

            // they aren't either, so return the "you should do this!!!1one"
            return NotCoach_Entry;
        }
    }
}

