# Rekord API C# SDK

This SDK provides a convenient and strongly-typed way to interact with the Rekord API from your .NET applications. Available as a NuGet package, it simplifies the process of integrating Rekord's functionalities.

## How to Use

First, ensure you have installed the SDK via NuGet. The package, named `Rekord.SDK`, is publicly hosted on **GitHub Packages**.

To install it, you need to add the GitHub Packages source to your NuGet configuration. This can be done via a `NuGet.Config` file in your solution directory, or by configuring it in Visual Studio's NuGet Package Manager settings.

**Example `NuGet.Config`:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/your-github-org/index.json" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```
TODO: add right config

Once the source is configured, you can install the package using the .NET CLI:

```bash
dotnet add package Rekord.SDK
```

To get started, you'll need to initialize the `RekordRestClient`. This involves providing an `HttpClient` instance, which should be configured with your API base URL and authentication token.

```csharp
using RekordRest;
using System.Net.Http;
using System; 

// 1. Initialize HttpClient
var httpClient = new HttpClient();

// 2. Obtain your authentication token (e.g., a Cognito ID Token)
var token = "your_cognito_id_token_here"; // Replace with your actual token

// 3. Add the Authorization header for authentication
httpClient.DefaultRequestHeaders.Add("Authorization", token);

// 4. Set the base address of the Rekord API
httpClient.BaseAddress = new Uri("https://api.rekord.example.com/"); // Replace with your actual base URL

// 5. Initialize the RekordRestClient
// The 'version' parameter is appended to the BaseAddress (e.g., "v1" or "production")
var rekordClient = new RekordRestClient("v1", httpClient);

// Now 'rekordClient' is ready to make API calls, e.g.:
// var response = await rekordClient.SomeApiMethodAsync(someParameters);
```

**Explanation:**

* **NuGet Package:** The SDK is distributed as a NuGet package named `Rekord.SDK`. It is publicly hosted on GitHub Packages, meaning you'll need to add the specific GitHub NuGet feed to your project's package sources to be able to install it.
* **`HttpClient` Initialization:** An instance of `HttpClient` is used to manage HTTP requests and responses. It's recommended to reuse a single `HttpClient` instance throughout the lifetime of your application for performance reasons.
* **Authentication Token:** The `token` variable should hold your valid Cognito ID Token. This token is crucial for authenticating your requests with the Rekord API.
* **`Authorization` Header:** The `DefaultRequestHeaders.Add("Authorization", token)` line attaches the bearer token to all subsequent requests made by this `HttpClient` instance, as required by the Rekord API for authentication.
* **`BaseAddress`:** Set this to the root URL of your Rekord API endpoint.
* **`RekordRestClient`:** This is the main client class provided by the SDK.
    * The first parameter (`"v1"` in the example) represents the API version or a specific path segment that needs to be appended to the `BaseAddress`.
    * The second parameter is the configured `HttpClient` instance.

With `rekordClient` initialized, you can now call the various methods exposed by the SDK to interact with the Rekord API.

---
## Methods

### RekordGETAllAsync Method
Retrieves a paginated list of Rekords, with optional filtering by group and workspace. Each Rekord represents a unique record with associated metadata, hashes, and blockchain details.

Parameters:
* page (string, optional): The page number to retrieve. Defaults to 1.
* limit (string, optional): The number of items per page. Defaults to 10.
* group (string, optional): Filters Rekords by their associated group name.
* workspace (string, optional): Filters Rekords by their workspace ID.

Returns:
Task<PaginatedRekordResponse>: A task representing the asynchronous operation, which resolves to a PaginatedRekordResponse. This response object includes a collection of Rekord items for the current page, along with pagination metadata like total items, page number, limit, and totalPages.

### RekordGETAsync Method
Retrieves a single Rekord by its unique identifier.

Parameters:
id (string): The unique ID of the Rekord you want to retrieve.
Returns:

