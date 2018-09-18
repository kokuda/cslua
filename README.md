# CSLua 

CSLua is a C# library for marshalling to the C interface in the native Windows Lua 5.1 DLL.  It allows you to run lua scripts using the same DLL as if building code natively with C or C++.

More details can be found on the original project page at http://notions.okuda.ca/csharpprojects/cslua/.

## Dependencies

This distribution includes the Lua Win32 DLL built with Visual Studio 15 from https://sourceforge.net/projects/luabinaries/files/5.1.5/Windows%20Libraries/Dynamic/.  It may be possible to work with other binary builds of Lua 5.1 but they have not been tested.

## API

CSLua supports the core Lua C API and includes a C# class wrapper for binding C# objects from Lua script.

### CSLua.LuaDll

The CSLua.LuaDll class contains the C# version of the interface defined in lua.h and lauxlib.h.  All functions and types in lua.h and lauxlib.h are represented.

See tests/LuaDllTests.cs for examples

### CSLua.LuaState

The CSLua.LuaState class is a more C# friendly API to the Lua runtime.  It supports registering Action<> and Func<> and C# Objects within Lua to expose as functions and tables within Lua.

See tests/LuaStateTests.cs for examples

## TODO

Add more documentation
Upgrade to newer versions of Lua
Either download the Lua DLL or build it from source rather than include it in the git repo.