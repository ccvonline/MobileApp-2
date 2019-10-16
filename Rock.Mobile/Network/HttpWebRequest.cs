using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Deserializers;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using RestSharp.Authenticators;

namespace Rock.Mobile
{
    namespace Network
    {
        internal class WebRequestManager
        {
            /// <summary>
            /// The timeout after which the REST call attempt is given up.
            /// </summary>
            const int RequestTimeoutMS = 15000;

            /// <summary>
            /// Interface that allows us to store multiple result handlers of varying generic types
            /// </summary>
            internal interface IResultHandler
            {
                void Invoke( HttpStatusCode statusCode, string statusDesc, IRestResponse response );
            }

            // Define the request result object. This is responsible for calling back
            // the appropriate delegate when the request is complete
            internal class RequestResultObject<TModel> : IResultHandler where TModel : new( )
            {
                /// <summary>
                /// The handler to call when the request is complete
                /// </summary>
                HttpRequest.RequestResult<TModel> ResultHandler { get; set; }

                public RequestResultObject( HttpRequest.RequestResult<TModel> resultHandler )
                {
                    ResultHandler = resultHandler;
                }

                TModel Deserialize( IRestResponse response )
                {
                    // watch for parse errors, and if that happens we'll return an error.
                    string contentType = response.ContentType.ToLower( );
                    try
                    {
                        if ( contentType.Contains( "application/json" ) || contentType.Contains( "text/json" ) )
                        {
                            JsonDeserializer deserializer = new JsonDeserializer();
                            return (TModel)deserializer.Deserialize<TModel>( response );
                        }

                        if ( contentType.Contains( "application/xml" ) || contentType.Contains( "text/xml" ) )
                        {
                            XmlDeserializer deser = new XmlDeserializer();
                            return (TModel) deser.Deserialize<TModel>( response );
                        }
                    }
#if DEBUG
                    catch(Exception e)
                    {
                        Rock.Mobile.Util.Debug.WriteLine( string.Format( "Parsing Error! {0}", e ) );
                    }
#else
                    catch
                    {
                    }
#endif

                    Rock.Mobile.Util.Debug.WriteLine( string.Format( "Unknown ContentType received from RestSharp. {0}", contentType ) );
                    return new TModel();
                }

                public void Invoke( HttpStatusCode statusCode, string statusDesc, IRestResponse response )
                {
                    TModel obj = default(TModel);

                    // if they want the actual RestResponse
                    if ( typeof( TModel ) == typeof( RestResponse ) )
                    {
                        // just cast and return it.
                        obj = (TModel)response;
                    }
                    // or if they wanted a particular type
                    else
                    {
                        // either deserialize to that type
                        if ( string.IsNullOrWhiteSpace( response.ContentType ) == false )
                        {
                            // deserialize this ourselves according to our generic type.
                            obj = Deserialize( response );
                        }
                        else
                        {
                            // or provide them with a new object of their desired type
                            obj = new TModel();
                        }
                    }

                    // exception or not, notify the caller of the desponse
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        { 
                            ResultHandler( statusCode, 
                                statusDesc,
                                obj );
                        } );
                }
            }


            /// <summary>
            /// Common interface to allow different generics in the same queue
            /// </summary>
            internal interface IWebRequestObject
            {
                void ProcessRequest( );
                string GetRequestUrl( );
                void AttachResultHandler( IResultHandler resultHandler );
            }


            /// <summary>
            /// Implementation of our request object. Stores the URL, request and handler to be executed later
            /// on the worker thread.
            /// </summary>
            internal class WebRequestObject<TModel> : IWebRequestObject where TModel : new( )
            {
                /// <summary>
                /// URL for the web request
                /// </summary>
                string RequestUrl { get; set; }

                /// <summary>
                /// The request object containing the relavent HTTP request data
                /// </summary>
                RestRequest Request { get; set; }

                /// <summary>
                /// The handlers to call when the request is complete
                /// </summary>
                List<IResultHandler> ResultHandlers { get; set; }

                /// <summary>
                /// If relavant, the cookie container this request should use.
                /// </summary>
                /// <value>The cookie container.</value>
                CookieContainer CookieContainer { get; set; }

                string BasicAuthentication_UserName { get; set; }
                string BasicAuthentication_Password { get; set; }

                public WebRequestObject( string requestUrl, RestRequest request, HttpRequest.RequestResult<TModel> callback, CookieContainer cookieContainer, string basicAuthUsername, string basicAuthPassword )
                {
                    RequestUrl = requestUrl;
                    Request = request;
                    CookieContainer = cookieContainer;

                    ResultHandlers = new List<IResultHandler>( );

                    AttachResultHandler( new RequestResultObject<TModel>( callback ) );

                    BasicAuthentication_UserName = basicAuthUsername;
                    BasicAuthentication_Password = basicAuthPassword;
                }

