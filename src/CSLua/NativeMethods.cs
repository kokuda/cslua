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
using static CSLua.LuaDll;

namespace CSLua
{
    using lua_Number = Double;
    using lua_Integer = Int32;
    using size_t = Int32;

    internal static class NativeMethods
    {
        /// <summary>
        /// Creates a new, independent state
        /// </summary>
        /// <param name="f">Allocator function</param>
        /// <param name="ud">An opaque pointer that Lua simply passes to the allocator in every call</param>
        /// <returns>Returns a new lua state.  Returns null if cannot create the state (due to lack of memory)</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_State lua_newstate(lua_Alloc f, IntPtr ud);

        /// <summary>
        /// Destroys all objects in the given Lua state (calling the corresponding garbage-collection metamethods, if any) and frees all dynamic memory used by this state.
        /// </summary>
        /// <param name="L">Lua state to close</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_close(lua_State L);

        /// <summary>
        /// Creates a new thread, pushes it on the stack, and returns a pointer to a lua_State that represents this new thread.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_State lua_newthread(lua_State L);

        /// <summary>
        /// Sets a new panic function and returns the old one.  If an error happens outside any protected environment, Lua calls a panic function and then calls exit(EXIT_FAILURE), thus exiting the host application.  Your panic function can avoid this exit by never returning (e.g., doing a long jump). The panic function can access the error message at the top of the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="panicf">Panic function</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_CFunction lua_atpanic(lua_State L, lua_CFunction panicf);

        /*
        ** basic stack manipulation
        */
        /// <summary>
        /// Returns the index of the top element in the stack.
        /// Because indices start at 1, this result is equal to the number of elements in the stack (and so 0 means an empty stack).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns the index of the top element in the stack</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_gettop(lua_State L);

        /// <summary>
        /// Accepts any acceptable index, or 0, and sets the stack top to this index.
        /// If the new top is larger than the old one, then the new elements are filled with nil.
        /// If index is 0, then all stack elements are removed.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">New top</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_settop(lua_State L, int idx);

        /// <summary>
        /// Pushes a copy of the element at the given valid index onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of element to push</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_pushvalue(lua_State L, int idx);

        /// <summary>
        /// Removes the element at the given valid index, shifting down the elements above this index to fill the gap. Cannot be called with a pseudo-index, because a pseudo-index is not an actual stack position.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to remove</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_remove(lua_State L, int idx);

        /// <summary>
        /// Moves the top element into the given valid index, shifting up the elements above this index to open space. Cannot be called with a pseudo-index, because a pseudo-index is not an actual stack position.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">New index for the top element.  Cannot be a pseudo-index</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_insert(lua_State L, int idx);

        /// <summary>
        /// Moves the top element into the given position (and pops it), without shifting any element (therefore replacing the value at the given position).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">New index</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_replace(lua_State L, int idx);

        /// <summary>
        /// Ensures that there are at least extra free stack slots in the stack. It returns false if it cannot grow the stack to that size. This function never shrinks the stack; if the stack is already larger than the new size, it is left unchanged.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="sz">Size to check</param>
        /// <returns>Returns 0 if it cannot grow the stack to that size.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_checkstack(lua_State L, int sz);

        /*
        ** access functions (stack -> C)
        */
        /// <summary>
        /// Returns 1 if the value at the given acceptable index is a number or a string convertible to a number, and 0 otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to check</param>
        /// <returns>Returns 1 if the value at the given acceptable index is a number or a string convertible to a number, and 0 otherwise.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_isnumber(lua_State L, int idx);

        /// <summary>
        /// Returns 1 if the value at the given acceptable index is a string or a number (which is always convertible to a string), and 0 otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to check</param>
        /// <returns>Returns 1 if the value at the given acceptable index is a string or a number (which is always convertible to a string), and 0 otherwise.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_isstring(lua_State L, int idx);

        /// <summary>
        /// Returns 1 if the value at the given acceptable index is a C function, and 0 otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to check</param>
        /// <returns>Returns 1 if the value at the given acceptable index is a C function, and 0 otherwise.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_iscfunction(lua_State L, int idx);

