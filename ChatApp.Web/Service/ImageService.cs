﻿using ChatApp.Web.Dtos;
using ChatApp.Web.Storage;

namespace ChatApp.Web.Service;

public class ImageService : IImageService
{
    private readonly IImageService _imageStore;
    public ImageService(IImageService imageStore)
    {
        _imageStore = imageStore;
    }
    public Task<UploadImageResponse?> UpsertProfilePicture(UploadImageRequest picture)
    {
        return _imageStore.UpsertProfilePicture(picture);
    }

    public Task<byte[]?> GetProfilePicture(string id)
    {
        return _imageStore.GetProfilePicture(id);
    }

    public Task DeleteProfilePicture(string id)
    {
        return _imageStore.DeleteProfilePicture(id);
    }
}