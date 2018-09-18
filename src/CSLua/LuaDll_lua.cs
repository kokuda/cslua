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

    /// <summary>
    /// Explicit type to represent the lua_State pointer from C instead of just using IntPtr.
    /// The Marshalling seems to work correctly with this definition.
    /// </summary>
    public struct lua_State
    {
        private IntPtr value;
        static public implicit operator IntPtr(lua_State l) { return l.value;}
        static public implicit operator lua_State(IntPtr l) { return new lua_State { value = l }; }
    }

    public partial class LuaDll
    {
        ///////////////////////////////////////////////////////////////////////
        // lua.h
        ///////////////////////////////////////////////////////////////////////

        /* option for multiple returns in `lua_pcall' and `lua_call' */
        public const int LUA_MULTRET = -1;

        /*
        ** pseudo-indices
        */
        public const int LUA_REGISTRYINDEX	= -10000;
        public const int LUA_ENVIRONINDEX = -10001;
        public const int LUA_GLOBALSINDEX = -10002;
        public static int lua_upvalueindex(int i) { return (LUA_GLOBALSINDEX - (i)); }

        /* thread status; 0 is OK */
        public const int LUA_YIELD = 1;
        public const int LUA_ERRRUN	= 2;
        public const int LUA_ERRSYNTAX	= 3;
        public const int LUA_ERRMEM	= 4;
        public const int LUA_ERRERR	= 5;

        /// <summary>
        /// Type for C# functions.
        /// When the function starts, lua_gettop(L) returns the number of arguments received by the function. The first argument (if any) is at index 1 and its last argument is at index lua_gettop(L).
        /// To return values to Lua, a function just pushes them onto the stack, in direct order (the first result is pushed first), and returns the number of results.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns the number of results push onto the stack</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_CFunction(lua_State L);

        /*
        ** prototype for memory-allocation functions
        */
        /// <summary>
        /// The type of the memory-allocation function used by Lua states.
        /// The allocator function must provide a functionality similar to realloc, but not exactly the same.
        /// When nsize is zero, the allocator must return NULL; if osize is not zero, it should free the block pointed to by ptr. When nsize is not zero, the allocator returns NULL if and only if it cannot fill the request. When nsize is not zero and osize is zero, the allocator should behave like malloc. When nsize and osize are not zero, the allocator behaves like realloc. Lua assumes that the allocator never fails when osize >= nsize.
        /// </summary>
        /// <param name="ud">An opaque pointer passed to lua_newstate</param>
        /// <param name="ptr">A pointer to the block being allocated/reallocated/freed</param>
        /// <param name="osize">The original size of the block.</param>
        /// <param name="nsize">The new size of the block.</param>
        /// <returns>Returns allocated block of memory</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr lua_Alloc(IntPtr ud, IntPtr ptr, size_t osize, size_t nsize);

        /*
        ** basic types
        */
        public const int LUA_TNONE = -1;

        public const int LUA_TNIL = 0;
        public const int LUA_TBOOLEAN = 1;
        public const int LUA_TLIGHTUSERDATA = 2;
        public const int LUA_TNUMBER = 3;
        public const int LUA_TSTRING = 4;
        public const int LUA_TTABLE = 5;
        public const int LUA_TFUNCTION = 6;
        public const int LUA_TUSERDATA = 7;
        public const int LUA_TTHREAD = 8;
        
        /*
        ** state manipulation
        */

        /// <summary>
        /// Creates a new, independent state
        /// </summary>
        /// <param name="f">Allocator function</param>
        /// <param name="ud">An opaque pointer that Lua simply passes to the allocator in every call</param>
        /// <returns>Returns a new lua state.  Returns null if cannot create the state (due to lack of memory)</returns>
        public static lua_State lua_newstate (lua_Alloc f, IntPtr ud)
        {
            return NativeMethods.lua_newstate(f, ud);
        }

        /// <summary>
        /// Destroys all objects in the given Lua state (calling the corresponding garbage-collection metamethods, if any) and frees all dynamic memory used by this state.
        /// </summary>
        /// <param name="L">Lua state to close</param>
        public static void lua_close (lua_State L)
        {
            NativeMethods.lua_close(L);
        }

        /// <summary>
        /// Creates a new thread, pushes it on the stack, and returns a pointer to a lua_State that represents this new thread.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        public static lua_State lua_newthread (lua_State L)
        {
            return NativeMethods.lua_newthread(L);
        }

        /// <summary>
        /// Sets a new panic function and returns the old one.  If an error happens outside any protected environment, Lua calls a panic function and then calls exit(EXIT_FAILURE), thus exiting the host application.  Your panic function can avoid this exit by never returning (e.g., doing a long jump). The panic function can access the error message at the top of the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="panicf">Panic function</param>
        /// <returns></returns>
        public static lua_CFunction lua_atpanic (lua_State L, lua_CFunction panicf)
        {
            return NativeMethods.lua_atpanic(L, panicf);
        }

        /*
        ** basic stack manipulation
        */
        /// <summary>
        /// Returns the index of the top element in the stack.
        /// Because indices start at 1, this result is equal to the number of elements in the stack (and so 0 means an empty stack).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns the index of the top element in the stack</returns>
        public static int lua_gettop (lua_State L)
        {
            return NativeMethods.lua_gettop(L);
        }
        /// <summary>
        /// Accepts any acceptable index, or 0, and sets the stack top to this index.
        /// If the new top is larger than the old one, then the new elements are filled with nil.
        /// If index is 0, then all stack elements are removed.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">New top</param>
        public static void lua_settop (lua_State L, int idx)
        {
            NativeMethods.lua_settop(L, idx);
        }

        /// <summary>
        /// Pushes a copy of the element at the given valid index onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of element to push</param>
        public static void lua_pushvalue (lua_State L, int idx)
        {
            NativeMethods.lua_pushvalue(L, idx);
        }

        /// <summary>
        /// Removes the element at the given valid index, shifting down the elements above this index to fill the gap. Cannot be called with a pseudo-index, because a pseudo-index is not an actual stack position.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to remove</param>
        public static void lua_remove(lua_State L, int idx)
        {
            NativeMethods.lua_remove(L, idx);
        }

        /// <summary>
        /// Moves the top element into the given valid index, shifting up the elements above this index to open space. Cannot be called with a pseudo-index, because a pseudo-index is not an actual stack position.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">New index for the top element.  Cannot be a pseudo-index</param>
        public static void lua_insert(lua_State L, int idx)
        {
            NativeMethods.lua_insert(L, idx);
        }

        /// <summary>
        /// Moves the top element into the given position (and pops it), without shifting any element (therefore replacing the value at the given position).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">New index</param>
        public static void lua_replace(lua_State L, int idx)
        {
            NativeMethods.lua_replace(L, idx);
        }

        /// <summary>
        /// Ensures that there are at least extra free stack slots in the stack. It returns false if it cannot grow the stack to that size. This function never shrinks the stack; if the stack is already larger than the new size, it is left unchanged.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="sz">Size to check</param>
        /// <returns>Returns 0 if it cannot grow the stack to that size.</returns>
        public static int lua_checkstack(lua_State L, int sz)
        {
            return NativeMethods.lua_checkstack(L, sz);
        }

        /*
        ** access functions (stack -> C)
        */
        /// <summary>
        /// Returns 1 if the value at the given acceptable index is a number or a string convertible to a number, and 0 otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to check</param>
        /// <returns>Returns 1 if the value at the given acceptable index is a number or a string convertible to a number, and 0 otherwise.</returns>
        public static int lua_isnumber(lua_State L, int idx)
        {
            return NativeMethods.lua_isnumber(L, idx);
        }

        /// <summary>
        /// Returns 1 if the value at the given acceptable index is a string or a number (which is always convertible to a string), and 0 otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to check</param>
        /// <returns>Returns 1 if the value at the given acceptable index is a string or a number (which is always convertible to a string), and 0 otherwise.</returns>
        public static int lua_isstring(lua_State L, int idx)
        {
            return NativeMethods.lua_isstring(L, idx);
        }

        /// <summary>
        /// Returns 1 if the value at the given acceptable index is a C function, and 0 otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to check</param>
        /// <returns>Returns 1 if the value at the given acceptable index is a C function, and 0 otherwise.</returns>
        public static int lua_iscfunction(lua_State L, int idx)
        {
            return NativeMethods.lua_iscfunction(L, idx);
        }

        /// <summary>
        /// Returns 1 if the value at the given acceptable index is a userdata (either full or light), and 0 otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to check</param>
        /// <returns>Returns 1 if the value at the given acceptable index is a userdata (either full or light), and 0 otherwise.</returns>
        public static int lua_isuserdata(lua_State L, int idx)
        {
            return NativeMethods.lua_isuserdata(L, idx);
        }

        /// <summary>
        /// Returns the type of the value in the given acceptable index, or LUA_TNONE for a non-valid index (that is, an index to an "empty" stack position).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value</param>
        /// <returns>Returns one of the following types LUA_TNIL, LUA_TNUMBER, LUA_TBOOLEAN, LUA_TSTRING, LUA_TTABLE, LUA_TFUNCTION, LUA_TUSERDATA, LUA_TTHREAD, and LUA_TLIGHTUSERDATA.</returns>
        public static int lua_type(lua_State L, int idx)
        {
            return NativeMethods.lua_type(L, idx);
        }

        /// <summary>
        /// Returns the name of the type encoded by the value tp, which must be one the values returned by lua_type.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="tp">Type to encode</param>
        /// <returns>Name of type</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static string        lua_typename(lua_State L, int tp)
        {
            string str = Marshal.PtrToStringAnsi(NativeMethods.__lua_typename(L, tp));
            return str;
        }

        /// <summary>
        /// Returns 1 if the two values in acceptable indices index1 and index2 are equal, following the semantics of the Lua == operator (that is, may call metamethods). Otherwise returns 0. Also returns 0 if any of the indices is non valid.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx1">Index of first value</param>
        /// <param name="idx2">Index of second value</param>
        /// <returns>Returns 1 if the two values in acceptable indices index1 and index2 are equal, following the semantics of the Lua == operator (that is, may call metamethods). Otherwise returns 0. Also returns 0 if any of the indices is non valid.</returns>
        public static int    lua_equal (lua_State L, int idx1, int idx2)
        {
            return NativeMethods.lua_equal(L, idx1, idx2);
        }

        /// <summary>
        /// Returns 1 if the two values in acceptable indices index1 and index2 are primitively equal (that is, without calling metamethods). Otherwise returns 0. Also returns 0 if any of the indices are non valid.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx1">Index of first value</param>
        /// <param name="idx2">Index of second value</param>
        /// <returns>Returns 1 if the two values in acceptable indices index1 and index2 are primitively equal (that is, without calling metamethods). Otherwise returns 0. Also returns 0 if any of the indices are non valid.</returns>
        public static int    lua_rawequal (lua_State L, int idx1, int idx2)
        {
            return NativeMethods.lua_rawequal(L, idx1, idx2);
        }

        /// <summary>
        /// Returns 1 if the value at acceptable index index1 is smaller than the value at acceptable index index2, following the semantics of the Lua < operator (that is, may call metamethods). Otherwise returns 0. Also returns 0 if any of the indices is non valid.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx1">Index of first value</param>
        /// <param name="idx2">Index of second value</param>
        /// <returns>Returns 1 if the value at acceptable index index1 is smaller than the value at acceptable index index2. Otherwise returns 0. Also returns 0 if any of the indices is non valid.</returns>
        public static int    lua_lessthan (lua_State L, int idx1, int idx2)
        {
            return NativeMethods.lua_lessthan(L, idx1, idx2);
        }

        /// <summary>
        /// Converts the Lua value at the given acceptable index to the C type lua_Number.
        /// The Lua value must be a number or a string convertible to a number; otherwise, lua_tonumber returns 0.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to convert to a number and return</param>
        /// <returns>Returns the value at stack index idx, converted to a number</returns>
        public static lua_Number lua_tonumber(lua_State L, int idx)
        {
            return NativeMethods.lua_tonumber(L, idx);
        }

        /// <summary>
        /// Converts the Lua value at the given acceptable index to a signed integer.
        /// The Lua value must be a number or a string convertible to a number; otherwise, lua_tointeger returns 0.
        /// If the number is not an integer, it is truncated in some non-specified way. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to convert to an integer and return</param>
        /// <returns>Returns the value at stack index idx, converted to an integer</returns>
        public static int        lua_tointeger (lua_State L, int idx)
        {
            return NativeMethods.lua_tointeger(L, idx);
        }

        /// <summary>
        /// Converts the Lua value at the given acceptable index to a boolean value. Like all tests in Lua, lua_toboolean returns true for any Lua value different from false and nil; otherwise it returns false. It also returns false when called with a non-valid index. (If you want to accept only actual boolean values, use lua_isboolean to test the value's type.)
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to return as a bool</param>
        /// <returns>Returns the value at the stack index as a bool</returns>
        public static bool       lua_toboolean (lua_State L, int idx)
        {
            return NativeMethods.lua_toboolean(L, idx);
        }

        /// <summary>
        /// Converts the Lua value at the given acceptable index to a string. If len is not NULL, it also sets len with the string length. The Lua value must be a string or a number; otherwise, the function returns null. If the value is a number, then lua_tolstring also changes the actual value in the stack to a string. (This change confuses lua_next when lua_tolstring is applied to keys during a table traversal.)
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to convert and return</param>
        /// <param name="len">Contains the length of the string on return</param>
        /// <returns>Returns the value at the stack index converted to a string</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static string            lua_tolstring(lua_State L, int idx, out int len)
        {
            IntPtr ptr = NativeMethods.lua_tolstring(L, idx, out len);
            string str = Marshal.PtrToStringAnsi(ptr, len);
            return str;
        }

        /// <summary>
        /// UNTESTED
        /// Returns the "length" of the value at the given acceptable index: for strings, this is the string length; for tables, this is the result of the length operator ('#'); for userdata, this is the size of the block of memory allocated for the userdata; for other values, it is 0.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to inspect</param>
        /// <returns></returns>
        public static int        lua_objlen (lua_State L, int idx)
        {
            return NativeMethods.lua_objlen(L, idx);
        }

        /// <summary>
        /// UNTESTED
        /// Converts a value at the given acceptable index to a C# function. That value must be a C function; otherwise, returns null.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to return</param>
        /// <returns>Returns value at given index as a C# function</returns>
        public static lua_CFunction  lua_tocfunction (lua_State L, int idx)
        {
            return NativeMethods.lua_tocfunction(L, idx);
        }

        /// <summary>
        /// If the value at the given acceptable index is a full userdata, returns its block address. If the value is a light userdata, returns its pointer. Otherwise, returns IntPtr(0).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to return as userdata</param>
        /// <returns>Returns value at given index an IntPtr</returns>
        public static IntPtr         lua_touserdata (lua_State L, int idx)
        {
            return NativeMethods.lua_touserdata(L, idx);
        }

        /// <summary>
        /// UNTESTED
        /// Converts the value at the given acceptable index to a Lua thread (represented as lua_State*). This value must be a thread; otherwise, the function returns IntPtr(0).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to return as lua_State</param>
        /// <returns>Returns value at given index as lua_State thread</returns>
        public static lua_State      lua_tothread (lua_State L, int idx)
        {
            return NativeMethods.lua_tothread(L, idx);
        }

        /// <summary>
        /// Converts the value at the given acceptable index to an IntPtr. The value can be a userdata, a table, a thread, or a function; otherwise, lua_topointer returns IntPtr(0). Different objects will give different pointers. There is no way to convert the pointer back to its original value.
        /// Typically this function is used only for debug information. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to convert to IntPtr</param>
        /// <returns>Returns value at given index to a pointer in an IntPtr</returns>
        public static IntPtr         lua_topointer (lua_State L, int idx)
        {
            return NativeMethods.lua_topointer(L, idx);
        }

        /*
        ** push functions (C -> stack)
        */
        /// <summary>
        /// Pushes a nil value onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        public static void lua_pushnil(lua_State L)
        {
            NativeMethods.lua_pushnil(L);
        }

        /// <summary>
        /// Pushes a number with value n onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Number to push</param>
        public static void lua_pushnumber(lua_State L, lua_Number n)
        {
            NativeMethods.lua_pushnumber(L, n);
        }

        /// <summary>
        /// Pushes a number with value n onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Number to push</param>
        public static void lua_pushinteger(lua_State L, lua_Integer n)
        {
            NativeMethods.lua_pushinteger(L, n);
        }

        /// <summary>
        /// Pushes the string pointed to by s with size len onto the stack.
        /// The string can contain embedded zeros.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">String to push</param>
        /// <param name="l">Length of string</param>
        public static void  lua_pushlstring (lua_State L, string s, int l)
        {
            NativeMethods.lua_pushlstring(L, s, l);
        }

        /// <summary>
        /// Pushes the string pointed to by s onto the stack.
        /// The string cannot contain embedded zeros; it is assumed to end at the first zero.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">String to push</param>
        public static void  lua_pushstring (lua_State L, string s)
        {
            NativeMethods.lua_pushstring(L, s);
        }

        /// <summary>
        /// Pushes onto the stack a formatted string (Similar to String.Format) and returns a copy of the string.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="fmt">Format string, see String.Format</param>
        /// <param name="args">Variable arguments</param>
        /// <returns>Returns the final string</returns>
        public static string lua_pushvfstring(lua_State L, string fmt, params object[] args)
        {
            string output = String.Format(fmt, args);
            lua_pushstring(L, output);
            return output;
        }

        /// <summary>
        /// Pushes onto the stack a formatted string (Similar to String.Format) and returns a copy of the string.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="fmt">Format string, see String.Format</param>
        /// <param name="args">Variable arguments</param>
        /// <returns>Returns the final string</returns>
        public static string lua_pushfstring(lua_State L, string fmt, params object[] args)
        {
            return lua_pushvfstring(L, fmt, args);
        }

        /// <summary>
        /// Pushes a new C# closure onto the stack.
        /// To associate values with a C# function, first these values should be pushed onto the stack (when there are multiple values, the first value is pushed first).
        /// Then lua_pushcclosure is called to create and push the C# function onto the stack, with the argument n telling how many values should be associated with the function.
        /// lua_pushcclosure also pops these values from the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="fn">Function to push</param>
        /// <param name="n">Number of closure "up values" on the stack.</param>
        public static void  lua_pushcclosure (lua_State L, lua_CFunction fn, int n)
        {
            NativeMethods.lua_pushcclosure(L, fn, n);
        }

        /// <summary>
        /// Pushes a boolean value with value b onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="b">Boolean value (0 for false, anything else for true)</param>
        public static void  lua_pushboolean (lua_State L, int b)
        {
            NativeMethods.lua_pushboolean(L, b);
        }

        /// <summary>
        /// Pushes a light userdata onto the stack.
        /// A light userdata represents an IntPtr to a C# object.
        /// It is a value (like a number): you do not create it, it has no individual metatable, and it is not collected (as it was never created).
        /// A light userdata is equal to "any" light userdata with the same address.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="o">IntPtr to object to push</param>
        public static void lua_pushlightuserdata(lua_State L, IntPtr o)
        {
            NativeMethods.lua_pushlightuserdata(L, o);
        }

        /// <summary>
        /// UNTESTED
        /// Pushes the thread represented by L onto the stack. Returns 1 if this thread is the main thread of its state.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns 1 if this thread is the main thread of its state.</returns>
        public static int   lua_pushthread (lua_State L)
        {
            return NativeMethods.lua_pushthread(L);
        }

        /*
        ** get functions (Lua -> stack)
        */
        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given valid index and k is the value at the top of the stack.
        /// This function pops the key from the stack (putting the resulting value in its place).
        /// As in Lua, this function may trigger a metamethod for the "index" event.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        public static void  lua_gettable (lua_State L, int idx)
        {
            NativeMethods.lua_gettable(L, idx);
        }

        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given valid index.
        /// As in Lua, this function may trigger a metamethod for the "index" event.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <param name="k">Key to retrieve</param>
        public static void  lua_getfield (lua_State L, int idx, string k)
        {
            NativeMethods.lua_getfield(L, idx, k);
        }

        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given valid index and k is the value at the top of the stack.
        /// This function pops the key from the stack (putting the resulting value in its place).
        /// Similar to lua_gettable, but does a raw access (i.e., without metamethods).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        public static void  lua_rawget (lua_State L, int idx)
        {
            NativeMethods.lua_rawget(L, idx);
        }

        /// <summary>
        /// Pushes onto the stack the value t[n], where t is the value at the given valid index.
        /// The access is raw; that is, it does not invoke metamethods.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <param name="n">Key to retrieve</param>
        public static void  lua_rawgeti (lua_State L, int idx, int n)
        {
            NativeMethods.lua_rawgeti(L, idx, n);
        }

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack.
        /// The new table has space pre-allocated for narr array elements and nrec non-array elements.
        /// This pre-allocation is useful when you know exactly how many elements the table will have.
        /// Otherwise you can use the function lua_newtable. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narr">Number of array elements</param>
        /// <param name="nrec">Number of non-array elements</param>
        public static void  lua_createtable (lua_State L, int narr, int nrec)
        {
            NativeMethods.lua_createtable(L, narr, nrec);
        }

        /// <summary>
        /// This function allocates a new block of memory with the given size, pushes onto the stack a new full userdata with the block address, and returns this address. 
        /// A full userdata represents a block of memory.
        /// It is an object (like a table): you must create it, it can have its own metatable, and you can detect when it is being collected.
        /// A full userdata is only equal to itself (under raw equality). 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="sz">Size of new block</param>
        /// <returns>Returns an IntPtr pointing to the new memory</returns>
        public static IntPtr lua_newuserdata (lua_State L, int sz)
        {
            return NativeMethods.lua_newuserdata(L, sz);
        }

        /// <summary>
        /// Pushes onto the stack the metatable of the value at the given acceptable index.
        /// If the index is not valid, or if the value does not have a metatable, the function returns 0 and pushes nothing on the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="objindex">Index of the object whose metatable should be pushed</param>
        /// <returns>Returns 0 if the object does not have a metatable or the index is invalid</returns>
        public static int   lua_getmetatable (lua_State L, int objindex)
        {
            return NativeMethods.lua_getmetatable(L, objindex);
        }

        /// <summary>
        /// Pushes onto the stack the environment table of the value at the given index. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index whose environment table is to be pushed</param>
        public static void  lua_getfenv (lua_State L, int idx)
        {
            NativeMethods.lua_getfenv(L, idx);
        }

        /*
        ** set functions (stack -> Lua)
        */
        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given valid index, v is the value at the top of the stack, and k is the value just below the top.
        /// This function pops both the key and the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        public static void  lua_settable (lua_State L, int idx)
        {
            NativeMethods.lua_settable(L, idx);
        }

        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given valid index and v is the value at the top of the stack.
        /// This function pops the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <param name="k">Key of value</param>
        public static void  lua_setfield (lua_State L, int idx, string k)
        {
            NativeMethods.lua_setfield(L, idx, k);
        }

        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given valid index, v is the value at the top of the stack, and k is the value just below the top.
        /// This function pops both the key and the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// Similar to lua_settable, but does a raw assignment (i.e., without metamethods).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        public static void  lua_rawset (lua_State L, int idx)
        {
            NativeMethods.lua_rawset(L, idx);
        }

        /// <summary>
        /// Does the equivalent of t[n] = v, where t is the value at the given valid index and v is the value at the top of the stack.
        /// This function pops the value from the stack. The assignment is raw; that is, it does not invoke metamethods. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <param name="n">Key of value</param>
        public static void  lua_rawseti (lua_State L, int idx, int n)
        {
            NativeMethods.lua_rawseti(L, idx, n);
        }

        /// <summary>
        /// Pops a table from the stack and sets it as the new metatable for the value at the given acceptable index.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="objindex">Index of object on which to apply the metatable</param>
        /// <returns></returns>
        public static int   lua_setmetatable (lua_State L, int objindex)
        {
            return NativeMethods.lua_setmetatable(L, objindex);
        }

        /// <summary>
        /// Pops a table from the stack and sets it as the new environment for the value at the given index.
        /// If the value at the given index is neither a function nor a thread nor a userdata, lua_setfenv returns 0.
        /// Otherwise it returns 1.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value on which to apply the environment</param>
        /// <returns>Returns 0 on error and 1 on success</returns>
        public static int   lua_setfenv (lua_State L, int idx)
        {
            return NativeMethods.lua_setfenv(L, idx);
        }

        /*
        ** `load' and `call' functions (load and run Lua code)
        */
        /// <summary>
        /// Calls a function.
        /// To call a function you must use the following protocol: first, the function to be called is pushed onto the stack; then, the arguments to the function are pushed in direct order; the first argument is pushed first.
        /// All arguments and the function value are popped from the stack when the function is called. The function results are pushed onto the stack when the function returns.
        /// The number of results is adjusted to nresults, unless nresults is LUA_MULTRET. In this case, all results from the function are pushed.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="nargs">Number of arguments</param>
        /// <param name="nresults">Number of expected results or LUA_MULTRET to get all results</param>
        public static void lua_call(lua_State L, int nargs, int nresults)
        {
            NativeMethods.lua_call(L, nargs, nresults);
        }

        /// <summary>
        /// Calls a function in protected mode. 
        /// To call a function you must use the following protocol: first, the function to be called is pushed onto the stack; then, the arguments to the function are pushed in direct order; the first argument is pushed first.
        /// All arguments and the function value are popped from the stack when the function is called. The function results are pushed onto the stack when the function returns.
        /// The number of results is adjusted to nresults, unless nresults is LUA_MULTRET. In this case, all results from the function are pushed.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="nargs">Number of arguments</param>
        /// <param name="nresults">Number of expected results or LUA_MULTRET to get all results</param>
        /// <param name="errfunc">If errfunc is 0, then the error message returned on the stack is exactly the original error message. Otherwise, errfunc is the stack index of an error handler function. This index cannot be a pseudo-index. In case of runtime errors, this function will be called with the error message and its return value will be the message returned on the stack by lua_pcall.</param>
        /// <returns>0 on success.  LUA_ERRRUN for a runtime error.  LUA_ERRMEM for a memory allocation error.  LUA_ERRERR for an error while running the error handler function.</returns>
        public static int lua_pcall(lua_State L, int nargs, int nresults, int errfunc)
        {
            return NativeMethods.lua_pcall(L, nargs, nresults, errfunc);
        }

        /// <summary>
        /// UNTESTED
        /// Calls the C# function func in protected mode.
        /// func starts with only one element in its stack, a light userdata containing ud.
        /// All values returned by func are discarded. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="func">Function to call</param>
        /// <param name="ud">Parameter to function (passed as lightuserdata)</param>
        /// <returns>Returns the same values as lua_pcall</returns>
        public static int lua_cpcall(lua_State L, lua_CFunction func, IntPtr ud)
        {
            return NativeMethods.lua_cpcall(L, func, ud);
        }

        //[DllImport("lua5.1.dll")]
        //public static extern int lua_load(lua_State L, lua_Reader reader, IntPtr dt, string chunkname);

        /*
        ** miscellaneous functions
        */

        /// <summary>
        /// Generates a Lua error.
        /// The error message (which can actually be a Lua value of any type) must be on the stack top.
        /// This function does a long jump, and therefore never returns.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Does not return</returns>
        public static int    lua_error (lua_State L)
        {
            return NativeMethods.lua_error(L);
        }

        /// <summary>
        /// Pops a key from the stack, and pushes a key-value pair from the table at the given index (the "next" pair after the given key).
        /// If there are no more elements in the table, then lua_next returns 0 (and pushes nothing).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <returns>Returns 0 if there are no more elements in the table</returns>
        public static int    lua_next (lua_State L, int idx)
        {
            return NativeMethods.lua_next(L, idx);
        }

        /// <summary>
        /// Concatenates the n values at the top of the stack, pops them, and leaves the result at the top.
        /// If n is 1, the result is the single value on the stack (that is, the function does nothing);
        /// if n is 0, the result is the empty string.
        /// Concatenation is performed following the usual semantics of Lua. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Number of values to concatenate</param>
        public static void   lua_concat (lua_State L, int n)
        {
            NativeMethods.lua_concat(L, n);
        }

        //[DllImport("lua5.1.dll")]
        //public static extern lua_Alloc  lua_getallocf (lua_State L, IntPtr ud);

        //[DllImport("lua5.1.dll")]
        //public static extern void lua_setallocf (lua_State L, lua_Alloc f, IntPtr ud);

        /* 
        ** ===============================================================
        ** some useful macros
        ** ===============================================================
        */

        /// <summary>
        /// Pops n elements from the stack. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Number of elements to pop from the stack</param>
        public static void lua_pop(lua_State L, int n)
        {
            lua_settop(L, -(n) - 1);
        }

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack. It is equivalent to lua_createtable(L, 0, 0). 
        /// </summary>
        /// <param name="L">Lua state</param>
        public static void lua_newtable(lua_State L) { lua_createtable(L, 0, 0); }

        /// <summary>
        /// Sets the C# function f as the new value of global name n.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Name of global function</param>
        /// <param name="f">Function to set</param>
        public static void lua_register(lua_State L, string n, lua_CFunction f)
        {
            lua_pushcfunction(L, (f));
            lua_setglobal(L, (n));
        }

        /// <summary>
        /// Pushes a C# function onto the stack.
        /// Equivalent to lua_pushcclosure(L, f, 0)
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="f">Function to push</param>
        public static void lua_pushcfunction(lua_State L, lua_CFunction f)
        {
            lua_pushcclosure(L, (f), 0);
        }

        /// <summary>
        /// Returns the length of the string at the given index.
        /// Equivalent to lua_objlen.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="i">Index of string, who's length to return</param>
        /// <returns></returns>
        public static int lua_strlen(lua_State L, int i)
        {
            return lua_objlen(L, (i));
        }

        /// <summary>
        /// Returns true if the value at the given acceptable index is a function (either C# or Lua), and false otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Index of value to verify is a function</param>
        /// <returns>Returns true if the value at the given index is a function and false otherwise.</returns>
        public static bool lua_isfunction(lua_State L, int n)
        {
            return (lua_type(L, (n)) == LUA_TFUNCTION);
        }

        /// <summary>
        /// Returns true if the value at the given acceptable index is a table, and false otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Index of value to verify is a table</param>
        /// <returns>Returns true if the value at the given acceptable index is a table, and false otherwise.</returns>
        public static bool lua_istable(lua_State L, int n)
        {
            return (lua_type(L, (n)) == LUA_TTABLE);
        }

        /// <summary>
        /// Returns false if the value at the given acceptable index is a light userdata, and false otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Index of value to verify is a light userdata</param>
        /// <returns>Returns false if the value at the given acceptable index is a light userdata, and false otherwise. </returns>
        public static bool lua_islightuserdata(lua_State L,int n)
        {
            return (lua_type(L, (n)) == LUA_TLIGHTUSERDATA);
        }

        /// <summary>
        /// Returns true if the value at the given acceptable index is nil, and false otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Index of value to verify is nil</param>
        /// <returns>Returns true if the value at the given acceptable index is nil, and false otherwise.</returns>
        public static bool lua_isnil(lua_State L, int n)
        {
            return (lua_type(L, (n)) == LUA_TNIL);
        }

        /// <summary>
        /// Returns true if the value at the given acceptable index has type boolean, and false otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Index of value to verify is a boolean</param>
        /// <returns>Returns true if the value at the given acceptable index has type boolean, and false otherwise.</returns>
        public static bool lua_isboolean(lua_State L, int n)
        {
            return (lua_type(L, (n)) == LUA_TBOOLEAN);
        }

        /// <summary>
        /// Returns true if the value at the given acceptable index is a thread, and false otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Index of value to verify is a thread</param>
        /// <returns>Returns true if the value at the given acceptable index is a thread, and false otherwise.</returns>
        public static bool lua_isthread(lua_State L, int n)
        {
            return (lua_type(L, (n)) == LUA_TTHREAD);
        }

        /// <summary>
        /// Returns true if the given index is not valid, and false otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Index to verify is invalid</param>
        /// <returns>Returns true if the given index is not valid, and false otherwise.</returns>
        public static bool lua_isnone(lua_State L, int n)
        {
            return  (lua_type(L, (n)) == LUA_TNONE);
        }

        /// <summary>
        /// Returns true if the given index is not valid or the value at that index is nil, and false otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Index to verify is invalid or nil</param>
        /// <returns>Returns true if the given index is not valid or the value at that index is nil, and false otherwise.</returns>
        public static bool lua_isnoneornil(lua_State L, int n)
        {
            return (lua_type(L, (n)) <= 0);
        }

        /// <summary>
        /// Pops a value from the stack and sets it as the new value of global name.
        /// Equivalent to lua_setfield(L, LUA_GLOBALSINDEX, s)
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">Global name of value</param>
        public static void lua_setglobal(lua_State L, string s)
        {
            lua_setfield(L, LUA_GLOBALSINDEX, (s));
        }

        /// <summary>
        /// Pushes onto the stack the value of the global name.
        /// Equivalent to lua_getfield(L, LUA_GLOBALSINDEX, s)
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">Name of global to push</param>
        public static void lua_getglobal(lua_State L, string s)
        {
            lua_getfield(L, LUA_GLOBALSINDEX, (s));
        }

        /// <summary>
        /// Converts the Lua value at the given acceptable index to a string. The Lua value must be a string or a number; otherwise, the function returns null.
        /// If the value is a number, then lua_tostring also changes the actual value in the stack to a string. (This change confuses lua_next when lua_tostring is applied to keys during a table traversal.)
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="i">Index of value to convert and return</param>
        /// <returns>Returns the value at the stack index converted to a string</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static string lua_tostring(lua_State L, int i)
        {
            // The same as lua_tolstring, but we ignore the length
            int len = 0;
            return lua_tolstring(L, (i), out len);
        }

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prints out the contents of the lua stack to Console.
        /// For debugging purposes.
        /// </summary>
        /// <param name="L">Lua state</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void stackDump(lua_State L)
        {
            Console.WriteLine("BeginStack");

            int i;
            int top = lua_gettop(L);
            for (i = 1; i <= top; i++)
            {  /* repeat for each level */
                int t = lua_type(L, i);
                Console.Write("{0} : ", i);
                switch (t)
                {
                    case LUA_TSTRING:  /* strings */
                        Console.Write("'{0}'", lua_tostring(L, i));
                        break;

                    case LUA_TBOOLEAN:  /* booleans */
                        Console.Write(lua_toboolean(L, i) ? "true" : "false");
                        break;

                    case LUA_TNUMBER:  /* numbers */
                        Console.Write(lua_tonumber(L, i));
                        break;

                    case LUA_TTABLE:    /* table */
                        Console.Write("table[" + lua_topointer(L, i).ToInt32().ToString("X") + "]");
                        break;

                    case LUA_TLIGHTUSERDATA:    /* light user data */
                        GCHandle h = GCHandle.FromIntPtr(LuaDll.lua_touserdata(L, i));
                        Console.Write("{0} [{1:X}]", h.Target.ToString(), GCHandle.ToIntPtr(h).ToInt32());
                        break;

                    default:  /* other values */
                        Console.Write(lua_typename(L, t));
                        break;

                }
                Console.WriteLine();
            }
            Console.WriteLine("EndStack");
        }
    }
}
