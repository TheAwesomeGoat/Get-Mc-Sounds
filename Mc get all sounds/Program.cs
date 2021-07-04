using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mc_get_all_sounds
{
    class Program
    {
        struct SoundData
        {
            public string SoundPath;
            public string Hash;
            public int Size;
        }

        static string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static ConsoleColor DefaultC = Console.ForegroundColor;

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            string[] Versions = Directory.GetFiles($@"{Appdata}\.minecraft\assets\indexes\");
            foreach (var item in Versions)
                Console.WriteLine($"[   ] - {item}");

            int Index = 0;
            int LastIndex = 0;
            while (true)
            {
                Console.SetCursorPosition(2, Index);
                Console.Write("#");

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
                        SoundPath = path,
                        Hash = SoundItem.hash,
                        Size = SoundItem.size
                    });
                    Console.WriteLine(path);
                }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            string SoundObject = "";
            foreach (var soundData in soundDataList)
            {
                Console.Write($"Writing Out: {soundData.SoundPath} - {soundData.Size}kb, ");
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
                    string[] split = soundData.SoundPath.Split('/');

                    string Filen = split.Last();
                    string pth = string.Join(@"\", split.Take(split.Count() - 1));

                    Directory.CreateDirectory(VersionFolder + @"\" + pth);
                    bool exists = File.Exists($@"{VersionFolder}\{pth}\{Filen}");
                    if (!exists)
                    {
                        File.Copy(SoundObject, $@"{VersionFolder}\{pth}\{Filen}");
                        PrintColor("Done.", ConsoleColor.Green);
                    }
                    else
                        PrintColor("Exists.", ConsoleColor.DarkYellow);
                }
                else
                    PrintColor("Hash Not Found.", ConsoleColor.Red);

            }
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("\nDone.");
            Console.WriteLine($"Elapsed Time: {elapsedTime}");
            Console.ReadKey();
        }
        static void PrintColor(string msg, ConsoleColor col)
        {
            Console.ForegroundColor = col;
            Console.WriteLine(msg);
            Console.ForegroundColor = DefaultC;
        }
    }
}
