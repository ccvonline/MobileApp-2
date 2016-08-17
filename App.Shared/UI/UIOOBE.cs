using System;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.Animation;
using System.IO;
using System.Collections.Generic;

namespace App.Shared.UI
{
    public class UIOOBE
    {
        public PlatformView View { get; set; }
        public PlatformImageView ImageLogo { get; set; }
        public PlatformImageView ImageBG { get; set; }

        public PlatformLabel NetworkErrorLabel { get; set; }
        public PlatformButton NetworkRetryButton { get; set; }

        public PlatformLabel WelcomeLabel { get; set; }

        public PlatformLabel CampusHeader { get; set; }
        public List<PlatformButton> CampusButtons { get; set; }

        public PlatformButton RegisterButton { get; set; }
        public PlatformView RegisterSeperator { get; set; }
        public PlatformButton LoginButton { get; set; }
        public PlatformView LoginSeperator { get; set; }
        public PlatformButton SkipButton { get; set; }

        public OnButtonClick OnClick { get; set; }

        enum OOBE_State
        {
            Startup,
            NetworkError,
            Welcome,
            CampusIntro,
            SelectCampus,
            WaitForCampus,
            AccountChoice,
            WaitForAccountChoice,
            Done
        }
        OOBE_State State { get; set; }

        public delegate void OnButtonClick( int index, bool isCampusSelection );

        public void Create( object masterView, string bgLayerImageName, string logoImageName, bool scaleImageLogo, RectangleF frame, OnButtonClick onClick )
        {
            // take the handler
            OnClick = onClick;

            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.OOBE_Splash_BG_Color;
            View.AddAsSubview( masterView );

            ImageBG = PlatformImageView.Create( true );
            ImageBG.AddAsSubview( View.PlatformNativeObject );
            MemoryStream stream = Rock.Mobile.IO.AssetConvert.AssetToStream( bgLayerImageName );
            if ( stream != null )
            {
                stream.Position = 0;
                ImageBG.Opacity = 0;
                ImageBG.Image = stream;
                ImageBG.SizeToFit( );
                ImageBG.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFill;
                stream.Dispose( );
            }

            NetworkErrorLabel = PlatformLabel.Create( );
            NetworkErrorLabel.SetFont( ControlStylingConfig.Font_Light, 18 );
            NetworkErrorLabel.TextColor = 0xCCCCCCFF;
            NetworkErrorLabel.Text = OOBEStrings.NetworkError;
            NetworkErrorLabel.TextAlignment = TextAlignment.Center;
            NetworkErrorLabel.Opacity = 0;
            NetworkErrorLabel.SizeToFit( );
            NetworkErrorLabel.AddAsSubview( View.PlatformNativeObject );


            NetworkRetryButton = PlatformButton.Create( );
            NetworkRetryButton.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
            NetworkRetryButton.TextColor = 0xCCCCCCFF;
            NetworkRetryButton.Text = OOBEStrings.NetworRetry;
            NetworkRetryButton.Opacity = 0;
            NetworkRetryButton.SizeToFit( );
            NetworkRetryButton.ClickEvent = (PlatformButton button ) =>
            {
                OnClick( -1, false );
            };
            NetworkRetryButton.AddAsSubview( View.PlatformNativeObject );


            WelcomeLabel = PlatformLabel.Create( );
            WelcomeLabel.SetFont( ControlStylingConfig.Font_Bold, 85 );
            WelcomeLabel.TextColor = 0xCCCCCCFF;
            WelcomeLabel.Text = OOBEStrings.Welcome;
            WelcomeLabel.Opacity = 0;
            WelcomeLabel.SizeToFit( );
            WelcomeLabel.AddAsSubview( View.PlatformNativeObject );

            CampusHeader = PlatformLabel.Create( );
            CampusHeader.SetFont( ControlStylingConfig.Font_Light, 18 );
            CampusHeader.TextColor = 0xCCCCCCFF;
            CampusHeader.Text = OOBEStrings.CampusIntro;
            CampusHeader.TextAlignment = TextAlignment.Center;
            CampusHeader.Opacity = 0;
            CampusHeader.SizeToFit( );
            CampusHeader.AddAsSubview( View.PlatformNativeObject );

            // we'll wait to setup campuses until after a successful download
            CampusButtons = new List<PlatformButton>( );

            RegisterButton = PlatformButton.Create( );
            RegisterButton.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Large_FontSize );
            RegisterButton.TextColor = 0xCCCCCCFF;
            RegisterButton.Text = string.Format( OOBEStrings.WantAccount, GeneralConfig.OrganizationShortName );
            RegisterButton.Opacity = 0;
            RegisterButton.SizeToFit( );
            RegisterButton.ClickEvent = (PlatformButton button ) =>
                {
                    // do not allow multiple register taps
                    if( State == OOBE_State.WaitForAccountChoice )
                    {
                        OnClick( 0, false );

                        EnterNextState( OOBE_State.Done );
                    }
                };
            RegisterButton.AddAsSubview( View.PlatformNativeObject );


