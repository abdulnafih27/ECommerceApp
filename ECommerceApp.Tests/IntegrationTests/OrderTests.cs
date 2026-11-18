
using ECommerceApp.DTOs.Requests.Orders;
using ECommerceApp.DTOs.Responses.Orders;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ECommerceApp.Tests.IntegrationTests
{
    [TestClass]
    public class OrderTests
    {
        private const string ValidPassword = "Somepassword";
        private static CustomWebApplicationFactory _factory;
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
        public async Task PlaceOrder_ValidCart_CreatesOrderClearsCartAndDecrementsStock()
        {
            var data = await SeedTestData("placeorderuser", "placeorder@test.com", 10, 20);

            await _factory.SeedCartItemAsync(data.UserId, data.ProductId, 2);

            var placeOrderDto = ValidOrderRequest();

            var response = await SendOrderRequest(data.AccessToken, placeOrderDto);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync(TestContext.CancellationToken));

            var placeResult = await GetJsonData<OrderDetailsResponse>(response.Content);
            var product = await _factory.GetProductAsync(data.ProductId);
            var cartItemCount = await _factory.GetCartItemCountAsync(data.UserId);

            Assert.IsNotNull(placeResult);

            Assert.AreEqual(40m, placeResult.TotalAmount, "Total amount doesn't match");
            Assert.HasCount(1, placeResult.Items);

            var item = placeResult.Items.Single();

            Assert.AreEqual("Abdul Nafih", placeResult.ShippingFullName);
            Assert.AreEqual(OrderStatus.Placed, placeResult.Status);
            Assert.AreEqual(2, item.Quantity, "Incorrect item quantity");
            Assert.AreEqual(20m, item.UnitPrice, "Incorrect unit price");

            Assert.IsNotNull(product);
            Assert.AreEqual(product.ProductName, item.ProductName, "Incorrect product in order");

            Assert.AreEqual(8, product.StockQuantity, "Stock should drop from 10 to 8");
            Assert.AreEqual(0, cartItemCount, "Cart should be empty after placing the order");
        }

        [TestMethod]
        public async Task PlaceOrder_EmptyCart_ReturnsBadRequest()
        {
            var user = await _factory.SeedUserAsync("emptycartuser", "emptycartuser@test.com", ValidPassword);

            var placeOrderDto = ValidOrderRequest();

            var response = await SendOrderRequest(user.AccessToken, placeOrderDto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, await response.Content.ReadAsStringAsync(TestContext.CancellationToken));
        }

        [TestMethod]
        public async Task PlaceOrder_InsufficientStock_ReturnsBadRequestAndWritesNothing()
        {
            var data = await SeedTestData("nostock", "nostock@test.com", 1, 20);
            await _factory.SeedCartItemAsync(data.UserId, data.ProductId, 2);

            var placeOrderDto = ValidOrderRequest();

            using var response = await SendOrderRequest(data.AccessToken, placeOrderDto);

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                response.StatusCode,
                await response.Content.ReadAsStringAsync(TestContext.CancellationToken));

            var product = await _factory.GetProductAsync(data.ProductId);
            Assert.IsNotNull(product);
            Assert.AreEqual(1, product.StockQuantity, "not enough stock");

            var cartItemsCount = await _factory.GetCartItemCountAsync(data.UserId);
            Assert.AreEqual(1, cartItemsCount);

            var orderCount = await _factory.GetOrderCountAsync(data.UserId);
            Assert.AreEqual(0, orderCount);

        }

        [TestMethod]
        public async Task GetMyOrder_OtherCustomersOrder_ReturnsNotFound()
        {
            var firstUser = await _factory.SeedUserAsync("firstuser", "firstuser@test.com", ValidPassword);
            var secondUser = await _factory.SeedUserAsync("seconduser", "seconduser@test.com", ValidPassword);
            var categoryId = await _factory.SeedCategoryAsync();
            var productId = await _factory.SeedProductAsync(categoryId, 1, 20);

            await _factory.SeedCartItemAsync(firstUser.UserId, productId, 1);

            var placeOrderDto = ValidOrderRequest();

            using var firstRequest = await SendOrderRequest(firstUser.AccessToken, placeOrderDto);

            Assert.AreEqual(HttpStatusCode.OK, firstRequest.StatusCode, await firstRequest.Content.ReadAsStringAsync(TestContext.CancellationToken));

            var order = await GetJsonData<OrderDetailsResponse>(firstRequest.Content);

            Assert.IsNotNull(order);

            using var firstResponse = await SendGetRequest(firstUser.AccessToken, $"api/order/me/{order.OrderId}");
            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            using var secondResponse = await SendGetRequest(secondUser.AccessToken, $"api/order/me/{order.OrderId}");
            Assert.AreEqual(HttpStatusCode.NotFound, secondResponse.StatusCode); 

        }

        [TestMethod]
        public async Task PlaceOrder_NoToken_ReturnsUnauthorized()
        {

            var placeOrderDto = ValidOrderRequest();

            using var response = await SendOrderRequest("", placeOrderDto);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync(TestContext.CancellationToken));
        }

        [TestMethod]
        public async Task GetAllOrders_AsCustomer_ReturnsForbidden()
        {
            var user = await _factory.SeedUserAsync("customer", "customer@test.com", ValidPassword);

            using var response = await SendGetRequest(user.AccessToken, $"api/order");
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync(TestContext.CancellationToken));
        }

        [TestMethod]
        public async Task PlaceOrder_RetiredProductInCart_ReturnsBadRequestAndNamesProduct()
        {
            var userData = await SeedTestData("newproductretired", "newproductretired@test.com", 1, 20);
            await _factory.SeedCartItemAsync(userData.UserId, userData.ProductId, 1);

            // Delete product
            await _factory.RetireProductAsync(userData.ProductId);


            // User place order
            var placeOrderDto = ValidOrderRequest();

            using var response = await SendOrderRequest(userData.AccessToken, placeOrderDto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, await response.Content.ReadAsStringAsync(TestContext.CancellationToken));

            var responseBody = await response.Content.ReadFromJsonAsync<ProblemDetails>(
                        cancellationToken: TestContext.CancellationToken);

            Assert.IsNotNull(responseBody);
            StringAssert.Contains(responseBody.Detail!, "no longer available");

            var product = await _factory.GetProductAsync(userData.ProductId);

            Assert.IsNotNull(product);
            Assert.IsTrue(product.IsDeleted);
            StringAssert.Contains(responseBody.Detail!, product.ProductName);

        }



        [TestMethod]
        public async Task PlaceOrder_TwoBuyersForLastUnit_ExactlyOneSucceeds()
        {
            for (int round = 1; round <= 20; round++)
            {

                var firstUser = await _factory.SeedUserAsync($"firstuser{round}", $"firstuser{round}@test.com", ValidPassword);
                var secondUser = await _factory.SeedUserAsync($"seconduser{round}", $"seconduser{round}@test.com", ValidPassword);
                var categoryId = await _factory.SeedCategoryAsync();
                var productId = await _factory.SeedProductAsync(categoryId, 1, 20);

                await _factory.SeedCartItemAsync(firstUser.UserId, productId, 1);
                await _factory.SeedCartItemAsync(secondUser.UserId, productId, 1);

                var placeOrderDto = ValidOrderRequest();

                var firstRequest = SendOrderRequest(firstUser.AccessToken, placeOrderDto);
                var secondRequest = SendOrderRequest(secondUser.AccessToken, placeOrderDto);

                var responses = await Task.WhenAll(firstRequest, secondRequest);


                var successCount = responses.Count(r => r.IsSuccessStatusCode);
                var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict || r.StatusCode == HttpStatusCode.BadRequest);

                Assert.AreEqual(1, successCount, $"Round {round}: Exactly one buyer should succeed.");
                Assert.AreEqual(1, conflictCount, $"Round {round}: The losing request should return 400 or 409. Responses: " +
                                                string.Join(" | ", await Task.WhenAll(
                                                    responses.Select(async r =>
                                                    $"{(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}"))));



                var product = await _factory.GetProductAsync(productId);

                Assert.IsNotNull(product);
                Assert.AreEqual(0, product.StockQuantity);

                var firstUserOrderCount = await _factory.GetOrderCountAsync(firstUser.UserId);
                var secondUserOrderCount = await _factory.GetOrderCountAsync(secondUser.UserId);

                Assert.AreEqual(1, firstUserOrderCount + secondUserOrderCount);

                var firstSucceeded = responses[0].IsSuccessStatusCode;
                var secondSucceeded = responses[1].IsSuccessStatusCode;

                Assert.AreNotEqual(firstSucceeded, secondSucceeded,
                    $"Round {round}: Exactly one buyer should successfully place an order.");

                var winner = firstSucceeded ? firstUser : secondUser;
                var loser = firstSucceeded ? secondUser : firstUser;

                var winnerCartCount = await _factory.GetCartItemCountAsync(winner.UserId);
                var loserCartCount = await _factory.GetCartItemCountAsync(loser.UserId);

                Assert.AreEqual(0, winnerCartCount, $"Round {round}: Winner's cart should be empty.");
                Assert.AreEqual(1, loserCartCount, $"Round {round}: Loser's cart should retain its item.");

            }
        }

        private async Task<HttpResponseMessage> SendOrderRequest(string accessToken, PlaceOrderRequest request)
        {
            using var client = _factory.CreateClient();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await client.PostAsJsonAsync("api/order", request);
        }

        private async Task<HttpResponseMessage> SendGetRequest(string accessToken, string url)
        {
            using var client = _factory.CreateClient();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await client.GetAsync(url);
        }

        private async Task<(int UserId, string AccessToken, int CategoryId, int ProductId)> SeedTestData(string username, string email, int stock, decimal price)
        {
            var user = await _factory.SeedUserAsync(username, email, ValidPassword);
            var categoryId = await _factory.SeedCategoryAsync();
            var productId = await _factory.SeedProductAsync(categoryId, stock, price);

            return (user.UserId, user.AccessToken, categoryId, productId);
        }

        private async Task<T?> GetJsonData<T>(HttpContent content)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());

            return await content.ReadFromJsonAsync<T>(
                options,
                cancellationToken: TestContext.CancellationToken);
        }

        private static PlaceOrderRequest ValidOrderRequest()
        {
            return new PlaceOrderRequest
            {
                ShippingFullName = "Abdul Nafih",
                ShippingPhone = "+919876543210",
                ShippingAddressLine = "123, Main Road, Kumbla",
                ShippingCity = "Kasaragod",
                ShippingState = "Kerala",
                ShippingPostalCode = "671321",
                PaymentMethod = PaymentMethod.CashOnDelivery
            };
        }

    }
}

