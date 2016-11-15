namespace ImageDownloader.Models
{
    public class Image
    {
        public string ImageUrl { get; set; }
        public string ImageName { get; set; }
        public bool IsLoaded { get; set; }
        public bool HasError { get; set; }
    }
}