using System;
namespace App.Shared
{
    public abstract class BibleService
    {
        public delegate void OnBibleResult( string htmlToRender );

        public abstract void RetrieveBiblePassage( string bibleAddress, OnBibleResult onResult );
    }
}
