using System;
using RestSharp;
using Rock.Mobile.Network;
using RestSharp.Deserializers;
using System.IO;

namespace App.Shared
{
    public class VimeoVideo
    {
        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "title")]
        public string Title { get; set; }

        [DeserializeAs(Name = "description")]
        public string Description { get; set; }

        [DeserializeAs(Name = "url")]
        public string Url { get; set; }

        [DeserializeAs(Name = "upload_date")]
        public string UploadDate { get; set; }

        [DeserializeAs(Name = "mobile_url")]
        public string MobileUrl { get; set; }

        [DeserializeAs(Name = "thumbnail_small")]
        public string ThumbnailSmall { get; set; }

        [DeserializeAs(Name = "thumbnail_medium")]
        public string ThumbnailMedium { get; set; }

        [DeserializeAs(Name = "thumbnail_large")]
        public string ThumbnailLarge { get; set; }

        [DeserializeAs(Name = "user_id")]
        public string UserId { get; set; }

        [DeserializeAs(Name = "user_name")]
        public string UserName { get; set; }

        [DeserializeAs(Name = "user_url")]
        public string UserUrl { get; set; }

        [DeserializeAs(Name = "user_portrait_small")]
        public string UserPortraitSmall { get; set; }

        [DeserializeAs(Name = "user_portrait_medium")]
        public string UserPortraitMedium { get; set; }

        [DeserializeAs(Name = "user_portrait_large")]
        public string UserPortraitLarge { get; set; }

        [DeserializeAs(Name = "user_portrait_huge")]
        public string UserPortraitHuge { get; set; }

        [DeserializeAs(Name = "stats_number_of_likes")]
        public int StatsNumberOfLikes { get; set; }

        [DeserializeAs(Name = "stats_number_of_plays")]
        public int StatsNumberOfPlays { get; set; }

        [DeserializeAs(Name = "stats_number_of_comments")]
        public int StatsNumberOfComments { get; set; }

        [DeserializeAs(Name = "duration")]
        public int Duration { get; set; }

        [DeserializeAs(Name = "width")]
        public int Width { get; set; }

        [DeserializeAs(Name = "height")]
        public int Height { get; set; }

        [DeserializeAs(Name = "tags")]
        public string Tags { get; set; }

        [DeserializeAs(Name = "embed_privacy")]
        public string EmbedPrivacy { get; set; }
    }

    public class VimeoManager
    {
        static VimeoManager _Instance = new VimeoManager( );
        public static VimeoManager Instance { get { return _Instance; } }

        HttpRequest HttpRequest { get; set; }

        VimeoManager( )
        {
            HttpRequest = new HttpRequest();
        }

        public delegate void RequestComplete( System.Net.HttpStatusCode statusCode, string statusDescription, MemoryStream imageBuffer );

        public void GetVideoThumbnail( string videoUrl, RequestComplete resultHandler )
        {
            // get the video's ID
            string videoId = videoUrl.Substring( videoUrl.LastIndexOf( "/" ) + 1 );
            videoId = videoId.Substring( 0, videoId.IndexOf( "." ) );

            // get the metadata for this video, which will contain the thumbnail url
            RestRequest request = new RestRequest( Method.GET );
            request.RequestFormat = DataFormat.Xml;

            string requestUrl = string.Format( "http://vimeo.com/api/v2/video/{0}.xml", videoId );
            HttpRequest.ExecuteAsync<VimeoVideo>( requestUrl, request, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, VimeoVideo model) 
                {
                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                    {
                        RequestThumbnail( model.ThumbnailLarge, resultHandler );
                    }
                    else
                    {
                        resultHandler( statusCode, statusDescription, null );
                    }
                } );
        }

        void RequestThumbnail( string thumbnailUrl, RequestComplete resultHandler )
        {
            // grab the actual image
            RestRequest request = new RestRequest( Method.GET );
                        
            // get the raw response
            HttpRequest.ExecuteAsync( thumbnailUrl, request, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, byte[] model )
                {
                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                    {
                        MemoryStream memoryStream = new MemoryStream( model );

                        resultHandler( statusCode, statusDescription, memoryStream );

                        memoryStream.Dispose( );
                    }
                    else
                    {
                        resultHandler( statusCode, statusDescription, null );
                    }
                } );
        }
    }
}

