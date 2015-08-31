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

                public List<Guid> CampusGuids { get; set; }

                // Set at runtime based on developer conditions
                public bool Private { get; set; }

                [JsonConstructor]
                public RockNews( string title, 
                                 string description, 
                                 string referenceUrl, 
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

                    IncludeImpersonationToken = rhs.IncludeImpersonationToken;

                    ImageURL = rhs.ImageURL;
                    ImageName = rhs.ImageName;

                    HeaderImageURL = rhs.HeaderImageURL;
                    HeaderImageName = rhs.HeaderImageName;

                    CampusGuids = rhs.CampusGuids;

                    // note we copy the private flag here, but don't set it in the default constructor
                    Private = rhs.Private;
                }
            }
        }
    }
}
