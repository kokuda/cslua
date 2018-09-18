//////////////////////////////////////////////////////////////////////////////
//
//  This file is part of CSLua: A C# Lua library interface
//
//  MIT License
//  Copyright(c) 2018 Kaz Okuda (http://notions.okuda.ca)
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
//
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace CSLua
{
    using lua_Number = Double;
    using lua_Integer = Int32;
    using size_t = Int32;

    public partial class LuaDll
    {
        ///////////////////////////////////////////////////////////////////////
        // lauxlib.h
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Type for arrays of functions to be registered by luaL_register.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct luaL_Reg
        {
            /// <summary>
            /// Function name
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;

            /// <summary>
            /// Function to register
            /// </summary>
            public lua_CFunction func;
        }

        public static void luaI_openlib(lua_State L, string libname, luaL_Reg[] l, int nup)
        {
            NativeMethods.luaI_openlib(L, libname, l, nup);
        }

        /// <summary>
        /// Opens a library.
        /// When called with libname equal to null, it simply registers all functions in the list l (see luaL_Reg) into the table on the top of the stack.
        /// When called with a non-null libname, luaL_register creates a new table t, sets it as the value of the global variable libname, sets it as the value of package.loaded[libname], and registers on it all functions in the list l. If there is a table in package.loaded[libname] or in variable libname, reuses this table instead of creating a new one.
        /// In any case the function leaves the table on the top of the stack. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="libname">Name of library</param>
        /// <param name="l">Functions to register</param>
        public static void luaL_register(lua_State L, string libname, luaL_Reg[] l)
        {
            NativeMethods.luaL_register(L, libname, l);
        }

        /// <summary>
        /// Pushes onto the stack the field e from the metatable of the object at index obj. If the object does not have a metatable, or if the metatable does not have this field, returns 0 and pushes nothing.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="obj">Object</param>
        /// <param name="e">Name of field</param>
        /// <returns>Returns 0 on error</returns>
        public static int luaL_getmetafield(lua_State L, int obj, string e)
        {
            return NativeMethods.luaL_getmetafield(L, obj, e);
        }

        /// <summary>
        /// Calls a metamethod.
        /// If the object at index obj has a metatable and this metatable has a field e, this function calls this field and passes the object as its only argument.
        /// In this case this function returns 1 and pushes onto the stack the value returned by the call.
        /// If there is no metatable or no metamethod, this function returns 0 (without pushing any value on the stack). 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="obj">Object</param>
        /// <param name="e">Name of metamethod to call</param>
        /// <returns>Returns 0 on error</returns>
        public static int luaL_callmeta(lua_State L, int obj, string e)
        {
            return NativeMethods.luaL_callmeta(L, obj, e);
        }

        /// <summary>
        /// Generates an error with a message like the following: 
        ///      "location: bad argument narg to 'func' (tname expected, got rt)"
        /// where location is produced by luaL_where, func is the name of the current function, and rt is the type name of the actual argument. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Index of the bad argument</param>
        /// <param name="tname">Name of expected type</param>
        /// <returns></returns>
        public static int luaL_typerror(lua_State L, int narg, string tname)
        {
            return NativeMethods.luaL_typerror(L, narg, tname);
        }

        /// <summary>
        /// Raises an error with the following message, where func is retrieved from the call stack:
        ///     "bad argument #<narg> to <func> (<extramsg>)"
        /// This function never returns, but it is an idiom to use it in C# functions as return luaL_argerror(args).
        /// </summary>
        /// <param name="L">Lua error</param>
        /// <param name="numarg">Index of bad argument</param>
        /// <param name="extramsg">Extra message</param>
        /// <returns></returns>
        public static int luaL_argerror(lua_State L, int numarg, string extramsg)
        {
            return NativeMethods.luaL_argerror(L, numarg, extramsg);
        }

        /// <summary>
        /// Checks whether the function argument narg is a string and returns this string; fills l with the string's length.
        /// This function uses lua_tolstring to get its result, so all conversions and caveats of that function apply here. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="numArg">Argument index</param>
        /// <param name="l">(out) returns containing length</param>
        /// <returns>Returns the string (if it is one)</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static string luaL_checklstring(lua_State L, int numArg, out size_t l)
        {
            IntPtr ptr = NativeMethods.luaL_checklstring(L, numArg, out l);
            string str = Marshal.PtrToStringAnsi(ptr, l);
            return str;
        }

        /// <summary>
        /// UNTESTED
        /// If the function argument narg is a string, returns this string.
        /// If this argument is absent or is nil, returns d.
        /// Otherwise, raises an error.
        /// Fills the position l with the results's length. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument number to check</param>
        /// <param name="def">Default value to return if narg is not a string</param>
        /// <param name="l">Contains length of string when returned</param>
        /// <returns>Argument at position narg if it is a string, otherwise def</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static string luaL_optlstring(lua_State L, int narg, string def, out size_t l)
        {
            IntPtr ptr = NativeMethods.luaL_optlstring(L, narg, def, out l);
            string str = Marshal.PtrToStringAnsi(ptr, l);
            return str;
        }

        /// <summary>
        /// Checks whether the function argument narg is a number and returns this number. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index to check</param>
        /// <returns>Returns the number at argument index narg</returns>
        public static lua_Number luaL_checknumber(lua_State L, int narg)
        {
            return NativeMethods.luaL_checknumber(L, narg);
        }

        /// <summary>
        /// If the function argument narg is a number, returns this number.
        /// If this argument is absent or is nil, returns d.
        /// Otherwise, raises an error.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        /// <param name="def">Default value if narg is absent or nil</param>
        /// <returns>Returnns argument at index narg as a number, or def if it is absent or nil</returns>
        public static lua_Number luaL_optnumber(lua_State L, int narg, lua_Number def)
        {
            return NativeMethods.luaL_optnumber(L, narg, def);
        }

        /// <summary>
        /// Checks whether the function argument narg is a number and returns this number cast to a lua_Integer.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Index of argument to return</param>
        /// <returns>Returns argument at index narg as an integer</returns>
        public static lua_Integer luaL_checkinteger(lua_State L, int narg)
        {
            return NativeMethods.luaL_checkinteger(L, narg);
        }

        /// <summary>
        /// If the function argument narg is a number, returns this number cast to a lua_Integer.
        /// If this argument is absent or is nil, returns d.
        /// Otherwise, raises an error.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        /// <param name="def">Default value if narg is nil or absent</param>
        /// <returns>Returns argument at index narg as an integer, or def it is absent or nil</returns>
        public static lua_Integer luaL_optinteger(lua_State L, int narg, lua_Integer def)
        {
            return NativeMethods.luaL_optinteger(L, narg, def);
        }

        /// <summary>
        /// Grows the stack size to top + sz elements, raising an error if the stack cannot grow to that size.
        /// msg is an additional text to go into the error message.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="sz">Number of elements to add to the stack</param>
        /// <param name="msg">Additional message to go into the error message if there is an error</param>
        public static void luaL_checkstack(lua_State L, int sz, string msg)
        {
            NativeMethods.luaL_checkstack(L, sz, msg);
        }

        /// <summary>
        /// Checks whether the function argument narg has type t.
        /// t must be one of the following values LUA_TNIL, LUA_TNUMBER, LUA_TBOOLEAN, LUA_TSTRING, LUA_TTABLE, LUA_TFUNCTION, LUA_TUSERDATA, LUA_TTHREAD, or LUA_TLIGHTUSERDATA.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        /// <param name="t">Type to confirm</param>
        public static void luaL_checktype(lua_State L, int narg, int t)
        {
            NativeMethods.luaL_checktype(L, narg, t);
        }

        /// <summary>
        /// Checks whether the function has an argument of any type (including nil) at position narg.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        public static void luaL_checkany(lua_State L, int narg)
        {
            NativeMethods.luaL_checkany(L, narg);
        }

        /// <summary>
        /// If the registry already has the key tname, returns 0.
        /// Otherwise, creates a new table to be used as a metatable for userdata, adds it to the registry with key tname, and returns 1.
        /// In both cases pushes onto the stack the final value associated with tname in the registry. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="tname">Name of metatable</param>
        /// <returns>0 if metatable by that name already exists, 1 otherwise</returns>
        public static int luaL_newmetatable(lua_State L, string tname)
        {
            return NativeMethods.luaL_newmetatable(L, tname);
        }

        /// <summary>
        /// Checks whether the function argument narg is a userdata of the type tname.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Index of argument</param>
        /// <param name="tname">Name of type</param>
        /// <returns>Returns the userdata if it is correct type, null otherwise?</returns>
        public static IntPtr luaL_checkudata(lua_State L, int narg, string tname)
        {
            return NativeMethods.luaL_checkudata(L, narg, tname);
        }

        /// <summary>
        /// Pushes onto the stack a string identifying the current position of the control at level lvl in the call stack.
        /// Typically this string has the following format:
        ///     "chunkname:currentline:"
        /// Level 0 is the running function, level 1 is the function that called the running function, etc.
        /// This function is used to build a prefix for error messages. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="lvl">Level to identify</param>
        public static void luaL_where(lua_State L, int lvl)
        {
            NativeMethods.luaL_where(L, lvl);
        }

        /// <summary>
        /// Raises an error.
        /// It also adds at the beginning of the message the file name and the line number where the error occurred, if this information is available.
        /// This function never returns, but it is an idiom to use it in C# functions as return luaL_error(args). 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">String to print</param>
        /// <returns></returns>
        public static int luaL_error(lua_State L, string s)
        {
            luaL_where(L, 1);
            lua_pushstring(L, s);
            lua_concat(L, 2);
            return lua_error(L);
        }

        /// <summary>
        /// Raises an error. The error message format is given by f plus any extra arguments, following the same rules of String.Format.
        /// It also adds at the beginning of the message the file name and the line number where the error occurred, if this information is available.
        /// This function never returns, but it is an idiom to use it in C# functions as return luaL_error(args). 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="f">Format string to print</param>
        /// <param name="args">Optional extra format string arguments (see String.Format)</param>
        /// <returns></returns>
        public static int luaL_error(lua_State L, string f, params object[] args)
        {
            string output = String.Format(f, args);
            return luaL_error(L, output);
        }

        /// <summary>
        /// UNTESTED.
        /// Checks whether the function argument narg is a string and searches for this string in the array lst (which must be null-terminated). Returns the index in the array where the string was found. Raises an error if the argument is not a string or if the string cannot be found.
        /// If def is not NULL, the function uses def as a default value when there is no argument narg or if this argument is nil.
        /// This is a useful function for mapping strings to C enums. (The usual convention in Lua libraries is to use strings instead of numbers to select options.) 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        /// <param name="def">Default result if narg is nil or invalid</param>
        /// <param name="lst">List of strings to check</param>
        /// <returns></returns>
        public static int luaL_checkoption(lua_State L, int narg, string def, string[] lst)
        {
            return NativeMethods.luaL_checkoption(L, narg, def, lst);
        }

        /// <summary>
        /// Creates and returns a reference, in the table at index t, for the object at the top of the stack (and pops the object).
        /// You can retrieve an object referred by reference r by calling lua_rawgeti(L, t, r).
        /// Function luaL_unref frees a reference and its associated object.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="t">Index of table</param>
        /// <returns>Returns reference</returns>
        public static int luaL_ref(lua_State L, int t)
        {
            return NativeMethods.luaL_ref(L, t);
        }

        /// <summary>
        /// Releases reference ref from the table at index t (returned from luaL_ref).
        /// The entry is removed from the table, so that the referred object can be collected.
        /// The reference ref is also freed to be used again.
        /// If ref is LUA_NOREF or LUA_REFNIL, luaL_unref does nothing. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="t">Index of table</param>
        /// <param name="r">Reference to release</param>
        public static void luaL_unref(lua_State L, int t, int r)
        {
            NativeMethods.luaL_unref(L, t, r);
        }

        /// <summary>
        /// UNTESTED.
        /// Loads a file as a Lua chunk (and does not run it).
        /// This function uses lua_load to load the chunk in the file named filename.
        /// If filename is NULL, then it loads from the standard input.
        /// The first line in the file is ignored if it starts with a #.
        /// This function returns the same results as lua_load, but it has an extra error code LUA_ERRFILE if it cannot open/read the file.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="filename">Name of file to load</param>
        /// <returns>
        /// 0 if no errors.
        /// LUA_ERRSYNTAX if a syntax error during pre-compilation.
        /// LUA_ERRMEM if memory allocation error.
        /// LUA_ERRFILE if it cannot open/read the file
        /// </returns>
        /// TODO: Wrap this in managed file IO instead of using implementation in DLL
        public static int luaL_loadfile(lua_State L, string filename)
        {
            return NativeMethods.luaL_loadfile(L, filename);
        }

        /// <summary>
        /// UNTESTED.
        /// Loads a buffer as a Lua chunk.
        /// This function uses lua_load to load the chunk in the buffer pointed to by buff with size sz.
        /// This function returns the same results as lua_load.
        /// name is the chunk name, used for debug information and error messages. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="buff">Buffer to load</param>
        /// <param name="sz">Size of buffer</param>
        /// <param name="name">Name used for debug information in error messages</param>
        /// <returns>
        /// 0 if no errors.
        /// LUA_ERRSYNTAX if a syntax error during pre-compilation.
        /// LUA_ERRMEM if memory allocation error.
        /// </returns>
        /// TODO: Should buff be a string or byte[]?
        public static int luaL_loadbuffer(lua_State L, string buff, size_t sz, string name)
        {
            return NativeMethods.luaL_loadbuffer(L, buff, sz, name);
        }

        /// <summary>
        /// Loads a string as a Lua chunk and does not run it.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">String to load</param>
        /// <returns>
        /// 0 if no errors.
        /// LUA_ERRSYNTAX if a syntax error during pre-compilation.
        /// LUA_ERRMEM if memory allocation error.
        /// </returns>
        public static int luaL_loadstring(lua_State L, string s)
        {
            return NativeMethods.luaL_loadstring(L, s);
        }

        /// <summary>
        /// Creates a new Lua state.
        /// It calls lua_newstate with an allocator based on the standard C realloc function and then sets a panic function (see lua_atpanic) that prints an error message to the standard error output in case of fatal errors.
        /// Returns the new state, or IntPtr(0) if there is a memory allocation error. 
        /// </summary>
        /// <returns>New lua state</returns>
        public static lua_State luaL_newstate()
        {
            return NativeMethods.luaL_newstate();
        }

        /*
        ** ===============================================================
        ** some useful macros
        ** ===============================================================
        */

        /// <summary>
        /// Checks whether cond is true.
        /// If not, raises an error with the following message, where func is retrieved from the call stack:
        ///     "bad argument #<narg> to <func> (<extramsg>)"
        /// </summary>
        /// <param name="L">Lua sttate</param>
        /// <param name="cond">Condition to test (0 for failure)</param>
        /// <param name="narg">Argument index</param>
        /// <param name="extramsg">Extra text for the error message</param>
        public static void luaL_argcheck(lua_State L, int cond, int narg, string extramsg)
        {
            if (cond == 0)
            {
                luaL_argerror(L, (narg), (extramsg));
            }
        }

        /// <summary>
        /// Checks whether the function argument narg is a string and returns this string.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        /// <returns>Returns the string</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static string luaL_checkstring(lua_State L, int narg)
        {
            int len;
            return luaL_checklstring(L, narg, out len);
        }

        /// <summary>
        /// Loads and runs the given string.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">String to load and run</param>
        /// <returns>Returns 0 if there are no errors or 1 in case of errors.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static int luaL_dostring(lua_State L, string s)
        {
            // Push the code
            GCHandle gch = GCHandle.Alloc(s);
            int ret = luaL_loadstring(L, s);
            if (ret == 0)
            {
                // Execute the code
                ret = lua_pcall(L, 0, LUA_MULTRET, 0);
            }
            gch.Free();
            return ret;
        }

        /// <summary>
        /// Pushes onto the stack the metatable associated with name tname in the registry.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="tname">Type name</param>
        /// <see>luaL_newmetatable</see>
        public static void luaL_getmetatable(lua_State L, string tname)
        {
            lua_getfield(L, LUA_REGISTRYINDEX, tname);
        }

        ///////////////////////////////////////////////////////////////////////
        // lualib.h
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// UNTESTED.
        /// Opens the basic library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        public static int luaopen_base(lua_State L)    /* opens the basic library */
        {
            return NativeMethods.luaopen_base(L);
        }

        /// <summary>
        /// UNTESTED.
        /// Opens the table library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        public static int luaopen_table(lua_State L)   /* opens the table library */
        {
            return NativeMethods.luaopen_table(L);
        }

        /// <summary>
        /// UNTESTED.
        /// Opens the I/O library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        public static int luaopen_io(lua_State L)      /* opens the I/O library */
        {
            return NativeMethods.luaopen_io(L);
        }

        /// <summary>
        /// UNTESTED.
        /// Opens the string library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        public static int luaopen_string(lua_State L)  /* opens the string lib. */
        {
            return NativeMethods.luaopen_string(L);
        }

        /// <summary>
        /// UNTESTED.
        /// Opens the math library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        public static int luaopen_math(lua_State L)    /* opens the math lib. */
        {
            return NativeMethods.luaopen_math(L);
        }

        /// <summary>
        /// Opens all standard Lua libraries into the given state.
        /// </summary>
        /// <param name="L">Lua state</param>
        public static void luaL_openlibs(lua_State L)         /* opens all standard libs */
        {
            NativeMethods.luaL_openlibs(L);
        }

    }
}
