
using ECommerceApp.DTOs.Requests.Auth;
using ECommerceApp.DTOs.Requests.User;
using ECommerceApp.DTOs.Responses.Auth;
using ECommerceApp.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceApp.Tests.IntegrationTests
{
    [TestClass]
    public class AuthTests 
    {
        private const string ValidPassword = "SomePassword";
        private static CustomWebApplicationFactory _factory;
        private HttpClient _client;
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _factory = new CustomWebApplicationFactory();
        }

        [TestInitialize]
        public async Task TestInit()
        {
            await _factory.ResetDatabase();
            _client = _factory.CreateClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (_factory != null)
            {
                _factory.Dispose();
            }
        }

        [TestMethod]
        public async Task Register_ValidInputs_ReturnsOk()
        {
            var createDto = new CreateUserRequest
            {
                Username = "newuser",
                Email = "NEWUSER@test.com",
                Password = ValidPassword
            };

            var response = await _client.PostAsJsonAsync("api/auth/signup", createDto);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

            var authResult = await response.Content.ReadFromJsonAsync<AuthUserResponse>(options, cancellationToken: TestContext.CancellationToken);

            Assert.IsNotNull(authResult, "The server returned an empty or invalid response body.");

            Assert.IsFalse(string.IsNullOrEmpty(authResult.Token), "The registration response did not include a valid JWT token string.");

            Assert.AreEqual(Roles.User, authResult.Role, "The new account was initialized with an incorrect role profile.");

            Assert.AreEqual("newuser@test.com", authResult.Email, "The system failed to normalize the email string to lowercase.");
        }
        [TestMethod]
        public async Task Register_DuplicateEmail_Returns409Conflict()
        {
            await _factory.SeedUserAsync("username", "duplicateemail@test.com", ValidPassword);

            var duplicateDto = new CreateUserRequest
            {
                Username = "differentusername",
                Email = "duplicateemail@test.com",
                Password = ValidPassword
            };

            var response = await _client.PostAsJsonAsync("api/auth/signup", duplicateDto);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode, await response.Content.ReadAsStringAsync());
        }
        [TestMethod]
        public async Task Register_DuplicateUsername_Returns409Conflict()
        {
            await _factory.SeedUserAsync("duplicateusername", "email@test.com", ValidPassword);

            var duplicateDto = new CreateUserRequest
            {
                Username = "duplicateusername",
                Email = "differentemail@test.com",
                Password = ValidPassword
            };

            var response = await _client.PostAsJsonAsync("api/auth/signup", duplicateDto);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task Register_DuplicateEmailInDifferentCase_Returns409Conflict()
        {
            await _factory.SeedUserAsync("caseusername", "dup@test.com", ValidPassword);

            var duplicateDto = new CreateUserRequest
            {
                Username = "caseuseradd",
                Email = "DUP@TEST.COM",
                Password = ValidPassword
            };

            var response = await _client.PostAsJsonAsync("api/auth/signup", duplicateDto);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task Login_WithEmail_Succeeds()
        {
            await _factory.SeedUserAsync("loginemail", "loginemail@test.com", ValidPassword);


            var loginDto = new LoginUserRequest
            {
                Identifier = "loginemail@test.com",
                Password = ValidPassword
            };

            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto, TestContext.CancellationToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task Login_WithUsername_Succeeds()
        {
            await _factory.SeedUserAsync("loginusername", "loginusername@test.com", ValidPassword);


            var loginDto = new LoginUserRequest
            {
                Identifier = "loginusername",
                Password = ValidPassword
            };

            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto, TestContext.CancellationToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task Login_WrongPassword_Returns401Unauthorized()
        {
            await _factory.SeedUserAsync("loginpassword", "loginpassword@test.com", ValidPassword);

            var loginDto = new LoginUserRequest
            {
                Identifier = "loginpassword",
                Password = ValidPassword + "x"
            };
    
            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto, TestContext.CancellationToken);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task Login_NonExistentUser_Returns401Unauthorized()
        {
            var loginDto = new LoginUserRequest
            {
                Identifier = "nonexistinguser",
                Password = ValidPassword
            };
    
            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto, TestContext.CancellationToken);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
        }
        
        [TestMethod]
        public async Task Login_EmailIsCaseInsensitive_Succeeds()
        {
            await _factory.SeedUserAsync("logincase", "logincase@test.com", ValidPassword);

            var loginDto = new LoginUserRequest
            {
                Identifier = "LOGINCASE@TEST.COM",
                Password = ValidPassword
            };
    
            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto, TestContext.CancellationToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task Login_WhenUserIsDeleted_Returns401Unauthorized()
        {
            var user = await _factory.SeedUserAsync("deleteuser", "deleteuser@test.com", ValidPassword);

            var deleteDto = new DeleteUserRequest
            {
                Password = ValidPassword
            };

            var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "api/user/me/delete")
            {
                Content = JsonContent.Create(deleteDto)
            };

         
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.AccessToken);
            var deleteResponse = await _client.SendAsync(deleteRequest, TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.OK, deleteResponse.StatusCode, await deleteResponse.Content.ReadAsStringAsync(TestContext.CancellationToken));
            _client.DefaultRequestHeaders.Authorization = null;

            var loginDto = new LoginUserRequest
            {
                Identifier = "deleteuser@test.com",
                Password = "SomePassword"
            };
            
            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto, TestContext.CancellationToken);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task Login_UnknownUserAndWrongPassword_ReturnSameMessage()
        {
            await _factory.SeedUserAsync("existing-user", "existing-user@test.com", ValidPassword, TestContext.CancellationToken);

            var unknownUserDto = new LoginUserRequest
            {
                Identifier = "completely-unknown-user@test.com",
                Password = ValidPassword
            };

            var wrongPasswordDto = new LoginUserRequest
            {
                Identifier = "existing-user@test.com",
                Password = ValidPassword + "x"
            };

    
            var responseUnknown = await _client.PostAsJsonAsync("api/auth/login", unknownUserDto, TestContext.CancellationToken);
            var responseWrongPass = await _client.PostAsJsonAsync("api/auth/login", wrongPasswordDto, TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.Unauthorized, responseUnknown.StatusCode, "Unknown user did not return 401.");
            Assert.AreEqual(HttpStatusCode.Unauthorized, responseWrongPass.StatusCode, "Wrong password did not return 401.");

            using var docUnknown = await System.Text.Json.JsonDocument.ParseAsync(
                await responseUnknown.Content.ReadAsStreamAsync(TestContext.CancellationToken),
                cancellationToken: TestContext.CancellationToken);

            using var docWrongPass = await System.Text.Json.JsonDocument.ParseAsync(
                await responseWrongPass.Content.ReadAsStreamAsync(TestContext.CancellationToken),
                cancellationToken: TestContext.CancellationToken);

            string detailUnknown = docUnknown.RootElement.GetProperty("detail").GetString() ?? string.Empty;
            string detailWrongPass = docWrongPass.RootElement.GetProperty("detail").GetString() ?? string.Empty;

            TestContext.WriteLine($"Unknown User Message: '{detailUnknown}'");
            TestContext.WriteLine($"Wrong Password Message: '{detailWrongPass}'");

            Assert.IsFalse(string.IsNullOrEmpty(detailUnknown));
            Assert.IsFalse(string.IsNullOrEmpty(detailWrongPass));

            Assert.AreEqual(detailUnknown, detailWrongPass, "Security Mismatch: The error messages differ, exposing a user enumeration leak!");
        }
    }
}
