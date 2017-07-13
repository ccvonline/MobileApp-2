using System;
using System.Collections.Generic;
using MobileApp.Shared.PrivateConfig;
using MobileApp;
using MobileApp.Shared.Config;
using MobileApp.Shared.Strings;

namespace MobileApp.Shared
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
        string[] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser );
    }

    public class StartingPoint_Engagement : IEngagement
    {
        string [] StartingPoint_Entry = { "Starting Point", "Begin your journey at CCV", Config.GeneralConfig.RockBaseUrl + "ma-startingpoint", "starting_point_thumb.png", "startingpoint" };

        public string[] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser )
        {
            return StartingPoint_Entry;
        }
    }

    public class Baptism_Engagement : IEngagement
    {
        string [] Baptism_Entry = { "Baptisms", "Make your faith public", Config.GeneralConfig.RockBaseUrl + "ma-baptism", "baptism_thumb.png", "baptism" };

        public string[ ] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser )
        {
            return Baptism_Entry;
        }
    }

    public class Worship_Engagement : IEngagement
    {
        string [] Worship_Entry = { "Worship", "Worship God with others", Config.GeneralConfig.RockBaseUrl + "ma-worship", "worship_thumb.png", "worship" };

        public string[ ] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser )
        {
            return Worship_Entry;
        }
    }

    public class Connect_Engagement : IEngagement
    {
        string [] Connect_Entry = { ConnectStrings.Main_Connect_GroupFinder, ConnectStrings.Main_Connect_GroupFinder_SubTitle, "", PrivateConnectConfig.GroupFinder_IconImage, "" };

        public string[ ] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser )
        {
            return Connect_Entry;
        }
    }

    public class Serve_Engagement : IEngagement
    {
        string [] Serve_Entry = { "Serve", "Impact lives by serving", Config.GeneralConfig.RockBaseUrl + "ma-serve", "serve_thumb.png", "serve" };

        public string[ ] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser )
        {
            return Serve_Entry;
        }
    }

    public class Give_Engagement : IEngagement
    {
        string [] Give_Entry = { "Give", "Trust God financially", "external:" + GiveConfig.GiveUrl, "give_thumb.png", "give" };

        public string[ ] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser )
        {
            return Give_Entry;
        }
    }

    public class Share_Engagement : IEngagement
    {
        string [] Share_Entry = { "Share", "Share your story", Config.GeneralConfig.RockBaseUrl + "ma-mystory", "share_thumb.png", "share" };

        public string[ ] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser )
        {
            return Share_Entry;
        }
    }

    /// <summary>
    /// Coach Engagement is the entry in the list for Becoming a Next Steps Coach, a Neighborhood Coach, or requesting to become or receieve a coach.
    /// </summary>
    public class Coach_Engagement : IEngagement
    {
        // note - we'll use the same URL for both; it's just the wording of the entry that changes.
        // Accessing toolboxes normally goes hand in hand with "IsTeaching", but for people < 18, they can't be 
        // considered Teaching, but CAN still access the toolbox. The ma-coaching page will handle that.
        
        // Coach
        string[] Coach_Entry = { "Coach", "Access your toolbox", Config.GeneralConfig.RockBaseUrl + "ma-coaching", "coach_thumb.png", "coach" };

        // Not Coach 
        string[] NotCoach_Entry = { "Coaches", "Learn about coaching", Config.GeneralConfig.RockBaseUrl + "ma-coaching", "coach_thumb.png", "coach" };

        public string[ ] GetEntry( MobileApp.Shared.Network.RockMobileUser mobileUser )
        {
            if( mobileUser.IsTeaching )
            {
                return Coach_Entry;
            }
            
            // they aren't either, so return the "you should do this!!!1one"
            return NotCoach_Entry;
        }
    }
}

