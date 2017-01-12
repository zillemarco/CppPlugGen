using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Passes;
using CppSharp.Types;
using CppAbi = CppSharp.Parser.AST.CppAbi;
using CppSharp.Parser;

namespace CppSharp
{
    /// <summary>
    /// Generates C# bindings for the CppPlug project.
    /// </summary>
    class CppPlugGen : ILibrary
    {
        internal readonly GeneratorKind Kind;
        internal readonly string Triple;
        internal readonly CppAbi Abi;
        internal readonly string CppPlugLibPath;
        internal readonly string TargetArch;
        internal readonly bool IsGnuCpp11Abi;

        public CppPlugGen(GeneratorKind kind, string triple, CppAbi abi, string cppPlugLibPath, string targetArch, bool isGnuCpp11Abi = false)
        {
            Kind = kind;
            Triple = triple;
            Abi = abi;
            CppPlugLibPath = cppPlugLibPath;
            TargetArch = targetArch;
            IsGnuCpp11Abi = isGnuCpp11Abi;
        }

        static string GetSourceDirectory(string dir)
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null)
            {
                var path = Path.Combine(directory.FullName, dir);

                if (Directory.Exists(path))
                    return path;

                directory = directory.Parent;
            }

            throw new Exception("Could not find build directory: " + dir);
        }

        public void Setup(Driver driver)
        {
            var parserOptions = driver.ParserOptions;
            parserOptions.TargetTriple = Triple;
            parserOptions.Abi = Abi;

            var options = driver.Options;
            options.LibraryName = "CppPlug";
            options.SharedLibraryName = "CppPlug.dll";
            options.GeneratorKind = Kind;
            options.Headers.Add("ModuleTools.hpp");
            // options.Libraries.Add("CppPlug_dll.lib");

            if (Abi == CppAbi.Microsoft)
                parserOptions.MicrosoftMode = true;

            if (Triple.Contains("apple"))
                SetupMacOptions(parserOptions);

            if (Triple.Contains("linux"))
                SetupLinuxOptions(parserOptions);

            var basePath = GetSourceDirectory("src");
            parserOptions.AddIncludeDirs(basePath);
            parserOptions.AddLibraryDirs(".");
            parserOptions.AddLibraryDirs(CppPlugLibPath);

            options.OutputDir = Path.Combine(GetSourceDirectory("src"), "bindings", Kind.ToString().ToLower(), TargetArch);

            // TODO     var extraTriple = IsGnuCpp11Abi ? "-cxx11abi" : string.Empty;
            // TODO     
            // TODO     if (Kind == GeneratorKind.CSharp)
            // TODO         options.OutputDir = Path.Combine(options.OutputDir, parserOptions.TargetTriple + extraTriple);

            options.OutputNamespace = "CppPlug";
            options.CheckSymbols = false;
            options.UnityBuild = true;
        }

        private void SetupLinuxOptions(ParserOptions options)
        {
            // TODO     options.MicrosoftMode = false;
            // TODO     options.NoBuiltinIncludes = true;
            // TODO     
            // TODO     var headersPath = Platform.IsLinux ? string.Empty :
            // TODO         Path.Combine(GetSourceDirectory("build"), "headers", "x86_64-linux-gnu");
            // TODO     
            // TODO     // Search for the available GCC versions on the provided headers.
            // TODO     var versions = Directory.EnumerateDirectories(Path.Combine(headersPath,
            // TODO         "usr/include/c++"));
            // TODO     
            // TODO     if (versions.Count() == 0)
            // TODO         throw new Exception("No valid GCC version found on system include paths");
            // TODO     
            // TODO     string gccVersionPath = versions.First();
            // TODO     string gccVersion = gccVersionPath.Substring(
            // TODO         gccVersionPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            // TODO     
            // TODO     string[] systemIncludeDirs = {
            // TODO         Path.Combine("usr", "include", "c++", gccVersion),
            // TODO         Path.Combine("usr", "include", "x86_64-linux-gnu", "c++", gccVersion),
            // TODO         Path.Combine("usr", "include", "c++", gccVersion, "backward"),
            // TODO         Path.Combine("usr", "lib", "gcc", "x86_64-linux-gnu", gccVersion, "include"),
            // TODO         Path.Combine("usr", "include", "x86_64-linux-gnu"),
            // TODO         Path.Combine("usr", "include")
            // TODO     };
            // TODO     
            // TODO     foreach (var dir in systemIncludeDirs)
            // TODO         options.AddSystemIncludeDirs(Path.Combine(headersPath, dir));
            // TODO     
            // TODO     options.AddDefines("_GLIBCXX_USE_CXX11_ABI=" + (IsGnuCpp11Abi ? "1" : "0"));
        }

        private static void SetupMacOptions(ParserOptions options)
        {
            // TODO     options.MicrosoftMode = false;
            // TODO     options.NoBuiltinIncludes = true;
            // TODO     
            // TODO     if (Platform.IsMacOS)
            // TODO     {
            // TODO         var headersPaths = new List<string> {
            // TODO             Path.Combine(GetSourceDirectory("deps"), "llvm/tools/clang/lib/Headers"),
            // TODO             Path.Combine(GetSourceDirectory("deps"), "libcxx", "include"),
            // TODO             "/usr/include",
            // TODO         };
            // TODO     
            // TODO         foreach (var header in headersPaths)
            // TODO             Console.WriteLine(header);
            // TODO     
            // TODO         foreach (var header in headersPaths)
            // TODO             options.AddSystemIncludeDirs(header);
            // TODO     }
            // TODO     
            // TODO     var headersPath = Path.Combine(GetSourceDirectory("build"), "headers",
            // TODO         "osx");
            // TODO     
            // TODO     options.AddSystemIncludeDirs(Path.Combine(headersPath, "include"));
            // TODO     options.AddSystemIncludeDirs(Path.Combine(headersPath, "clang", "4.2", "include"));
            // TODO     options.AddSystemIncludeDirs(Path.Combine(headersPath, "libcxx", "include"));
            // TODO     options.AddArguments("-stdlib=libc++");
        }

        public void SetupPasses(Driver driver)
        {
            driver.Options.GenerateDefaultValuesForArguments = true;
            driver.Options.MarshalCharAsManagedChar = true;

            driver.AddTranslationUnitPass(new CheckMacroPass());
        }

        public void Preprocess(Driver driver, ASTContext ctx)
        {
        }

        public void Postprocess(Driver driver, ASTContext ctx)
        {
            ctx.SetClassAsValueType("CPluginInfo");
            ctx.SetClassAsValueType("CLoadModuleResult");
            ctx.SetClassAsValueType("CCreatedPlugin");
            ctx.SetClassAsValueType("CModuleDependenciesCollection");

            new CaseRenamePass(
                RenameTargets.Function | RenameTargets.Method | RenameTargets.Property | RenameTargets.Delegate |
                RenameTargets.Field | RenameTargets.Variable,
                RenameCasePattern.UpperCamelCase).VisitASTContext(driver.Context.ASTContext);
        }

        public static void Main(string[] args)
        {
            string cppPlugLibPath = "";

            // if (args.Length == 1)
            //     cppPlugLibPath = args[0];
            // else
            // {
            //     Console.WriteLine("Invalid CppPlug lib path given.\n");
            //     Console.WriteLine("Usage: CppPlugGen[.exe] <path to CppPlug lib directory>");
            //     return;
            // }
            
            if (Platform.IsWindows)
            {
                // Not supported by CppPlug
                //
                // Console.WriteLine("Generating the C++/CLI parser bindings for Windows...");
                // ConsoleDriver.Run(new CppPlugGen(GeneratorKind.CLI, "i686-pc-win32-msvc", CppAbi.Microsoft, cppPlugLibPath, "x86"));
                // Console.WriteLine();

                Console.WriteLine("Generating the C# parser bindings for Windows...");
                ConsoleDriver.Run(new CppPlugGen(GeneratorKind.CSharp, "i686-pc-win32-msvc", CppAbi.Microsoft, cppPlugLibPath, "x86"));
                Console.WriteLine();

                // Not supported by CppPlug
                //
                // Console.WriteLine("Generating the C# 64-bit parser bindings for Windows...");
                // ConsoleDriver.Run(new CppPlugGen(GeneratorKind.CSharp, "x86_64-pc-win32-msvc", CppAbi.Microsoft, cppPlugLibPath, "x64"));
                // Console.WriteLine();
            }

            // TODO     var osxHeadersPath = Path.Combine(GetSourceDirectory("build"), @"headers\osx");
            // TODO     if (Directory.Exists(osxHeadersPath) || Platform.IsMacOS)
            // TODO     {
            // TODO         Console.WriteLine("Generating the C# parser bindings for OSX...");
            // TODO         ConsoleDriver.Run(new CppPlugGen(GeneratorKind.CSharp, "i686-apple-darwin12.4.0", CppAbi.Itanium, cppPlugLibPath, "x86"));
            // TODO         Console.WriteLine();
            // TODO     
            // TODO         Console.WriteLine("Generating the C# parser bindings for OSX...");
            // TODO         ConsoleDriver.Run(new CppPlugGen(GeneratorKind.CSharp, "x86_64-apple-darwin12.4.0", CppAbi.Itanium, cppPlugLibPath, "x64"));
            // TODO         Console.WriteLine();
            // TODO     }
            // TODO     
            // TODO     var linuxHeadersPath = Path.Combine(GetSourceDirectory("build"), @"headers\x86_64-linux-gnu");
            // TODO     if (Directory.Exists(linuxHeadersPath) || Platform.IsLinux)
            // TODO     {
            // TODO         Console.WriteLine("Generating the C# parser bindings for Linux...");
            // TODO         ConsoleDriver.Run(new CppPlugGen(GeneratorKind.CSharp, "x86_64-linux-gnu", CppAbi.Itanium, cppPlugLibPath, "x64"));
            // TODO         Console.WriteLine();
            // TODO     
            // TODO         Console.WriteLine("Generating the C# parser bindings for Linux (GCC C++11 ABI)...");
            // TODO         ConsoleDriver.Run(new CppPlugGen(GeneratorKind.CSharp, "x86_64-linux-gnu", CppAbi.Itanium, cppPlugLibPath, "x64", isGnuCpp11Abi: true));
            // TODO         Console.WriteLine();
            // TODO     }
        }
    }
}