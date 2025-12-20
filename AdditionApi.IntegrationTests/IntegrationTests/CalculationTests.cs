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
            var request = new AddRequest { A = num1, B = num2 };

            var response = await _client.PostAsJsonAsync("/api/addition", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var savedCalculation = await response.Content.ReadFromJsonAsync<Calculation>();

            savedCalculation.Should().NotBeNull();
            savedCalculation!.Result.Should().Be(30);
            savedCalculation.Operand1.Should().Be(num1);
            savedCalculation.Operand2.Should().Be(num2);
            savedCalculation.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Get_ShouldReturnAllCalculations()
        {
            var request = new AddRequest { A = 5, B = 5 };
            await _client.PostAsJsonAsync("/api/addition", request);

            var response = await _client.GetAsync("/api/addition");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var calculations = await response.Content.ReadFromJsonAsync<List<Calculation>>();

            calculations.Should().NotBeNull();
            calculations.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Add_ShouldReturnBadRequest_WhenInputIsInvalid()
        {
            var content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/addition", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}