
using Microsoft.AspNetCore.Mvc.Testing;
using ApiUnitTests;
using Microsoft.EntityFrameworkCore.Storage;
using System.Net;
using System.Net.Http.Json;

public class ItemTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{



    [Fact]
    public async Task GivenNotExistItem_WhenGettingItem_ThenItemNotFound()
    {

        //Arrange
        var client = fixture.WebApplicationFactory.CreateClient();

        //Act
        var response = await client.GetAsync("/items/1");

        //Assert

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Given_Ivalid_Item_WhenPostingItem_ThenBadRequest()
    {

        //Arrange
        var client = fixture.WebApplicationFactory.CreateClient();
        int invalidItem = -1;


        //Act

        var response = await client.PostAsJsonAsync("/items", new Item(1, "Item1", invalidItem));

        //Assert

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenValidItem_WhenPostingItem_ThenCreated()
    {
        // Arrange
        var client = fixture.WebApplicationFactory.CreateClient();
        var newItem = new Item("Item1", 10);

        // Act
        var response = await client.PostAsJsonAsync("/items", newItem);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Read the created item from the response
        var createdItem = await response.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(createdItem);
        Assert.True(createdItem.Id > 0);

        // Fetch it back from the API
        var getResponse = await client.GetAsync($"/items/{createdItem.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var returnedItem = await getResponse.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(returnedItem);

        // Validate fields
        Assert.Equal(createdItem.Id, returnedItem.Id);
        Assert.Equal(newItem.Key, returnedItem.Key);
        Assert.Equal(newItem.Value, returnedItem.Value);
    }




}