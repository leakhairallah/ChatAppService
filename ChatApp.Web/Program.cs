using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using ChatApp.Web.Configuration;
using ChatApp.Web.Storage;
using ChatApp.Web.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Configuration
builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<BlobSettings>(builder.Configuration.GetSection("Blob"));

// Add Services
builder.Services.AddSingleton<IProfileStore, ProfileStore>();
builder.Services.AddSingleton<IImageStore, ImageStore>();
builder.Services.AddSingleton<IConversationStore, ConversationStore>();
builder.Services.AddSingleton<IMessageStore, MessageStore>();
builder.Services.AddSingleton(sp =>
{
    var cosmosOptions = sp.GetRequiredService<IOptions<CosmosSettings>>();
    return new CosmosClient(cosmosOptions.Value.ConnectionString);
});
builder.Services.AddSingleton(sp =>
{
    var blobOptions = sp.GetRequiredService<IOptions<BlobSettings>>();
    return new BlobContainerClient(blobOptions.Value.ConnectionString, "profileimages");
});

builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddSingleton<IProfileService, ProfileService>();
builder.Services.AddSingleton<IMessageService, MessageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat App");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }