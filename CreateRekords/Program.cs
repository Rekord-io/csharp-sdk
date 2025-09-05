using Rekord.Api.Test;
using RekordRest;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

class Program
{
  private static Random random = new Random();
  private static IConfiguration configuration;

  static async Task Main(string[] args)
  {
    // Load configuration
    configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    if (args.Length == 0)
    {
      Console.WriteLine("Usage:");
      Console.WriteLine("  CreateRekords check <pageFrom> <pageTo> [pageSize] - Check records status");
      Console.WriteLine("  CreateRekords create-loop [baseSleep] [noiseRange] - Create records in infinite loop");
      Console.WriteLine("  CreateRekords create-once <count> - Create a specific number of records");
      Console.WriteLine("");
      Console.WriteLine("Examples:");
      Console.WriteLine("  CreateRekords check 1 10 100");
      Console.WriteLine("  CreateRekords create-loop");
      Console.WriteLine("  CreateRekords create-loop 120 50");
      Console.WriteLine("  CreateRekords create-once 50");
      return;
    }

    string mode = args[0].ToLower();

    try
    {
      switch (mode)
      {
        case "check":
          if (args.Length < 3)
          {
            Console.WriteLine("Error: check mode requires pageFrom and pageTo arguments");
            Console.WriteLine("Usage: CreateRekords check <pageFrom> <pageTo> [pageSize]");
            return;
          }

          int pageFrom = int.Parse(args[1]);
          int pageTo = int.Parse(args[2]);
          string pageSize = args.Length > 3 ? args[3] : "100";

          await CheckRecords(pageFrom, pageTo, pageSize);
          break;

        case "create-loop":
          int baseSleep = args.Length > 1 ? int.Parse(args[1]) : 120;
          int noiseRange = args.Length > 2 ? int.Parse(args[2]) : 50;

          Console.WriteLine($"Starting create-loop mode with baseSleep: {baseSleep}ms, noiseRange: ±{noiseRange}ms");
          await CreateRecords(baseSleep, noiseRange);
          break;

        case "create-once":
          if (args.Length < 2)
          {
            Console.WriteLine("Error: create-once mode requires count argument");
            Console.WriteLine("Usage: CreateRekords create-once <count>");
            return;
          }

          int count = int.Parse(args[1]);
          await CreateNoRecords(count);
          break;

        default:
          Console.WriteLine($"Error: Unknown mode '{mode}'");
          Console.WriteLine("Valid modes: check, create-loop, create-once");
          break;
      }
    }
    catch (FormatException)
    {
      Console.WriteLine("Error: Invalid number format in arguments");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
    }
  }

  private static async Task CheckRecords(int pageFrom, int pageTo, string size = "100")
  {
    var rekordClient = await CreateClient();

    var succeded = 0;
    var all = 0;
    for (int i = pageFrom; i < pageTo; i++)
    {
      var rekords = await rekordClient.ListRekordsAsync(i.ToString(), size, null, null);
      foreach(var rekord in rekords.Items)
      {
        if (rekord.RekordStatus != RekordMetaRekordStatus.RECORDED)
        {

          Console.WriteLine(string.Format("RekordId: {0}, IssuedAt: {1}, status: {2}, txId: {3}", rekord.Id, rekord.IssuedAt, rekord.RekordStatus, rekord.BlockchainMeta?.TransactionId));
        }
        else
        {
          succeded++;
        }
        all++;
      }
      Console.WriteLine($"Page checked: ${i}");
    }
    Console.WriteLine($"{succeded}/{all}");

  }

  private static async Task CreateRecords(int baseSleep, int noiseRange)
  {
    var rekordClient = await CreateClient();
    var workspaces = await rekordClient.ListWorkspacesAsync(null, null);
    var workspace = workspaces.Items.FirstOrDefault();

    int reloginAfter = 3_600_000 / (baseSleep + noiseRange);

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
          Group = "group-default",
          OriginalFileName = "item.json",
          Workspace = new Guid(workspace!.Id),
          Payload = payload,
          PayloadType = RekordRequestPayloadType.JSON,
        });
        Console.WriteLine($"Created rekord: {newlyCreated.Id}");
        var noise = random.Next(-noiseRange, noiseRange);
        var sleep = baseSleep + noise;
        Console.WriteLine($"Wait: {sleep}, created: {++createdNo}");
        await Task.Delay(sleep);
        if (i > reloginAfter)
        {
          // recreate client with new token
          rekordClient = await CreateClient();
          i = 0;
        }
        i++;
      }
      catch (Exception ex)
      {
        await Task.Delay(1000);
        Console.WriteLine(ex.ToString());
        try
        {
          rekordClient = await CreateClient();
        }
        catch { }
      }

    }
  }

  private static async Task CreateNoRecords(int reqNumber)
  {
    var rekordClient = await CreateClient();
    var workspaces = await rekordClient.ListWorkspacesAsync(null, null);
    var workspace = workspaces.Items.FirstOrDefault();

    int i = 0;
    var start = DateTime.UtcNow;
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
          Group = "group-default",
          OriginalFileName = "item.json",
          Workspace = new Guid(workspace!.Id),
          Payload = payload,
          PayloadType = RekordRequestPayloadType.JSON,
        });
        Console.WriteLine($"Created rekord: {newlyCreated.Id}");
        i++;
        if (i > reqNumber)
          break;
      }
      catch (Exception ex)
      {
        rekordClient = await CreateClient();
        Console.WriteLine(ex.ToString());
      }
    }
    var end = DateTime.UtcNow;
    Console.WriteLine(end-start);
  }

  private static async Task<RekordRestClient> CreateClient()
  {
    var idToken = await AuthenticateAndGetToken();
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

    var baseUrl = configuration["ApiSettings:BaseUrl"];
    return new RekordRestClient(baseUrl, httpClient);
  }

  private static async Task<string> AuthenticateAndGetToken()
  {
    Console.WriteLine("Starting Cognito user authentication...");
    var authenticator = new CognitoAuthenticator();

    // Read configuration values from appsettings.json
    string awsRegion = configuration["AwsSettings:Region"];
    string clientId = configuration["AwsSettings:ClientId"];
    string username = configuration["AwsSettings:Username"];
    string password = configuration["AwsSettings:Password"];

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