        /// <summary>
        /// Returns 1 if the value at the given acceptable index is a userdata (either full or light), and 0 otherwise.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index to check</param>
        /// <returns>Returns 1 if the value at the given acceptable index is a userdata (either full or light), and 0 otherwise.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_isuserdata(lua_State L, int idx);

        /// <summary>
        /// Returns the type of the value in the given acceptable index, or LUA_TNONE for a non-valid index (that is, an index to an "empty" stack position).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value</param>
        /// <returns>Returns one of the following types LUA_TNIL, LUA_TNUMBER, LUA_TBOOLEAN, LUA_TSTRING, LUA_TTABLE, LUA_TFUNCTION, LUA_TUSERDATA, LUA_TTHREAD, and LUA_TLIGHTUSERDATA.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_type(lua_State L, int idx);

        /// <summary>
        /// Returns the name of the type encoded by the value tp, which must be one the values returned by lua_type.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="tp">Type to encode</param>
        /// <returns>Name of type</returns>

        [DllImport("lua5.1.dll", EntryPoint = "lua_typename")]
        internal static extern IntPtr __lua_typename(lua_State L, int tp);

        /// <summary>
        /// Returns 1 if the two values in acceptable indices index1 and index2 are equal, following the semantics of the Lua == operator (that is, may call metamethods). Otherwise returns 0. Also returns 0 if any of the indices is non valid.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx1">Index of first value</param>
        /// <param name="idx2">Index of second value</param>
        /// <returns>Returns 1 if the two values in acceptable indices index1 and index2 are equal, following the semantics of the Lua == operator (that is, may call metamethods). Otherwise returns 0. Also returns 0 if any of the indices is non valid.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_equal(lua_State L, int idx1, int idx2);

        /// <summary>
        /// Returns 1 if the two values in acceptable indices index1 and index2 are primitively equal (that is, without calling metamethods). Otherwise returns 0. Also returns 0 if any of the indices are non valid.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx1">Index of first value</param>
        /// <param name="idx2">Index of second value</param>
        /// <returns>Returns 1 if the two values in acceptable indices index1 and index2 are primitively equal (that is, without calling metamethods). Otherwise returns 0. Also returns 0 if any of the indices are non valid.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_rawequal(lua_State L, int idx1, int idx2);

        /// <summary>
        /// Returns 1 if the value at acceptable index index1 is smaller than the value at acceptable index index2, following the semantics of the Lua < operator (that is, may call metamethods). Otherwise returns 0. Also returns 0 if any of the indices is non valid.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx1">Index of first value</param>
        /// <param name="idx2">Index of second value</param>
        /// <returns>Returns 1 if the value at acceptable index index1 is smaller than the value at acceptable index index2. Otherwise returns 0. Also returns 0 if any of the indices is non valid.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_lessthan(lua_State L, int idx1, int idx2);

        /// <summary>
        /// Converts the Lua value at the given acceptable index to the C type lua_Number.
        /// The Lua value must be a number or a string convertible to a number; otherwise, lua_tonumber returns 0.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to convert to a number and return</param>
        /// <returns>Returns the value at stack index idx, converted to a number</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_Number lua_tonumber(lua_State L, int idx);

        /// <summary>
        /// Converts the Lua value at the given acceptable index to a signed integer.
        /// The Lua value must be a number or a string convertible to a number; otherwise, lua_tointeger returns 0.
        /// If the number is not an integer, it is truncated in some non-specified way. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to convert to an integer and return</param>
        /// <returns>Returns the value at stack index idx, converted to an integer</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_tointeger(lua_State L, int idx);

        /// <summary>
        /// Converts the Lua value at the given acceptable index to a boolean value. Like all tests in Lua, lua_toboolean returns true for any Lua value different from false and nil; otherwise it returns false. It also returns false when called with a non-valid index. (If you want to accept only actual boolean values, use lua_isboolean to test the value's type.)
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to return as a bool</param>
        /// <returns>Returns the value at the stack index as a bool</returns>
        [DllImport("lua5.1.dll")]
        internal static extern bool lua_toboolean(lua_State L, int idx);

