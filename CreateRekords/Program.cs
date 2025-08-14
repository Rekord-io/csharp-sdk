
using Newtonsoft.Json.Linq;
using Rekord.Api.Test;
using RekordRest;
using System.Net.Http.Headers;
using System.Net.Http;

class Program
{
  private static Random random = new Random();
  static async Task Main(string[] args)
  {
    var rekordClient = await CreateClient();
    var workspaces = await rekordClient.ListWorkspacesAsync(null, null);
    var workspace = workspaces.Items.FirstOrDefault();


    int i = 0;
    long createdNo = 0;
    while (true)
    {

      try
      {
        var payload = new
        {
          Serial = Guid.NewGuid().ToString()
        };
        var newlyCreated = await rekordClient.CreateRekordAsync(new RekordRequest
        {
          Description = "Serial",
          IssuedAt = DateTime.UtcNow,
          Group = "SAMPLE",
          OriginalFileName = "item.json",
          Workspace = new Guid(workspace!.Id),
          Payload = payload,
          PayloadType = RekordRequestPayloadType.JSON,
        });
        Console.WriteLine($"Created rekord: {newlyCreated.Id}");
        var noise = random.Next(-1000, 1000 + 1);
        var sleep = 1300 + noise;
        Console.WriteLine($"Wait: {sleep}, created: {++createdNo}");
        await Task.Delay(sleep);
        if (i > 800)
        {
          // recreate client with new token
          rekordClient = await CreateClient();
          i = 0;
        }
        i++;
      }
      catch(Exception ex)
      {
        rekordClient = await CreateClient();
        Console.WriteLine(ex.ToString());
      }

    }
  }

  private static async Task<RekordRestClient> CreateClient()
  {
    var idToken = await AuthenticateAndGetToken();
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
    return new RekordRestClient("https://gx3grbvtti.execute-api.eu-west-1.amazonaws.com/rekord-web-application-dev-api-gw-stage", httpClient);
  }

  private static async Task<string> AuthenticateAndGetToken()
  {
    Console.WriteLine("Starting Cognito user authentication...");
    var authenticator = new CognitoAuthenticator();

    // The values below have been updated based on your request.
    string awsRegion = "eu-west-1";
    string clientId = "";
    string username = "";
    string password = "";

    // Call the authentication method.
    string idToken = await authenticator.AuthenticateUserAsync(awsRegion, clientId, username, password);

    // Check the returned ID token.
    if (idToken != null)
    {
      Console.WriteLine("\nSuccessfully received ID Token:");
      Console.WriteLine(idToken);
      // You can now use this token to access other AWS services.
    }
    else
    {
      Console.WriteLine("\nAuthentication failed. Could not retrieve ID Token.");
    }

    // Return the token.
    return idToken;
  }
}