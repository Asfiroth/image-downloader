using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Syncfusion.XlsIO;

namespace ImageDownloader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            var spin = new ConsoleSpiner();
            Console.WriteLine("*****DOWNLOAD THEM ALL*****");
            Console.WriteLine("===========================");
            var products = new List<Product>();
            var path = string.Empty;
            var destination = string.Empty;
            while ((string.IsNullOrEmpty(path) || !File.Exists(path)) && string.IsNullOrEmpty(destination))
            {
                Console.WriteLine("Ingresa la ruta del archivo: ");
                path = Console.ReadLine();
                Console.WriteLine("Ingresa la ruta de destino: ");
                destination = Console.ReadLine();

                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Ingrese ruta de Archivo para descarga...");
                }

                if (string.IsNullOrEmpty(destination))
                {
                    Console.WriteLine("Ingrese ruta de carpeta para descarga...");
                }


                if (!string.IsNullOrEmpty(path) && !File.Exists(path))
                {
                    Console.WriteLine("EL ARCHIVO NO EXISTE...");
                }
                Console.WriteLine("===========================");
            }
            Console.WriteLine("===========================");
            Console.WriteLine("Iniciando Carga de datos...");
            using (var engine = new ExcelEngine())
            {
                var workbook = engine.Excel.Workbooks.Open(path);
                workbook.Version = ExcelVersion.Excel2013;

                var sheet = workbook.Worksheets[0];
                
                var count = 0;
                foreach (var row in sheet.Rows)
                {
                    spin.Turn();
                    count++;
                    if (count == 1) continue;
                    var product = new Product
                    {
                        Sku = row["A" + count].DisplayText,
                        Images = new List<Image>()
                    };
                    for (var number = 1; number < row.Columns.Length; number++)
                    {
                        var cell = row.Columns[number];
                        if (string.IsNullOrEmpty(cell.DisplayText)) continue;
                        product.Images.Add(new Image
                        {
                            ImageUrl = cell.DisplayText,
                            ImageName = (number == 1) ? $"{product.Sku}.jpg" : $"{product.Sku}__{number}.jpg"
                        });
                    }
                    products.Add(product);
                }
            }

            Console.WriteLine("Carga Finalizada...");
            Console.WriteLine("===========================");
            Console.WriteLine("Iniciando Descarga...");

            using (var logFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"), true))
            {
                foreach (var product in products)
                {
                    foreach (var image in product.Images)
                    {
                        try
                        {
                            Console.WriteLine(image.ImageUrl);
                            using (var client = new WebClient())
                            {
                                if (destination != null)
                                    client.DownloadFile(new Uri(image.ImageUrl, UriKind.Absolute),
                                        Path.Combine(destination, image.ImageName));
                            }
                        }
                        catch (Exception)
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine("==============================");
                            sb.AppendLine($"ERROR URL: {image.ImageUrl}");
                            sb.AppendLine($"SKU: {product.Sku}");
                            sb.AppendLine("==============================");
                            logFile.WriteLine(sb.ToString());
                        }
                    }
                }
                
            }
            Console.WriteLine("Descarga Finalizada...");
            Console.WriteLine("===========================");
            Console.WriteLine("Presione una tecla para salir...");
            Console.ReadKey();
        }
    }

    public class ConsoleSpiner
    {
        private int _counter;
        public ConsoleSpiner()
        {
            _counter = 0;
        }
        public void Turn()
        {
            _counter++;
            switch (_counter % 4)
            {
                case 0: Console.Write("/"); break;
                case 1: Console.Write("-"); break;
                case 2: Console.Write("\\"); break;
                case 3: Console.Write("|"); break;
                default: throw new ArgumentOutOfRangeException(nameof(_counter), _counter, "No counter found");

            }
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
    }

    public class Product
    {
        public string Sku { get; set; }
        public List<Image> Images { get; set; }
    }

    public class Image
    {
        public string ImageUrl { get; set; }
        public string ImageName { get; set; }
    }

}