            RegisterSeperator = PlatformView.Create( );
            RegisterSeperator.BackgroundColor = ControlStylingConfig.BG_Layer_BorderColor;
            RegisterSeperator.Bounds = new RectangleF( 0, 0, 0, 1 );
            RegisterSeperator.AddAsSubview( View.PlatformNativeObject );


            LoginButton = PlatformButton.Create( );
            LoginButton.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Large_FontSize );
            LoginButton.TextColor = 0xCCCCCCFF;
            LoginButton.Text = string.Format( OOBEStrings.HaveAccount, GeneralConfig.OrganizationShortName );
            LoginButton.Opacity = 0;
            LoginButton.SizeToFit( );
            LoginButton.ClickEvent = (PlatformButton button ) =>
                {
                    // do not allow multiple register taps
                    if( State == OOBE_State.WaitForAccountChoice )
                    {
                        OnClick( 1, false );

                        EnterNextState( OOBE_State.Done );
                    }
                };
            LoginButton.AddAsSubview( View.PlatformNativeObject );

            LoginSeperator = PlatformView.Create( );
            LoginSeperator.BackgroundColor = ControlStylingConfig.BG_Layer_BorderColor;
            LoginSeperator.Bounds = new RectangleF( 0, 0, 0, 1 );
            LoginSeperator.AddAsSubview( View.PlatformNativeObject );


