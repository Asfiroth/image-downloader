using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ImageDownloader.Models;
using ImageDownloader.Services.Contract;
using Syncfusion.XlsIO;

namespace ImageDownloader.Services.Implementation
{
    public class ImageHandler : IImageHandler
    {
        public IList<Product> LoadOnMemory(string filePath)
        {
            using (var engine = new ExcelEngine())
            {
                var workbook = engine.Excel.Workbooks.Open(filePath);
                workbook.Version = ExcelVersion.Excel2013;

                var sheet = workbook.Worksheets[0];

                var products = sheet.Rows.Skip(1).Where(cell => !string.IsNullOrEmpty(cell.DisplayText)).Select(row =>
                {
                    var product = new Product
                    {
                        Sku = row.DisplayText,
                        Images = row.Columns.Where(cell => !string.IsNullOrEmpty(cell.DisplayText)).Skip(1).Select((cell, position) =>
                        {
                            var image = new Image
                            {
                                ImageName = position == 0 ? $"{row.DisplayText}.jpg" : $"{row.DisplayText}__{position}.jpg",
                                ImageUrl = cell.DisplayText
                            };
                            return image;
                        }).ToList()
                    };
                    return product;
                });
                return products.ToList();
            }
        }

        public void DownloadImages(IEnumerable<Product> products, string destinationPath)
        {
            using(var logFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"), true))
            {
                foreach (var product in products)
                {
                    foreach (var image in product.Images)
                    {
                        try
                        {
                            using (var client = new WebClient())
                            {
                                if (destinationPath != null)
                                    client.DownloadFile(new Uri(image.ImageUrl, UriKind.Absolute),
                                        Path.Combine(destinationPath, image.ImageName));
                            }
                            image.IsLoaded = true;
                        }
                        catch (Exception)
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine("==============================");
                            sb.AppendLine($"ERROR URL: {image.ImageUrl}");
                            sb.AppendLine($"SKU: {product.Sku}");
                            sb.AppendLine("==============================");
                            logFile.WriteLine(sb.ToString());
                            image.HasError = true;
                        }
                    }
                }
            }
        }
    }
}