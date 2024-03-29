﻿using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatApp.Web.Dtos;
using ChatApp.Web.Service.Profiles;
using ChatApp.Web.Service.Images;


namespace ChatApp.Web.Tests.Controllers;

public class ImageControllerTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IImageService> _imageStoreMock = new();
    private readonly HttpClient _httpClient;
    
    public ImageControllerTests(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_imageStoreMock.Object); });
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

        var postResponse = await _httpClient.PostAsync("api/Image", formData);
        // Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
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

        var response = await _httpClient.PostAsync("api/Image", formData);
        // Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var response2 = await _httpClient.PostAsync("api/Image", null);
        // Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        var response3 = await _httpClient.GetAsync("api/Image{}");
        // Assert.Equal(HttpStatusCode.NotFound,response3.StatusCode);
    }
}