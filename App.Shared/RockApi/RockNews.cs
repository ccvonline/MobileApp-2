using System;
using Newtonsoft.Json;

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

                public Guid CampusGuid { get; set; }

                [JsonConstructor]
                public RockNews( string title, string description, string referenceUrl, string imageUrl, string imageName, string headerImageUrl, string headerImageName, Guid campusGuid )
                {
                    Title = title;
                    Description = description;
                    ReferenceURL = referenceUrl;

                    ImageURL = imageUrl;
                    ImageName = imageName;

                    HeaderImageURL = headerImageUrl;
                    HeaderImageName = headerImageName;

                    CampusGuid = campusGuid;
                }

                // create a copy constructor
                public RockNews( RockNews rhs )
                {
                    Title = rhs.Title;
                    Description = rhs.Description;
                    ReferenceURL = rhs.ReferenceURL;

                    ImageURL = rhs.ImageURL;
                    ImageName = rhs.ImageName;

                    HeaderImageURL = rhs.HeaderImageURL;
                    HeaderImageName = rhs.HeaderImageName;

                    CampusGuid = rhs.CampusGuid;
                }
            }
        }
    }
}
