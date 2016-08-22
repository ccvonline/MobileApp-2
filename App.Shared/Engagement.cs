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
        string [] StartingPoint_Entry = { "Starting Point", "Begin your journey at CCV", Config.GeneralConfig.RockBaseUrl + "ma-startingpoint", "starting_point_thumb.png", "startingpoint" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return StartingPoint_Entry;
        }
    }

    public class Baptism_Engagement : IEngagement
    {
        string [] Baptism_Entry = { "Baptisms", "Make your faith public", Config.GeneralConfig.RockBaseUrl + "ma-baptism", "baptism_thumb.png", "baptism" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Baptism_Entry;
        }
    }

    public class Worship_Engagement : IEngagement
    {
        string [] Worship_Entry = { "Worship", "Worship God with others", Config.GeneralConfig.RockBaseUrl + "ma-worship", "worship_thumb.png", "worship" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Worship_Entry;
        }
    }

    public class Connect_Engagement : IEngagement
    {
        string [] Connect_Entry = { ConnectStrings.Main_Connect_GroupFinder, ConnectStrings.Main_Connect_GroupFinder_SubTitle, "", PrivateConnectConfig.GroupFinder_IconImage, "" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Connect_Entry;
        }
    }

    public class Serve_Engagement : IEngagement
    {
        string [] Serve_Entry = { "Serve", "Impact lives by serving", Config.GeneralConfig.RockBaseUrl + "ma-serve", "serve_thumb.png", "serve" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Serve_Entry;
        }
    }

    public class Give_Engagement : IEngagement
    {
        string [] Give_Entry = { "Give", "Trust God financially", "external:" + GiveConfig.GiveUrl, "give_thumb.png", "give" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            return Give_Entry;
        }
    }

    public class Share_Engagement : IEngagement
    {
        string [] Share_Entry = { "Share", "Share your story", Config.GeneralConfig.RockBaseUrl + "ma-mystory", "share_thumb.png", "share" };

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
        string[] Coach_Entry = { "Coach", "Access your toolbox", Config.GeneralConfig.RockBaseUrl + "ma-my-groups", "coach_thumb.png", "coach" };

        // Not Coach 
        string[] NotCoach_Entry = { "Coaches", "Learn about coaching", Config.GeneralConfig.RockBaseUrl + "ma-request-coach", "coach_thumb.png", "coach" };

        public string[] GetEntry( List<Rock.Client.Group> groups )
        {
            // NEXT STEPS COACH / ASST COACH
            int nsCoachGroupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.GroupType_NextStepsGroupId, PrivateGeneralConfig.GroupTypeRole_NSGroup_CoachId );
            int nsAsstCoachGroupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.GroupType_NextStepsGroupId, PrivateGeneralConfig.GroupTypeRole_NSGroup_AsstCoachId );

            // NEIGHBORHOOD COACH / ASST COACH
            int nhCoachGroupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.GroupType_NeighborhoodGroupId, PrivateGeneralConfig.GroupTypeRole_NHGroup_CoachId );
            int nhAsstCoachGroupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.GroupType_NeighborhoodGroupId, PrivateGeneralConfig.GroupTypeRole_NHGroup_AsstCoachId );

            // NEXTGEN COACH / ASST COACH
            int ngCoachGroupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.GroupType_NextGenGroupId, PrivateGeneralConfig.GroupTypeRole_NGGroup_CoachId );
            int ngAsstCoachgroupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.GroupType_NextGenGroupId, PrivateGeneralConfig.GroupTypeRole_NGGroup_AsstCoachId );

            // YOUNG ADULT COACH / ASST COACH
            int yaCoachGroupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.GroupType_YoungAdultsGroupId, PrivateGeneralConfig.GroupTypeRole_YAGroup_CoachId );
            int yaAsstCoachgroupId = Rock.Mobile.Util.GroupExtensions.IsMemberTypeOfGroup( groups, PrivateGeneralConfig.GroupType_YoungAdultsGroupId, PrivateGeneralConfig.GroupTypeRole_YAGroup_AsstCoachId );

            // if any of these are > -1, they're some type of coach
                 
                 // Next Steps
            if ( nsCoachGroupId > -1 || 
                 nsAsstCoachGroupId > -1 || 
                 
                 // Neighborhood
                 nhCoachGroupId > -1 || 
                 nhAsstCoachGroupId > -1 || 

                 //Next Gen
                 ngCoachGroupId > -1 || 
                 ngAsstCoachgroupId > -1 || 

                 // Young Adults
                 yaCoachGroupId > -1 || 
                 yaAsstCoachgroupId > -1 )
            {
                return Coach_Entry;
            }

            // they aren't either, so return the "you should do this!!!1one"
            return NotCoach_Entry;
        }
    }
}

