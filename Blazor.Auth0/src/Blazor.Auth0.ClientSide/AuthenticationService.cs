// <copyright file="AuthenticationService.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Authentication;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Timers;
    using ClientSide.Properties;
    using Models;
    using Models.Enumerations;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;
    using Timer = System.Timers.Timer;

    /// <inheritdoc/>
    public class AuthenticationService : IAuthenticationService, IDisposable
    {
        private readonly ClientOptions clientOptions;
        private readonly NavigationManager navigationManager;
        private readonly HttpClient httpClient;
        private readonly IJSRuntime jsRuntime;

        private readonly ILogger logger;
        private readonly DotNetObjectReference<AuthenticationService> dotnetObjectRef;

        private SessionAuthorizationTransaction sessionAuthorizationTransaction;
        private Timer logOutTimer;
        private SessionStates sessionState = SessionStates.Undefined;

        /// <inheritdoc/>
        public event EventHandler<SessionStates> SessionStateChangedEvent;

        /// <summary>
        /// The event fired just before staring a silent login.
        /// </summary>
        public event EventHandler<bool> BeforeSilentLoginEvent;

        /// <inheritdoc/>
        public UserInfo User { get; private set; }

        /// <inheritdoc/>
        public SessionStates SessionState
        {
            get => sessionState;
            private set
            {
                if (value != sessionState)
                {
                    sessionState = value;
                    SessionStateChangedEvent?.Invoke(this, SessionState);
                }
            }
        }

        /// <inheritdoc/>
        public SessionInfo SessionInfo { get; private set; }

        private bool RequiresNonce => clientOptions.ResponseType == ResponseTypes.IdToken || clientOptions.ResponseType == ResponseTypes.TokenAndIdToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="logger">A <see cref="ILogger"/> param.</param>
        /// <param name="componentContext">A <see cref="IComponentContext"/> param.</param>
        /// <param name="httpClient">A <see cref="HttpClient"/> param.</param>
        /// <param name="jsRuntime">A <see cref="IJSRuntime"/> param.</param>
        /// <param name="navigationManager">A <see cref="NavigationManager"/> param.</param>
        /// <param name="options">A <see cref="ClientOptions"/> param.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "I like this best ;)")]
        public AuthenticationService(ILogger<AuthenticationService> logger, HttpClient httpClient, IJSRuntime jsRuntime, NavigationManager navigationManager, ClientOptions options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            clientOptions = options ?? throw new ArgumentNullException(nameof(options));

            dotnetObjectRef = DotNetObjectReference.Create(this);

            Task.Run(async () =>
            {
                // Ugly but necesary :\
                await this.jsRuntime.InvokeVoidAsync("window.eval", Resources.ClientSideJs);
                await ValidateSession().ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Stops the next Silent Login iterarion.
        /// </summary>
        public void StopSilentLogin()
        {
            logOutTimer?.Stop();
        }

        /// <inheritdoc/>
        public async Task Authorize()
        {
            AuthorizeOptions options = BuildAuthorizeOptions();

            if (clientOptions.LoginMode == LoginModes.Popup)
            {
                await Authentication.AuthorizePopup(jsRuntime, dotnetObjectRef, navigationManager, options).ConfigureAwait(false);
            }
            else
            {
                await Authentication.Authorize(jsRuntime, navigationManager, options).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task LogOut()
        {
            await LogOut(null).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task LogOut(string redirectUri)
        {
            string logoutUrl = CommonAuthentication.BuildLogoutUrl(clientOptions.Domain, clientOptions.ClientId, redirectUri);

            await jsRuntime.InvokeAsync<object>($"{Resources.InteropElementName}.logOut", logoutUrl).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(redirectUri))
            {
                navigationManager.NavigateTo(redirectUri);
            }
            else if (clientOptions.RequireAuthenticatedUser)
            {
                await Authorize().ConfigureAwait(false);
            }
            else
            {
                // There's no redirectUri and an authenticated user is not required
            }

            if (clientOptions.RequireAuthenticatedUser)
            {
                await Task.Delay(3000).ConfigureAwait(false);
            }

            ClearSession();
        }

        /// <inheritdoc/>
        public Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            GenericIdentity identity = null;

            if (SessionState == SessionStates.Active)
            {
                identity = new GenericIdentity(User?.Name ?? string.Empty, "JWT");

                if (!string.IsNullOrEmpty(User.Sub?.Trim()))
                {
                    identity.AddClaim(new Claim("sub", User.Sub));
                }

                if (!string.IsNullOrEmpty(User.Name?.Trim()))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, User.Name));
                }

                if (!string.IsNullOrEmpty(User.GivenName?.Trim()))
                {
                    identity.AddClaim(new Claim("given_name", User.GivenName));
                }

                if (!string.IsNullOrEmpty(User.FamilyName?.Trim()))
                {
                    identity.AddClaim(new Claim("family_name", User.FamilyName));
                }

                if (!string.IsNullOrEmpty(User.MiddleName?.Trim()))
                {
                    identity.AddClaim(new Claim("middle_name", User.MiddleName));
                }

                if (!string.IsNullOrEmpty(User.Nickname?.Trim()))
                {
                    identity.AddClaim(new Claim("nickname", User.Nickname));
                }

                if (!string.IsNullOrEmpty(User.PreferredUsername?.Trim()))
                {
                    identity.AddClaim(new Claim("preferred_username", User.PreferredUsername));
                }

                if (!string.IsNullOrEmpty(User.Profile?.Trim()))
                {
                    identity.AddClaim(new Claim("profile", User.Profile));
                }

                if (!string.IsNullOrEmpty(User.Picture?.Trim()))
                {
                    identity.AddClaim(new Claim("picture", User.Picture));
                }

                if (!string.IsNullOrEmpty(User.Website?.Trim()))
                {
                    identity.AddClaim(new Claim("website", User.Website));
                }

                if (!string.IsNullOrEmpty(User.Email?.Trim()))
                {
                    identity.AddClaim(new Claim("email", User.Email));
                }

                identity.AddClaim(new Claim("email_verified", User.EmailVerified.ToString()));

                if (!string.IsNullOrEmpty(User.Gender?.Trim()))
                {
                    identity.AddClaim(new Claim("gender", User.Gender));
                }

                if (!string.IsNullOrEmpty(User.Birthdate?.Trim()))
                {
                    identity.AddClaim(new Claim("birthdate", User.Birthdate));
                }

                if (!string.IsNullOrEmpty(User.Zoneinfo?.Trim()))
                {
                    identity.AddClaim(new Claim("zoneinfo", User.Zoneinfo));
                }

                if (!string.IsNullOrEmpty(User.Locale?.Trim()))
                {
                    identity.AddClaim(new Claim("locale", User.Locale));
                }

                if (!string.IsNullOrEmpty(User.PhoneNumber?.Trim()))
                {
                    identity.AddClaim(new Claim("phone_number", User.PhoneNumber));
                }

                identity.AddClaim(new Claim("phone_number_verified", User.PhoneNumberVerified.ToString()));

                if (!string.IsNullOrEmpty(User.Address?.Trim()))
                {
                    identity.AddClaim(new Claim("address", User.Address));
                }

                identity.AddClaim(new Claim("updated_at", User.UpdatedAt.ToString()));

                identity.AddClaims(User.CustomClaims.Select(customClaim => new Claim(customClaim.Key, customClaim.Value.GetRawText(), customClaim.Value.ValueKind.ToString())));

                identity.AddClaims(User.Permissions.Select(permission => new Claim("permissions", permission, "permissions")));
            }
            else
            {
                identity = new GenericIdentity(string.Empty, "JWT");
            }

            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        /// <inheritdoc/>
        public async Task ValidateSession()
        {
            await ValidateSession(navigationManager.Uri).ConfigureAwait(false);
        }

        [JSInvokable]
        public async Task ValidateSession(string path)
        {
            // TODO: Add validation such as same host and similars

            // Let's validate the hash
            Uri absoluteUri = navigationManager.ToAbsoluteUri(path);

            ParsedHash parsedHash = Authentication.ParseHash(new ParseHashOptions
            {
                ResponseType = clientOptions.ResponseType,
                AbsoluteUri = absoluteUri,
            });

            // No hash found?!
            if (parsedHash == null)
            {
                // Should we keep the session alive?
                if (clientOptions.SlidingExpiration || clientOptions.RequireAuthenticatedUser)
                {
                    await SilentLogin().ConfigureAwait(false);
                }
                else
                {
                    await LogOut().ConfigureAwait(false);

                    ClearSession();
                }
            }
            else
            {
                // We have a valid hash parameter collection, let's validate the authorization response
                await HandleAuthorizationResponseAsync(new AuthorizationResponse
                {
                    AccessToken = parsedHash.AccessToken,
                    Code = parsedHash.Code,
                    Error = parsedHash.Error,
                    ErrorDescription = parsedHash.ErrorDescription,
                    ExpiresIn = 15,
                    IdToken = parsedHash.IdToken,
                    IsTrusted = false,
                    Origin = absoluteUri.Authority,
                    Scope = string.Empty,
                    State = parsedHash.State,
                    TokenType = "bearer", // TODO: Improve this validation
                    Type = nameof(ResponseModes.Query), // TODO: Improve this validation
                }).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        [JSInvokable]
        /// <summary>
        /// Meant for internal API use only.
        /// </summary>
        public async Task HandleAuthorizationResponseAsync(AuthorizationResponse authorizationResponse)
        {
            try
            {
                sessionAuthorizationTransaction = await TransactionManager.GetStoredTransactionAsync(jsRuntime, clientOptions, authorizationResponse.State).ConfigureAwait(false);

                Authentication.ValidateAuthorizationResponse(authorizationResponse, clientOptions.Domain, sessionAuthorizationTransaction?.State);

                SessionInfo tempSessionInfo = await GetSessionInfoAsync(authorizationResponse).ConfigureAwait(false);

                UserInfo tempIdTokenInfo = await GetUserAsync(tempSessionInfo.AccessToken, tempSessionInfo.IdToken).ConfigureAwait(false);

                ValidateIdToken(tempIdTokenInfo, authorizationResponse.AccessToken);

                InitiateUserSession(tempIdTokenInfo, tempSessionInfo);

                ScheduleLogOut();
            }
            catch (AuthenticationException ex)
            {
                await OnLoginRequestValidationError(authorizationResponse.Error, ex.Message).ConfigureAwait(false);
            }
            finally
            {
                RedirectToHome();
            }
        }

        private void RedirectToHome()
        {
            Uri absoluteUri = new Uri(navigationManager.Uri);

            sessionAuthorizationTransaction = null;

            // Redirect to home (removing the hash)
            navigationManager.NavigateTo(absoluteUri.GetLeftPart(UriPartial.Path));
        }

        private async Task<SessionInfo> GetSessionInfoAsync(AuthorizationResponse authorizationResponse)
        {
            if (authorizationResponse is null)
            {
                throw new ArgumentNullException(nameof(authorizationResponse));
            }

            if (clientOptions.ResponseType == ResponseTypes.Code)
            {
                return await GetSessionInfoAsync(authorizationResponse.Code).ConfigureAwait(false);
            }

            return new SessionInfo
            {
                AccessToken = authorizationResponse.AccessToken,
                ExpiresIn = authorizationResponse.ExpiresIn,
                IdToken = authorizationResponse.IdToken,
                Scope = authorizationResponse.Scope,
                TokenType = authorizationResponse.TokenType,
            };
        }

        private async Task<SessionInfo> GetSessionInfoAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(code));
            }

            return await Authentication.GetAccessToken(
                    httpClient,
                    clientOptions.Domain,
                    clientOptions.ClientId,
                    code,
                    audience: clientOptions.Audience,
                    codeVerifier: sessionAuthorizationTransaction?.CodeVerifier,
                    secret: clientOptions.ClientSecret,
                    redirectUri: sessionAuthorizationTransaction?.RedirectUri)
                .ConfigureAwait(false);
        }

        private async Task<UserInfo> GetUserAsync(string accessToken, string idToken = null)
        {
            if (!string.IsNullOrEmpty(idToken) && (RequiresNonce || clientOptions.GetUserInfoFromIdToken))
            {
                return CommonAuthentication.DecodeTokenPayload<UserInfo>(idToken);
            }
            else
            {
                // In case we're not getting the id_token from the message response or GetUserInfoFromIdToken is set to false try to get it from Auth0's API
                return await CommonAuthentication.UserInfo(httpClient, clientOptions.Domain, accessToken).ConfigureAwait(false);
            }
        }

        private void ValidateIdToken(UserInfo idTokenInfo, string accessToken)
        {
            if (RequiresNonce)
            {
                bool? nonceIsValid = idTokenInfo?.Nonce.Replace(' ', '+').Equals(sessionAuthorizationTransaction?.Nonce.Replace(' ', '+'));

                if (nonceIsValid.HasValue && !nonceIsValid.Value)
                {
                    throw new AuthenticationException(Resources.InvalidNonceError);
                }

                if (string.IsNullOrEmpty(idTokenInfo?.AtHash))
                {
                    logger.LogWarning(Resources.NotAltChashWarning);
                }
                else
                {
                    bool atHashIsValid = Authentication.ValidateAccessTokenHash(idTokenInfo?.AtHash, accessToken);

                    if (!atHashIsValid)
                    {
                        throw new AuthenticationException(Resources.InvalidAccessTokenHashError);
                    }
                }
            }
        }

        private void InitiateUserSession(UserInfo userInfo, SessionInfo sessionInfo)
        {
            if (!string.IsNullOrEmpty(clientOptions.Audience) && !string.IsNullOrEmpty(sessionInfo.AccessToken))
            {
                List<string> permissionsList = CommonAuthentication.DecodeTokenPayload<AccessTokenPayload>(sessionInfo.AccessToken).Permissions ?? new List<string>();
                userInfo.Permissions.AddRange(permissionsList);
            }

            User = userInfo;

            SessionInfo = sessionInfo;

            SessionState = SessionStates.Active;
        }

        private async Task OnLoginRequestValidationError(string error, string validationMessage)
        {
            // In case of any error negate the access
            if (!string.IsNullOrEmpty(validationMessage))
            {
                ClearSession();

                logger.LogError("Login Error: " + validationMessage);

                if (error.ToLowerInvariant() == "login_required" && clientOptions.RequireAuthenticatedUser)
                {
                    await Authorize().ConfigureAwait(false);
                    System.Threading.Thread.Sleep(30000);
                    navigationManager.NavigateTo("/");
                }
            }
        }

        private AuthorizeOptions BuildAuthorizeOptions()
        {
            bool isUsingSecret = !string.IsNullOrEmpty(clientOptions.ClientSecret);
            ResponseTypes responseType = isUsingSecret ? ResponseTypes.Code : clientOptions.ResponseType;
            ResponseModes responseMode = isUsingSecret ? ResponseModes.Query : clientOptions.ResponseMode;
            CodeChallengeMethods codeChallengeMethod = !isUsingSecret && responseType == ResponseTypes.Code ? CodeChallengeMethods.S256 : CodeChallengeMethods.None;
            string codeVerifier = codeChallengeMethod != CodeChallengeMethods.None ? CommonAuthentication.GenerateNonce(clientOptions.KeyLength) : null;
            string codeChallenge = codeChallengeMethod != CodeChallengeMethods.None ? Utils.GetSha256(codeVerifier) : null;
            string nonce = CommonAuthentication.GenerateNonce(clientOptions.KeyLength);

            return new AuthorizeOptions
            {
                Audience = clientOptions.Audience,
                ClientID = clientOptions.ClientId,
                CodeChallengeMethod = codeChallengeMethod,
                CodeVerifier = codeVerifier,
                CodeChallenge = codeChallenge,
                Connection = clientOptions.Connection,
                Domain = clientOptions.Domain,
                Nonce = nonce,
                Realm = clientOptions.Realm,
                RedirectUri = BuildRedirectUrl(),
                ResponseMode = responseMode,
                ResponseType = responseType,
                Scope = clientOptions.Scope,
                State = CommonAuthentication.GenerateNonce(clientOptions.KeyLength),
                Namespace = clientOptions.Namespace,
                KeyLength = clientOptions.KeyLength,
            };
        }

        private void ClearSession()
        {
            SessionState = clientOptions.RequireAuthenticatedUser ? SessionStates.Undefined : SessionStates.Inactive;
            User = null;
            SessionInfo = null;
            sessionAuthorizationTransaction = null;
            logOutTimer?.Stop();
        }

        private async Task SilentLogin()
        {

            BeforeSilentLoginEvent?.Invoke(this, false);

            AuthorizeOptions options = BuildAuthorizeOptions();
            options.ResponseMode = ResponseModes.Web_Message;

            options = await TransactionManager.Proccess(jsRuntime, options).ConfigureAwait(false);

            string authorizeUrl = Authentication.BuildAuthorizeUrl(options);

            await jsRuntime.InvokeAsync<object>($"{Resources.InteropElementName}.drawIframe", dotnetObjectRef, $"{authorizeUrl}&prompt=none").ConfigureAwait(false);
        }

        private void ScheduleLogOut()
        {
            logOutTimer?.Stop();

            if (logOutTimer == null)
            {
                logOutTimer = new Timer();
                logOutTimer.Elapsed += async (object source, ElapsedEventArgs e) =>
                {
                    logOutTimer.Stop();

                    if (clientOptions.SlidingExpiration)
                    {
                        await SilentLogin().ConfigureAwait(false);
                        return;
                    }

                    await LogOut().ConfigureAwait(false);

                    ClearSession();
                };
            }

            logOutTimer.Interval = (SessionInfo.ExpiresIn - 5) * 1000;

            logOutTimer.Start();
        }

        private string BuildRedirectUrl()
        {
            Uri absoluteUri = new Uri(navigationManager.Uri);
            string uri = !string.IsNullOrEmpty(clientOptions.RedirectUri) ? clientOptions.RedirectUri : clientOptions.RedirectAlwaysToHome ? absoluteUri.GetLeftPart(UriPartial.Authority) : absoluteUri.AbsoluteUri;

            return !string.IsNullOrEmpty(clientOptions.RedirectUri) && !clientOptions.RedirectAlwaysToHome ? clientOptions.RedirectUri : uri;
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    dotnetObjectRef.Dispose();
                    httpClient.Dispose();
                    logOutTimer?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}