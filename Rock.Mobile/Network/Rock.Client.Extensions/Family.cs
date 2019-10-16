using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rock.Client
{
    public class Family
    {
        [JsonProperty]
        public int Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public Rock.Client.Location HomeLocation { get; set; }

        [JsonProperty]
        public List<Rock.Client.GroupMember> FamilyMembers { get; set; }

        [JsonProperty]
        public Rock.Client.PhoneNumber MainPhoneNumber { get; set; }

        public Family( )
        {
            FamilyMembers = new List<Rock.Client.GroupMember>( );
            HomeLocation = new Rock.Client.Location();
            MainPhoneNumber = new Rock.Client.PhoneNumber();
        }
    }
}

