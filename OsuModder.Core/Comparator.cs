using System.Collections.ObjectModel;
using Mono.Cecil;

namespace OsuModder.Core.Comparator
{
    public class ILAsm
    {
        public static void applyDOTNETversion(string assemblyPath, DefaultAssemblyResolver resolver)
        {
             //get the version prefix from the assembly
            string VersionPrefix = "8.0"; // (DotNet 8.0) version Fallback
            using (var tempAssembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadWrite = false }))
            {
                foreach (var attribute in tempAssembly.CustomAttributes)
                {
                    if (attribute.Constructor.DeclaringType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
                    {
                        string frameworkName = (string)attribute.ConstructorArguments[0].Value;
                        if (frameworkName.StartsWith(".NETCoreApp,Version=v"))
                        {
                            VersionPrefix = frameworkName.Substring(".NETCoreApp,Version=v".Length);
                        }
                    }
                }
            }
            Console.WriteLine("Detected .NET version prefix: " + VersionPrefix);

            //get the path to the .NET runtime assemblies
            string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? "/usr/share/dotnet";
            string runtimePath = Path.GetFullPath(Path.Combine(dotnetRoot, "shared", "Microsoft.NETCore.App"));
            Console.WriteLine("Searching for runtime assemblies in: " + runtimePath);
            if (Directory.Exists(runtimePath))
            {
                resolver.AddSearchDirectory(runtimePath);
                Console.WriteLine("Found runtime path: " + runtimePath);
                var matchedVersion = Directory.GetDirectories(runtimePath)
                    .Select(Path.GetFileName)
                    .Where(v => v.StartsWith(VersionPrefix))
                    .OrderByDescending(v => v)
                    .FirstOrDefault();

                if (matchedVersion == null)
                {
                    Console.WriteLine("No matching runtime version found for prefix: " + VersionPrefix);
                    Console.WriteLine("It is highly recommended to have same version of .NET runtime as DLL.");
                    return;
                }

                string matchedRuntimePath = Path.Combine(runtimePath, matchedVersion);
                resolver.AddSearchDirectory(matchedRuntimePath);
                Console.WriteLine("Added runtime search directory: " + matchedRuntimePath);

             }else
             {
                 Console.WriteLine("Runtime path does not exist: " + runtimePath);
                 return;
             }
        }

        public static bool versionCheck(AssemblyDefinition assemblyPath, string version)
        {
            foreach (var attribute in assemblyPath.CustomAttributes)
            {
                if (attribute.Constructor.DeclaringType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
                {
                    string VersionPrefix;
                    string frameworkName = (string)attribute.ConstructorArguments[0].Value;
                    if (frameworkName.StartsWith(".NETCoreApp,Version=v"))
                    {
                        VersionPrefix = frameworkName.Substring(".NETCoreApp,Version=v".Length);
                        if (VersionPrefix == version) return true;
                        else continue;
                    }
                }
            }
            Console.WriteLine("Mod: " + assemblyPath , "compiled on different .NET version");
            return false;
        }
    }
    public class ILPar
    {
        public static bool Match(
            Mono.Collections.Generic.Collection<ParameterDefinition> modParameter,
            Mono.Collections.Generic.Collection<ParameterDefinition> gameParameter
        )
        {
            if (modParameter.Count != gameParameter.Count)
            {
                return false;
            }
            for (int i = 0; i < modParameter.Count; i++)
            {
                if (modParameter[i].ParameterType.FullName !=
                    gameParameter[i].ParameterType.FullName)
                {
                    return false;
                }
            }
            return true;
        }

    }
}