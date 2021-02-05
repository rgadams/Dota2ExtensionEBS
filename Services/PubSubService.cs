using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using JWT.Serializers;
using JWT.Algorithms;
using JWT;
using Dota2ExtensionEBS;
using Microsoft.IdentityModel.Tokens;
using System.Security.Principal;

public interface IPubSubService    
{
    Boolean SendPubSubMessage(string channel_id, Dota2Data message);
    bool VerifyJWT(string token);
}
public class PubSubService : IPubSubService
{
    private HttpClient httpClient;
    private string authToken;

    public PubSubService()
    {
        authToken = GetAuthToken();
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authToken
        );
        httpClient.DefaultRequestHeaders.Add(
            "Client-Id", Environment.GetEnvironmentVariable("EXTENSION_CLIENT_ID")
        );

    }

    public Boolean SendPubSubMessage(string channel_id, Dota2Data message)
    {
        Console.WriteLine("Sending Message");
        string PubSubUrl = String.Format("https://api.twitch.tv/extensions/message/{0}", channel_id);
        var payload = new Dictionary<string, object>
        {
            { "content_type", "application/json" },
            { "message", message.ToString() },
            { "targets", new string[] { "broadcast" } }
        };

        string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);

        string authToken = GetAuthToken();
        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authToken
        );
        httpClient.DefaultRequestHeaders.Add(
            "Client-Id", Environment.GetEnvironmentVariable("EXTENSION_CLIENT_ID")
        );

        HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = httpClient.PostAsync(PubSubUrl, content);
        response.Wait();
        Console.WriteLine(response.Result.ToString());
        switch ((int) response.Result.StatusCode)
        {
            case 403:
                // authToken = GetAuthToken();
                // SendPubSubMessage(channel_id, message);
                return false;
            case 200:
            case 204:
                return true;
            default:
                return false;
        }
    }

    
    private string GetAuthToken()
    {
        var payload = new Dictionary<string, object>
        {
            { "exp", (GetEpoch()+(1*60*60)) },
            { "user_id", "softshadow" },
            { "role", "external"},
            { "channel_id", "99663797" },
            { "pubsub_perms", new Dictionary<string, object>
                {
                    { "send", new string[] { "*" } },
                }
            }
        };

        IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        IJsonSerializer serializer = new JsonNetSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

        var token = encoder.Encode(payload, Convert.FromBase64String(Environment.GetEnvironmentVariable("EXTENSION_SECRET_KEY")));
        return token;
    }
    private int GetEpoch()
    {
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        int secondsSinceEpoch = (int)t.TotalSeconds;
        return secondsSinceEpoch;
    }

    public bool VerifyJWT(string authToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters();

        SecurityToken validatedToken;
        IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
        return true;
    }

    private static TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters()
        {
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("EXTENSION_SECRET_KEY"))) // The same key as the one that generate the token
        };
    }
}