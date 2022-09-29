﻿using System.Text;
using TheatersOfTheCity.Business.Options;
using TheatersOfTheCity.Core.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Serilog;
using TheatersOfTheCity.Core.Domain;
using TheatersOfTheCity.Core.External;


namespace TheatersOfTheCity.Business.External;

public class GoogleService : IGoogleService
{
    private const string UserInfoUrl = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={0}";
    private const string AuthEndpoint = "https://oauth2.googleapis.com/token";
    
    private readonly ClientCredentials _clientCredentials;
    private readonly IHttpClientFactory _clientFactory;

    public GoogleService(ClientCredentials clientCredentials, IHttpClientFactory clientFactory)
    {
        _clientCredentials = clientCredentials;
        _clientFactory = clientFactory;
    }

    public async Task<GoogleTokenBody> GetAccessTokenAsync(string code)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _clientCredentials.ClientId },
            { "client_secret", _clientCredentials.ClientSecret },
            { "grant_type", "authorization_code" },
            { "code", code},
            { "access_type", "offline" },
            {"redirect_uri", "https://developers.google.com/oauthplayground"},
        };

        var content = new FormUrlEncodedContent(queryParams);
        
        Log.Information("Google: Trying to request token");
        var authResponse = await _clientFactory.CreateClient().PostAsync(AuthEndpoint, content);
        authResponse.EnsureSuccessStatusCode();
        Log.Information("Google: refresh token was successfully received");
        
        var stringData = await authResponse.Content.ReadAsStringAsync();
            
        var result = JsonConvert.DeserializeObject<GoogleTokenBody>(stringData);
        
        return result ?? throw new ArgumentNullException($"can't deserialize access token");
    }

    public async Task<UserProfile> GetUserProfile(string accessToken)
    {
        var formattedUrl = string
            .Format(UserInfoUrl, accessToken);
        

        Log.Information("Auth: Trying to get user info");
        var authResponse = await _clientFactory.CreateClient().GetAsync(formattedUrl);
        authResponse.EnsureSuccessStatusCode();
        Log.Information("Auth: user profile was successfully received");

        var stringData = await authResponse.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<UserProfile>(stringData);

        return user ?? throw new NullReferenceException();
    }

    public async Task<GoogleTokenBody> RefreshAccessToken(string refreshToken)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _clientCredentials.ClientId },
            { "client_secret", _clientCredentials.ClientSecret },
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
        };

        var content = new FormUrlEncodedContent(queryParams);

        var authResponse = await _clientFactory.CreateClient().PostAsync(AuthEndpoint, content);
        authResponse.EnsureSuccessStatusCode();

        var stringData = await authResponse.Content.ReadAsStringAsync();
        var newAccessToken = JsonConvert.DeserializeObject<GoogleTokenBody>(stringData);

        return newAccessToken ?? throw new ArgumentNullException(newAccessToken?.Error);
    }
}