using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using AdditionApi.Models;

namespace AdditionApi.IntegrationTests
{
    public class CalculationTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly HttpClient _client;

        public CalculationTests(IntegrationTestWebAppFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Add_ShouldReturnCorrectSum_AndSaveToDatabase()
        {
            int num1 = 10;
            int num2 = 20;

            var response = await _client.PostAsync($"/api/addition?a={num1}&b={num2}", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var savedCalculation = await response.Content.ReadFromJsonAsync<Calculation>();

            savedCalculation.Should().NotBeNull();
            savedCalculation!.Result.Should().Be(30);
            savedCalculation.Operand1.Should().Be(num1);
            savedCalculation.Operand2.Should().Be(num2);
            savedCalculation.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Add_ShouldReturnBadRequest_WhenInputIsInvalid()
        {
            string invalidUrl = "/api/addition?a=abc&b=20";
            var response = await _client.PostAsync(invalidUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}