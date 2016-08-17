using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace App
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Contains a news item for the news display.
            /// </summary>
            public class RockNews
            {
                public string Title { get; set; }
                public string Description { get; set; }
                public string ReferenceURL { get; set; }

                public string ImageURL { get; set; }
                public string ImageName { get; set; }

                public string HeaderImageURL { get; set; }
                public string HeaderImageName { get; set; }

                public bool ReferenceUrlLaunchesBrowser { get; set; }

                public bool IncludeImpersonationToken { get; set; }

                public bool SkipDetailsPage { get; set; }

                public List<Guid> CampusGuids { get; set; }

                // Set at runtime based on developer conditions
                public DateTime Developer_StartTime { get; set; }
                public DateTime? Developer_EndTime { get; set; }
                public bool Developer_Private { get; set; }
                public int Developer_Priority { get; set; }
                public Rock.Client.Enums.ContentChannelItemStatus Developer_ItemStatus { get; set; }

                [JsonConstructor]
                public RockNews( string title, 
                                 string description, 
                                 string referenceUrl, 
                                 bool skipDetailsPage,
                                 bool referenceUrlLaunchesBrowser, 
                                 bool includeImpersonationToken, 
                                 string imageUrl, 
                                 string imageName, 
                                 string headerImageUrl, 
                                 string headerImageName, 
                                 List<Guid> campusGuids )
                {
                    Title = title;
                    Description = description;
                    ReferenceURL = referenceUrl;

                    ReferenceUrlLaunchesBrowser = referenceUrlLaunchesBrowser;

                    SkipDetailsPage = skipDetailsPage;

                    IncludeImpersonationToken = includeImpersonationToken;

                    ImageURL = imageUrl;
                    ImageName = imageName;

                    HeaderImageURL = headerImageUrl;
                    HeaderImageName = headerImageName;

                    CampusGuids = campusGuids;
                }

                // create a copy constructor
                public RockNews( RockNews rhs )
                {
                    Title = rhs.Title;
                    Description = rhs.Description;
                    ReferenceURL = rhs.ReferenceURL;

                    ReferenceUrlLaunchesBrowser = rhs.ReferenceUrlLaunchesBrowser;

                    SkipDetailsPage = rhs.SkipDetailsPage;

                    IncludeImpersonationToken = rhs.IncludeImpersonationToken;

                    ImageURL = rhs.ImageURL;
                    ImageName = rhs.ImageName;

                    HeaderImageURL = rhs.HeaderImageURL;
                    HeaderImageName = rhs.HeaderImageName;

                    CampusGuids = rhs.CampusGuids;

                    // note we copy the developer flags here, but don't set it in the default constructor
                    Developer_Private = rhs.Developer_Private;
                    Developer_StartTime = rhs.Developer_StartTime;
                    Developer_EndTime = rhs.Developer_EndTime;
                    Developer_ItemStatus = rhs.Developer_ItemStatus;
                }

                public string GetDeveloperInfo( )
                {
                    string developerInfo = "";

                    developerInfo += "\n\n----DEVELOPER INFO----";
                    developerInfo += "\n-=-=-=-=-=-=-=-=-=-=-=-=-";

                    string approvalStatus = "\nApproval Status: {0}";

                    switch( Developer_ItemStatus )
                    {
                        case Rock.Client.Enums.ContentChannelItemStatus.Approved:
                        {
                            approvalStatus = string.Format( approvalStatus, "Approved" );
                            break;
                        }

                        case Rock.Client.Enums.ContentChannelItemStatus.PendingApproval:
                        {
                            approvalStatus = string.Format( approvalStatus, "Pending" );
                            break;
                        }

                        case Rock.Client.Enums.ContentChannelItemStatus.Denied:
                        {
                            approvalStatus = string.Format( approvalStatus, "Denied" );
                            break;
                        }

                    }
                    developerInfo += approvalStatus;

                    developerInfo += string.Format( "\n\nPriority: {0}", Developer_Priority );

                    string campuses = "";

                    if ( CampusGuids.Count > 0 )
                    {
                        foreach ( Guid campusGuid in CampusGuids )
                        {
                            campuses += "\n" + App.Shared.Network.RockLaunchData.Instance.Data.CampusGuidToName( campusGuid );
                        }
                        developerInfo += string.Format( "\n\nCampuses:{0}", campuses );
                    }
                    else
                    {
                        campuses = "\nAll Campuses";
                    }


                    developerInfo += string.Format( "\n\nStart Time: {0}", Developer_StartTime );
                    developerInfo += string.Format( "\nEnd Time: " );

                    if ( Developer_EndTime.HasValue == true )
                    {
                        developerInfo += string.Format( "{0}", Developer_EndTime.Value );
                    }
                    else
                    {
                        developerInfo += string.Format( "None" );
                    }

                    return developerInfo;
                }
            }
        }
    }
}
