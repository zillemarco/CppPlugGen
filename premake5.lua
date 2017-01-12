-- This is the starting point of the build scripts for the project.
-- It defines the common build settings that all the projects share
-- and calls the build scripts of all the sub-projects.

dofile "CppSharp/build/Helpers.lua"
dofile "CppSharp/build/LLVM.lua"

solution "CppPlugGen"

  configurations { "Release" }
  architecture "x86_64"

  filter "system:windows"
    architecture "x86"

  filter "system:macosx"
    architecture "x86"

  filter "configurations:Release"
    flags { "Optimize" }    

  filter {}

  characterset "Unicode"
  symbols "On"

  local action = _ACTION or ""
  
  location ("build/")

  objdir (path.join("./build/", "obj"))
  targetdir (path.join("./build/", "lib"))

  startproject "CppPlugGen"

  group "CppSharp"
    include("CppSharp/src/Core")
    include("CppSharp/src/AST")
    include("CppSharp/src/CppParser")
    include("CppSharp/src/CppParser/Bindings")
    include("CppSharp/src/CppParser/ParserGen")
    include("CppSharp/src/Parser")
    include("CppSharp/src/Generator")
    include("CppSharp/src/Runtime")

  group ""
  project "CppPlugGen"
    kind "ConsoleApp"
    language "C#"
    dotnetframework "4.6"
    location ("build/")
    
    objdir (path.join("./build/", "obj"))
    targetdir (path.join("./build/", "lib"))

    files { "src/*.cs" }

    links { "CppSharp", "CppSharp.AST", "CppSharp.Generator", "CppSharp.Parser", "CppSharp.Parser.CLI", "CppSharp.Runtime", "System" }
    dependson { "CppSharp", "CppSharp.AST", "CppSharp.Generator", "CppSharp.Parser", "CppSharp.Parser.CLI", "CppSharp.Runtime" }