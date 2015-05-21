using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp.Serializers;

namespace App.Shared
{
    namespace Notes.Model
    {
        public class NoteDB
        {
            public NoteDB( )
            {
                HostDomain = "";
                SeriesList = new List<Series>( );
            }

            /// <summary>
            /// Should be called after a successful creation / download, 
            /// </summary>
            public void MakeURLsAbsolute( )
            {
                foreach ( Series singleSeries in SeriesList )
                {
                    foreach ( Series.Message message in singleSeries.Messages )
                    {
                        message.MakeURLsAbsolute( HostDomain );
                    }

                    singleSeries.MakeURLsAbsolute( HostDomain );
                }
            }

            public void ProcessPrivateNotes( bool allowPrivate )
            {
                List<Series> privateSeries = new List<Series>( );
                List<Series.Message> privateMessages = new List<Series.Message>( );
                
                // if allowing private is false, remove them any private series or message.
                if ( allowPrivate == false )
                {
                    // SERIES
                    foreach ( Series singleSeries in SeriesList )
                    {
                        // first, is this series private? If so it's going away,
                        // so there's no point in processing its messages
                        if ( singleSeries.Private == true )
                        {
                            privateSeries.Add( singleSeries );
                        }
                        else
                        {
                            // MESSAGES
                            foreach ( Series.Message message in singleSeries.Messages )
                            {
                                // if the message is marked as private, add it to our list for removal
                                if ( message.Private == true )
                                {
                                    privateMessages.Add( message );
                                }
                            }

                            // now remove each private message from the series
                            foreach ( Series.Message privateMessage in privateMessages )
                            {
                                singleSeries.Messages.Remove( privateMessage );
                            }
                        }
                    }

                    // finally remove all private series
                    foreach ( Series series in privateSeries )
                    {
                        SeriesList.Remove( series );
                    }
                }
            }

            /// <summary>
            /// The root server that the series are stored on, allowing the series themselves
            /// to define locations in relative terms, rather than absolute
            /// </summary>
            public string HostDomain { get; set; }


            /// <summary>
            /// The list of sermon series and messages. This goes here so that
            /// we can shortcut the user to the latest message from the main page if they want.
            /// It also allows us to store them so they don't need to be downloaded every time they
            /// visit the Messages page.
            /// </summary>
            /// <value>The series.</value>
            public List<Series> SeriesList { get; set; }
        }

        /// <summary>
        /// Represents a "series" of weekly messages, like "At The Movies", "White Christmas", or "Wisdom"
        /// </summary>
        public class Series
        {
            public static char[] TrimChars = new char[] { ' ', '\n', '\t' };

            /// <summary>
            /// Represents the individual messages within each series
            /// </summary>
            public class Message
            {
                public Message( )
                {
                }

                [JsonConstructor]
                public Message( string name, string speaker, string date, string noteUrl, string audioUrl, string watchUrl, string shareUrl )
                {
                    Name = name;
                    Speaker = speaker;
                    Date = date;
                    NoteUrl = noteUrl;
                    AudioUrl = audioUrl;
                    WatchUrl = watchUrl;
                    ShareUrl = shareUrl;
                }

                public void MakeURLsAbsolute( string hostDomain )
                {
                    // for any URL that isn't absolute, prefix the host domain
                    if ( _AudioUrl != null && _AudioUrl.Contains( "http://" ) == false )
                    {
                        _AudioUrl = _AudioUrl.Insert( 0, hostDomain );
                    }

                    if ( _NoteUrl != null && _NoteUrl.Contains( "http://" ) == false )
                    {
                        _NoteUrl = _NoteUrl.Insert( 0, hostDomain );
                    }

                    if ( _WatchUrl != null && _WatchUrl.Contains( "http://" ) == false )
                    {
                        _WatchUrl = _WatchUrl.Insert( 0, hostDomain );
                    }

                    if ( _ShareUrl != null && _ShareUrl.Contains( "http://" ) == false )
                    {
                        _ShareUrl = _ShareUrl.Insert( 0, hostDomain );
                    }
                }