            SkipButton = PlatformButton.Create( );
            SkipButton.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Large_FontSize );
            SkipButton.TextColor = 0xCCCCCCFF;
            SkipButton.Text = OOBEStrings.SkipAccount;
            SkipButton.Opacity = 0;
            SkipButton.SizeToFit( );
            SkipButton.ClickEvent = (PlatformButton button ) =>
                {
                    // do not allow multiple register taps
                    if( State == OOBE_State.WaitForAccountChoice )
                    {
                        OnClick( 2, false );

                        EnterNextState( OOBE_State.Done );
                    }
                };
            SkipButton.AddAsSubview( View.PlatformNativeObject );

            stream = Rock.Mobile.IO.AssetConvert.AssetToStream( logoImageName );
            stream.Position = 0;
            ImageLogo = PlatformImageView.Create( scaleImageLogo );
            ImageLogo.AddAsSubview( View.PlatformNativeObject );
            ImageLogo.Image = stream;
            ImageLogo.SizeToFit( );
            ImageLogo.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
            stream.Dispose( );

            State = OOBE_State.Startup;
        }

        void CreateCampusButtons( )
        {
            // we wait to call this later, once we know we have campuses downloaded.
            
            // TODO: We need to support scrolling for the eventual day we have too many campuses for a single screen.
            // Setup campuses
            CampusButtons = new List<PlatformButton>( );
            foreach ( Rock.Client.Campus campus in App.Shared.Network.RockLaunchData.Instance.Data.Campuses )
            {
                PlatformButton campusButton = PlatformButton.Create( );
                campusButton.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Large_FontSize );
                campusButton.TextColor = 0xCCCCCCFF;
                campusButton.Text = campus.Name;
                campusButton.Opacity = 0;
                campusButton.SizeToFit( );
                campusButton.ClickEvent = (PlatformButton button ) =>
                {
                    // do not allow multiple campus button taps
                    if( State == OOBE_State.WaitForCampus )
                    {
                        OnClick( campus.Id, true );

                        EnterNextState( OOBE_State.AccountChoice );
                        PerformAccountChoice( );
                    }
                };
                campusButton.AddAsSubview( View.PlatformNativeObject );

                CampusButtons.Add( campusButton );
            }
        }

        public void Destroy( )
        {
            // clean up resources (looking at you, Android)
            ImageLogo.Destroy( );
            ImageBG.Destroy( );
        }

        static float WelcomeHeightPerc = .01f;
        public void LayoutChanged( RectangleF frame )
        {
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            ImageBG.Frame = View.Frame;

            if ( (int)State > (int)OOBE_State.CampusIntro )
            {
                WelcomeLabel.Position = new PointF( ( ( View.Frame.Width - WelcomeLabel.Frame.Width ) / 2 ), View.Frame.Height * WelcomeHeightPerc );
            }
            else
            {
                WelcomeLabel.Position = new PointF( ( ( View.Frame.Width - WelcomeLabel.Frame.Width ) / 2 ), View.Frame.Height * .35f );
            }

            float welcomeFinalBottom = ( View.Frame.Height * WelcomeHeightPerc ) + WelcomeLabel.Bounds.Height;
            float availableHeight = ( View.Frame.Height - welcomeFinalBottom );

            ImageLogo.Frame = new RectangleF( ( ( View.Frame.Width - ImageLogo.Frame.Width ) / 2 ), ( ( View.Frame.Height - ImageLogo.Frame.Height ) / 2 ) + 2, ImageLogo.Frame.Width, ImageLogo.Frame.Height );

            // position the network error where the Campus Header is. We'll only show one or the other.
            NetworkErrorLabel.Position = new PointF( ( ( View.Frame.Width - NetworkErrorLabel.Frame.Width ) / 2 ), welcomeFinalBottom );
            NetworkRetryButton.Position = new PointF( ( ( View.Frame.Width - NetworkRetryButton.Frame.Width ) / 2 ), NetworkErrorLabel.Frame.Bottom + 10 );

            // position the campus header just below "Welcome"
            if ( (int)State <= (int)OOBE_State.WaitForCampus )
            {
                float currYPos = welcomeFinalBottom + Rock.Mobile.Graphics.Util.UnitToPx( 0 );
                CampusHeader.Position = new PointF( ( ( View.Frame.Width - CampusHeader.Frame.Width ) / 2 ), currYPos );

                // if this is a compact screen size (like, iPhone 4s or smaller), reduce the spacing
                float buttonSpacing = 0;
                if( View.Frame.Height > Rock.Mobile.Graphics.Util.UnitToPx( 480 ) )
                {
                    buttonSpacing = Rock.Mobile.Graphics.Util.UnitToPx( 16 );
                }
                else
                {
                    buttonSpacing = Rock.Mobile.Graphics.Util.UnitToPx( 8 );
                }

                // for the campus buttons, we want to center them within the available space below "Welcome".
                // so figure out that screen space, and the total height of all the campus buttons (with their padding)
                float totalCampusButtonHeight = 0;
                foreach ( PlatformButton campusButton in CampusButtons )
                {
                    totalCampusButtonHeight += campusButton.Frame.Height + buttonSpacing;
                }

                // now lay them out evenly
                currYPos = welcomeFinalBottom + ( ( availableHeight - totalCampusButtonHeight ) / 2 );
                foreach ( PlatformButton campusButton in CampusButtons )
                {
                    campusButton.Position = new PointF( ( ( View.Frame.Width - campusButton.Frame.Width ) / 2 ), currYPos );

                    currYPos = campusButton.Frame.Bottom + buttonSpacing;
                }
            }
            else
            {
                foreach ( PlatformButton campusButton in CampusButtons )
                {
                    campusButton.Position = PointF.Empty;
                }
            }

            if ( (int)State > (int)OOBE_State.WaitForCampus )
            {
                float totalRegisterHeight = ( Rock.Mobile.Graphics.Util.UnitToPx( 16 ) * 5 ) + RegisterButton.Frame.Height + RegisterSeperator.Frame.Height + LoginButton.Frame.Height + LoginSeperator.Frame.Height + SkipButton.Frame.Height;
                float registerYPos = welcomeFinalBottom + ( ( availableHeight - totalRegisterHeight ) / 2 );

                RegisterButton.Position = new PointF( ( ( View.Frame.Width - RegisterButton.Frame.Width ) / 2 ), registerYPos );

                RegisterSeperator.Position = new PointF( RegisterButton.Position.X + ( ( RegisterButton.Frame.Width - RegisterSeperator.Bounds.Width ) / 2 ), RegisterButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );

                LoginButton.Position = new PointF( ( ( View.Frame.Width - LoginButton.Frame.Width ) / 2 ), RegisterSeperator.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );

                LoginSeperator.Position = new PointF( LoginButton.Position.X + ( ( LoginButton.Frame.Width - LoginSeperator.Bounds.Width ) / 2 ), LoginButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );

                SkipButton.Position = new PointF( ( ( View.Frame.Width - SkipButton.Frame.Width ) / 2 ), LoginSeperator.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );
            }
            else
            {
                RegisterButton.Position = PointF.Empty;
                LoginButton.Position = PointF.Empty;
                SkipButton.Position = PointF.Empty;
            }
        }

        public void PerformStartup( bool networkSuccess )
        {
            // Fade in the background image
            SimpleAnimator_Float imageBGAlphaAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .25f, delegate(float percent, object value )
                {
                    ImageBG.Opacity = (float)value;
                },
                null );
            imageBGAlphaAnim.Start( );

            // Fade OUT the logo
            SimpleAnimator_Float imageAlphaAnim = new SimpleAnimator_Float( ImageLogo.Opacity, 0.00f, .13f, delegate(float percent, object value )
                {
                    ImageLogo.Opacity = (float)value;
                },
                null );
            imageAlphaAnim.Start( );

            // Scale UP the logo
            SimpleAnimator_SizeF imageSizeAnim = new SimpleAnimator_SizeF( ImageLogo.Frame.Size, new SizeF( View.Frame.Width, View.Frame.Height ), .25f, delegate(float percent, object value )
                {
                    SizeF imageSize = (SizeF)value;
                    ImageLogo.Frame = new RectangleF( ( View.Frame.Width - imageSize.Width ) / 2, ( View.Frame.Height - imageSize.Height ) / 2, imageSize.Width, imageSize.Height );
                },
                delegate 
                {
                    // when finished, wait, then go to the next state
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = 500;
                    timer.AutoReset = false;
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            // do this ON the UI thread
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    View.BackgroundColor = ControlStylingConfig.BackgroundColor;
                                    
                                    // if the network is ok, continue.
                                    if( networkSuccess == true )
                                    {
                                        EnterNextState( OOBE_State.Welcome );
                                    }
                                    else
                                    {
                                        // if not, let them know they need a network connection for their first run.
                                        EnterNextState( OOBE_State.NetworkError );
                                    }
                                });
                        };
                    timer.Start( );
                } );
            imageSizeAnim.Start( );
        }

        void PerformNetworkError( )
        {
            SimpleAnimator_Float anim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
            {
                NetworkErrorLabel.Opacity = (float)value;
                NetworkRetryButton.Opacity = (float)value;
            },
            delegate
            {
                // when finished, wait
            } );
            anim.Start( );
        }

        public void HandleNetworkFixed( )
        {
            // this is called if the app retried connecting and it worked.
            
            // fade out the "error" and then we can continue!
            SimpleAnimator_Float anim = new SimpleAnimator_Float( 1.00f, 0.00f, .50f, delegate(float percent, object value )
            {
                NetworkErrorLabel.Opacity = (float)value;
                NetworkRetryButton.Opacity = (float)value;
            },
            delegate
            {
                // when finished, wait, then go to the next state
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 1000;
                timer.AutoReset = false;
                timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                {
                    // do this ON the UI thread
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                    {
                        //AnimateRegSeperator( );
                        EnterNextState( OOBE_State.Welcome );
                    });
                };
                timer.Start( );
            } );
            anim.Start( );
        }

        void PerformWelcome( )
        {
            CreateCampusButtons( );

            SimpleAnimator_Float anim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                {
                    WelcomeLabel.Opacity = (float)value;
                },
                delegate
                {
                    // when finished, wait, then go to the next state
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = 1000;
                    timer.AutoReset = false;
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            // do this ON the UI thread
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                            {
                                //AnimateRegSeperator( );
                                EnterNextState( OOBE_State.CampusIntro );
                            });
                        };
                    timer.Start( );
                } );
            anim.Start( );
        }

        void PerformCampusIntro( )
        {
            LayoutChanged( View.Frame );

            // Fade OUT the welcome
            SimpleAnimator_Float animDown = new SimpleAnimator_Float( 1.00f, .15f, 2.00f, delegate(float percent, object value )
                {
                    WelcomeLabel.Opacity = (float)value;
                }, null );
            animDown.Start( );

            // Move UP the welcome
            SimpleAnimator_PointF posAnim = new SimpleAnimator_PointF( WelcomeLabel.Position, new PointF( WelcomeLabel.Position.X, View.Frame.Height * WelcomeHeightPerc ), 1.75f,
                delegate(float posPercent, object posValue )
                {
                    WelcomeLabel.Position = (PointF) posValue;
                },
                delegate
                {
                    // once moving up the welcome is done, kick off a timer that will fade in the
                    // campus header.
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = 1000;
                    timer.AutoReset = false;
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            // do this ON the UI thread
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    // now fade in the campuses intro.
                                    SimpleAnimator_Float campusAnim = new SimpleAnimator_Float( 0.00f, 1.00f, 1.50f, delegate(float percent, object value )
                                        {
                                            CampusHeader.Opacity = (float)value;
                                        },
                                        delegate
                                        {
                                            // do this ON the UI thread
                                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                                {
                                                    EnterNextState( OOBE_State.SelectCampus );
                                                });
                                        } );
                                    campusAnim.Start( );
                                });
                        };
                    timer.Start( );

                });
            posAnim.Start( SimpleAnimator.Style.CurveEaseOut );
        }

        void PerformSelectCampus( )
        {
            // IF THERE IS A COLLISION (i.e. no room) between the CampusHeader and CampusButtons,
            // we will fade OUT the header and leave it up for longer.
            bool fadeOutHeader  = false;
            float animTime = 2.00f;
            if ( CampusButtons[ 0 ].Frame.Top <= CampusHeader.Frame.Bottom )
            {
                fadeOutHeader = true;
                animTime = 5.00f;
            }
                
            // fade down the campus header and fade up the campuses.
            SimpleAnimator_Float animDown = new SimpleAnimator_Float( 1.00f, .00f, animTime, delegate(float percent, object value )
                {
                    // IF THERE IS A COLLISION (i.e. no room) between the CampusHeader and CampusButtons,
                    // fade down the campus header and fade up the campuses.
                    if ( fadeOutHeader )
                    {
                        CampusHeader.Opacity = (float)value;
                    }
                }, 
                delegate
                {
                    // now fade in the campuses
                    foreach( PlatformButton campusButton in CampusButtons )
                    {
                        SimpleAnimator_Float campusAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                            {
                                campusButton.Opacity = (float)value;
                            },
                            null );
                        campusAnim.Start( );
                    }

                    // do this ON the UI thread
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            // immediately move to the next state (while the campuses are fading in)
                            EnterNextState( OOBE_State.WaitForCampus );
                        });
                } );
            animDown.Start( SimpleAnimator.Style.CurveEaseIn );
        }

        void PerformAccountChoice( )
        {
            // fade out the campus choices and header.
            bool accountFadeInBegan = false;
            SimpleAnimator_Float animDown = new SimpleAnimator_Float( 1.00f, 0.00f, 1.10f, delegate(float percent, object value )
                {
                    // take either the lowered alpha value OR the current opacity. That way if
                    // the header is already faded out we won't do anything.
                    CampusHeader.Opacity = Math.Min( (float)value, CampusHeader.Opacity );

                    foreach ( PlatformButton campusButton in CampusButtons )
                    {
                        campusButton.Opacity = (float)value;
                    }
                }, 
                delegate
                {
                    // make sure we only begin fading in the account stuff ONCE, and not
                    // for each button animating.
                    if( accountFadeInBegan == false )
                    {
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                            {
                                LayoutChanged( View.Frame );
                            } );

                        accountFadeInBegan = true;

                        // now fade in Register
                        SimpleAnimator_Float regAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                            {
                                RegisterButton.Opacity = (float)value;
                            },
                            delegate
                            {
                                // now Login
                                SimpleAnimator_Float loginAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                                    {
                                        LoginButton.Opacity = (float)value;
                                    },
                                    delegate
                                    {
                                        // finally skip
                                        SimpleAnimator_Float skipAnim = new SimpleAnimator_Float( 0.00f, 1.00f, .50f, delegate(float percent, object value )
                                            {
                                                SkipButton.Opacity = (float)value;
                                            },
                                            delegate
                                            {
                                                EnterNextState( OOBE_State.WaitForAccountChoice );
                                                AnimateRegSeperator( );
                                            });
                                        skipAnim.Start( );
                                    });
                                loginAnim.Start( );
                            } );
                        regAnim.Start( );
                    }

                } );
            animDown.Start( SimpleAnimator.Style.CurveEaseIn );
        }

        void AnimateRegSeperator( )
        {
            RectangleF startRegBorder = new RectangleF( RegisterButton.Position.X + RegisterButton.Frame.Width / 2, 
                RegisterButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ),
                0, 1 );

            RectangleF finalRegBorder = new RectangleF( RegisterButton.Position.X + ( ( RegisterButton.Frame.Width - (View.Bounds.Width * .75f) ) / 2 ), 
                RegisterButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ),
                View.Bounds.Width * .75f, 1 );
            
            SimpleAnimator_RectF regBorderAnim = new SimpleAnimator_RectF( startRegBorder, finalRegBorder, .25f, delegate(float percent, object value )
                {
                     RegisterSeperator.Frame = (RectangleF)value;
                }, delegate { AnimateLoginSeperator( ); } );

            regBorderAnim.Start( );
        }

        void AnimateLoginSeperator( )
        {
            RectangleF startBorder = new RectangleF( LoginButton.Position.X + LoginButton.Frame.Width / 2, 
                LoginButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ),
                0, 1 );

            RectangleF finalBorder = new RectangleF( LoginButton.Position.X + ( ( LoginButton.Frame.Width - (View.Bounds.Width * .75f) ) / 2 ), 
                LoginButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ),
                View.Bounds.Width * .75f, 1 );

            SimpleAnimator_RectF borderAnim = new SimpleAnimator_RectF( startBorder, finalBorder, .25f, delegate(float percent, object value )
                {
                    LoginSeperator.Frame = (RectangleF)value;
                }, null );

            borderAnim.Start( );
        }

        void EnterNextState( OOBE_State nextState )
        {
            switch( nextState )
            {
                case OOBE_State.NetworkError:
                {
                    PerformNetworkError( );
                    break;
                }

                case OOBE_State.Welcome:
                {
                    PerformWelcome( );
                    break;
                }

                case OOBE_State.CampusIntro:
                {
                    PerformCampusIntro( );
                    break;
                }

                case OOBE_State.SelectCampus:
                {
                    PerformSelectCampus( );
                    break;
                }

                case OOBE_State.WaitForCampus:
                {
                    // just wait for them to pick
                    break;
                }

                case OOBE_State.AccountChoice:
                {
                    PerformAccountChoice( );
                    break;
                }

                case OOBE_State.WaitForAccountChoice:
                {
                    break;
                }
            }

            State = nextState;
        }
    }
}
