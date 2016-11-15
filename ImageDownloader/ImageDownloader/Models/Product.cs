using System.Collections.Generic;

namespace ImageDownloader.Models
{
    public class Product
    {
        public string Sku { get; set; }
        public List<Image> Images { get; set; }
    }
}