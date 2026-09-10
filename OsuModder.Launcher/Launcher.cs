using OsuModder.Core;
using OsuModder.Core.Comparator;
using OsuModder.Core.Patch;
using Mono.Cecil;
using System.Diagnostics;

namespace OsuModder.Launcher
{
    class Program
    {
        public static string version = "0.0.1";

        static void Main(string[] args)
        {

            Console.WriteLine("OsuModder | Version: " + version);
            string RunningOS = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            string osuBin;
            string osuMods;
            
            if (RunningOS.ToLower().Contains("linux"))
            {
                osuBin = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "", "Documents", "APPS", "squashfs-root", "usr", "bin");
                osuMods = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "", ".local", "share", "osu", "mods");
            
            }else if (RunningOS.ToLower().Contains("windows"))
            {
                osuBin = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "", "osulazer", "current");
                osuMods = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "", "osulazer", "mods");
            }else
            {
                Console.WriteLine("OS cannot be defined");
                return;
            }

            if (!Directory.Exists(osuBin))
            {
                Console.WriteLine("Osu's Binary Folder not found: " + osuBin);
                return;
            }

            if (!Directory.Exists(osuMods))
            {
                Directory.CreateDirectory(osuMods);
            }
            var GameAssemblyPath = Path.Combine(osuBin, "osu.Game.dll");

            if (!File.Exists(GameAssemblyPath))
            {
                Console.WriteLine("Assembly not found: " + GameAssemblyPath);
                return;
            }

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(osuBin);
            resolver.AddSearchDirectory(osuMods);

            ILAsm.applyDOTNETversion(GameAssemblyPath, resolver);
            
            var readerParameters = new ReaderParameters { ReadWrite = true, AssemblyResolver = resolver };
            Console.WriteLine("Patching library: " + GameAssemblyPath);

            Modder.PatchAssembly(GameAssemblyPath, Path.Combine(osuMods, "OsuMod.dll"), resolver);
            
            if(!File.Exists(GameAssemblyPath + ".ExtraBackup"))
            {
                File.Copy(GameAssemblyPath, GameAssemblyPath + ".ExtraBackup");
                Console.WriteLine("Created .ExtraBackup of osu.Game.dll in case if something bad will happen :P");
            }

            if(!File.Exists(GameAssemblyPath + ".DONOTDELETE"))
            {
                File.Move(GameAssemblyPath, GameAssemblyPath + ".DONOTDELETE");
            }

            File.Move("osuPatched.tmp", GameAssemblyPath);

            Console.WriteLine("Patched");
            using (Process OSUSoftware = Process.Start(osuBin + "/osu!"))
            {
                OSUSoftware.WaitForExit();
            }
            if(File.Exists(GameAssemblyPath + ".DONOTDELETE"))
            {
                File.Delete(GameAssemblyPath);
                File.Move(GameAssemblyPath + ".DONOTDELETE", GameAssemblyPath);
                Console.WriteLine("[CODE 0] Original osu.Game.dll were recovered");
            }
        }
    }
}