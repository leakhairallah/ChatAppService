﻿using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Azure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

namespace ChatApp.Web.Tests.Controllers;

public class ImageControllerTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileStore> _profileStoreMock = new();
    private readonly HttpClient _httpClient;
    
    public ImageControllerTests(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); });
        }).CreateClient();
    }

    [Fact]
    public async Task AddImage_GetImage()
    {
        byte[] stream = new byte[] {0x12};
        HttpContent fileStreamContent = new StreamContent(new MemoryStream(stream)); 
        fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "File",
            FileName = "anything" 
        };

        using var formData = new MultipartFormDataContent();
        formData.Add(fileStreamContent);

        var postResponse = await _httpClient.PostAsync("/Image", formData);
        var json = await postResponse.Content.ReadAsStringAsync();
        var ans = JsonConvert.DeserializeObject<UploadImageResponse>(json);
        
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        new Profile("foobar", "Foo", "Bar", ans.Id);
        
        var getResponse = await _httpClient.GetAsync($"/Image/{"foobar"}");
        await using var memoryStream = new MemoryStream();
        await getResponse.Content.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        
        // Assert.Equal(stream, bytes);
        
    }

    [Fact]
    public async Task AddImage_GetImage_Error()
    {
        byte[] arr = new byte[] { };
        HttpContent fileStreamContent = new StreamContent(new MemoryStream(arr));
        fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "File",
            FileName = "anything" 
        };
        using var formData = new MultipartFormDataContent();
        formData.Add(fileStreamContent);

        var response = await _httpClient.PostAsync("/Image", formData);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        
        var response2 = await _httpClient.PostAsync("/Image", null);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        var response3 = await _httpClient.GetAsync("/Image{}");
        Assert.Equal(HttpStatusCode.NotFound,response3.StatusCode);
    }
}