
using System;
using System.Collections.Generic;
using App.Shared.Config;


#if __IOS__
using Foundation;
using LocalyticsBinding;
#elif __ANDROID__
using Java.Util;
using Com.Localytics.Android;
#endif

namespace App.Shared
{
    namespace Analytics
    {
        /// <summary>
        /// Base Event class from which all events to be tagged derive. 
        /// </summary>
        public abstract class EventAnalytic
        {
            /// <summary>
            /// Category is the task performed within the event, like
            /// reading sermon notes vs watching a sermon podcast.
            /// </summary>
            protected class Category
            {
                /// <summary>
                /// True if an action within this category should only be recorded once per run.
                /// Example: The Sermon Notes category could be set to true, and then each sermon note viewed will
                /// only be recorded once during that run of the app, rather than every time that sermon note is looked at.
                /// </summary>
                public bool OncePerRun { get; protected set; }

                /// <summary>
                /// The name describing what this event category records.
                /// Example: "Sermon Notes"
                /// </summary>
                public string Name { get; protected set; }

                /// <summary>
                /// A unique set of all actions performed within this category.
                /// Whether or not OncePerRun is true for the category, we will only add
                /// one instance of an action to the set.
                /// </summary>
                /// <value>The performed actions.</value>
                public HashSet<string> PerformedActions { get; set; }

                public Category( string name, bool oncePerRun )
                {
                    PerformedActions = new HashSet<string>( );
                    Name = name;
                    OncePerRun = oncePerRun;
                }
            }
            protected List<Category> Categories { get; set; }

            protected string Name { get; set; }

            protected EventAnalytic( )
            {
                Categories = new List<Category>( );
            }

            public void Trigger( string category )
            {
                Trigger( category, "" );
            }

            public void Trigger( string category, string action )
            {
                // make sure this category exists. It must be added by the constructor of the derived event.
                Category categoryObj = Categories.Find( c => c.Name == category );
                if ( categoryObj == null )
                {
                    throw new Exception( string.Format( "Unknown Category {0} triggered for Event {1}", category, Name ) );
                }

                // if the action hasn't been performed yet, or we can perform it more than once, fire the analytic
                if ( categoryObj.PerformedActions.Contains( action ) == false || categoryObj.OncePerRun == false )
                {
                    // add it, which will fail if it's alread in the list
                    categoryObj.PerformedActions.Add( action );

                    if ( GeneralConfig.Use_Localytics == true )
                    {
                        #if !DEBUG
                        #if __IOS__
                        NSDictionary attribs = NSDictionary.FromObjectAndKey( new NSString( action ), new NSString( categoryObj.Name ) );
                        Localytics.TagEvent( Name, attribs );
                        #elif __ANDROID__
                        System.Collections.Generic.Dictionary<string, string> attribs = new System.Collections.Generic.Dictionary<string, string>();
                        attribs.Add( categoryObj.Name, action );
                        Localytics.TagEvent( Name, attribs );
                        #endif
                        #endif
                    }
                }
            }
        }

        public class MessageAnalytic : EventAnalytic
        {
            public const string BrowseSeries = "Browse Series";
            public const string Read = "Read";
            public const string Watch = "Watch";
            public const string Listen = "Listen";

            protected MessageAnalytic( ) : base( )
            {
                Name = "Message";

                Categories.Add( new Category( BrowseSeries, true ) );
                Categories.Add( new Category( Read, true ) );
                Categories.Add( new Category( Watch, true ) );
                Categories.Add( new Category( Listen, true ) );
            }

            public static MessageAnalytic Instance = new MessageAnalytic( );
        }

        public class GiveAnalytic : EventAnalytic
        {
            public const string Give = "Give";

            protected GiveAnalytic( ) : base( )
            {
                Name = "Give";

                Categories.Add( new Category( Give, true ) );
            }

            public static GiveAnalytic Instance = new GiveAnalytic( );
        }

        public class PrayerAnalytic : EventAnalytic
        {
            public const string Read = "Read";
            public const string Create = "Create";

            protected PrayerAnalytic( ) : base( )
            {
                Name = "Prayer";

                Categories.Add( new Category( Read, true ) );
                Categories.Add( new Category( Create, false ) );
            }

            public static PrayerAnalytic Instance = new PrayerAnalytic( );
        }

        public class GroupFinderAnalytic : EventAnalytic
        {
            public const string Location = "Location";
            public const string Neighborhood = "Neighborhood";
            public const string OutOfBounds = "Out of Bounds";

            protected GroupFinderAnalytic( ) : base( )
            {
                Name = "Group Finder";

                Categories.Add( new Category( Location, false ) );
                Categories.Add( new Category( Neighborhood, false ) );
                Categories.Add( new Category( OutOfBounds, false ) );
            }

            public static GroupFinderAnalytic Instance = new GroupFinderAnalytic( );
        }

        public class ProfileAnalytic : EventAnalytic
        {
            public const string Login = "Login";
            public const string Register = "Register";
            public const string Update = "Update";

            protected ProfileAnalytic( ) : base( )
            {
                Name = "Profile";

                Categories.Add( new Category( Login, false ) );
                Categories.Add( new Category( Register, false ) );
                Categories.Add( new Category( Update, false ) );
            }

            public static ProfileAnalytic Instance = new ProfileAnalytic( );
        }
    }
}