                public void AttachResultHandler( IResultHandler resultHandler )
                {
                    ResultHandlers.Add( resultHandler );
                }

                public string GetRequestUrl( )
                {
                    return RequestUrl;
                }

                public void ProcessRequest( )
                {
                    RestClient restClient = new RestClient( );
                    restClient.CookieContainer = CookieContainer;

                    if( string.IsNullOrWhiteSpace( BasicAuthentication_UserName ) == false ||
                        string.IsNullOrWhiteSpace( BasicAuthentication_Password ) == false )
                    {
                        restClient.Authenticator = new HttpBasicAuthenticator( BasicAuthentication_UserName, BasicAuthentication_Password );
                    }

                    restClient.BaseUrl = new System.Uri( RequestUrl );

                    // if no custom timeout value was set, then default to a 15 second wait time.
                    if( Request.Timeout == 0 )
                    { 
                        Request.Timeout = RequestTimeoutMS;
                    }

                    // execute the request and get the response
                    IRestResponse response = restClient.Execute( Request );

                    // now invoke each handler, and give it the response. It will manage deserializing according
                    // to the type of wants.
                    foreach ( IResultHandler handler in ResultHandlers )
                    {
                        handler.Invoke( response != null ? response.StatusCode : HttpStatusCode.RequestTimeout, 
                            response != null ? response.StatusDescription : "Client has no connection.", 
                            response );
                    }
                }
            }

            /// <summary>
            /// The singleton for our request manager. All web requests will funnel thru this.
            /// </summary>
            static WebRequestManager _Instance = new WebRequestManager( );
            public static WebRequestManager Instance { get { return _Instance; } }

            /// <summary>
            /// The queue of web requests that need to be executed
            /// </summary>
            ConcurrentQueue<IWebRequestObject> RequestQueue { get; set; }

            /// <summary>
            /// Pointer to the worker thread for downloading
            /// </summary>
            System.Threading.Thread DownloadThread { get; set; }

            /// <summary>
            /// Simply lets the download thread sleep while the queue is empty.
            /// </summary>
            /// <value>The queue process handle.</value>
            EventWaitHandle QueueProcessHandle { get; set; }

            EventWaitHandle RequestUpdateHandle { get; set; }

            public WebRequestManager( )
            {
                // create our queue and fire up the download thread
                RequestQueue = new ConcurrentQueue<IWebRequestObject>();

                DownloadThread = new System.Threading.Thread( ThreadProc );
                DownloadThread.Start( );

                QueueProcessHandle = new EventWaitHandle( false, EventResetMode.AutoReset );
                RequestUpdateHandle = new EventWaitHandle( true, EventResetMode.AutoReset );
            }


            /// <summary>
            /// The one entry point, this is where requests should be sent
            /// </summary>
            public void TryPushRequest( IWebRequestObject requestObj, IResultHandler resultHandler )
            {
                // first, lock the queue
                RequestUpdateHandle.WaitOne( );

                // now we can be sure that it won't be pulled out and processed while we're trying to attach a handler.
                IWebRequestObject currObj = RequestQueue.Where( r => r.GetRequestUrl( ) == requestObj.GetRequestUrl( ) ).SingleOrDefault( );
                if ( currObj != null )
                {
                    Rock.Mobile.Util.Debug.WriteLine( string.Format( "{0} already requested. Not queueing.", currObj.GetRequestUrl( ) ) );
                    currObj.AttachResultHandler( resultHandler );
                }
                else
                {
                    RequestQueue.Enqueue( requestObj );

                    Rock.Mobile.Util.Debug.WriteLine( "Setting Wait Handle" );
                    QueueProcessHandle.Set( );
                }

                // notify the thread it's ok to pull out more queue objects
                RequestUpdateHandle.Set( );
            }

            void ThreadProc( )
            {
                while ( true )
                {
                    Rock.Mobile.Util.Debug.WriteLine( "ThreadProc: Sleeping..." );
                    QueueProcessHandle.WaitOne( );
                    Rock.Mobile.Util.Debug.WriteLine( "ThreadProc: Waking for work" );

                    // while there are requests pending, process them
                    while ( RequestQueue.IsEmpty == false )
                    {
                        Rock.Mobile.Util.Debug.WriteLine( "ThreadProc: Processing Request" );

                        // get the web request out of the queue
                        RequestUpdateHandle.WaitOne( ); //Wait to make sure no other thread is using the Queue
                        IWebRequestObject requestObj = null;
                        RequestQueue.TryDequeue( out requestObj ); //yank it out
                        RequestUpdateHandle.Set( ); // all clear

                        if( requestObj != null )
                        {
                            // execute it
                            requestObj.ProcessRequest( );
                        }
                    }
                }
            }
        }
        
