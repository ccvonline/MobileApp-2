using System;
using System.Collections.Generic;
using System.Drawing;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Reusable "carousel" that gives the illusion of an infinitely long list.
        /// All you have to do is create it, and set views for each "card". Each
        /// "card" should be the same dimensions, as these are the repeating items.
        /// </summary>
        public abstract class PlatformCardCarousel
        {
            public static PlatformCardCarousel Create( object parentView, float cardWidth, float cardHeight, RectangleF boundsInParent, float animationDuration )
            {
                #if __IOS__
                return new iOSCardCarousel( parentView, cardWidth, cardHeight, boundsInParent, animationDuration );
                #endif

                #if __ANDROID__
                return new DroidCardCarousel( parentView, cardWidth, cardHeight, boundsInParent, animationDuration );
                #endif

                #if __WIN__
                return null;
                #endif
            }

            protected enum PanGestureState
            {
                Began,
                Changed,
                Ended
            };

            float AnimationDuration { get; set; }

            /// <summary>
            /// This is the range of cards that will pan
            /// when moving cards. It needs to be two to guarantee
            /// you never see gaps between panning cards and non-panning
            /// </summary>
            const int CardPanRange = 2;

            protected class Card
            {
                public int PositionIndex { get; set; }
                public PlatformView View { get; set; }
            }
            protected List<Card> Cards { get; set; }

            PointF CenterCardPos { get; set; }

            /// <summary>
            /// Direction we're currently panning. Important for syncing the card positions
            /// </summary>
            int PanDir { get; set; }

            /// <summary>
            /// True when an animation to restore card positions is playing.
            /// Needed so we know when to allow "fast" panning.
            /// </summary>
            protected bool Animating = false;

            RectangleF BoundsInParent { get; set; }
            float CardWidth { get; set; }
            float CardHeight { get; set; }

            float CardXSpacing { get; set; }


            public int CenterCardIndex { get; protected set; }

            bool _Hidden { get; set; }
            public bool Hidden
            {
                set
                {
                    _Hidden = value;
                    foreach ( Card card in Cards )
                    {
                        card.View.Hidden = value;
                    }
                }

                get { return _Hidden; }
            }

            protected object ParentView { get; set; }

            protected PlatformCardCarousel( object parentView, float cardWidth, float cardHeight, RectangleF boundsInParent, float animationDuration )
            {
                AnimationDuration = animationDuration;
                BoundsInParent = boundsInParent;
                CardWidth = cardWidth;
                CardHeight = cardHeight;
                ParentView = parentView;

                Cards = new List<Card>();

                // the center position should be center on screen
                CenterCardPos = new PointF( ((BoundsInParent.Width - CardWidth) / 2), BoundsInParent.Y );

                // left should be exactly one screen width to the left, and right one screen width to the right
                CardXSpacing = BoundsInParent.Width * .88f;

                CenterCardIndex = 0;
            }

            public void AddCard( PlatformView view )
            {
                view.Position = new PointF( CenterCardPos.X + ( CardXSpacing * Cards.Count ), BoundsInParent.Y );
                view.AddAsSubview( ParentView );

                // make sure the newly added card respects the hidden flag
                Card card = new Card() { PositionIndex = Cards.Count, View = view };
                card.View.Hidden = Hidden;

                Cards.Add( card );
            }

            public virtual void LayoutChanged( float cardWidth, float cardHeight, RectangleF boundsInParent )
            {
                // update the card bounds
                BoundsInParent = boundsInParent;
                CardWidth = cardWidth;
                CardHeight = cardHeight;

                // the center position should be center on screen
                CenterCardPos = new PointF( ((BoundsInParent.Width - CardWidth) / 2), BoundsInParent.Y );

                // left should be exactly one screen width to the left, and right one screen width to the right
                CardXSpacing = BoundsInParent.Width * .88f;

                // reset the position of each card
                for( int i = 0; i < Cards.Count; i++ )
                {
                    Cards[ i ].View.Position = new PointF( CenterCardPos.X + ( CardXSpacing * i ), BoundsInParent.Y );
                    Cards[ i ].PositionIndex = i;
                }

                CenterCardIndex = 0;
            }

            public void Clear( )
            {
                foreach ( Card card in Cards )
                {
                    card.View.RemoveAsSubview( ParentView );
                }
                Cards.Clear( );

                CenterCardIndex = 0;
            }

            //int numSamples { get; set; }
            //PointF mAvgPan = new PointF( );

            protected void OnPanGesture(PanGestureState state, PointF currVelocity, PointF deltaPan) 
            {
                switch( state )
                {
                    case PanGestureState.Began:
                    {
                        //numSamples = 1;
                        //mAvgPan = deltaPan;

                        // when panning begins, clear our pan values
                        PanDir = 0;
                        break;
                    }

                    case PanGestureState.Changed:
                    {
                        //PointF filteredPan = new PointF();

                        /*numSamples++;
                        mAvgPan.X += deltaPan.X;
                        mAvgPan.Y += deltaPan.Y;

                        filteredPan.X = mAvgPan.X / numSamples;
                        filteredPan.Y = mAvgPan.Y / numSamples;*/

                        // use the velocity to determine the direction of the pan
                        if( currVelocity.X < 0 )
                        {
                            PanDir = -1;
                        }
                        else
                        {
                            PanDir = 1;
                        }

                        //Rock.Mobile.Util.Debug.WriteLine( "Delta Pan: {0}, {1}", deltaPan, filteredPan );

                        // Update the positions of the cards
                        TryPanCards( deltaPan );
                        break;
                    }

                    case PanGestureState.Ended:
                    {
                        // when panning is complete, restore the cards to their natural positions
                        AnimateCardsToNeutral( );

                        //numSamples = 0;
                        //mAvgPan = PointF.Empty;
                        break;
                    }
                }
            }

            void TryPanCards( PointF panPos )
            {
                int startIndex = System.Math.Max( CenterCardIndex - CardPanRange, 0 );
                int endIndex = System.Math.Min( CenterCardIndex + CardPanRange, Cards.Count );

                for( int i = startIndex; i < endIndex; i++ )
                {
                    Cards[ i ].View.Position = new PointF( Cards[ i ].View.Position.X + panPos.X, Cards[ i ].View.Position.Y );
                }
            }

            /// <summary>
            /// Only called if the user didn't pan. Used primarly to detect
            /// the user tapping DURING an animation so we can pause the card movement.
            /// </summary>
            public abstract void TouchesBegan( );

            /// <summary>
            /// Only called if the user didn't pan. Used primarly to detect
            /// which direction to resume the cards if the user touched and
            /// released without panning.
            /// </summary>
            public virtual void TouchesEnded( )
            {
                // Attempt to restore the cards to their natural position. This
                // will NOT be called if the user invoked the pan gesture. (which is a good thing)
                AnimateCardsToNeutral( );

                //Rock.Mobile.Util.Debug.WriteLine( "Touches Ended" );
            }

            /// <summary>
            /// Animates a card from startPos to endPos over time
            /// </summary>
            protected abstract void AnimateCard( object platformObject, string animName, PointF startPos, PointF endPos, float duration, PlatformCardCarousel parentDelegate );

            void AnimateCardsToNeutral( )
            {
                // this will animate each card to its neutral resting point
                Animating = true;

                int startIndex = System.Math.Max( CenterCardIndex - CardPanRange, 0 );
                int endIndex = System.Math.Min( CenterCardIndex + CardPanRange, Cards.Count );
                for( int i = startIndex; i < endIndex; i++ )
                {
                    // get the resting point for this card
                    float targetPos = CenterCardPos.X + ( Cards[ i ].PositionIndex * CardXSpacing );

                    // move the card from where it currently is to its nearest index
                    AnimateCard( Cards[ i ].View.PlatformNativeObject, new Random().Next( ).ToString( ), Cards[ i ].View.Position, new PointF( targetPos, Cards[ i ].View.Position.Y ), AnimationDuration, this );
                }
            }

            protected void UpdateCardPositions( )
            {
                if ( Cards.Count > 0 )
                {
                    // start by assuming our center card index won't change
                    int newCenterCardIndex = CenterCardIndex;

                    // get the range of cards that is moved via panning and animation
                    int startIndex = System.Math.Max( CenterCardIndex - CardPanRange, 0 );
                    int endIndex = System.Math.Min( CenterCardIndex + CardPanRange, Cards.Count );

                    // get the new position index of the center card
                    Cards[ CenterCardIndex ].PositionIndex = GetCenterCardPosIndex( CenterCardIndex, Cards[ CenterCardIndex ] );

                    // now that we know the position of the center card, we can safely adjust the indices of
                    // all the cards to its left and right that pan.

                    // update indices to the left of center
                    for ( int i = CenterCardIndex - 1; i >= startIndex; i-- )
                    {
                        Cards[ i ].PositionIndex = Cards[ i + 1 ].PositionIndex - 1;

                        // store the card that currently is the center index.
                        if ( Cards[ i ].PositionIndex == 0 )
                        {
                            newCenterCardIndex = i;
                        }
                    }

                    // update indices to the right of center
                    for ( int i = CenterCardIndex + 1; i < endIndex; i++ )
                    {
                        // get the new position index for the card
                        Cards[ i ].PositionIndex = Cards[ i - 1 ].PositionIndex + 1;

                        // store the card that currently is the center index.
                        if ( Cards[ i ].PositionIndex == 0 )
                        {
                            newCenterCardIndex = i;
                        }
                    }

                    // now take the left and right edges of our range
                    int leftMinIndex = CenterCardIndex - CardPanRange;
                    int rightMaxIndex = CenterCardIndex + CardPanRange;

                    // determine where our center card was left off
                    float centerCardPosX = Cards[ CenterCardIndex ].View.Position.X;

                    // move all cards outside our range to the left to be spaced out relative to the center card
                    for ( int i = leftMinIndex; i >= System.Math.Max( leftMinIndex - 1, 0 ); i-- )
                    {
                        Cards[ i ].PositionIndex = ( Cards[ i + 1 ].PositionIndex - 1 );

                        PointF position = new PointF( centerCardPosX + ( ( Cards[ i ].PositionIndex - 1 ) * CardXSpacing ), CenterCardPos.Y );
                        Cards[ i ].View.Position = position;

                    }

                    // move all cards outside our range to the right to be spaced out relative to the center card
                    for ( int i = rightMaxIndex; i < System.Math.Min( rightMaxIndex + 1, Cards.Count ); i++ )
                    {
                        Cards[ i ].PositionIndex = ( Cards[ i - 1 ].PositionIndex + 1 );

                        PointF position = new PointF( centerCardPosX + ( ( Cards[ i ].PositionIndex + 1 ) * CardXSpacing ), CenterCardPos.Y );
                        Cards[ i ].View.Position = position;
                    }

                    // take the new center index
                    CenterCardIndex = newCenterCardIndex;
                }
            }

            /// <summary>
            /// Given the center card and its absolute position in memory,
            /// determine what it's visual carousel index should be.
            /// </summary>
            static float sCardPanThreshold = 25;
            int GetCenterCardPosIndex( int absCenterCardIndex, Card centerCard )
            {
                // first see how far the card has been moved
                float deltaPos = centerCard.View.Position.X - CenterCardPos.X;

                // start with its current position
                int positionIndex = centerCard.PositionIndex;

                // if the user moved enough left or right, adjust the index
                if ( deltaPos < -sCardPanThreshold )
                {
                    positionIndex -= 1;
                }
                else if ( deltaPos > sCardPanThreshold )
                {
                    positionIndex += 1;
                }

                // now clamp so that if the card is at the edge of the list in memory, we don't allow moving it further
                // note that we do this by clamping to the minimum and maximum its possible for the given card to move indices, which
                // has the effect of "stopping" the cards when you get to either end.
                return System.Math.Max( -( Cards.Count - 1 ) + absCenterCardIndex, System.Math.Min( positionIndex, absCenterCardIndex ) );
            }
        }
    }
}
