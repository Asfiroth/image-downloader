using System;
using System.Collections;
using System.Collections.Generic;
using ImageDownloader.Models;

namespace ImageDownloader.Services.Contract
{
    public interface IImageHandler
    {
        IList<Product> LoadOnMemory(string filePath);
        void DownloadImages(IEnumerable<Product> products, string destinationPath);
    }
}