        /// <summary>
        /// Converts the Lua value at the given acceptable index to a string. If len is not NULL, it also sets len with the string length. The Lua value must be a string or a number; otherwise, the function returns null. If the value is a number, then lua_tolstring also changes the actual value in the stack to a string. (This change confuses lua_next when lua_tolstring is applied to keys during a table traversal.)
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to convert and return</param>
        /// <param name="len">Contains the length of the string on return</param>
        /// <returns>Returns the value at the stack index converted to a string</returns>
        [DllImport("lua5.1.dll", EntryPoint = "lua_tolstring")]
        internal static extern IntPtr lua_tolstring(lua_State L, int idx, out int len);

        /// <summary>
        /// UNTESTED
        /// Returns the "length" of the value at the given acceptable index: for strings, this is the string length; for tables, this is the result of the length operator ('#'); for userdata, this is the size of the block of memory allocated for the userdata; for other values, it is 0.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to inspect</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_objlen(lua_State L, int idx);

        /// <summary>
        /// UNTESTED
        /// Converts a value at the given acceptable index to a C# function. That value must be a C function; otherwise, returns null.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to return</param>
        /// <returns>Returns value at given index as a C# function</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_CFunction lua_tocfunction(lua_State L, int idx);

        /// <summary>
        /// If the value at the given acceptable index is a full userdata, returns its block address. If the value is a light userdata, returns its pointer. Otherwise, returns IntPtr(0).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to return as userdata</param>
        /// <returns>Returns value at given index an IntPtr</returns>
        [DllImport("lua5.1.dll")]
        internal static extern IntPtr lua_touserdata(lua_State L, int idx);

        /// <summary>
        /// UNTESTED
        /// Converts the value at the given acceptable index to a Lua thread (represented as lua_State*). This value must be a thread; otherwise, the function returns IntPtr(0).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to return as lua_State</param>
        /// <returns>Returns value at given index as lua_State thread</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_State lua_tothread(lua_State L, int idx);

        /// <summary>
        /// Converts the value at the given acceptable index to an IntPtr. The value can be a userdata, a table, a thread, or a function; otherwise, lua_topointer returns IntPtr(0). Different objects will give different pointers. There is no way to convert the pointer back to its original value.
        /// Typically this function is used only for debug information. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value to convert to IntPtr</param>
        /// <returns>Returns value at given index to a pointer in an IntPtr</returns>
        [DllImport("lua5.1.dll")]
        internal static extern IntPtr lua_topointer(lua_State L, int idx);

        /*
        ** push functions (C -> stack)
        */
        /// <summary>
        /// Pushes a nil value onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_pushnil(lua_State L);

        /// <summary>
        /// Pushes a number with value n onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Number to push</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_pushnumber(lua_State L, lua_Number n);

        /// <summary>
        /// Pushes a number with value n onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Number to push</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_pushinteger(lua_State L, lua_Integer n);

        /// <summary>
        /// Pushes the string pointed to by s with size len onto the stack.
        /// The string can contain embedded zeros.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">String to push</param>
        /// <param name="l">Length of string</param>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern void lua_pushlstring(lua_State L, [MarshalAs(UnmanagedType.LPStr)]string s, int l);

        /// <summary>
        /// Pushes the string pointed to by s onto the stack.
        /// The string cannot contain embedded zeros; it is assumed to end at the first zero.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="s">String to push</param>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern void lua_pushstring(lua_State L, [MarshalAs(UnmanagedType.LPStr)]string s);


        /// <summary>
        /// Pushes a new C# closure onto the stack.
        /// To associate values with a C# function, first these values should be pushed onto the stack (when there are multiple values, the first value is pushed first).
        /// Then lua_pushcclosure is called to create and push the C# function onto the stack, with the argument n telling how many values should be associated with the function.
        /// lua_pushcclosure also pops these values from the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="fn">Function to push</param>
        /// <param name="n">Number of closure "up values" on the stack.</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_pushcclosure(lua_State L, lua_CFunction fn, int n);

        /// <summary>
        /// Pushes a boolean value with value b onto the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="b">Boolean value (0 for false, anything else for true)</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_pushboolean(lua_State L, int b);

