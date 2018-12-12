using LiBCAT;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace BCATDownloader
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: BCATDownloader.exe <Title ID> <Passphrase>");
                return;
            }

            var TitleID = Convert.ToUInt64(args[0], 16);
            var Dirs = (JArray)JObject.Parse(Bcat.Data.GetNxData(TitleID, args[1]))["directories"];

            using (var MD5 = new MD5CryptoServiceProvider())
            using (var Cli = new WebClient())
            {
                string Hash(byte[] In) => BitConverter.ToString(MD5.ComputeHash(In)).Replace("-", string.Empty).ToLower();

                for (int i = 0; i < Dirs.Count; i++)
                {
                    var DirName = Dirs[i]["name"];
                    var NewDir = $"{TitleID:x16}/{DirName}";
                    Directory.CreateDirectory(NewDir);
                    var DataList = (JArray)Dirs[i]["data_list"];

                    for (int j = 0; j < DataList.Count; j++)
                    {
                        var JSON = DataList[j];
                        var Filename = JSON["filename"].ToString();
                        Console.WriteLine("Downloading {0}/{1}...", DirName, Filename);
                        var Data = (byte[])Bcat.GetData(JSON["url"].ToString(), false, TitleID, args[1]);
                        if (Hash(Data) != JSON["digest"].ToString()) throw new Exception("Error: Bad data!");
                        File.WriteAllBytes($"{NewDir}/{Filename}", Data);
                    }
                }
            }

            Console.WriteLine("\nDone!");
        }
    }
}