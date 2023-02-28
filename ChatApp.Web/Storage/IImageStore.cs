﻿using ChatApp.Web.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Storage;

public interface IImageStore
{
    Task<UploadImageResponse?> UpsertProfilePicture(UploadImageRequest picture);
    Task<byte[]?> GetProfilePicture(string id);
    Task DeleteProfilePicture(string id);
}