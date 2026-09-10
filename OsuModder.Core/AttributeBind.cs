using OsuModder.Core.Comparator;
using Mono.Cecil;
using OsuModder.Core.Patch;

namespace OsuModder.Core.BindAttributes
{
    public class AttributeBinder
    {
        public static void Bind(AssemblyDefinition originalAssembly, AssemblyDefinition moddedAssembly, TypeDefinition Modtype, TypeDefinition targettype)
        {
            foreach (var modMethod in Modtype.Methods)
            {
                //for [Overwrite]
                bool isOverwrite = modMethod.CustomAttributes.Any(attribute =>
                attribute.AttributeType.FullName == "OsuModAPI.OverwriteAttribute");

                if(!isOverwrite) continue;

                var targetMethods = targettype.Methods
                    .Where( gameMethod => gameMethod.Name == modMethod.Name &&
                            gameMethod.IsStatic == modMethod.IsStatic &&
                            gameMethod.ReturnType.FullName == modMethod.ReturnType.FullName &&
                            ILPar.Match(gameMethod.Parameters, modMethod.Parameters)
                    )
                    .ToList();
            
                if (targetMethods.Count == 0)
                {
                    Console.WriteLine("Overwrite target not found");
                    continue;
                }

                if (targetMethods.Count > 1)
                {
                    Console.WriteLine("Overwrite targets not found");
                    continue;
                }

                var targetMethod = targetMethods[0];

                Console.WriteLine("Overwrite target found : "+ targetMethod);
                Modder.ReplaceMethodBody(originalAssembly, moddedAssembly, targetMethod, modMethod);

            }
        }
    }
}