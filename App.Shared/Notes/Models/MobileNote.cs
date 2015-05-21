using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using RestSharp.Deserializers;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            namespace Model
            {
                /// <summary>
                /// The object that is saved / loaded and stores the user's state for a particular note.
                /// </summary>
                public class NoteState
                {
                    /// <summary>
                    /// Represents a user's custom note. Stores the last position, whether it was open or not,
                    /// and the note itself.
                    /// </summary>
                    public class UserNoteContent
                    {
                        /// <summary>
                        /// The percentage position of this note on screen. (So that should they
                        /// view this note on another device, it'll be in the appropriate spot)
                        /// </summary>
                        public float PositionPercX { get; set; }
                        public float PositionPercY { get; set; }

                        /// <summary>
                        /// The note's contents.
                        /// </summary>
                        public string Text { get; set; }

                        /// <summary>
                        /// Stores the last state of this note
                        /// </summary>
                        public bool WasOpen { get; set; }

                        public UserNoteContent( float positionPercX, float positionPercY, string text, bool wasOpen )
                        {
                            PositionPercX = positionPercX;
                            PositionPercY = positionPercY;

                            Text = text;
                            WasOpen = wasOpen;
                        }
                    }

                    /// <summary>
                    /// Represents the state of a reveal box within the notes. Tracked so
                    /// when loading the notes, we can restore any reveal box that was previously revealed.
                    /// </summary>
                    public class RevealBoxState
                    {
                        /// <summary>
                        /// The state of the reveal box. True if it was tapped and revealed, false otherwise.
                        /// </summary>
                        /// <value><c>true</c> if revealed; otherwise, <c>false</c>.</value>
                        public bool Revealed { get; set; }

                        public RevealBoxState( bool revealed )
                        {
                            Revealed = revealed;
                        }
                    }

                    /// <summary>
                    /// Represents the state of a text input within the noters. Tracked
                    /// so when loading the notes, we can restore the text that was previously in the
                    /// text input.
                    /// </summary>
                    public class TextInputState
                    {
                        /// <summary>
                        /// The text typed in the text input box.
                        /// </summary>
                        /// <value>The text.</value>
                        public string Text { get; set; }

                        public TextInputState( string text )
                        {
                            Text = text;
                        }
                    }

                    /// <summary>
                    /// List of all the user's custom notes
                    /// </summary>
                    /// <value>The mobile notes.</value>
                    public List<UserNoteContent> UserNoteContentList { get; set; }

                    /// <summary>
                    /// List of all the reveal box's current states
                    /// </summary>
                    /// <value>The reveal box states.</value>
                    public List<RevealBoxState> RevealBoxStateList { get; set; }

                    /// <summary>
                    /// List of all the text input's current text
                    /// </summary>
                    /// <value>The text input state list.</value>
                    public List<TextInputState> TextInputStateList { get; set; }

                    /// <summary>
                    /// The last scrolled percent position of the note
                    /// </summary>
                    public float ScrollOffsetPercent { get; set; }
                }
            }
        }
    }
}