        /// <summary>
        /// Pushes a light userdata onto the stack.
        /// A light userdata represents an IntPtr to a C# object.
        /// It is a value (like a number): you do not create it, it has no individual metatable, and it is not collected (as it was never created).
        /// A light userdata is equal to "any" light userdata with the same address.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="o">IntPtr to object to push</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_pushlightuserdata(lua_State L, IntPtr o);

        /// <summary>
        /// UNTESTED
        /// Pushes the thread represented by L onto the stack. Returns 1 if this thread is the main thread of its state.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns 1 if this thread is the main thread of its state.</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_pushthread(lua_State L);

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
        [DllImport("lua5.1.dll")]
        internal static extern void lua_gettable(lua_State L, int idx);

        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given valid index.
        /// As in Lua, this function may trigger a metamethod for the "index" event.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <param name="k">Key to retrieve</param>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern void lua_getfield(lua_State L, int idx, [MarshalAs(UnmanagedType.LPStr)]string k);

        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the given valid index and k is the value at the top of the stack.
        /// This function pops the key from the stack (putting the resulting value in its place).
        /// Similar to lua_gettable, but does a raw access (i.e., without metamethods).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_rawget(lua_State L, int idx);

        /// <summary>
        /// Pushes onto the stack the value t[n], where t is the value at the given valid index.
        /// The access is raw; that is, it does not invoke metamethods.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <param name="n">Key to retrieve</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_rawgeti(lua_State L, int idx, int n);

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack.
        /// The new table has space pre-allocated for narr array elements and nrec non-array elements.
        /// This pre-allocation is useful when you know exactly how many elements the table will have.
        /// Otherwise you can use the function lua_newtable. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narr">Number of array elements</param>
        /// <param name="nrec">Number of non-array elements</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_createtable(lua_State L, int narr, int nrec);

        /// <summary>
        /// This function allocates a new block of memory with the given size, pushes onto the stack a new full userdata with the block address, and returns this address. 
        /// A full userdata represents a block of memory.
        /// It is an object (like a table): you must create it, it can have its own metatable, and you can detect when it is being collected.
        /// A full userdata is only equal to itself (under raw equality). 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="sz">Size of new block</param>
        /// <returns>Returns an IntPtr pointing to the new memory</returns>
        [DllImport("lua5.1.dll")]
        internal static extern IntPtr lua_newuserdata(lua_State L, int sz);

        /// <summary>
        /// Pushes onto the stack the metatable of the value at the given acceptable index.
        /// If the index is not valid, or if the value does not have a metatable, the function returns 0 and pushes nothing on the stack.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="objindex">Index of the object whose metatable should be pushed</param>
        /// <returns>Returns 0 if the object does not have a metatable or the index is invalid</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_getmetatable(lua_State L, int objindex);

        /// <summary>
        /// Pushes onto the stack the environment table of the value at the given index. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index whose environment table is to be pushed</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_getfenv(lua_State L, int idx);

        /*
        ** set functions (stack -> Lua)
        */
        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given valid index, v is the value at the top of the stack, and k is the value just below the top.
        /// This function pops both the key and the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_settable(lua_State L, int idx);

        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given valid index and v is the value at the top of the stack.
        /// This function pops the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <param name="k">Key of value</param>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern void lua_setfield(lua_State L, int idx, [MarshalAs(UnmanagedType.LPStr)]string k);

        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the given valid index, v is the value at the top of the stack, and k is the value just below the top.
        /// This function pops both the key and the value from the stack. As in Lua, this function may trigger a metamethod for the "newindex" event.
        /// Similar to lua_settable, but does a raw assignment (i.e., without metamethods).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_rawset(lua_State L, int idx);

        /// <summary>
        /// Does the equivalent of t[n] = v, where t is the value at the given valid index and v is the value at the top of the stack.
        /// This function pops the value from the stack. The assignment is raw; that is, it does not invoke metamethods. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <param name="n">Key of value</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_rawseti(lua_State L, int idx, int n);

        /// <summary>
        /// Pops a table from the stack and sets it as the new metatable for the value at the given acceptable index.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="objindex">Index of object on which to apply the metatable</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_setmetatable(lua_State L, int objindex);

