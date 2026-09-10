using OsuModder.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OsuModder.Core.BindAttributes;
using OsuModder.Core.Comparator;

namespace OsuModder.Core.Patch
{
    public class Modder
    {
        public static void InjectAsmResolver(ModuleDefinition ogMod, string originalAssemblyPath)
        {
            var programType = ogMod.Types.FirstOrDefault(t => t.Name == "Program" || t.Name == "OsuDesktopProgram" || t.Name == "OsuGame");
            if (programType == null)
            {
                Console.WriteLine("Can't find main class D:");
                return;
            }

            var cctor = programType.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
            if (cctor == null)
            {
                cctor = new MethodDefinition(".cctor",
                    MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    ogMod.TypeSystem.Void);
                programType.Methods.Add(cctor);
                cctor.Body = new MethodBody(cctor);
                cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            var processor = cctor.Body.GetILProcessor();
            var firstInstruction = cctor.Body.Instructions.First();

            var CurrentDomainMethod = typeof(AppDomain).GetProperty("CurrentDomain");
            var AsmResolveMethod = typeof(AppDomain).GetEvent("AssemblyResolve");
            if (AsmResolveMethod == null || CurrentDomainMethod == null)
            {
                Console.WriteLine("Can't find Resolver event");
                return;
            }
            

            var getCurrentDomainMethod = ogMod.ImportReference(CurrentDomainMethod.GetGetMethod(true));
            var addAsmResolveMethod = ogMod.ImportReference(AsmResolveMethod.GetAddMethod(true));
            
            var loadFromMethod = ogMod.ImportReference(
                typeof(System.Reflection.Assembly).GetMethod("LoadFrom", new[]{typeof(string)})
            );

            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldstr, originalAssemblyPath));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, loadFromMethod));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Pop));
            Console.WriteLine("Resolver injected");
        }
        public static void PatchAssembly(string OriginalAssemblyPath, string ModifiedAssemblyPath, DefaultAssemblyResolver resolver)
        {
            using var originalAssembly = AssemblyDefinition.ReadAssembly(OriginalAssemblyPath, new ReaderParameters { ReadWrite = true, AssemblyResolver = resolver});
            InjectAsmResolver(originalAssembly.MainModule, OriginalAssemblyPath);

            using var moddedAssembly = AssemblyDefinition.ReadAssembly(ModifiedAssemblyPath, new ReaderParameters { ReadWrite = false, AssemblyResolver = resolver});
            InjectAsmResolver(originalAssembly.MainModule, ModifiedAssemblyPath);
            var modAssemblyName = moddedAssembly.Name;
            var originalModule = originalAssembly.MainModule;

            if(!originalModule.AssemblyReferences.Any(r => r.Name == modAssemblyName.Name))
            {
                originalModule.AssemblyReferences.Add(modAssemblyName);
            }

            foreach (var modType in moddedAssembly.MainModule.Types)
            {
                if (modType.FullName == "<Module>" || modType.BaseType == null) continue;
                string TargetFullTypeName = modType.BaseType.FullName;

                var targettype = originalModule.Types
                    .FirstOrDefault(t => t.FullName == TargetFullTypeName);

                if (targettype == null)
                {
                    Console.WriteLine("Target not found");
                    continue;
                }
                
                Console.WriteLine($"Found type: {TargetFullTypeName}.");

                AttributeBinder.Bind(originalAssembly, moddedAssembly, modType, targettype);

                var modCtor = modType.Methods.Where(m => m.IsConstructor && !m.IsStatic);
                if (!modCtor.Any()) continue;


                //var modMethods = modType.Methods.Where(m => !m.IsConstructor&& !m.IsStatic && !m.IsSpecialName).ToList();

                foreach (var gameType in originalModule.Types)
                {
                    /*
                        bool istargetMatch = gameType.FullName == TargetFullTypeName;
                        if (!istargetMatch)
                        {
                            var currentBase = gameType.BaseType;
                            while (currentBase != null)
                            {
                            if (currentBase.FullName == TargetFullTypeName)
                            {
                                istargetMatch = true;
                                break;
                            }
                            try { currentBase = currentBase.Resolve()?.BaseType; } catch { break; }
                            }
                        }
                        if (!istargetMatch) continue;
                    */
                    foreach (var method in gameType.Methods)
                    {
                        if (!method.HasBody) continue;
                        var IlProcessor = method.Body.GetILProcessor();
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {

                            //var matchingModMethod = modCtor.FirstOrDefault(m => m.Name == method.Name);
                            
                            var instruction = method.Body.Instructions[i];
                            if (instruction.OpCode == OpCodes.Newobj && instruction.Operand is MethodReference methodRef)
                            {
                                var matchingModCtor = modCtor.FirstOrDefault(mc => mc.Parameters.Count == methodRef.Parameters.Count &&
                                ILPar.Match(mc.Parameters, methodRef.Parameters)
                                );
                                bool isInstantiatedTypeMatch = false;
                                var declType = methodRef.DeclaringType.Resolve();
                                while (declType != null)
                                {
                                    if (declType.FullName == TargetFullTypeName)
                                    {
                                        isInstantiatedTypeMatch = true;
                                        break;
                                    }
                                    try {declType = declType.BaseType?.Resolve();} catch{break;}
                                }
                                //var firstInstruction = method.Body.Instructions.First();
                                if (isInstantiatedTypeMatch)
                                {
                                    if(matchingModCtor != null)
                                    {
                                    var importedModCtor = originalModule.ImportReference(matchingModCtor);
                                    //IlProcessor.InsertBefore(firstInstruction, IlProcessor.Create(OpCodes.Call, importedModCtor));
                                    instruction.Operand = importedModCtor;
                                    Console.WriteLine($"Injected constructor call in {gameType.FullName}.{method.Name}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            originalAssembly.Write("osuPatched.tmp");
        }

        /*

        Oh god please forgive me, I used AI for part below this comment. Please understand how many attempts I've done for this part. I think I'm going insane. 
        My sanity is vital for me :P . Again, sowwy >.<
        
        - SunnyMatta
        P.S. I learned my previous mistakes from code below. S-Shall I get blessed by epic devs >//< ?

        */
        public static void ReplaceMethodBody(AssemblyDefinition originalAssembly, AssemblyDefinition moddedAssembly, MethodDefinition targetMethod, MethodDefinition modMethod)
        {
            if (!modMethod.HasBody)
            {
                Console.WriteLine($"Overwrite source has no body: {modMethod.FullName}");
                return;
            }

            var originalModule = originalAssembly.MainModule;
            var sourceBody = modMethod.Body;
            var replacementBody = new MethodBody(targetMethod)
            {
                InitLocals = sourceBody.InitLocals,
                MaxStackSize = sourceBody.MaxStackSize
            };

            foreach (var sourceVariable in sourceBody.Variables)
            {
                replacementBody.Variables.Add(new VariableDefinition(
                    originalModule.ImportReference(sourceVariable.VariableType)));
            }

            var instructionMap = new Dictionary<Instruction, Instruction>();

            foreach (var sourceInstruction in sourceBody.Instructions)
            {
                var clonedInstruction = Instruction.Create(OpCodes.Nop);
                clonedInstruction.OpCode = sourceInstruction.OpCode;
                instructionMap.Add(sourceInstruction, clonedInstruction);
                replacementBody.Instructions.Add(clonedInstruction);
            }

            for (int i = 0; i < sourceBody.Instructions.Count; i++)
            {
                var sourceInstruction = sourceBody.Instructions[i];
                var clonedInstruction = replacementBody.Instructions[i];
                clonedInstruction.Operand = ImportOperand(
                    sourceInstruction.Operand,
                    originalModule,
                    targetMethod,
                    replacementBody,
                    instructionMap);
            }

            foreach (var sourceHandler in sourceBody.ExceptionHandlers)
            {
                var clonedHandler = new ExceptionHandler(sourceHandler.HandlerType)
                {
                    CatchType = sourceHandler.CatchType == null
                        ? null
                        : originalModule.ImportReference(sourceHandler.CatchType),
                    TryStart = MapInstruction(sourceHandler.TryStart, instructionMap),
                    TryEnd = MapInstruction(sourceHandler.TryEnd, instructionMap),
                    HandlerStart = MapInstruction(sourceHandler.HandlerStart, instructionMap),
                    HandlerEnd = MapInstruction(sourceHandler.HandlerEnd, instructionMap),
                    FilterStart = sourceHandler.FilterStart == null
                        ? null
                        : MapInstruction(sourceHandler.FilterStart, instructionMap)
                };

                replacementBody.ExceptionHandlers.Add(clonedHandler);
            }

            targetMethod.Body = replacementBody;
            Console.WriteLine($"Replaced method body: {targetMethod.FullName}");
        }

        private static object? ImportOperand(
            object? operand,
            ModuleDefinition originalModule,
            MethodDefinition targetMethod,
            MethodBody replacementBody,
            IReadOnlyDictionary<Instruction, Instruction> instructionMap)
        {
            return operand switch
            {
                null => null,
                Instruction instruction => MapInstruction(instruction, instructionMap),
                Instruction[] instructions => instructions
                    .Select(instruction => MapInstruction(instruction, instructionMap))
                    .ToArray(),
                MethodReference method => originalModule.ImportReference(method),
                FieldReference field => originalModule.ImportReference(field),
                TypeReference type => originalModule.ImportReference(type),
                CallSite callSite => ImportCallSite(callSite, originalModule),
                ParameterDefinition parameter => targetMethod.Parameters[parameter.Index],
                VariableDefinition variable => replacementBody.Variables[variable.Index],
                _ => operand
            };
        }

        private static Instruction MapInstruction(
            Instruction instruction,
            IReadOnlyDictionary<Instruction, Instruction> instructionMap)
        {
            if (!instructionMap.TryGetValue(instruction, out var mappedInstruction))
            {
                throw new InvalidOperationException("Method body references an unknown instruction.");
            }

            return mappedInstruction;
        }

        private static CallSite ImportCallSite(CallSite source, ModuleDefinition originalModule)
        {
            var importedCallSite = new CallSite(
                originalModule.ImportReference(source.ReturnType))
            {
                CallingConvention = source.CallingConvention,
                HasThis = source.HasThis,
                ExplicitThis = source.ExplicitThis
            };

            foreach (var parameter in source.Parameters)
            {
                importedCallSite.Parameters.Add(new ParameterDefinition(
                    parameter.Name,
                    parameter.Attributes,
                    originalModule.ImportReference(parameter.ParameterType)));
            }

            return importedCallSite;
        }
    }
}