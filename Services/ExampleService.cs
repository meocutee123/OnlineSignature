using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OnlineSignature.Models;
using OnlineSignature.Ultilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OnlineSignature.Services
{
    public interface IExampleService
    {
        (OAuth.OAuthToken, OAuth.UserInfo.Account) RequestAccessToken(string code);
        Task<(OAuth.OAuthToken, OAuth.UserInfo.Account)> RefreshToken(string refreshToken);
        Uri GetAuthorizationUri();
        Task<EnvelopCreationResponse> SendEnvelopeForEmbeddedSigning(EnvelopeCreationInfo envelopCreationInfo);
    }
    public class ExampleService : IExampleService
    {
        private readonly AppCredentials _appCredentials;
        public ExampleService(IOptions<AppCredentials> appCredentials)
        {
            _appCredentials = appCredentials.Value; ;
        }

        public (OAuth.OAuthToken, OAuth.UserInfo.Account) RequestAccessToken(string code)
        {
            try
            {
                var apiClient = new ApiClient(ApiClient.Demo_REST_BasePath);

                var accessToken = apiClient.GenerateAccessToken(_appCredentials.ClientId, _appCredentials.ClientSecret, code);

                var userInfo = apiClient.GetUserInfo(accessToken.access_token);

                return (accessToken, userInfo.Accounts.Find(x => x.IsDefault.Equals("true")));
            }
            catch (Exception ex)
            {
                throw new Exception("[BAD REQUEST] Retrive access token failed", ex);
            }
        }
        public Uri GetAuthorizationUri()
        {
            var apiClient = new ApiClient(ApiClient.Demo_REST_BasePath);

            return apiClient.GetAuthorizationUri(_appCredentials.ClientId, _appCredentials.Scopes, _appCredentials.RedirectUri, "code");
        }

        public async Task<(OAuth.OAuthToken, OAuth.UserInfo.Account)> RefreshToken(string refreshToken)
        {
            var formData = new Dictionary<string, string>
                {
                    {"grant_type", "refresh_token" },
                    {"refresh_token", refreshToken }
                };

            HttpContent httpContent = new FormUrlEncodedContent(formData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(_appCredentials.ClientId + ":" + _appCredentials.ClientSecret));

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedData);

            var httpResponseMessage = await httpClient.PostAsync(_appCredentials.TokenEndpoint, httpContent);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                var oAuthToken = JsonConvert.DeserializeObject<OAuth.OAuthToken>(response);
                if (oAuthToken is not null)
                {
                    var apiClient = new ApiClient(ApiClient.Demo_REST_BasePath);
                    var userInfo = apiClient.GetUserInfo(oAuthToken.access_token);
                    return (oAuthToken, userInfo.Accounts.Find(x => x.IsDefault.Equals("true")));
                };
            }
            throw new Exception("[BAD REQUEST] Token invalid");

        }

        public async Task<EnvelopCreationResponse> SendEnvelopeForEmbeddedSigning(EnvelopeCreationInfo envelopeCreationInfo)
        {
            try
            {
                var apiClient = new ApiClient(ApiClient.Demo_REST_BasePath);
                apiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + envelopeCreationInfo.AccessToken);

                var envelopesApi = new EnvelopesApi(apiClient);
                var envelopeSummary = await envelopesApi.CreateEnvelopeAsync(envelopeCreationInfo.AccountId, envelopeCreationInfo.EnvelopeDefinition);

                return new EnvelopCreationResponse { Id = envelopeSummary.EnvelopeId };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

    }
}
