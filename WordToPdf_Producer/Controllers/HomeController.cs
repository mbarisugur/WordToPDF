using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using WordToPdf_Producer.Models;

namespace WordToPdf_Producer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult WordToPdf()
        {
            ViewBag.result = "";
            return View();
        }

        [HttpPost]
        public IActionResult WordToPdf(WordToPdfFile wordToPdfFile)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = new Uri(_configuration["ConnectionString:RabbitMQCloudString"])
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("Convert_Exchange", ExchangeType.Direct, true, false, null);

                    channel.QueueDeclare(queue: "File", durable: true, exclusive: false, autoDelete: false, arguments: null);

                    channel.QueueBind("File", "Convert_Exchange", "WordToPdf");
                    MessageWordToPdf messageWordToPdf = new MessageWordToPdf();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        wordToPdfFile.File.CopyTo(ms);
                        messageWordToPdf.WordByte = ms.ToArray();
                    }
                    messageWordToPdf.Email = wordToPdfFile.Email;
                    messageWordToPdf.FileName = Path.GetFileNameWithoutExtension(wordToPdfFile.File.FileName);

                    string serializeMessage = JsonConvert.SerializeObject(messageWordToPdf);

                    byte[] ByteMessage = Encoding.UTF8.GetBytes(serializeMessage);

                    var properties = channel.CreateBasicProperties();

                    properties.Persistent = true;

                    channel.BasicPublish("Convert_Exchange", routingKey: "WordToPdf", basicProperties: properties, ByteMessage);
                }
            }
            ViewBag.result = "Dönüştürme işlemi başarılı bir şekilde gerçekleşmiştir. Yine bekleriz.";
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
