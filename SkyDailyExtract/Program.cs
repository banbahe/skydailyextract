using Newtonsoft.Json.Linq;
using SkyDailyExtract.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SkyDailyExtract
{
    public class Program
    {
        private static Dictionary<string, string> dictionary = new Dictionary<string, string>()
        {
            { "QA" , "Basic YThhM2VjMmIzNjQxNTkzYTM5MjVlNjQ5MGJjMjg0ZGFlNjhmMDk5MzZAc2t5LW14Mi50ZXN0OjIyOTc5NTkwZjI0OTEwYTY4NDgwZjQ4MzcyYzFhOGE4NmVlNWM3OTdjMjI5Y2U5MjE1ZmFkZmNlN2FhZjE1YzY=" },
            { "DEV" , "Basic YThhM2VjMmIzNjQxNTkzYTM5MjVlNjQ5MGJjMjg0ZGFlNjhmMDk5MzZAc2t5LW14Mi50ZXN0OjIyOTc5NTkwZjI0OTEwYTY4NDgwZjQ4MzcyYzFhOGE4NmVlNWM3OTdjMjI5Y2U5MjE1ZmFkZmNlN2FhZjE1YzY=" },
            { "PROD", "Basic YThhM2VjMmIzNjQxNTkzYTM5MjVlNjQ5MGJjMjg0ZGFlNjhmMDk5MzZAc2t5LW14OjAwZTVlZTRkNmI2ODc0NTQ0NDI3ZDZlMGM3ZDg1YWY4ZDdiNDQzM2E4MzE1ZGQ3NzllYWEzM2QyYmM4NGY0NDk=" }
        };

        private static string Authorization { get; set; }
        private static int attemp = 0;
        private static string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal, Environment.SpecialFolderOption.Create);

        static void Main(string[] args)
        {
            Console.WriteLine(" ======================================================== ");
            Console.WriteLine("Daily Extract");
            Console.WriteLine("Elija Esquema y/o Instancia");
            Console.WriteLine("1 sky-mx1.test QA");
            Console.WriteLine("2 sky-mx2.test DEV");
            Console.WriteLine("3 sky-mx PRODUCCIÓN");
            Console.WriteLine(" ======================================================== ");

            int defaultConfig = 0;
            string res = Console.ReadLine();

            int.TryParse(res, out defaultConfig);

            switch (defaultConfig)
            {
                case 1:
                    Authorization = dictionary["QA"];
                    break;
                case 2:
                    Authorization = dictionary["DEV"];
                    break;
                case 3:
                    Authorization = dictionary["PROD"];
                    break;
                default:
                    Console.WriteLine("Ingrese opción valida");
                    Thread.Sleep(1700);
                    throw new Exception();
            }

            Console.WriteLine("¿ Cuantos dias se van a extraer, anterior al dia " + DateTime.Now.ToString("dd/MM/yyyy") + " ?, de la opción " + defaultConfig);
            Console.WriteLine("Ingrese solo números y que no sea mayor a 90 , si ingresas 0 extraera todo)");
            res = string.Empty;
            res = Console.ReadLine();
            int dayBefore = 0;
            int.TryParse(res, out dayBefore);

            // dayBefore = defaultConfig == 3 ? 0 : dayBefore;
            if (dayBefore >= 900)
            {
                Console.WriteLine(" Esto no es número correcto");
                Thread.Sleep(3800);
                throw new Exception();
            };

            Console.WriteLine(" En hora buena proceso iniciado ");
            List<OFSCModel> result = new List<OFSCModel>();
            var vGetFolders = GetFolders(dayBefore);

            int total = vGetFolders.Count();
            int indice = 1;
            int indiceDownload = 1;
            // int indiceTotal = 0;

            foreach (var item in vGetFolders)
            {
              
                Console.WriteLine(" Obteniendo información del folder " + indice + " de " + total);
                var vGetFilesPerFolder = GetFilesPerFolder(item, Authorization);
                if (vGetFilesPerFolder == null)
                {
                    Console.WriteLine(item.name + " item omitido No contiene información");
                    continue;
                }
                result.Add(vGetFilesPerFolder);
                indice++;
            }

            Console.Clear();
            indice = 1;
            total = result.Count();
            foreach (var item in result)
            {
                string pathTmp = CreateFolder(defaultConfig, item.name);
                Console.Clear();
                Console.WriteLine(" Carpeta " + item.name + " " + indice + " de " + total);
                Console.WriteLine(" Contiene " + item.FilesModel.Count() + " archivos a descargar");
                indiceDownload = 1;
                foreach (var itemFile in item.FilesModel)
                {
                    //if (itemFile.nameFile != "provider")
                    {
                        try
                        {
                            Console.WriteLine(" Descargando " + itemFile.nameFile + " " + indiceDownload + " De " + item.FilesModel.Count());
                            DownloadFile(itemFile.hrefFile, string.Concat(@pathTmp, @"\", itemFile.nameFile), itemFile.mediaType);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error" + ex.Message);
                        }
                        indiceDownload++;
                    }
                }
            }
            Console.Clear();
            Console.WriteLine("Termino");
            Console.ReadLine();
        }

        private static List<OFSCModel> GetFolders(int daybefore)
        {
            List<OFSCModel> ListOFSC = new List<OFSCModel>();

            for (int i = 0; i <= daybefore; i++)
            {
                DateTime dateTimeCurrent = DateTime.Now;
                OFSCModel oFSCModel = new OFSCModel();
                dateTimeCurrent = dateTimeCurrent.AddDays(-i);
                oFSCModel.name = dateTimeCurrent.ToString("yyyy-MM-dd");
                oFSCModel.href = "https://api.etadirect.com/rest/ofscCore/v1/folders/dailyExtract/folders/" + dateTimeCurrent.ToString("yyyy-MM-dd");
                ListOFSC.Add(oFSCModel);
            }

            Console.WriteLine(ListOFSC.Count() + " total de folders a extraer informacion");
            return ListOFSC;
        }

        //private static List<OFSCModel> GetFolders(int daybefore)
        //{
        //    List<OFSCModel> ListOFSC = new List<OFSCModel>();
        //    // string WEBSERVICE_URL = "https://api.etadirect.com/rest/ofscCore/v1/folders/dailyExtract/folders";
        //    // string result = WebRequestLocal(WEBSERVICE_URL);
        //    ResponseOFSC result = UtilWebRequest.SendWayAsync("rest/ofscCore/v1/folders/dailyExtract/folders", enumMethod.GET, string.Empty, Authorization);

        //    if (string.IsNullOrEmpty(result.Content))
        //        return new List<OFSCModel>();

        //    JObject o = JObject.Parse(result.Content);
        //    var aitems = o["folders"]["items"];

        //    foreach (var item in aitems)
        //    {

        //        OFSCModel oFSCModel = new OFSCModel();

        //        if (daybefore == 0)
        //        {
        //            oFSCModel.name = item["name"].ToString();
        //            oFSCModel.href = item["links"][0]["href"].ToString();
        //            ListOFSC.Add(oFSCModel);
        //        }
        //        else
        //        {
        //            DateTime dateTimeCurrent = DateTime.Now;
        //            dateTimeCurrent = dateTimeCurrent.AddDays(-daybefore);
        //            //if (item["name"].ToString().Substring(0,7) == "2019-02")
        //            //{
        //            //    Console.WriteLine("test");
        //            //}

        //            if (item["name"].ToString() == dateTimeCurrent.ToString("yyyy-MM-dd"))
        //            {
        //                oFSCModel.name = item["name"].ToString();
        //                oFSCModel.href = item["links"][0]["href"].ToString();
        //                ListOFSC.Add(oFSCModel);
        //            }
        //            daybefore = daybefore - 1;

        //        }
        //    }
        //    Console.WriteLine(ListOFSC.Count() + " total de folders a extraer informacion");
        //    return ListOFSC;
        //}

        private static OFSCModel GetFilesPerFolder(OFSCModel OFSCModel, string token)
        {
            OFSCModel result = new OFSCModel();
            string WEBSERVICE_URL = string.Concat("rest/ofscCore/v1/folders/dailyExtract/folders/", OFSCModel.name, "/files");

            ResponseOFSC response = UtilWebRequest.SendWayAsync(WEBSERVICE_URL, enumMethod.GET, string.Empty, token);
            if (string.IsNullOrEmpty(response.Content) || (int)response.statusCode >= 400)
                return null;

            JObject o = JObject.Parse(response.Content);
            var aitems = o["files"]["items"];

            OFSCModel.FilesModel = new List<FileModel>();
            if (aitems != null)
            {
                foreach (var item in aitems)
                {
                    FileModel FileModel = new FileModel();
                    FileModel.nameFile = item["name"].ToString();
                    FileModel.mediaType = item["mediaType"].ToString();
                    FileModel.hrefFile = item["links"][0]["href"].ToString();
                    FileModel.bytes = item["bytes"].ToString();
                    OFSCModel.FilesModel.Add(FileModel);
                }
            }
            return OFSCModel;
        }

        private static string CreateFolder(int option, string subFolder)
        {
            string pathfull = string.Empty;
            try
            {
                switch (option)
                {
                    case 1:
                        pathfull = Path.Combine(path, "assetsskymx1", subFolder);

                        break;
                    case 2:
                        pathfull = Path.Combine(path, "assetsskymx2", subFolder);

                        break;
                    case 3:
                        pathfull = Path.Combine(path, "assetsskymx", subFolder);
                        break;
                }
                pathfull = pathfull.Replace(@"C:\", @"D:\");
                bool exists = Directory.Exists(pathfull);

                if (!exists)
                    System.IO.Directory.CreateDirectory(pathfull);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return pathfull;
        }

        private static void DownloadFile(string url, string pathfull, string contentType)
        {
            if (!File.Exists(@pathfull))
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Set("Authorization", Authorization);
                    webClient.Headers.Set("Content-Type", contentType);
                    webClient.DownloadFile(url, pathfull);
                }
            }
        }

        private static string WebRequestLocalsdad(string webserviceurl)
        {
            // string WEBSERVICE_URL = webserviceurl;
            string result = string.Empty;
            var webRequest = System.Net.WebRequest.Create(webserviceurl);
            try
            {
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 18000;
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("Authorization", Authorization);

                    using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                    {
                        //  total = s.Length;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (((System.Net.HttpWebResponse)((System.Net.WebException)ex).Response).StatusCode == HttpStatusCode.NotFound)
                    return string.Empty;
                //if (ex.Message == "The operation has timed out")
                attemp++;
                //   return WebRequestLocal(webserviceurl);
                throw;
            }
        }
    }
}
