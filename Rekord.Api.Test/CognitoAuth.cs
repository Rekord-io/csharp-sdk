using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

namespace Rekord.Api.Test
{
  public class CognitoAuthenticator
  {
    /// <summary>
    /// Authenticates a user with an Amazon Cognito User Pool and returns the ID Token.
    /// </summary>
    /// <param name="awsRegion">The AWS region of the Cognito User Pool (e.g., "us-east-1").</param>
    /// <param name="clientId">The client ID of your App client.</param>
    /// <param name="username">The username of the user to authenticate.</param>
    /// <param name="password">The password of the user to authenticate.</param>
    /// <returns>The ID Token as a string, or null if authentication fails.</returns>
    public static async Task<string> AuthenticateUserAsync(string awsRegion, string clientId, string username, string password)
    {
      // Use a try-catch block for robust error handling.
      try
      {
        // Create a new Amazon Cognito Identity Provider client.
        // The client will use the default credential provider chain to find credentials,
        // or you can configure them explicitly.
        var cognitoClient = new AmazonCognitoIdentityProviderClient(Amazon.RegionEndpoint.GetBySystemName(awsRegion));

        // Create an authentication request.
        var authRequest = new InitiateAuthRequest
        {
          AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
          ClientId = clientId,
          AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", username },
                    { "PASSWORD", password }
                }
        };

        // Initiate the authentication process.
        var authResponse = await cognitoClient.InitiateAuthAsync(authRequest);

        // Check if the authentication was successful and an ID token was returned.
        if (authResponse.AuthenticationResult != null && !string.IsNullOrEmpty(authResponse.AuthenticationResult.IdToken))
        {
          Console.WriteLine("Authentication successful!");
          return authResponse.AuthenticationResult.IdToken;
        }
        else
        {
          // This branch would typically not be reached for a successful USER_PASSWORD_AUTH flow,
          // but it's good practice to handle it.
          Console.WriteLine("Authentication failed. No token returned.");
          return null;
        }
      }
      catch (Exception ex)
      {
        // Catch and log any exceptions that occur during the authentication process.
        // This could be due to incorrect credentials, invalid client ID, or network issues.
        Console.WriteLine($"An error occurred during authentication: {ex.Message}");
        return null;
      }
    }
  }
}