                /// <summary>
                /// Name of the message
                /// </summary>
                string _Name;
                public string Name
                {
                    get
                    {
                        return _Name;
                    }

                    protected set
                    {
                        _Name = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }


                /// <summary>
                /// Speaker for this message
                /// </summary>
                string _Speaker;
                public string Speaker
                {
                    get
                    {
                        return _Speaker;
                    }

                    protected set
                    {
                        _Speaker = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// The date this message was given.
                /// </summary>
                string _Date;
                public string Date
                {
                    get
                    {
                        return _Date;
                    }

                    protected set
                    {
                        _Date = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// Url of the note for this message
                /// </summary>
                string _NoteUrl;
                public string NoteUrl
                {
                    get
                    {
                        return _NoteUrl;
                    }

                    protected set
                    {
                        _NoteUrl = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// Url where the video can be found for listening
                /// </summary>
                string _AudioUrl;
                public string AudioUrl
                {
                    get
                    {
                        return _AudioUrl;
                    }

                    protected set
                    {
                        _AudioUrl = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// Url where the video can be found for watching in the browser
                /// </summary>
                string _WatchUrl;
                public string WatchUrl
                {
                    get
                    {
                        return _WatchUrl;
                    }

                    protected set
                    {
                        _WatchUrl = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// Url to use when sharing the video message with someone. (Differs from the WatchUrl
                /// because it may need to link to say the company website's embedded view page) 
                /// </summary>
                string _ShareUrl;
                public string ShareUrl
                {
                    get
                    {
                        return _ShareUrl;
                    }

                    protected set
                    {
                        _ShareUrl = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// If true, this will only be displayed if the user has 'refresh notes' enabled.
                /// This allows notes to be worked on without the public seeing them.
                /// </summary>
                [SerializeAs(Name = "Private")]
                public bool Private { get; protected set; }
            }

            public Series( )
            {
            }

            [JsonConstructor]
            public Series( string name, string description, string billboardUrl, string thumbnailUrl, string dateRanges, List<Message> messages )
            {
                Name = name;
                Description = description;
                BillboardUrl = billboardUrl;
                ThumbnailUrl = thumbnailUrl;
                DateRanges = dateRanges;

                Messages = messages;
            }

            public void MakeURLsAbsolute( string hostDomain )
            {
                if ( _BillboardUrl != null && _BillboardUrl.Contains( "http://" ) == false )
                {
                    _BillboardUrl = _BillboardUrl.Insert( 0, hostDomain );
                }

                if ( _ThumbnailUrl != null && _ThumbnailUrl.Contains( "http://" ) == false )
                {
                    _ThumbnailUrl = _ThumbnailUrl.Insert( 0, hostDomain );
                }
            }

            /// <summary>
            /// Name of the series
            /// </summary>
            string _Name;
            public string Name
            {
                get
                {
                    return _Name;
                }

                protected set
                {
                    _Name = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// Summary of what the messages in this series will cover
            /// </summary>
            string _Description;
            public string Description
            { 
                get
                {
                    return _Description;
                }

                protected set
                {
                    _Description = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// Url to the billboard graphic representing this series
            /// </summary>
            string _BillboardUrl;
            public string BillboardUrl
            { 
                get
                {
                    return _BillboardUrl;
                }

                protected set
                {
                    _BillboardUrl = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// Url to the thumbnail graphic representing this series
            /// </summary>
            string _ThumbnailUrl;
            public string ThumbnailUrl
            { 
                get
                {
                    return _ThumbnailUrl;
                }

                protected set
                {
                    _ThumbnailUrl = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// The range of dates this series covered.
            /// </summary>
            string _DateRanges;
            public string DateRanges
            {
                get
                {
                    return _DateRanges;
                }

                protected set
                {
                    _DateRanges = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// If true, this will only be displayed if the user has 'refresh notes' enabled.
            /// This allows notes to be worked on without the public seeing them.
            /// </summary>
            [SerializeAs(Name = "Private")]
            public bool Private { get; protected set; }

            /// <summary>
            /// List of all the messages within this series
            /// </summary>
            public List<Message> Messages { get; protected set; }
        }
    }
}

