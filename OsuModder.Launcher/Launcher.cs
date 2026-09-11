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
                osuBin = "./squashfs-root/usr/bin/";
                if (!Directory.Exists(osuBin))
                {
                    Console.WriteLine("No existing binary folder were found, extracting from appimage");
                    if (args.Length == 0)
                    {
                        Console.WriteLine("Please open the launcher script and define AppImage location");
                        return;
                    }
                    string appimagelocation = args[0];
                    ProcessStartInfo extractInfo = new ProcessStartInfo
                    {
                        FileName = appimagelocation,
                        Arguments = "--appimage-extract",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using(Process extractor = Process.Start(extractInfo))
                    {
                        extractor.WaitForExit();
                        if (extractor.ExitCode != 0)
                        {
                            string error = extractor.StandardError.ReadToEnd();
                            Console.WriteLine("Failed to extract appimage. Can you try to use command: ' ./osu.AppImage --appimage-extract ' and run the launcher again?");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Binaries extracted");
                        }
                    }
                }

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
            
            Console.WriteLine("Patching library: " + GameAssemblyPath);

            string[] modlist = Directory.GetFiles(osuMods, "*.dll");
            
            string currentAsm = GameAssemblyPath;
            
            if (!Directory.Exists("./tmp"))
            {
                Directory.CreateDirectory("tmp");
            }
            else
            {
                Directory.Delete("tmp", recursive: true);
                Directory.CreateDirectory("tmp");
            }
            foreach (string mod in modlist)
            {
                string nextAsm = Path.Combine("tmp", $"osuPatcher{Guid.NewGuid():N}.part");
                if(mod == "ModApi"){}
                Console.WriteLine("MOD: " + mod);
                Modder.PatchAssembly(currentAsm, Path.Combine(osuMods, mod), resolver, nextAsm);
                currentAsm = nextAsm;
            }

            if(!File.Exists(GameAssemblyPath + ".ExtraBackup"))
            {
                File.Copy(GameAssemblyPath, GameAssemblyPath + ".ExtraBackup");
                Console.WriteLine("Created .ExtraBackup of osu.Game.dll in case if something bad will happen :P");
            }

            if(!File.Exists(GameAssemblyPath + ".DONOTDELETE"))
            {
                File.Move(GameAssemblyPath, GameAssemblyPath + ".DONOTDELETE");
            }
            if(File.Exists(GameAssemblyPath))
            {
                File.Delete(GameAssemblyPath);
            }

            /* in case if mod need to get access to osu binary directly (which i dont think because im injecting custom asm resolver into osu)
            foreach (string mod in modlist)
            {
                    string destination = Path.Combine(
                        osuBin,
                    Path.GetFileName(mod));
                    File.Copy(mod, destination, overwrite: true);
            }
            */

            File.Move(currentAsm, GameAssemblyPath);

            if (Directory.Exists("./tmp"))
            {
                Directory.Delete("tmp", recursive: true);
            }

            Console.WriteLine("Patched");
            using (Process OSUSoftware = Process.Start(osuBin + "/osu!"))
            {
                OSUSoftware.WaitForExit();
            }
            if(File.Exists(GameAssemblyPath + ".DONOTDELETE"))
            {
                File.Delete(GameAssemblyPath);
                File.Move(GameAssemblyPath + ".DONOTDELETE", GameAssemblyPath);
                Console.WriteLine("Original osu.Game.dll was recovered");
            }
            else
            {
                Console.WriteLine("Original osu.Game.dll was NOT recovered, please consider to copy osu.Game.dll.ExtraBackup");
            }
        }
    }
}