using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rock.Mobile
{
    /// <summary>
    /// Very basic profiler for getting the time across tasks.
    /// Usage: 
    /// Profiler.Start( "MySample" );
    /// Perform Task
    /// float timeMS = Profiler.Stop( "MySample" );
    /// 
    /// NOTE: If this is being used recursively, the parent sample may be artificially
    /// high due to the sampler Start/Stop overhead itself.
    /// </summary>
    public sealed class Profiler
    {
        private static Profiler _Instance = new Profiler( );
        public static Profiler Instance { get { return _Instance; } }

        private class Sample
        {
            public string Name { get; set; }
            public Stopwatch Timer { get; set; }
        }

        private List<Sample> Samples { get; set; }

        private Profiler( )
        {
            Samples = new List<Sample>( );
        }

        /// <summary>
        /// Starts a specified sample.
        /// </summary>
        /// <param name="name">Name.</param>
        public void Start( string name )
        {
            Samples.Add( 
                new Sample()
                {
                    Name = name,
                    Timer = Stopwatch.StartNew()
                });

        }

        /// <summary>
        /// Stops the specified sample and optionally prints the result.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="printResult">If set to <c>true</c> print result.</param>
        public float Stop( string name, bool printResult = true )
        {
            Sample sample = Samples.Find( s => s.Name == name );
            if( sample != null )
            {
                sample.Timer.Stop();
                Samples.Remove( sample );

                if( printResult == true )
                {
                    Rock.Mobile.Util.Debug.WriteLine( string.Format( "{0} completed in {1} ms", name, sample.Timer.Elapsed.TotalMilliseconds ) ); 
                }
                return sample.Timer.ElapsedMilliseconds;
            }

            return -1.00f;
        }
    }
}