        public class HttpRequest
        {
            /// <summary>
            /// Request Response delegate that does not require a returned object
            /// </summary>
            public delegate void RequestResult(System.Net.HttpStatusCode statusCode, string statusDescription);

            /// <summary>
            /// Request response delegate that does require a returned object
            /// </summary>
            public delegate void RequestResult<TModel>(System.Net.HttpStatusCode statusCode, string statusDescription, TModel model);

            public CookieContainer CookieContainer { get; set; }

            /// <summary>
            /// Wrapper for ExecuteAsync<> that requires no generic Type.
            /// </summary>
            /// <param name="request">Request.</param>
            /// <param name="resultHandler">Result handler.</param>
            public void ExecuteAsync( string requestUrl, RestRequest request, RequestResult resultHandler )
            {
                ExecuteAsync<object>( requestUrl, request, delegate(HttpStatusCode statusCode, string statusDescription, object model) 
                    {
                        // call the provided handler and drop the dummy object
                        if ( resultHandler != null )
                        {
                            resultHandler( statusCode, statusDescription );
                        }
                    });
            }

            /// <summary>
            /// Wrapper for ExecuteAsync<> that returns to the user the raw bytes of the request (useful for image retrieval or any non-object based return type)
            /// </summary>
            /// <param name="request">Request.</param>
            /// <param name="resultHandler">Result handler.</param>
            public void ExecuteAsync( string requestUrl, RestRequest request, RequestResult<byte[]> resultHandler )
            {
                // to give them the raw data, we'll call ExecuteAsync<> and pass in the acutal response object as the type.
                ExecuteAsync<RestSharp.RestResponse>( requestUrl, request, delegate(HttpStatusCode statusCode, string statusDescription, RestSharp.RestResponse model) 
                    {
                        // then, we'll call the result handler and pass the raw bytes.
                        resultHandler( statusCode, statusDescription, model.RawBytes );
                    });
            }

            public void ExecuteAsync<TModel>( string requestUrl, RestRequest request, RequestResult<TModel> resultHandler ) where TModel : new( )
            {
                WebRequestManager.WebRequestObject<TModel> requestObj = new WebRequestManager.WebRequestObject<TModel>( requestUrl, request, resultHandler, CookieContainer, string.Empty, string.Empty );

                WebRequestManager.Instance.TryPushRequest( requestObj, new WebRequestManager.RequestResultObject<TModel>( resultHandler ) );
            }

            public void ExecuteAsync<TModel>( string requestUrl, RestRequest request, string basicAuthUsername, string basicAuthPassword, RequestResult<TModel> resultHandler ) where TModel : new()
            {
            	WebRequestManager.WebRequestObject<TModel> requestObj = new WebRequestManager.WebRequestObject<TModel>( requestUrl, request, resultHandler, CookieContainer, basicAuthUsername, basicAuthPassword );

            	WebRequestManager.Instance.TryPushRequest( requestObj, new WebRequestManager.RequestResultObject<TModel>( resultHandler ) );
            }
        }

        /// <summary>
        /// Implements a RestSharp Json deserializer that uses Json.Net,
        /// which has better compatibility with things like ICollection
        /// </summary>
        class JsonDeserializer : IDeserializer
        {
            //
            // Properties
            //
            public string DateFormat
            {
                get;
                set;
            }

            public string Namespace
            {
                get;
                set;
            }

            public string RootElement
            {
                get;
                set;
            }

            //
            // Methods
            //
            public T Deserialize<T>( IRestResponse response )
            {
                return (T)JsonConvert.DeserializeObject<T>( response.Content );
            }
        }

        public class Util
        {
            /// <summary>
            /// Convenience method when you just need to know if a return was in the 200 range.
            /// </summary>
            /// <returns><c>true</c>, if in success range was statused, <c>false</c> otherwise.</returns>
            /// <param name="code">Code.</param>
            public static bool StatusInSuccessRange( HttpStatusCode code )
            {
                switch( code )
                {
                    case HttpStatusCode.Accepted:
                    case HttpStatusCode.Created:
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.NonAuthoritativeInformation:
                    case HttpStatusCode.OK:
                    case HttpStatusCode.PartialContent:
                    case HttpStatusCode.ResetContent:
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