        /// <summary>
        /// Pops a table from the stack and sets it as the new environment for the value at the given index.
        /// If the value at the given index is neither a function nor a thread nor a userdata, lua_setfenv returns 0.
        /// Otherwise it returns 1.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of value on which to apply the environment</param>
        /// <returns>Returns 0 on error and 1 on success</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_setfenv(lua_State L, int idx);

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
        [DllImport("lua5.1.dll")]
        internal static extern void lua_call(lua_State L, int nargs, int nresults);

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
        [DllImport("lua5.1.dll")]
        internal static extern int lua_pcall(lua_State L, int nargs, int nresults, int errfunc);

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
        [DllImport("lua5.1.dll")]
        internal static extern int lua_cpcall(lua_State L, lua_CFunction func, IntPtr ud);

        //[DllImport("lua5.1.dll")]
        //internal static extern int lua_load(lua_State L, lua_Reader reader, IntPtr dt, string chunkname);

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
        [DllImport("lua5.1.dll")]
        internal static extern int lua_error(lua_State L);

        /// <summary>
        /// Pops a key from the stack, and pushes a key-value pair from the table at the given index (the "next" pair after the given key).
        /// If there are no more elements in the table, then lua_next returns 0 (and pushes nothing).
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="idx">Index of table</param>
        /// <returns>Returns 0 if there are no more elements in the table</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int lua_next(lua_State L, int idx);

        /// <summary>
        /// Concatenates the n values at the top of the stack, pops them, and leaves the result at the top.
        /// If n is 1, the result is the single value on the stack (that is, the function does nothing);
        /// if n is 0, the result is the empty string.
        /// Concatenation is performed following the usual semantics of Lua. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="n">Number of values to concatenate</param>
        [DllImport("lua5.1.dll")]
        internal static extern void lua_concat(lua_State L, int n);

        //[DllImport("lua5.1.dll")]
        //internal static extern lua_Alloc  lua_getallocf (lua_State L, IntPtr ud);

        //[DllImport("lua5.1.dll")]
        //internal static extern void lua_setallocf (lua_State L, lua_Alloc f, IntPtr ud);

        ///////////////////////////////////////////////////////////////////////
        // lauxlib.h
        ///////////////////////////////////////////////////////////////////////

        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern void luaI_openlib(lua_State L, [MarshalAs(UnmanagedType.LPStr)]string libname, luaL_Reg[] l, int nup);

        /// <summary>
        /// Opens a library.
        /// When called with libname equal to null, it simply registers all functions in the list l (see luaL_Reg) into the table on the top of the stack.
        /// When called with a non-null libname, luaL_register creates a new table t, sets it as the value of the global variable libname, sets it as the value of package.loaded[libname], and registers on it all functions in the list l. If there is a table in package.loaded[libname] or in variable libname, reuses this table instead of creating a new one.
        /// In any case the function leaves the table on the top of the stack. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="libname">Name of library</param>
        /// <param name="l">Functions to register</param>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern void luaL_register(
            lua_State L, 
            [MarshalAs(UnmanagedType.LPStr)]string libname, 
            [MarshalAs(UnmanagedType.LPArray, 
            ArraySubType = UnmanagedType.Struct, 
            SizeParamIndex = 0)]
            luaL_Reg[] l
        );

        /// <summary>
        /// Pushes onto the stack the field e from the metatable of the object at index obj. If the object does not have a metatable, or if the metatable does not have this field, returns 0 and pushes nothing.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="obj">Object</param>
        /// <param name="e">Name of field</param>
        /// <returns>Returns 0 on error</returns>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_getmetafield(lua_State L, int obj, [MarshalAs(UnmanagedType.LPStr)]string e);

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
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_callmeta(lua_State L, int obj, [MarshalAs(UnmanagedType.LPStr)]string e);

        /// <summary>
        /// Generates an error with a message like the following: 
        ///      "location: bad argument narg to 'func' (tname expected, got rt)"
        /// where location is produced by luaL_where, func is the name of the current function, and rt is the type name of the actual argument. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Index of the bad argument</param>
        /// <param name="tname">Name of expected type</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_typerror(lua_State L, int narg, [MarshalAs(UnmanagedType.LPStr)]string tname);

