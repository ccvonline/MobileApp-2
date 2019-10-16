using System;
using System.Collections.Generic;

namespace Rock.Client
{
    public class GuestFamily
    {
        public class Member
        {
            public int Id { get; set; }
            public int PersonAliasId { get; set; }
            public Guid Guid { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhotoUrl { get; set; }
            public bool CanCheckin { get; set; }
            public string Role { get; set; }
            public Rock.Client.Enums.Gender Gender { get; set; }
        }
        
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid Guid { get; set; }

        public List<Member> FamilyMembers { get; set; }

        public GuestFamily( )
        {
        }
    }
}

