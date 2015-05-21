using System;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.Animation;
using System.IO;

namespace App.Shared.UI
{
    public class UIOOBE
    {
        public PlatformView View { get; set; }
        public PlatformImageView ImageLogo { get; set; }
        public PlatformImageView ImageBG { get; set; }

        public PlatformLabel WelcomeLabel { get; set; }

        public PlatformButton RegisterButton { get; set; }
        public PlatformView RegisterSeperator { get; set; }
        public PlatformButton LoginButton { get; set; }
        public PlatformView LoginSeperator { get; set; }
        public PlatformButton SkipButton { get; set; }

        public UIOOBE( )
        {
        }

        enum OOBE_State
        {
            Startup,
            Welcome,
            RevealControls,
            Done
        }
        OOBE_State State { get; set; }

        public delegate void OnButtonClick( int index );

        public void Create( object masterView, string bgLayerImageName, string logoImageName, RectangleF frame, OnButtonClick onClick )
        {
            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.OOBE_Splash_BG_Color;
            View.AddAsSubview( masterView );

            ImageBG = PlatformImageView.Create( true );
            ImageBG.AddAsSubview( View.PlatformNativeObject );
            MemoryStream stream = Rock.Mobile.IO.AssetConvert.AssetToStream( bgLayerImageName );
            stream.Position = 0;
            ImageBG.Opacity = 0;
            ImageBG.Image = stream;
            ImageBG.SizeToFit( );
            ImageBG.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFill;
            stream.Dispose( );

            WelcomeLabel = PlatformLabel.Create( );
            WelcomeLabel.SetFont( ControlStylingConfig.Font_Bold, 85 );
            WelcomeLabel.TextColor = 0xCCCCCCFF;
            WelcomeLabel.Text = OOBEStrings.Welcome;
            WelcomeLabel.Opacity = 0;
            WelcomeLabel.SizeToFit( );
            WelcomeLabel.AddAsSubview( View.PlatformNativeObject );

            RegisterButton = PlatformButton.Create( );
            RegisterButton.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Large_FontSize );
            RegisterButton.TextColor = 0xCCCCCCFF;
            RegisterButton.Text = string.Format( OOBEStrings.WantAccount, GeneralConfig.OrganizationShortName );
            RegisterButton.Opacity = 0;
            RegisterButton.SizeToFit( );
            RegisterButton.ClickEvent = (PlatformButton button ) =>
            {
                onClick( 0 );
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
                    onClick( 1 );
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
                    onClick( 2 );
                };
            SkipButton.AddAsSubview( View.PlatformNativeObject );

            stream = Rock.Mobile.IO.AssetConvert.AssetToStream( logoImageName );
            stream.Position = 0;
            ImageLogo = PlatformImageView.Create( true );
            ImageLogo.AddAsSubview( View.PlatformNativeObject );
            ImageLogo.Image = stream;
            ImageLogo.SizeToFit( );
            ImageLogo.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
            stream.Dispose( );

            State = OOBE_State.Startup;
        }

        public void Destroy( )
        {
            // clean up resources (looking at you, Android)
            ImageLogo.Destroy( );
            ImageBG.Destroy( );
        }

        public void LayoutChanged( RectangleF frame )
        {
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            ImageBG.Frame = View.Frame;

            if ( State == OOBE_State.Done )
            {
                WelcomeLabel.Position = new PointF( ( ( View.Frame.Width - WelcomeLabel.Frame.Width ) / 2 ), View.Frame.Height * .25f );
            }
            else
            {
                WelcomeLabel.Position = new PointF( ( ( View.Frame.Width - WelcomeLabel.Frame.Width ) / 2 ), View.Frame.Height * .35f );
            }

            float welcomeFinalBottom = ( View.Frame.Height * .25f ) + WelcomeLabel.Bounds.Height;
            RegisterButton.Position = new PointF( ( ( View.Frame.Width - RegisterButton.Frame.Width ) / 2 ), welcomeFinalBottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );

            RegisterSeperator.Position = new PointF( RegisterButton.Position.X + (( RegisterButton.Frame.Width - RegisterSeperator.Bounds.Width ) / 2), RegisterButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );

            LoginButton.Position = new PointF( ( ( View.Frame.Width - LoginButton.Frame.Width ) / 2 ), RegisterSeperator.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );

            LoginSeperator.Position = new PointF( LoginButton.Position.X + (( LoginButton.Frame.Width - LoginSeperator.Bounds.Width ) / 2), LoginButton.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );

            SkipButton.Position = new PointF( ( ( View.Frame.Width - SkipButton.Frame.Width ) / 2 ), LoginSeperator.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 16 ) );

            ImageLogo.Frame = new RectangleF( (( View.Frame.Width - ImageLogo.Frame.Width ) / 2), (( View.Frame.Height - ImageLogo.Frame.Height ) / 2) + 2, ImageLogo.Frame.Width, ImageLogo.Frame.Height );
        }

        public void PerformStartup( )
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
                                    EnterNextState( OOBE_State.Welcome );
                                });
                        };
                    timer.Start( );
                } );
            imageSizeAnim.Start( );
        }

        void PerformWelcome( )
        {
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
                                EnterNextState( OOBE_State.RevealControls );
                            });
                        };
                    timer.Start( );
                } );
            anim.Start( );
        }

        void PerformRevealControls( )
        {
            // this will be fun. Chain the animations so they go serially. Start with moving up Welcome
            // now animate it down to a lighter color
            SimpleAnimator_Float animDown = new SimpleAnimator_Float( 1.00f, .15f, 1.10f, delegate(float percent, object value )
                {
                    WelcomeLabel.Opacity = (float)value;
                }, null );
            animDown.Start( );

            SimpleAnimator_PointF posAnim = new SimpleAnimator_PointF( WelcomeLabel.Position, new PointF( WelcomeLabel.Position.X, View.Frame.Height * .25f ), .55f,
                delegate(float posPercent, object posValue )
                {
                    WelcomeLabel.Position = (PointF) posValue;
                },
                delegate
                {
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
                                            EnterNextState( OOBE_State.Done );
                                            AnimateRegSeperator( );
                                        });
                                    skipAnim.Start( );
                                });
                                loginAnim.Start( );
                        } );
                    regAnim.Start( );
                    
                });
            posAnim.Start( );
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
                     Rock.Mobile.Util.Debug.WriteLine( string.Format( "{0}", RegisterSeperator.Frame ) );
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
                case OOBE_State.Startup:
                {
                    PerformStartup( ); 
                    break;
                }

                case OOBE_State.Welcome:
                {
                    PerformWelcome( );
                    break;
                }

                case OOBE_State.RevealControls:
                {
                    PerformRevealControls( );
                    break;
                }

                case OOBE_State.Done:
                {
                    break;
                }
            }

            State = nextState;
        }
    }
}