        /// <summary>
        /// Raises an error with the following message, where func is retrieved from the call stack:
        ///     "bad argument #<narg> to <func> (<extramsg>)"
        /// This function never returns, but it is an idiom to use it in C# functions as return luaL_argerror(args).
        /// </summary>
        /// <param name="L">Lua error</param>
        /// <param name="numarg">Index of bad argument</param>
        /// <param name="extramsg">Extra message</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_argerror(lua_State L, int numarg, [MarshalAs(UnmanagedType.LPStr)]string extramsg);

        /// <summary>
        /// Checks whether the function argument narg is a string and returns this string; fills l with the string's length.
        /// This function uses lua_tolstring to get its result, so all conversions and caveats of that function apply here. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="numArg">Argument index</param>
        /// <param name="l">(out) returns containing length</param>
        /// <returns>Returns the string (if it is one)</returns>
        [DllImport("lua5.1.dll")]
        internal static extern IntPtr luaL_checklstring(lua_State L, int numArg, out size_t l);

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
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern IntPtr luaL_optlstring(lua_State L, int narg, [MarshalAs(UnmanagedType.LPStr)]string def, out size_t l);

        /// <summary>
        /// Checks whether the function argument narg is a number and returns this number. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index to check</param>
        /// <returns>Returns the number at argument index narg</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_Number luaL_checknumber(lua_State L, int narg);

        /// <summary>
        /// If the function argument narg is a number, returns this number.
        /// If this argument is absent or is nil, returns d.
        /// Otherwise, raises an error.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        /// <param name="def">Default value if narg is absent or nil</param>
        /// <returns>Returnns argument at index narg as a number, or def if it is absent or nil</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_Number luaL_optnumber(lua_State L, int narg, lua_Number def);

        /// <summary>
        /// Checks whether the function argument narg is a number and returns this number cast to a lua_Integer.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Index of argument to return</param>
        /// <returns>Returns argument at index narg as an integer</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_Integer luaL_checkinteger(lua_State L, int narg);

        /// <summary>
        /// If the function argument narg is a number, returns this number cast to a lua_Integer.
        /// If this argument is absent or is nil, returns d.
        /// Otherwise, raises an error.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        /// <param name="def">Default value if narg is nil or absent</param>
        /// <returns>Returns argument at index narg as an integer, or def it is absent or nil</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_Integer luaL_optinteger(lua_State L, int narg, lua_Integer def);

        /// <summary>
        /// Grows the stack size to top + sz elements, raising an error if the stack cannot grow to that size.
        /// msg is an additional text to go into the error message.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="sz">Number of elements to add to the stack</param>
        /// <param name="msg">Additional message to go into the error message if there is an error</param>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern void luaL_checkstack(lua_State L, int sz, [MarshalAs(UnmanagedType.LPStr)]string msg);

        /// <summary>
        /// Checks whether the function argument narg has type t.
        /// t must be one of the following values LUA_TNIL, LUA_TNUMBER, LUA_TBOOLEAN, LUA_TSTRING, LUA_TTABLE, LUA_TFUNCTION, LUA_TUSERDATA, LUA_TTHREAD, or LUA_TLIGHTUSERDATA.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        /// <param name="t">Type to confirm</param>
        [DllImport("lua5.1.dll")]
        internal static extern void luaL_checktype(lua_State L, int narg, int t);

        /// <summary>
        /// Checks whether the function has an argument of any type (including nil) at position narg.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Argument index</param>
        [DllImport("lua5.1.dll")]
        internal static extern void luaL_checkany(lua_State L, int narg);

        /// <summary>
        /// If the registry already has the key tname, returns 0.
        /// Otherwise, creates a new table to be used as a metatable for userdata, adds it to the registry with key tname, and returns 1.
        /// In both cases pushes onto the stack the final value associated with tname in the registry. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="tname">Name of metatable</param>
        /// <returns>0 if metatable by that name already exists, 1 otherwise</returns>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_newmetatable(lua_State L, [MarshalAs(UnmanagedType.LPStr)]string tname);

