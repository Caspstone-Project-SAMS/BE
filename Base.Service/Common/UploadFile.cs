using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Common;

public interface IUploadFile
{
    Task<RawUploadResult> UploadImage(IFormFile image);
}

internal class UploadFile : IUploadFile
{
    private readonly Cloudinary _cloudinary;
    public UploadFile(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<RawUploadResult> UploadImage(IFormFile image)
    {
        var uploadFile = new RawUploadParams
        {
            File = new FileDescription(image.FileName, image.OpenReadStream())
        };

        return await _cloudinary.UploadAsync(uploadFile);
    }
}
