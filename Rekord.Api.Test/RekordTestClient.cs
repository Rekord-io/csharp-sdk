using RekordRest;
namespace Rekord.Api.Test
{
  [TestClass]
  public class RekordTestClient
  {
    private RekordRestClient rekordClient;

    public RekordTestClient()
    {
      var httpClient = new HttpClient();
      var token = "eyJraWQiOiJ4Z0dtT2dXbFhUd05OY1dVaXoxTjFpclYrUGlHNytvMDFnalE2bFhcL2FoWT0iLCJhbGciOiJSUzI1NiJ9.eyJzdWIiOiI0MmE1MTQxNC03MDcxLTcwNzItYTBmMy1mMGI2ZWY5MGEzZmUiLCJpc3MiOiJodHRwczpcL1wvY29nbml0by1pZHAuZXUtd2VzdC0xLmFtYXpvbmF3cy5jb21cL2V1LXdlc3QtMV9NWEN5cVg5M2siLCJjb2duaXRvOnVzZXJuYW1lIjoiNDJhNTE0MTQtNzA3MS03MDcyLWEwZjMtZjBiNmVmOTBhM2ZlIiwib3JpZ2luX2p0aSI6IjFkZmYxOTc3LTllNzgtNGRjOS04NjJiLTdiMTAxYmM5MDAyNSIsImF1ZCI6IjR1cXU4ZHM0N2pnbjFhNWJrYWlzNzFsa2FiIiwiZXZlbnRfaWQiOiJmMjQ2NWFkZi1iMDU4LTQ1OTItYjFhZC02ZmQzMzMyYjY0ZGEiLCJ0b2tlbl91c2UiOiJpZCIsImF1dGhfdGltZSI6MTc0OTEyMjIwMiwiZXhwIjoxNzQ5MTI1ODAyLCJpYXQiOjE3NDkxMjIyMDIsImp0aSI6ImJhNGI1ZGU1LThjNjItNGQ4MC1iNDkzLTE2YzM5NjAxOTdhYiIsImVtYWlsIjoic2FudG9mcmFAcmVrb3JkLmlvIn0.HxKZTdMWG9bySEzVFW_RHcxRcQ0ZnWmu5pI0N5O5Uxtq6vCXw7sUrwLG0zWvUkzX8AeAZ-3882kCNemtSsgX4Q5A2fMbUPmuA6RjK_OXAYQKHRCBXUImE7RMWnhZ1VP8QTzWgwgBk5gI34GYMv-wcKWBZasJkhyA8KpOIt3wwuL9hkOxDEHq-2y4KF-bqxm7z4tIvr7fYLNcDnw34m_9wEQKvkqOYj9jnA5e-yhbCn4lpUpgEMX-V8RC8tUMQ3kzabogKuHxNNGSY_pQ-0Wic20NK9U-q1a1bjLgmhkJQ-25zxVNQbYq6tXlPb0FczIBlTO72knrrot5fKpMpENnGQ";
      httpClient.DefaultRequestHeaders.Add("Authorization", token);
      httpClient.BaseAddress = new Uri("https://gx3grbvtti.execute-api.eu-west-1.amazonaws.com/");
      rekordClient = new RekordRestClient("rekord-web-application-dev-api-gw-stage", httpClient);
    }

    [TestMethod]
    public async Task TestGetAndCreateRecordJson()
    {
      var rekords = await rekordClient.RekordGETAsync(null, null, null);

      var rekord = rekords.Items.FirstOrDefault();
      if (rekord != null)
      {
        await rekordClient.RekordGET2Async(rekord.Id);
        await rekordClient.MetaAsync(rekord.Id);
      }
      else
      {
        Assert.Fail("No rekord found!");
      }

      var workspaces = await rekordClient.WorkspaceGETAsync(null, null);
      var workspace = workspaces.Items.FirstOrDefault();
      if (workspace != null)
      {
        await rekordClient.WorkspaceGET2Async(workspace.Id);
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

      Assert.IsNotNull(newlyCreated);
    }

    [TestMethod]
    public async Task TestGetAndCreateRecordFile()
    {
      var workspaces = await rekordClient.WorkspaceGETAsync(null, null);
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

      var responseGet = await rekordClient.PayloadUrlGETAsync(createdRecord.Id);
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

      var workspace = await rekordClient.WorkspaceGET2Async(workspaceResponse.Id);
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
