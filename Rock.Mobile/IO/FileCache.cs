using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Collections;
using Rock.Mobile.Network;
using RestSharp;
using System.Net;

namespace Rock.Mobile.IO
{
    /// <summary>
    /// A class that allows files to be written to disk (cached), and loaded later.
    /// The typical use is to download a file and write it to cache. On load,
    /// ask the cache for it, and if null is returned,  download it.
    /// </summary>
    public class FileCache
    {
        /// <summary>
        /// The subfolder where cached files should be stored
        /// </summary>
        public const string CacheDirectory = "cache";

        /// <summary>
        /// The length of time a file should remain cached. After this, it will be deleted
        /// from cache. (7 days)
        /// </summary>
        static TimeSpan CacheFileDefaultExpiration = new TimeSpan( 7, 0, 0, 0 );

        /// <summary>
        /// Ok, it's not technically NONE, but it's 10 years. Come on...
        /// </summary>
        public static TimeSpan CacheFileNoExpiration = new TimeSpan( 3650, 0, 0, 0 );


        static FileCache _Instance = new FileCache( );
        public static FileCache Instance { get { return _Instance; } }

        System.Collections.Hashtable CacheMap { get; set; }

        private object locker = new object();

        FileCache( )
        {
            // ensure the cache directory exists
            if ( Directory.Exists( CachePath ) == false )
            {
                Directory.CreateDirectory( CachePath );
            }

            LoadCacheMap( );
        }

        void LoadCacheMap( )
        {
            // attemp to read the cache map
            try
            {
                using ( FileStream reader = new FileStream( CachePath + "/" + "cache.dat", FileMode.Open ) )
                {
                    StreamReader mapStream = new StreamReader( reader );
                    BinaryFormatter formatter = new BinaryFormatter( );

                    CacheMap = (System.Collections.Hashtable)formatter.Deserialize( mapStream.BaseStream );

                    CleanUp( );
                }
            }
            catch( Exception )
            {
                // if it fails for any reason, simply create a new one
                CacheMap = new System.Collections.Hashtable();
            }
        }

        public void SaveCacheMap( )
        {
            try
            {
                using( FileStream writer = new FileStream( CachePath + "/" + "cache.dat", FileMode.Create ) )
                {
                    BinaryFormatter formatter = new BinaryFormatter( );
                    StreamWriter mapStream = new StreamWriter( writer );
                    formatter.Serialize( mapStream.BaseStream, CacheMap );
                }
            }
            catch( Exception )
            {
            }
        }

        string CachePath
        {
            get
            {
                // get the path based on the platform
                #if __IOS__
                //Note: Xamarin warns that we should use the below commented out version. But it doesn't work...the original does. 
                //http://developer.xamarin.com/guides/ios/application_fundamentals/working_with_the_file_system/ Their comment here says it's temporary, so maybe the docs are out of date?
                //string cachePath = MonoTouch.Foundation.NSFileManager.DefaultManager.GetUrls (MonoTouch.Foundation.NSSearchPathDirectory.DocumentDirectory, MonoTouch.Foundation.NSSearchPathDomain.User) [0].ToString();
                string cachePath = System.IO.Path.Combine ( Environment.GetFolderPath(Environment.SpecialFolder.Personal), "" );
                #else
                string cachePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                // JHM 4-24-15: Suddenly, certain android devices began crashing because GetExternalFilesDir returned null. It seems to be
                // that the device no longer has an sd card mounted. Using the above not only works, but is more consistent with what
                // I use everywhere else in the app.
                //string cachePath = Rock.Mobile.PlatformSpecific.Android.Core.Context.GetExternalFilesDir( null ).ToString( );
                #endif

                cachePath += "/" + CacheDirectory;
                return cachePath;
            }
        }

        /// <summary>
        /// Scans the cache hashtable and removes any entries that are expired. Additionally, it deletes the file
        /// from the cache folder.
        /// CAUTION: If you pass true, all files will automatically be erased
        /// </summary>
        public void CleanUp( bool forceEraseAll = false )
        {
            lock ( locker )
            {
                Rock.Mobile.Util.Debug.WriteLine( "Running cleanup" );

                List< DictionaryEntry > expiredItems = new List< DictionaryEntry >( );

                // scan our cache and remove anything older than the expiration time.
                foreach ( DictionaryEntry entry in CacheMap )
                {
                    DateTime entryValue = (DateTime) entry.Value;

                    // if it's older than our expiration time, delete it
                    TimeSpan deltaTime = ( DateTime.Now - entryValue );
                    if ( DateTime.Now >= entryValue || forceEraseAll == true )
                    {
                        // delete the entry
                        File.Delete( CachePath + "/" + entry.Key );

                        expiredItems.Add( entry );

                        Rock.Mobile.Util.Debug.WriteLine( string.Format( "{0} expired. Age: {1} minutes past expiration.", (string)entry.Key, deltaTime.TotalMinutes ) );
                    }
                    else
                    {
                        Rock.Mobile.Util.Debug.WriteLine( string.Format( "{0} still fresh NOT REMOVING.", (string)entry.Key ) );
                    }
                }

                // remove all entries that have been expired
                foreach ( DictionaryEntry entry in expiredItems )
                {
                    CacheMap.Remove( entry.Key );
                }

                Rock.Mobile.Util.Debug.WriteLine( "Cleanup complete" );
            }
        }

