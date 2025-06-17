using RekordRest;

namespace Rekord.Test.Integration
{
  [TestClass]
  public class RekordTestClient
  {
    private RekordRestClient rekordClient;

    public RekordTestClient()
    {
      var httpClient = new HttpClient();
      // token_id
      var token = "";
      httpClient.DefaultRequestHeaders.Add("Authorization", token);
      httpClient.BaseAddress = new Uri("");
      rekordClient = new RekordRestClient("", httpClient);
    }

    [TestMethod]
    public async Task TestGetAndCreateRecordJson()
    {
      var rekords = await rekordClient.RekordGETAllAsync(null, null, null, null);

      var rekord = rekords.Items.FirstOrDefault();
      if (rekord != null)
      {
        await rekordClient.RekordGETAsync(rekord.Id);
        await rekordClient.MetaAsync(rekord.Id);
      }
      else
      {
        Assert.Fail("No rekord found!");
      }

      var workspaces = await rekordClient.WorkspaceGETAllAsync(null, null);
      var workspace = workspaces.Items.FirstOrDefault();
      if (workspace != null)
      {
        await rekordClient.WorkspaceGETAsync(workspace.Id);
      }
      else
      {
        Assert.Fail("No workspace found!");
      }

      var payload = new
      {
        Test = Guid.NewGuid().ToString()
      };

      var newlyCreated = await rekordClient.RekordPOSTAsync(new RekordRequest
      {
        Description = "Test",
        IssuedAt = DateTime.UtcNow,
        Group = "test",
        OriginalFileName = "test.txt",
        Workspace = new Guid(workspace.Id),
        Payload = payload,
        PayloadType = RekordRequestPayloadType.JSON,
      });
      Console.WriteLine(newlyCreated.Id);
      Assert.IsNotNull(newlyCreated);
    }

    [TestMethod]
    public async Task TestGetAndCreateRecordFile()
    {
      var workspaces = await rekordClient.WorkspaceGETAllAsync(null, null);
      var workspace = workspaces.Items.FirstOrDefault();
      if (workspace == null)
      {
        Assert.Fail();
      }
      var key = Guid.NewGuid().ToString();
      var contentType = "application/pdf";
      var res = await rekordClient.PayloadUrlPOSTAsync(new Body
      {
        Key = key,
        ContentType = contentType,
        Workspace = workspace.Id
      });

      var fileBytes = File.ReadAllBytes(@"C:\Transactions\dummy.pdf");
      var succeeded = await UploadToS3(res.Url, contentType, fileBytes);
      Assert.IsTrue(succeeded);

      var rekordRequest = new RekordRequest
      {
        Description = "Test",
        IssuedAt = DateTime.UtcNow,
        Group = "test",
        OriginalFileName = "dummy.pdf",
        Workspace = new Guid(workspace.Id),
        PayloadType = RekordRequestPayloadType.FILE,
        File = res.Key
      };

      var createdRecord = await rekordClient.RekordPOSTAsync(rekordRequest);
      Assert.IsNotNull(createdRecord);

      Assert.AreEqual(rekordRequest.Description, createdRecord.Description);
      Assert.AreEqual(rekordRequest.File, createdRecord.File);
      Assert.AreEqual(rekordRequest.Workspace.ToString(), createdRecord.Workspace);

      await rekordClient.PayloadUrlGETAsync(createdRecord.Id);
    }

    [TestMethod]
    public async Task TestWorkspace()
    {
      var workspaceName = $"Test_{Guid.NewGuid()}";
      var workspaceResponse = await rekordClient.WorkspacePOSTAsync(new WorkspaceRequest
      {
        Blockchain = "bsv",
        Name = workspaceName
      });

      var updatedWorkspaceName = workspaceName + "_new";
      await rekordClient.WorkspacePUTAsync(workspaceResponse.Id, new WorkspaceRequest
      {
        Blockchain = "bsv",
        Name = updatedWorkspaceName
      });

      var workspace = await rekordClient.WorkspaceGETAsync(workspaceResponse.Id);
      Assert.AreEqual(workspace.Name, updatedWorkspaceName);
      await rekordClient.WorkspaceDELETEAsync(workspace.Id);
    }

    private async Task<bool> UploadToS3(string url, string contentType, byte[] fileBytes)
    {
      var client = new HttpClient();
      using var streamContent = new StreamContent(new MemoryStream(fileBytes));

      streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
      // Upload to bucket
      var uploadResponse = await client.PutAsync(url, streamContent);
      if (!uploadResponse.IsSuccessStatusCode)
      {
        Console.WriteLine(await uploadResponse.Content.ReadAsStringAsync());
        return false;
      }
      return true;
    }
  }
}