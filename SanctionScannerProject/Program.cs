using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
// Manage nuget paket yöneticisinden eklemeliyiz.
using HtmlAgilityPack;

namespace SanctionScannerProject
{

    // Advert isimli class'ımızı oluşturup , özelliklerini belirttik.
    class Advert
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Brand { get; set; }
        public Uri Link { get; set; }

    }

    class Program
    {
        // Verilen uzantının html içeriğini getiren fonksiyon
        public static string GetPagesContent(string urlAdress)
        {

            // Siteye bağlantı talebi istiyoruz.
            WebRequest request = HttpWebRequest.Create(new Uri(urlAdress));

            // Siteden gelen cevabı alıyoruz.
            WebResponse response = request.GetResponse();


            // Gelen cevabı okuyoruz.
            StreamReader incomingMessage = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

            // Gelen cevabı, string' dönüştürüyoruz.
            string pageHtmlCode = incomingMessage.ReadToEnd();

            // Akışı kapatıyoruz.
            response.Close();

            // Sayfa içeriğini string olarak döndürüyoruz.
            return pageHtmlCode;
        }

        // document'e dönüşüm yapan fonksiyon
        public static HtmlDocument HtmlDocumentConvert(string pageHtmlCode)
        {

            // string'e çevirdiğimiz kodları incelemek için Html Agility Pack sınıfından nesne oluşturuyoruz.
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();

            // string içindeki html kodlarını document nesnesine yüklüyoruz.
            document.LoadHtml(pageHtmlCode);

            return document;
        }

        // Ana vitrindeki ilanların özelliklerini getiren fonksiyon
        public static void GetAllAdverts(string domainAdress, List<Advert> adverts)
        {
            // Verilen uzantının ait olduğu sayfanın html kodlarını alıyoruz.
            var mainContent = GetPagesContent(domainAdress);

            // string halindeki html kodlarını, document'çeviriyoruz.
            var mainDocument = HtmlDocumentConvert(mainContent);

            // İncelenmek istenen etiketin yolunu vererek etikete ulaşıyoruz.
            HtmlNode mainNodes = mainDocument.DocumentNode.SelectSingleNode("//*[@id='container']/div[3]/div/div[3]/div[3]/ul");


            // ul etiketi içindeki li etiketleri içinde dönüyoruz.
            foreach (var node in mainNodes.SelectNodes("li"))
            {
                // Her li etiketi içindeki a etiketine ulaşarak kontrol sağlıyoruz.
                if (node.SelectSingleNode("a") != null)
                {
                    // null olmama durumunda a etiketi içindeki bağlantıyı alıyoruz.
                    var advertAdress = node.SelectSingleNode("a").GetAttributeValue("href", null);

                    // İlanın bağlantısının başına domain bağlantısını ekliyoruz.
                    var newAdress = domainAdress + advertAdress;


                    Console.WriteLine("Veriler çekiliyor!!!");
                    // 429 too many requests hatası almamak için 2 sn uyutuyoruz.
                    Thread.Sleep(2000);

                    // ilanın bağlantısı verilerek , detay sayfasındaki html kodunu alıyoruz.
                    var advertContent = GetPagesContent(newAdress);
                    // Document'e çevrme işlemi yapıyoruz.
                    var advertDocument = HtmlDocumentConvert(advertContent);

                    // İlan nesnesi oluşturup özelliklerini atıyoruz.
                    var advert = new Advert();
                    advert.Id = Convert.ToInt32(advertDocument.DocumentNode.SelectSingleNode("//*[@id='classifiedId']").InnerText);
                    advert.Title = advertDocument.DocumentNode.SelectSingleNode("//*[@id='classifiedDetail']/div/div[1]/h1").InnerText;
                    advert.Brand = advertDocument.DocumentNode.SelectSingleNode("//*[@id='classifiedDetail']/div/div[2]/div[2]/ul/li[3]/span").InnerText;
                    advert.Link = new Uri(newAdress);

                    //Adverts tipinden oluşturduğumuz listeye ekliyoruz.
                    adverts.Add(advert);
                }
            }


        }

        // internet bağlantımızı kontrol eden fonksiyon
        public static bool InternetConnection()
        {

            try
            {
                // Verilen linke ping atarak internete bağlı olup olmadığımızı kontrol ediyoruz.
                System.Net.Sockets.TcpClient kontrol_client = new System.Net.Sockets.TcpClient("www.google.com.tr", 80);
                kontrol_client.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("lütfen internet bağlantınızı kontrol ediniz!");
                return false;
            }
        }

        public static void Main(string[] args)
        {

            // İnternet bağlantımızı kontrol ediyoruz.
            if (InternetConnection())
            {
                string domain = "https://www.sahibinden.com";

                List<Advert> adverts = new List<Advert>();

                GetAllAdverts(domain, adverts);

                var counter = 1;
                foreach (var advert in adverts)
                {

                    Console.WriteLine("{0}. İlan Bilgileri\n", counter);
                    Console.WriteLine("İlanın No : {0}\n", advert.Id);
                    Console.WriteLine("İlan Başlığı : {0}\n", advert.Title);
                    Console.WriteLine("İlan Markası : {0}\n", advert.Brand);
                    Console.WriteLine("İlan linki : {0}\n", advert.Link);
                    Console.WriteLine("-------------------------------------------");

                    counter++;
                }
            }

            Console.ReadLine();
        }
    }
}
