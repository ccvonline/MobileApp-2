#if __WIN__

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// For windows, all we need to do is underline existing text.
        /// It'd be nice to support animation, but that really isn't needed for
        /// the platform currently, since notes are just going to be authored here.
        /// </summary>
        public class WinRevealLabel : WinLabel
        {
            public WinRevealLabel( ) : base()
            {
                AddUnderline( );
            }
        }
    }
}

#endif