Task<Rekord>: A task representing the asynchronous operation, which resolves to the Rekord object matching the provided ID.

### MetaAsync Method
Retrieves the metadata of a specific Rekord by its unique identifier. This method returns a full Rekord object, where the metadata properties are populated.

Parameters:

id (string): The unique ID of the Rekord whose metadata you want to retrieve.
Returns:

Task<Rekord>: A task representing the asynchronous operation, which resolves to a Rekord object containing the metadata for the specified ID.

## RekordPOSTAsync Method
Creates a new Rekord by submitting its content and associated metadata to the Rekord API.

Parameters:

body (RekordRequest): An object containing the details for the new Rekord.
Returns:

Task<Rekord>: The newly created Rekord object upon successful creation.

RekordRequest Class
Defines the structure of the request body used to create a new Rekord.

Properties:

* Payload (object): The actual content of the Rekord (can be any serializable object).
* IssuedAt (DateTimeOffset): The timestamp when the Rekord was issued.
* Group (string, optional): The source or group to which the Rekord belongs (3-50 chars, alphanumeric, space, hyphen, underscore).
* Workspace (Guid, optional): The unique identifier of the workspace.
* Description (string): A concise description of the Rekord (3-50 chars).
* OriginalFileName (string, optional): The original file name if applicable (max 255 chars).
* PayloadType (RekordRequestPayloadType): Specifies the type of content within the payload (e.g., JSON, FILE).
* File (string, optional): If PayloadType is FILE, this is the file key from the upload-URL endpoint.

## PayloadUrlPOSTAsync Method
Generates a signed URL for uploading a file to S3, enabling secure direct uploads.

Parameters:

body (Body): An object containing details necessary to generate the signed URL, such as the desired file key, content type, and workspace ID.
Returns:

Task<Response2>: A task representing the asynchronous operation, which resolves to a Response2 object containing the signed URL and potentially other upload-related information.


Body Class
Defines the structure of the request body used to obtain a signed URL for file uploads.
Properties:
* Key (string): The desired key (path/filename) for the file in S3. This is always required.
* ContentType (string, optional): The MIME type of the file being uploaded (e.g., application/pdf, image/jpeg).
* Workspace (string): The ID of the workspace where the file will be associated. This is always required.

## PayloadUrlGETAsync Method
Retrieves a signed URL for accessing a file previously uploaded to an S3 bucket, identified by its Rekord ID.

Parameters:

id (string): The unique ID of the Rekord associated with the file you want to retrieve.
Returns:

Task<Response3>: A task representing the asynchronous operation, which resolves to a Response3 object containing the signed URL to the file in the S3 bucket.

## WorkspaceGETAllAsync Method
Retrieves a paginated list of all available workspaces.

Parameters:

page (string, optional): The page number to retrieve (defaults to 1).
limit (string, optional): The number of items per page (defaults to 10).
Returns:

Task<PaginatedWorkspaceResponse>: A task representing the asynchronous operation, which resolves to a PaginatedWorkspaceResponse containing the requested workspaces and pagination metadata.

## WorkspacePOSTAsync Method
This asynchronous method is designed to create a new workspace. It takes a WorkspaceRequest as input, detailing the workspace's properties. Upon successful creation, it returns the new Workspace object. Be aware it can throw an ApiException for server-side errors.

WorkspaceRequest Details
This defines the data needed to create a workspace:

* Name: The required name for the workspace (cannot be empty).
* Blockchain: An optional blockchain code to associate with the workspace.

## WorkspaceDELETEAsync Method
This asynchronous method handles the deletion of a workspace. You provide the ID of the workspace you want to remove, and the method initiates its deletion.

## WorkspacePUTAsync
This asynchronous method handles the update of a workspace.  You provide the ID of the workspace you want to update. 

Paramters: 
* Name: name for the workspace
* Blockchain: the blockchain code associated with the workspace