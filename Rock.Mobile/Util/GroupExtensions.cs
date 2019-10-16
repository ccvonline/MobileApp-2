using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Mobile.Util
{
    public static class GroupExtensions
    {
        /// <summary>
        /// Given a groupType and groupRole, returns either -1 if they aren't the specified groupRole in the group,
        /// or the ID for the GROUP if they are.
        /// We can think about it like this because we know the user is SOME type of member for each Group in the list.
        /// </summary>
        public static int IsMemberTypeOfGroup( List<Rock.Client.Group> groups, int groupType, int groupRole )
        {
            // find all the groups of 'groupType' that
            List<Rock.Client.Group> groupList = groups.Where( g => g.GroupTypeId == groupType && g.IsActive == true ).ToList( );
            if( groupList != null )
            {
                // now for each group, find the groupMember for the person we're looking for.
                foreach( Rock.Client.Group targetGroup in groupList )
                {
                    // there should really only be one result. How could the same member multiple times in a single group?
                    // but, just check for any result, and if we get one, return the target groupID.
                    Rock.Client.GroupMember groupMember = targetGroup.Members
                        .Where( m => m.GroupRoleId == groupRole && m.GroupMemberStatus == Client.Enums.GroupMemberStatus.Active )
                        .FirstOrDefault();

                    if( groupMember != null )
                    {
                        return targetGroup.Id;
                    }
                }
            }

            return -1;
        }
    }
}

