using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;
using App.Shared.Network;
using System.IO;
using App.Shared.Config;
using Rock.Mobile.UI;
using App.Shared.Strings;
using App.Shared;
using Rock.Mobile.Threading;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using System.Collections.Generic;
using Rock.Mobile.Animation;
using CoreGraphics;
using App.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared.Analytics;
using App.Shared.PrivateConfig;

namespace iOS
{
	partial class LoginViewController : UIViewController
	{
        /// <summary>
        /// Reference to the parent springboard for returning apon completion
        /// </summary>
        /// <value>The springboard.</value>
        public SpringboardViewController Springboard { get; set; }

        /// <summary>
        /// Timer to allow a small delay before returning to the springboard after a successful login.
        /// </summary>
        /// <value>The login successful timer.</value>
        System.Timers.Timer LoginSuccessfulTimer { get; set; }

        UIBlockerView BlockerView { get; set; }

        WebLayout WebLayout { get; set; }

		public LoginViewController (IntPtr handle) : base (handle)
		{
            // setup our timer
            LoginSuccessfulTimer = new System.Timers.Timer();
            LoginSuccessfulTimer.AutoReset = false;
            LoginSuccessfulTimer.Interval = 2000;
		}

        protected enum LoginState
        {
            Out,
            Trying,

            // Deprecated state
            In
        };
        LoginState State { get; set; }

        UIImageView LogoView { get; set; }

        UIImageView FBImageView { get; set; }

        StyledTextField UserNameField { get; set; }

        StyledTextField PasswordField { get; set; }

        UIButton LoginButton { get; set; }
        UIButton RegisterButton { get; set; }

        UIButton FacebookLogin { get; set; }
        UIButton CancelButton { get; set; }

        StyledTextField LoginResult { get; set; }

        UIView HeaderView { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BlockerView = new UIBlockerView( View, View.Frame.ToRectF( ) );

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            UserNameField = new StyledTextField();
            View.AddSubview( UserNameField.Background );

            UserNameField.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( UserNameField.Field, LoginStrings.UsernamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( UserNameField.Background );
            UserNameField.Field.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryRockBind();
                    return true;
                };

            PasswordField = new StyledTextField();
            View.AddSubview( PasswordField.Background );
            PasswordField.Field.AutocorrectionType = UITextAutocorrectionType.No;
            PasswordField.Field.SecureTextEntry = true;

            ControlStyling.StyleTextField( PasswordField.Field, LoginStrings.PasswordPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( PasswordField.Background );
            PasswordField.Field.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryRockBind();
                    return true;
                };

            // obviously attempt a login if login is pressed
            LoginButton = UIButton.FromType( UIButtonType.System );
            View.AddSubview( LoginButton );
            ControlStyling.StyleButton( LoginButton, LoginStrings.LoginButton, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            LoginButton.SizeToFit( );
            LoginButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        RockMobileUser.Instance.LogoutAndUnbind( );

                        SetUIState( LoginState.Out );
                    }
                    else
                    {
                        TryRockBind();
                    }
                };

            RegisterButton = UIButton.FromType( UIButtonType.System );
            View.AddSubview( RegisterButton );
            ControlStyling.StyleButton( RegisterButton, LoginStrings.RegisterButton, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            RegisterButton.SizeToFit( );
            RegisterButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    Springboard.RegisterNewUser( );
                };

            // setup the result
            LoginResult = new StyledTextField( );
            View.AddSubview( LoginResult.Background );

            ControlStyling.StyleTextField( LoginResult.Field, "", ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( LoginResult.Background );
            LoginResult.Field.UserInteractionEnabled = false;
            LoginResult.Field.TextAlignment = UITextAlignment.Center;

            // setup the facebook button
            FacebookLogin = new UIButton( );
            View.AddSubview( FacebookLogin );
            string imagePath = NSBundle.MainBundle.BundlePath + "/" + "facebook_login.png";
            FBImageView = new UIImageView( new UIImage( imagePath ) );

            FacebookLogin.SetTitle( "", UIControlState.Normal );
            FacebookLogin.AddSubview( FBImageView );
            FacebookLogin.Layer.CornerRadius = 4;
            FBImageView.Layer.CornerRadius = 4;

            FacebookLogin.TouchUpInside += (object sender, EventArgs e) => 
                {
                    TryFacebookBind();
                };

            // If cancel is pressed, notify the springboard we're done.
            CancelButton = UIButton.FromType( UIButtonType.System );
            View.AddSubview( CancelButton );
            CancelButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            CancelButton.SetTitle( GeneralStrings.Cancel, UIControlState.Normal );
            CancelButton.SizeToFit( );
            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // don't allow canceling while we wait for a web request.
                    if( LoginState.Trying != State )
                    {
                        Springboard.ResignModelViewController( this, null );
                    }
                };
            
