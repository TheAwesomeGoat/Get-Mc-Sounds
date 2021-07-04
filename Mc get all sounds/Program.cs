using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mc_get_all_sounds
{
    class Program
    {
        struct SoundData
        {
            public string Filename;
            public string Hash;
            public int Size;
        }

        static string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static ConsoleColor DefaultC = Console.ForegroundColor;

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            string[] Versions = Directory.GetFiles($@"{Appdata}\.minecraft\assets\indexes\");

            int Index = 0;
            int LastIndex = 0;

            foreach (var item in Versions)
                Console.WriteLine($"[   ] - {item}");

            Console.SetCursorPosition(2, 0);
            Console.Write("#");

            while (true)
            {
                ConsoleKey key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.Enter:
                        Extract(Versions[Index]);
                        return;
                    case ConsoleKey.Escape:
                        return;
                    case ConsoleKey.UpArrow:
                        Index--;
                        break;
                    case ConsoleKey.DownArrow:
                        Index++;
                        break;
                }

                if (Index > Versions.Count()-1)
                    Index = 0;
                if (Index < 0)
                    Index = Versions.Count() - 1;

                Console.SetCursorPosition(2, Index);
                Console.Write("#");
                Console.SetCursorPosition(2, LastIndex);
                Console.Write(" ");

                LastIndex = Index;
            }
        }

        static void Extract(string Version)
        {
            string VersionFolder = Path.GetFileName(Version).Replace(".json", "");
            dynamic data = JObject.Parse(File.ReadAllText(Version));
            string[] SoundFiles = Directory.GetFiles($@"{Appdata}\.minecraft\assets\objects\", "*.*", SearchOption.AllDirectories);

            JObject sounds = data.objects;

            List<SoundData> soundDataList = new List<SoundData>();

            foreach (var item in sounds.Children())
            {
                if (item.Path.Contains("minecraft/sounds/"))
                {
                    string path = item.Path.Replace("objects['", "").Replace("']", ""); //objects['minecraft/sounds/liquid/heavy_splash.ogg']
                    dynamic SoundItem = item.First;
                    soundDataList.Add(new SoundData
                    {
                        Filename = path,
                        Hash = SoundItem.hash,
                        Size = SoundItem.size
                    });
                    Console.WriteLine(path);
                }
            }
            string SoundObject = "";
            foreach (var soundData in soundDataList)
            {
                Console.Write($"Writing Out: {soundData.Filename} - {soundData.Size}kb ");
                foreach (var item in SoundFiles)
                {
                    if (item.Contains(soundData.Hash))
                    {
                        SoundObject = item;
                        break;
                    }
                }
                if (File.Exists(SoundObject))
                {
                    byte[] soundBytes = File.ReadAllBytes(SoundObject);

                    string[] split = soundData.Filename.Split('/');

                    string Filen = split.Last();
                    string pth = string.Join(@"\", split.Take(split.Count() - 1));

                    Directory.CreateDirectory(VersionFolder + @"\" + pth);
                    bool exists = File.Exists($@"{VersionFolder}\{pth}\{Filen}");
                    if (!exists)
                    {
                        File.WriteAllBytes($@"{VersionFolder}\{pth}\{Filen}", soundBytes);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Done.");
                        Console.ForegroundColor = DefaultC;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("Exists.");
                        Console.ForegroundColor = DefaultC;
                    }
                }
            }
            Console.WriteLine("\nDone.");
            Console.ReadKey();
        }
    }
}