        /// <summary>
        /// Checks whether the function argument narg is a userdata of the type tname.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="narg">Index of argument</param>
        /// <param name="tname">Name of type</param>
        /// <returns>Returns the userdata if it is correct type, null otherwise?</returns>
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern IntPtr luaL_checkudata(lua_State L, int narg, [MarshalAs(UnmanagedType.LPStr)]string tname);

        /// <summary>
        /// Pushes onto the stack a string identifying the current position of the control at level lvl in the call stack.
        /// Typically this string has the following format:
        ///     "chunkname:currentline:"
        /// Level 0 is the running function, level 1 is the function that called the running function, etc.
        /// This function is used to build a prefix for error messages. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="lvl">Level to identify</param>
        [DllImport("lua5.1.dll")]
        internal static extern void luaL_where(lua_State L, int lvl);


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
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_checkoption(lua_State L, int narg, [MarshalAs(UnmanagedType.LPStr)]string def, [MarshalAs(UnmanagedType.LPStr)]string[] lst);

        /// <summary>
        /// Creates and returns a reference, in the table at index t, for the object at the top of the stack (and pops the object).
        /// You can retrieve an object referred by reference r by calling lua_rawgeti(L, t, r).
        /// Function luaL_unref frees a reference and its associated object.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="t">Index of table</param>
        /// <returns>Returns reference</returns>
        [DllImport("lua5.1.dll")]
        internal static extern int luaL_ref(lua_State L, int t);

        /// <summary>
        /// Releases reference ref from the table at index t (returned from luaL_ref).
        /// The entry is removed from the table, so that the referred object can be collected.
        /// The reference ref is also freed to be used again.
        /// If ref is LUA_NOREF or LUA_REFNIL, luaL_unref does nothing. 
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <param name="t">Index of table</param>
        /// <param name="r">Reference to release</param>
        [DllImport("lua5.1.dll")]
        internal static extern void luaL_unref(lua_State L, int t, int r);

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
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_loadfile(lua_State L, [MarshalAs(UnmanagedType.LPStr)]string filename);

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
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_loadbuffer(lua_State L, [MarshalAs(UnmanagedType.LPStr)]string buff, size_t sz, [MarshalAs(UnmanagedType.LPStr)]string name);

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
        [DllImport("lua5.1.dll", ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern int luaL_loadstring(lua_State L, [MarshalAs(UnmanagedType.LPStr)]string s);

        /// <summary>
        /// Creates a new Lua state.
        /// It calls lua_newstate with an allocator based on the standard C realloc function and then sets a panic function (see lua_atpanic) that prints an error message to the standard error output in case of fatal errors.
        /// Returns the new state, or IntPtr(0) if there is a memory allocation error. 
        /// </summary>
        /// <returns>New lua state</returns>
        [DllImport("lua5.1.dll")]
        internal static extern lua_State luaL_newstate();

        ///////////////////////////////////////////////////////////////////////
        // lualib.h
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// UNTESTED.
        /// Opens the basic library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern int luaopen_base(lua_State L);    /* opens the basic library */

        /// <summary>
        /// UNTESTED.
        /// Opens the table library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern int luaopen_table(lua_State L);   /* opens the table library */

        /// <summary>
        /// UNTESTED.
        /// Opens the I/O library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern int luaopen_io(lua_State L);      /* opens the I/O library */

        /// <summary>
        /// UNTESTED.
        /// Opens the string library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern int luaopen_string(lua_State L);  /* opens the string lib. */

        /// <summary>
        /// UNTESTED.
        /// Opens the math library.  Should be called through lua_call.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns></returns>
        [DllImport("lua5.1.dll")]
        internal static extern int luaopen_math(lua_State L);    /* opens the math lib. */

        /// <summary>
        /// Opens all standard Lua libraries into the given state.
        /// </summary>
        /// <param name="L">Lua state</param>
        [DllImport("lua5.1.dll")]
        internal static extern void luaL_openlibs(lua_State L);         /* opens all standard libs */

    }
}
