using System.Net;
using System.Net.Http.Json;
using AdditionApi.Models;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;



namespace AdditionApi.IntegrationTests;

public class AddEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AddEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SaveCalculation_ShouldReturnOk()
    {
        var calc = new Calculation
        {
            Operand1 = 5,
            Operand2 = 3,
            Operation = "+",
            Result = 8
        };

        var response = await _client.PostAsJsonAsync("/Storage/SaveCalculation", calc);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCalculations_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/Storage/GetCalculations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