        public bool SaveFile( object buffer, string filename, TimeSpan? expirationTime = null )
        {
            // sync point so we don't read multiple times at once.
            // Note: If we ever need to support multiple threads reading from the cache at once, we'll need
            // a table to hash multiple locks per filename. This serializes all file loads across all threads.
            lock ( locker )
            {
                bool result = false;

                try
                {
                    // attempt to write the file out to disk
                    using ( FileStream writer = new FileStream( CachePath + "/" + filename, FileMode.Create ) )
                    {
                        BinaryFormatter formatter = new BinaryFormatter( );
                        StreamWriter mapStream = new StreamWriter( writer );
                        formatter.Serialize( mapStream.BaseStream, buffer );

                        mapStream.Dispose( );

                        // store in our cachemap the filename and when we wrote it.
                        // Don't ever add it twice. If it exists, remove it
                        if( CacheMap.Contains( filename ) )
                        {
                            CacheMap.Remove( filename );
                            Rock.Mobile.Util.Debug.WriteLine( string.Format( "{0} is already cached. Updating time to {1}", filename, DateTime.Now ) );
                        }
                        else
                        {
                            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Adding {0} to cache. Time {1}", filename, DateTime.Now ) );
                        }

                        if( expirationTime.HasValue == false )
                        {
                            expirationTime = CacheFileDefaultExpiration;
                        }

                        // and now it'll be added a second time.
                        CacheMap.Add( filename, DateTime.Now + expirationTime.Value );

                        // if an exception occurs we won't set result to true
                        result = true;

                        writer.Close( );
                        writer.Dispose( );
                    }
                }
                catch ( Exception e )
                {
                    Rock.Mobile.Util.Debug.WriteLine( e.Message );
                }

                // return the result
                return result;
            }
        }

        public bool FileExists( string filename )
        {
            // get our lock
            lock ( locker )
            {
                // first make sure it's in our cahce. If not,
                // we want to consider it not existing. It was orphaned.
                // (Maybe we crashed before we could save?)
                if ( CacheMap.ContainsKey( filename ) )
                {
                    // validate it does exist on disk
                    if ( File.Exists( CachePath + "/" + filename ) )
                    {
                        // and return
                        return true;
                    }
                    else
                    {
                        // otherwise, it didn't exist on disk, but
                        // was somehow in our cache. Get rid of it.
                        CacheMap.Remove( filename );
                    }
                }

                return false;
            }
        }

        public object LoadFile( string filename )
        {
            try
            {
                using ( FileStream reader = new FileStream( CachePath + "/" + filename, FileMode.Open ) )
                {
                    StreamReader mapStream = new StreamReader( reader );
                    BinaryFormatter formatter = new BinaryFormatter( );

                    object loadedObj = formatter.Deserialize( mapStream.BaseStream );
                    mapStream.Dispose( );

                    reader.Close( );
                    reader.Dispose( );

                    return loadedObj;
                }
            }
            catch( Exception e )
            {
                // only print the exception if it's something other than file not found.
                if ( e as FileNotFoundException == null )
                {
                    Rock.Mobile.Util.Debug.WriteLine( string.Format( "{0} failed to load. Exception {1}. Removing from Cache.", filename, e ) );
                    RemoveFile( filename );
                }
            }

            return null;
        }

        public void RemoveFile( string filename )
        {
            // take our lock
            lock ( locker )
            {
                // delete the entry
                if ( string.IsNullOrWhiteSpace( filename ) == false )
                {
                    File.Delete( CachePath + "/" + filename );
                    CacheMap.Remove( filename );
                }
            }
        }

        public delegate void FileDownloaded( bool result );
        public void DownloadFileToCache( string downloadUrl, string cachedFilename, TimeSpan? expirationTime = null, FileDownloaded callback = null )
        {
            if ( string.IsNullOrWhiteSpace( downloadUrl ) == false )
            {
                HttpRequest webRequest = new HttpRequest();
                RestRequest restRequest = new RestRequest( Method.GET );
                restRequest.AddHeader( "Accept", "text/*, image/*" );

                webRequest.ExecuteAsync( downloadUrl, restRequest, 
                    delegate(HttpStatusCode statusCode, string statusDescription, byte[] model )
                    {
                        bool result = false;

                        if ( model != null && Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                        {
                            // write it to cache
                            MemoryStream fileBuffer = new MemoryStream( model );
                            fileBuffer.Position = 0;
                            FileCache.Instance.SaveFile( fileBuffer, cachedFilename, expirationTime );
                            fileBuffer.Dispose( );

                            result = true;
                        }

                        if ( callback != null )
                        {
                            callback( result );
                        }
                    } );
            }
            else
            {
                if ( callback != null )
                {
                    callback( false );
                }
            }
        }
    }
}

