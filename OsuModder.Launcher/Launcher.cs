using OsuModder.Core;
using OsuModder.Core.Comparator;
using OsuModder.Core.Patch;
using Mono.Cecil;
using System.Diagnostics;

namespace OsuModder.Launcher
{
    class Program
    {

        static void Main(string[] args)
        {
            //get the path to the osu's game assembly
            var osuBin = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "", "Documents", "APPS", "squashfs-root", "usr", "bin"); // /home/sunny/Documents/APPS/squashfs-root/usr/bin/
            var osuMods = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "", ".local", "share", "osu", "mods"); // /home/sunny/Documents/APPS/squashfs-root/usr/bin/
            
            if (!Directory.Exists(osuBin))
            {
                Console.WriteLine("Osu's Binary Folder not found: " + osuBin);
                return;
            }

            if (!Directory.Exists(osuMods))
            {
                Console.WriteLine("Mods Folder not found: " + osuMods);
                return;
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
            }
        }
    }
}