            // setup the fake header
            HeaderView = new UIView( );
            View.AddSubview( HeaderView );
            HeaderView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            imagePath = NSBundle.MainBundle.BundlePath + "/" + PrivatePrimaryNavBarConfig.LogoFile_iOS;
            LogoView = new UIImageView( new UIImage( imagePath ) );
            HeaderView.AddSubview( LogoView );
        }

        public override void ViewDidLayoutSubviews( )
        {
            base.ViewDidLayoutSubviews( );

            UserNameField.SetFrame( new CGRect( -10, View.Frame.Height * .25f, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            PasswordField.SetFrame( new CGRect( UserNameField.Background.Frame.Left, UserNameField.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );

            LoginButton.Frame = new CGRect( View.Frame.Left + 10, PasswordField.Background.Frame.Bottom + 20, View.Bounds.Width / 2 - 10, ControlStyling.ButtonHeight );

            RegisterButton.Frame = new CGRect( View.Frame.Right - View.Bounds.Width / 2 + 10, PasswordField.Background.Frame.Bottom + 20, View.Bounds.Width / 2 - 20, ControlStyling.ButtonHeight );

            LoginResult.SetFrame( new CGRect( UserNameField.Background.Frame.Left, LoginButton.Frame.Bottom + 20, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );

            FacebookLogin.Frame = new CGRect( -2, LoginResult.Background.Frame.Bottom + 20, View.Frame.Width + 4, ControlStyling.ButtonHeight );

            CancelButton.Frame = new CGRect( ( View.Frame.Width - CancelButton.Frame.Width ) / 2, FacebookLogin.Frame.Bottom + 20, CancelButton.Frame.Width, CancelButton.Frame.Height );

            HeaderView.Frame = new CGRect( View.Frame.Left, View.Frame.Top, View.Frame.Width, StyledTextField.StyledFieldHeight );

            // setup the header shadow
            UIBezierPath shadowPath = UIBezierPath.FromRect( HeaderView.Bounds );
            HeaderView.Layer.MasksToBounds = false;
            HeaderView.Layer.ShadowColor = UIColor.Black.CGColor;
            HeaderView.Layer.ShadowOffset = new CoreGraphics.CGSize( 0.0f, .0f );
            HeaderView.Layer.ShadowOpacity = .23f;
            HeaderView.Layer.ShadowPath = shadowPath.CGPath;

            LogoView.Layer.Position = new CoreGraphics.CGPoint( HeaderView.Bounds.Width / 2, HeaderView.Bounds.Height / 2 );
            FBImageView.Layer.Position = new CoreGraphics.CGPoint( FacebookLogin.Bounds.Width / 2, FacebookLogin.Bounds.Height / 2 );

            if ( WebLayout != null )
            {
                WebLayout.LayoutChanged( View.Frame );
            }

            BlockerView.SetBounds( View.Frame.ToRectF( ) );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            LoginResult.Background.Layer.Opacity = 0.00f;

            // clear these only on the appearance of the view. (As opposed to also 
            // when the state becomes LogOut.) This way, if they do something like mess
            // up their password, it won't force them to retype it all in.
            UserNameField.Field.Text = string.Empty;
            PasswordField.Field.Text = string.Empty;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // restore the buttons
            CancelButton.Hidden = false;
            LoginButton.Hidden = false;
            RegisterButton.Hidden = false;

            // if we're logged in, the UI should be slightly different
            if( RockMobileUser.Instance.LoggedIn )
            {
                // populate them with the user's info
                UserNameField.Field.Text = RockMobileUser.Instance.UserID;
                PasswordField.Field.Text = RockMobileUser.Instance.RockPassword;

                SetUIState( LoginState.In );
            }
            else
            {
                SetUIState( LoginState.Out );
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // if they tap somewhere outside of the text fields, 
            // hide the keyboard
            UserNameField.Field.ResignFirstResponder( );
            PasswordField.Field.ResignFirstResponder( );
        }

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            // insist they stay in portait on iPhones
            return Springboard.GetSupportedInterfaceOrientations( );
        }

        public override bool PrefersStatusBarHidden()
        {
            return Springboard.PrefersStatusBarHidden();
        }

        public void TryRockBind()
        {
            if( ValidateInput( ) )
            {
                SetUIState( LoginState.Trying );

                BlockerView.BringToFront( );

                RockMobileUser.Instance.BindRockAccount( UserNameField.Field.Text, PasswordField.Field.Text, BindComplete );

                ProfileAnalytic.Instance.Trigger( ProfileAnalytic.Login, "Rock" );
            }
        }

        bool ValidateInput( )
        {
            bool inputValid = true;

            uint targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( UserNameField.Field.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                inputValid = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, UserNameField.Background );

            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( PasswordField.Field.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                inputValid = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, PasswordField.Background );

            return inputValid;
        }

        public void TryFacebookBind( )
        {
            SetUIState( LoginState.Trying );

            // have our rock mobile user begin the facebook bind process
            RockMobileUser.Instance.BindFacebookAccount( delegate(string fromUri, Facebook.FacebookClient session) 
            {
                    // it's ready, so create a webView that will take them to the FBLogin page
                    WebLayout = new WebLayout( View.Frame );
                    WebLayout.DeleteCacheAndCookies( );

                    View.AddSubview( WebLayout.ContainerView );

                    // set it totally transparent so we can fade it in
                    WebLayout.ContainerView.BackgroundColor = UIColor.Black;
                    WebLayout.ContainerView.Layer.Opacity = 0.00f;
                    WebLayout.SetCancelButtonColor( ControlStylingConfig.TextField_PlaceholderTextColor );

                    // do a nice fade-in
                    SimpleAnimator_Float floatAnimator = new SimpleAnimator_Float( 0.00f, 1.00f, .25f, 
                        delegate(float percent, object value) 
                        {
                            WebLayout.ContainerView.Layer.Opacity = (float)value;
                        },
                        delegate 
                        {
                            // once faded in, begin loading the page
                            WebLayout.ContainerView.Layer.Opacity = 1.00f;

                            WebLayout.LoadUrl( fromUri, delegate(WebLayout.Result result, string url) 
                                {
                                    // if fail/success comes in
                                    if( result != WebLayout.Result.Cancel )
                                    {
                                        // see if it's a valid facebook response

                                        // if an empty url was returned, it's NOT. Fail.
                                        if( string.IsNullOrEmpty( url ) == true )
                                        {
                                            WebLayout.ContainerView.RemoveFromSuperview( );
                                            BindComplete( false );
                                        }
                                        // otherwise, try to parse the response and move forward
                                        else if ( RockMobileUser.Instance.HasFacebookResponse( url, session ) )
                                        {
                                            // it is, continue the bind process
                                            WebLayout.ContainerView.RemoveFromSuperview( );
                                            RockMobileUser.Instance.FacebookCredentialResult( url, session, BindComplete );

                                            ProfileAnalytic.Instance.Trigger( ProfileAnalytic.Login, "Facebook" );
                                        }
                                    }
                                    else
                                    {
                                        // they pressed cancel, so simply cancel the attempt
                                        WebLayout.ContainerView.RemoveFromSuperview( );
                                        LoginComplete( System.Net.HttpStatusCode.ResetContent, "" );
                                    }
                                } );
                        });

                    floatAnimator.Start( );
            });
        }

        public void BindComplete( bool success )
        {
            if ( success )
            {
                // However we chose to bind, we can now login with the bound account
                RockMobileUser.Instance.Login( LoginComplete );
            }
            else
            {
                LoginComplete( System.Net.HttpStatusCode.BadRequest, "" );
            }
        }

        protected void SetUIState( LoginState state )
        {
            // reset the result label
            LoginResult.Field.Text = "";

            switch( state )
            {
                case LoginState.Out:
                {
                    UserNameField.Field.Enabled = true;
                    PasswordField.Field.Enabled = true;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    RegisterButton.Hidden = false;
                    RegisterButton.Enabled = true;

                    LoginButton.SetTitle( LoginStrings.LoginButton, UIControlState.Normal );

                    break;
                }

                case LoginState.Trying:
                {
                    FadeLoginResult( false );
                    BlockerView.Show( null );

                    UserNameField.Field.Enabled = false;
                    PasswordField.Field.Enabled = false;
                    LoginButton.Enabled = false;
                    CancelButton.Enabled = false;
                    RegisterButton.Enabled = false;

                    LoginButton.SetTitle( LoginStrings.LoginButton, UIControlState.Normal );

                    break;
                }

                // Deprecated state
                case LoginState.In:
                {
                    UserNameField.Field.Enabled = false;
                    PasswordField.Field.Enabled = false;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    RegisterButton.Hidden = true;
                    RegisterButton.Enabled = false;

                    LoginButton.SetTitle( "Logout", UIControlState.Normal );

                    break;
                }
            }

            State = state;
        }

        public void LoginComplete( System.Net.HttpStatusCode statusCode, string statusDescription )
        {
            switch ( statusCode )
            {
                // if we received No Content, we're logged in
                case System.Net.HttpStatusCode.NoContent:
                {
                    RockMobileUser.Instance.GetProfileAndCellPhone( ProfileComplete );
                    break;
                }

                case System.Net.HttpStatusCode.Unauthorized:
                {
                    BlockerView.Hide( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            // wrong user name / password
                            FadeLoginResult( true );
                            LoginResult.Field.Text = LoginStrings.Error_Credentials;
                        } );
                    break;
                }

                case System.Net.HttpStatusCode.ResetContent:
                {
                    // consider this a cancellation
                    BlockerView.Hide( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            LoginResult.Field.Text = "";
                        } );

                    break;
                }

                default:
                {
                    BlockerView.Hide( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            // failed to login for some reason
                            FadeLoginResult( true );
                            LoginResult.Field.Text = LoginStrings.Error_Unknown;
                        } );
                    break;
                }
            }
        }

        public void ProfileComplete(System.Net.HttpStatusCode code, string desc, Rock.Client.Person model) 
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    UIThread_ProfileComplete( code, desc, model );
                } );
        }

        void UIThread_ProfileComplete( System.Net.HttpStatusCode code, string desc, Rock.Client.Person model ) 
        {
            BlockerView.Hide( delegate
                {
                    switch ( code )
                    {
                        case System.Net.HttpStatusCode.OK:
                        {
                            // get their address
                            RockMobileUser.Instance.GetFamilyAndAddress( AddressComplete );

                            break;
                        }

                        default:
                        {
                            // if we couldn't get their profile, that should still count as a failed login.
                            SetUIState( LoginState.Out );

                            // failed to login for some reason
                            FadeLoginResult( true );
                            LoginResult.Field.Text = LoginStrings.Error_Unknown;

                            RockMobileUser.Instance.LogoutAndUnbind( );
                            break;
                        }
                    }
                } );
        }

        public void AddressComplete( System.Net.HttpStatusCode code, string desc, List<Rock.Client.Group> model )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    UIThread_AddressComplete( code, desc, model );
                } );
        }

        void UIThread_AddressComplete( System.Net.HttpStatusCode code, string desc, List<Rock.Client.Group> model ) 
        {
            BlockerView.Hide( delegate
                {
                    switch ( code )
                    {
                        case System.Net.HttpStatusCode.OK:
                        {
                            // see if we should set their viewing campus
                            if( RockMobileUser.Instance.PrimaryFamily.CampusId.HasValue == true )
                            {
                                RockMobileUser.Instance.ViewingCampus = RockMobileUser.Instance.PrimaryFamily.CampusId.Value;
                            }
                            
                            // if they have a profile picture, grab it.
                            RockMobileUser.Instance.TryDownloadProfilePicture( PrivateGeneralConfig.ProfileImageSize, ProfileImageComplete );

                            // update the UI
                            FadeLoginResult( true );
                            LoginResult.Field.Text = string.Format( LoginStrings.Success, RockMobileUser.Instance.PreferredName( ) );

                            // start the timer, which will notify the springboard we're logged in when it ticks.
                            LoginSuccessfulTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                                {
                                    // when the timer fires, notify the springboard we're done.
                                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                        {
                                            Springboard.ResignModelViewController( this, null );
                                        } );
                                };

                            LoginSuccessfulTimer.Start( );

                            break;
                        }

                        default:
                        {
                            // if we couldn't get their profile, that should still count as a failed login.
                            SetUIState( LoginState.Out );

                            // failed to login for some reason
                            FadeLoginResult( true );
                            LoginResult.Field.Text = LoginStrings.Error_Unknown;

                            RockMobileUser.Instance.LogoutAndUnbind( );
                            break;
                        }
                    }
                } );
        }

        public void ProfileImageComplete( System.Net.HttpStatusCode code, string desc )
        {
            switch( code )
            {
                case System.Net.HttpStatusCode.OK:
                {
                    // sweet! make the UI update.
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate { Springboard.UpdateProfilePic( ); } );
                    break;
                }

                default:
                {
                    // bummer, we couldn't get their profile picture. Doesn't really matter...
                    break;
                }
            }
        }

        void FadeLoginResult( bool fadeIn )
        {
            UIView.Animate( .33f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                new Action( 
                    delegate 
                    { 
                        LoginResult.Background.Layer.Opacity = fadeIn == true ? 1.00f : 0.00f;
                    })

                , new Action(
                    delegate
                    {
                    })
            );
        }
	}
}
