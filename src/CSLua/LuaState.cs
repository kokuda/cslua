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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace CSLua
{

    /// <summary>
    /// C# class interface to Lua runtime system
    /// </summary>
    public class LuaState
    {

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructor - creates a new lua state.
        /// </summary>
        public LuaState()
        {
            mLuaFunctionCallAdapterDelegate = functionCallAdapter;
            mgcAdapterDelegate = gcAdapter;
            mtostringAdapterDelegate = tostringAdapter;
            meqAdapterDelegate = eqAdapter;

            L = LuaDll.luaL_newstate();
            LuaDll.luaL_openlibs(L);

            LuaDll.lua_atpanic(L, mPanicFunctionDelegate);

            // Redirect stderr from the Lua DLL to an error file.
            //FileStream fs = new FileStream("error.txt", FileMode.Create);
            //mOutputRef = new HandleRef(fs, fs.Handle);
            //SetStdHandle(-12, mOutputRef);

            // Install a new print function (to capture stdout)
            LuaDll.lua_register(L, "print", sLuaPrintDelegate);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Destructor - destroys the lua state
        /// </summary>
        ~LuaState()
        {
            LuaDll.lua_close(L);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load and execute the string s in the contained Lua state.
        /// Lua code in s can return 0 or many values.
        /// </summary>
        /// <param name="s">String containing lua code</param>
        /// <returns>
        /// Returns results returned from executing lua code
        /// May return 0 or many return values (depending on the code).
        /// Return will be null if there are no return values.
        /// </returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public MultiValue dostring(string s)
        {
            int top = LuaDll.lua_gettop(L);
            int ret = LuaDll.luaL_dostring(L, s);
            if (ret != 0)
            {
                string err = LuaDll.lua_tostring(L, -1);
                Console.WriteLine("ERROR: " + err);
                LuaDll.lua_pop(L, 1);
            }
            else
            {
                int newtop = LuaDll.lua_gettop(L);
                return getValues(top + 1, newtop - top);
            }

            return null;
        }

        ///////////////////////////////////////////////////////////////////////

        // Call a lua function with the following parameters
        /// <summary>
        /// Call the function of the name func in the lua context passing in args as the arguments.
        /// </summary>
        /// <param name="func">Name of function to call</param>
        /// <param name="args">List of parameters to pass into function</param>
        /// <returns>Returns all results from function call.  Returns null if no results.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public MultiValue call(string func, params object[] args)
        {
            int top = LuaDll.lua_gettop(L);

            MultiValue mv = new MultiValue(args);
            LuaDll.lua_getfield(L, LuaDll.LUA_GLOBALSINDEX, func);
            int count = pushValue(mv);
            int ret = LuaDll.lua_pcall(L, count, LuaDll.LUA_MULTRET, 0);
            if (ret != 0)
            {
                string err = LuaDll.lua_tostring(L, -1);
                Console.WriteLine("ERROR: " + err);
                LuaDll.lua_pop(L, 1);
            }
            else
            {
                int newtop = LuaDll.lua_gettop(L);
                return getValues(top + 1, newtop - top);
            }

            return null;
        }

        /// <summary>
        /// Access to internal lua_State object
        /// </summary>
        public lua_State _L
        {
            get { return L; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register a C# Action (takes no arguments and returns no values) as a global function in lua.
        /// Function will be registered with its own name.
        /// </summary>
        /// <param name="f">Action to register</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void registerFunction(Action f)
        {
            registerFunction((Delegate)f);
        }

        /// <summary>
        /// Register a single parameter C# function with void return as a global lua function.
        /// Function will be registered with its own name.
        /// </summary>
        /// <typeparam name="TParameter0">Type of parameter</typeparam>
        /// <param name="f">Function to regiser</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void registerFunction<TParameter0>(Action<TParameter0> f)
        {
            registerFunction((Delegate)f);
        }

        /// <summary>
        /// Register a zero argument C# function with one return as a global lua function.
        /// Function will be registered with its own name.
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <param name="f">Function to register</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void registerFunction<TResult>(Func<TResult> f)
        {
            registerFunction((Delegate)f);
        }

        /// <summary>
        /// Register a single parameter C# function with one return as a global lua function.
        /// Function will be registered with its own name.
        /// </summary>
        /// <typeparam name="TParameter0">Type of parameter</typeparam>
        /// <typeparam name="TResult">Type of return value</typeparam>
        /// <param name="f">Function to register</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void registerFunction<TParameter0, TResult>(Func<TParameter0, TResult> f)
        {
            registerFunction((Delegate)f);
        }

        /// <summary>
        /// Register a Delegate as a global lua function.
        /// Function will be registered with its own name.
        /// </summary>
        /// <param name="f">Delegate to register</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void registerFunction(Delegate f)
        {
            System.Reflection.MethodInfo mi = f.Method;

            // Check that this function has been declared as bindable.
            // If the name is set, then bind using that name (otherwise defaults to mi.Name)
            // We don't insist that the method must have the correct attributes because it
            // is being registered manually.  Only those methods that are registered through
            // a class interface must have the correct attributes.
            string bindName = isBindable(mi) ? getBindableName(mi, mi.Name) : mi.Name;
            
            Console.WriteLine("Registering " + mi.Name + " as " + bindName);
            registerFunctionAs(mi, bindName);
        }

        /// <summary>
        /// Register a Delegate as a global function with the name 'name'.
        /// Ignores the Bindable attribute name of the function.
        /// This is useful for anonymous delegates that have unfriendly names.
        /// </summary>
        /// <param name="d">Delegate to register</param>
        /// <param name="name">Global name in lua</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void registerFunctionAs(Delegate d, string name)
        {
            Console.WriteLine("Registering " + d.Method.Name + " as " + name);
            registerFunctionAs(d.Method, name);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register a C# object as an object in lua as the global 'name'.
        /// All public functions will be made accessible from lua.
        /// </summary>
        /// <param name="name">Global name of object</param>
        /// <param name="o">Object to register</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void registerObject(string name, Object o)
        {
            // Push the object onto the stack (this will also register the class as needed)
            pushObject(o);

            // Now set the object (which is now at the top of the stack) to be a global
            // with the given name.
            LuaDll.lua_setglobal(L, name);
        }

        public void unregisterObject(Object o)
        {
            Console.WriteLine("unregisterObject not implemented");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inspects the registered class of the given object o.
        /// Prints class details to the debug Console.  For debugging purposes.
        /// </summary>
        /// <param name="o">Object whose class to inspect</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void inspectClass(Object o)
        {
            int top = LuaDll.lua_gettop(L);

            // check if this class is registered and inspect the metatable
            LuaDll.luaL_getmetatable(L, o.GetType().FullName);
            if (LuaDll.lua_isnil(L, -1))
            {
                Console.WriteLine("inspectClass - metatable not found for class [{0}]", o.GetType().FullName);
                return;
            }

            Console.WriteLine("Inspecting {0}", o.GetType().FullName);

            /* table is in the stack at index 't' */
            int t = LuaDll.lua_gettop(L);
            LuaDll.lua_pushnil(L);  /* first key */
            while (LuaDll.lua_next(L, t) != 0)
            {
                // Use built-in "print" function to print the two values on the stack.
                LuaDll.lua_getglobal(L, "print");
                LuaDll.lua_pushstring(L, "\t");
                LuaDll.lua_pushvalue(L, -4);
                LuaDll.lua_pushvalue(L, -4);
                LuaDll.lua_call(L, 3, 0);

                /* removes 'value'; keeps 'key' for next iteration */
                LuaDll.lua_pop(L, 1);
            }

            // Pop the metatable
            LuaDll.lua_pop(L, 1);

            Debug.Assert(top == LuaDll.lua_gettop(L), "Error in stack");
        }

        ///////////////////////////////////////////////////////////////////////
        // Private methods
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Persistent delegate of functionCallAdapter.
        /// This is needed as a dynamic delegate could be freed before it is unregistered from lua.
        /// </summary>
        private LuaDll.lua_CFunction mLuaFunctionCallAdapterDelegate;

        /// <summary>
        /// Single lua function used for all C# function or method calls from lua.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns number of results pushed onto the stack</returns>
        private int functionCallAdapter(lua_State L)
        {

            // Figure out which function to call.
            // Our upvalue is a table of all methods with the same name as this one.

            // Get the function name.
            LuaDll.lua_getfield(L, LuaDll.lua_upvalueindex(1), kFunctionNameKey);
            string simpleName = LuaDll.lua_tostring(L, -1);
            LuaDll.lua_pop(L, 1);

            // Lookup the mangled name from the lut stored at upvalueindex.
            string mangledName = buildMangledNameFromLuaParameters(simpleName);
            LuaDll.lua_getfield(L, LuaDll.lua_upvalueindex(1), mangledName);

            if (LuaDll.lua_isnil(L, -1))
            {
                LuaDll.luaL_error(L, "Unknown function " + mangledName + " \"" + demangleName(mangledName) + "\"");
                return 0;
            }

            // Get the function delegate from the stack.
            GCHandle h = GCHandle.FromIntPtr(LuaDll.lua_topointer(L, -1));
            LuaDll.lua_pop(L, 1);

            System.Reflection.MethodInfo fd = (System.Reflection.MethodInfo)h.Target;
            System.Reflection.ParameterInfo[] pinfo = fd.GetParameters();
            int firstParam = 0;
            Object self = null;

            // Check if this is a static method or not.  If it is not static, then the first
            // parameter must be an instance of the class.
            if (!fd.IsStatic)
            {
                // Get the pointer to the object (and verify the type).
                IntPtr ud = LuaDll.luaL_checkudata(L, 1, fd.ReflectedType.FullName);

                // Copy the address of the object from the userdata.
                GCHandle hSelf = GCHandle.FromIntPtr(Marshal.ReadIntPtr(ud));

                // Since this is a non-static method, it will have an extra parameter.
                firstParam = 1;
                self = hSelf.Target;
            }

            // Verify that we are getting the correct number of parameters
            if (pinfo.Length != (LuaDll.lua_gettop(L) - firstParam))
            {
                LuaDll.luaL_error(L, "Invalid number of parameters for function '{0}'", fd.Name);
                return 1;
            }

            Object[] parms = new Object[pinfo.Length];
            int firstLuaParam = firstParam + 1;

            for (int i = 0; i < pinfo.Length; ++i)
            {
                if (pinfo[i].ParameterType == typeof(int))
                {
                    parms[i] = LuaDll.luaL_checkinteger(L, i + firstLuaParam);
                }
                else if (pinfo[i].ParameterType == typeof(double))
                {
                    parms[i] = LuaDll.luaL_checknumber(L, i + firstLuaParam);
                }
                else if (pinfo[i].ParameterType == typeof(string))
                {
                    parms[i] = LuaDll.luaL_checkstring(L, i + firstLuaParam);
                }
                else if (pinfo[i].ParameterType == typeof(bool))
                {
                    LuaDll.luaL_checktype(L, i + firstLuaParam, LuaDll.LUA_TBOOLEAN);
                    parms[i] = LuaDll.lua_toboolean(L, i + firstLuaParam);
                }
                else
                {
                    // Get the pointer to the object.
                    IntPtr ud = LuaDll.lua_touserdata(L, i + firstLuaParam);
                    parms[i] = GCHandle.FromIntPtr(Marshal.ReadIntPtr(ud)).Target;
                }
            }
            object result = fd.Invoke(self, parms);

            return pushValue(result);
        }

        /// <summary>
        /// Persistent delegate of gcAdapter.
        /// This is needed as a dynamic delegate could be freed before it is unregistered from lua.
        /// </summary>
        private LuaDll.lua_CFunction mgcAdapterDelegate;

        /// <summary>
        /// Implementation of __gc metamethod.
        /// Called when a userdata object is being released by the lua garbage collector.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns 0</returns>
        private int gcAdapter(lua_State L)
        {
            // Remove the referenced object from the global reference list so that
            // it can be cleaned up by the GC.
            IntPtr ud = LuaDll.lua_touserdata(L, 1);
            GCHandle gh = GCHandle.FromIntPtr(Marshal.ReadIntPtr(ud));

            Console.WriteLine("[gcAdapter] - Releasing " + gh.Target.ToString());

            bool result = mLuaReferences.Remove(gh);
            Debug.Assert(result, "[gcAdapter] - garbage collector release object not in mLuaReferences");

            return 0;
        }

        // __eq metamethod, used for comparing userdata objects
        private LuaDll.lua_CFunction meqAdapterDelegate;

        /// <summary>
        /// Implementation of __eq metamethod.
        /// This is used to compare two objects of the same type for equality.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns 0 on the stack if the arguments are not equal, 1 otherwise</returns>
        private int eqAdapter(lua_State L)
        {
            // Get the objects
            IntPtr ud1 = LuaDll.lua_touserdata(L, 1);
            GCHandle gh1 = GCHandle.FromIntPtr(Marshal.ReadIntPtr(ud1));

            IntPtr ud2 = LuaDll.lua_touserdata(L, 2);
            GCHandle gh2 = GCHandle.FromIntPtr(Marshal.ReadIntPtr(ud2));

            bool result = gh1.Target.Equals(gh2.Target);
            LuaDll.lua_pushboolean(L, result ? 1 : 0);

            return 1;
        }

        private LuaDll.lua_CFunction mtostringAdapterDelegate;

        /// <summary>
        /// Implementation of __tostring metamethod.
        /// Used to convert a userdata object to a string for printing.
        /// </summary>
        /// <param name="L">Lua state</param>
        /// <returns>Returns one string on the stack</returns>
        private int tostringAdapter(lua_State L)
        {
            // Get the object
            IntPtr ud = LuaDll.lua_touserdata(L, 1);
            GCHandle gh = GCHandle.FromIntPtr(Marshal.ReadIntPtr(ud));

            // Generate the string result
            string result = String.Format("{0}: {1:X}",  gh.Target.ToString(), ud.ToInt32());

            // Push the string and return it (return 1)
            LuaDll.lua_pushstring(L, result);
            return 1;
        }

        /// <summary>
        /// Build a mangled name from a MethodInfo.
        /// This is to allow each unique overloaded method in C# to have a unique name in lua.
        /// Method names must be in the same format generated by buildMangledNameFromLuaParameters.
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Returns a string containing a unique name of the given method</returns>
        /// <see>buildMangledNameFromLuaParameters</see>
        private string buildMangledNameFromMethodInfo(System.Reflection.MethodInfo mi, string bindName)
        {
            System.Reflection.ParameterInfo[] parameterList = mi.GetParameters();
            StringBuilder sb = new StringBuilder(bindName + "_");

            // If it is not a static function then the first parameter should be
            // itself.
            if (!mi.IsStatic)
            {
                sb.Append("c");
            }

            foreach (System.Reflection.ParameterInfo pi in parameterList)
            {
                Type t = pi.ParameterType;
                if (t == typeof(bool))
                {
                    sb.Append("b");
                }
                else if ((t == typeof(double)) || (t == typeof(float)) || (t == typeof(int)))
                {
                    sb.Append("n");
                }
                else if (t == typeof(string))
                {
                    sb.Append("s");
                }
                else if (t.IsClass)
                {
                    sb.Append("c");
                }
                else
                {
                    // Error, unexpected type, cannot mangle.
                    throw new Exception("Cannot mangle parameter of type " + t.FullName);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Build a mangled name from a lua function call.
        /// Assumes that the function arguments are on the stack.
        /// This is to allow each unique overloaded method in C# to have a unique name in lua.
        /// Method names must be in the same format generated by buildMangledNameFromMethodInfo.
        /// </summary>
        /// <param name="simpleName">Unmangled name of method</param>
        /// <returns>Returns a string containing a unique name of the given method</returns>
        /// <see>buildMangledNameFromMethodInfo</see>
        private string buildMangledNameFromLuaParameters(string simpleName)
        {
            StringBuilder sb = new StringBuilder(simpleName + "_");

            int paramcount = LuaDll.lua_gettop(L);
            Type[] types = new Type[paramcount];
            for (int i = 0; i < paramcount; ++i)
            {
                int luaType = LuaDll.lua_type(L, i + 1);
                switch (luaType)
                {
                    case LuaDll.LUA_TBOOLEAN:
                        sb.Append("b");
                        break;
                    case LuaDll.LUA_TNUMBER:
                        sb.Append("n");
                        break;
                    case LuaDll.LUA_TSTRING:
                        sb.Append("s");
                        break;
                    case LuaDll.LUA_TUSERDATA:
                        sb.Append("c");
                        break;
                    default:
                        Console.WriteLine("Invalid parameter #{0} as {1}", i + 1, LuaDll.lua_typename(L, luaType));
                        break;
                }
            }

            return sb.ToString();
        }

        private string demangleName(string mangled)
        {
            StringBuilder sb = new StringBuilder();

            int underscore = mangled.IndexOf('_');
            sb.Append(mangled.Substring(0, underscore));
            sb.Append('(');
            for (int i = underscore+1; i < mangled.Length; ++i)
            {
                if (i > underscore + 1)
                {
                    sb.Append(',');
                }

                switch (mangled[i])
                {
                    case 'c':
                        sb.Append("Object");
                        break;
                    case 'n':
                        sb.Append("Number");
                        break;
                    case 's':
                        sb.Append("String");
                        break;
                    case 'b':
                        sb.Append("Bool");
                        break;
                    default:
                        throw new Exception("Invalid mangled name " + mangled);
                }
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Push the lua lookup table (lut) of the given MethodInfo onto the lua stack.
        /// The lut contains a mapping between mangled names and actual functions.
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Returns the stack position of the lut (current top)</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private int pushFunctionLut(System.Reflection.MethodInfo mi, string bindName)
        {
            int top = LuaDll.lua_gettop(L);

            // Push global lut to the stack
            string globalLutName = "__Lua.cs__LUT__";
            LuaDll.lua_getfield(L, LuaDll.LUA_REGISTRYINDEX, globalLutName);
            if (LuaDll.lua_isnil(L, -1))
            {
                // If the global lut does not exist then create it and push it onto the stack.

                // Pop the nil value
                LuaDll.lua_pop(L, 1);

                // Push a new table and set it as the global globalLutName
                LuaDll.lua_newtable(L);
                LuaDll.lua_pushvalue(L, -1);    // Dupe for setglobal call.
                LuaDll.lua_setfield(L, LuaDll.LUA_REGISTRYINDEX, globalLutName);
            }
            int lut = LuaDll.lua_gettop(L);

            Debug.Assert(lut == (top + 1));

            // Push function lut to the stack.
            string fullName = mi.ReflectedType.FullName + "." + bindName;
            LuaDll.lua_getfield(L, lut, fullName);

            if (LuaDll.lua_isnil(L, -1))
            {
                // If the function lut does not exist then create it.

                // Pop off the nil result.
                LuaDll.lua_pop(L, 1);

                // Push in a new table
                LuaDll.lua_newtable(L);

                // Add the name to the table as kFunctionNameKey
                LuaDll.lua_pushstring(L, bindName);
                LuaDll.lua_setfield(L, -2, kFunctionNameKey);

                // Add this to the global lut, keyed on FullName
                LuaDll.lua_pushvalue(L, -1);
                LuaDll.lua_setfield(L, lut, fullName);
            }

            // Remove the global lut
            LuaDll.lua_remove(L, lut);

            // Now we have the function lut at the top of the stack.
            int result = LuaDll.lua_gettop(L);

            Debug.Assert(result == top + 1);

            // Return the stack position of the lut.
            return result;
        }

        /// <summary>
        /// Registers and pushes the given function definition onto the lua stack.
        /// </summary>
        /// <param name="mi">MethodInfo</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void pushFunction(System.Reflection.MethodInfo mi, string bindName)
        {
            int top = LuaDll.lua_gettop(L);

            // Generate unique name based on parameter list.
            string mangle = buildMangledNameFromMethodInfo(mi, bindName);

            // Push the Look Up Table for this function onto the Lua stack.
            int flut = pushFunctionLut(mi, bindName);

            // Lookup the manged name.
            LuaDll.lua_getfield(L, flut, mangle);

            // If not already defined then go ahead and define it
            if (LuaDll.lua_isnil(L, -1))
            {
                // Pop nil value
                LuaDll.lua_pop(L, 1);

                // Store the function in a GCHandle in a global list so it
                // doesn't get garbage collected.
                GCHandle h = GCHandle.Alloc(mi);
                mLuaReferences.Add(h);

                // functionLut[mangledName] = MethodInfo
                LuaDll.lua_pushlightuserdata(L, GCHandle.ToIntPtr(h));
                LuaDll.lua_setfield(L, flut, mangle);

                // Push the function call delegate with the function lut as an up value.
                LuaDll.lua_pushcclosure(L, mLuaFunctionCallAdapterDelegate, 1);
            }
            else
            {
                System.Reflection.MethodInfo oldMi = (System.Reflection.MethodInfo)GCHandle.FromIntPtr(LuaDll.lua_touserdata(L, -1)).Target;
                throw new Exception(String.Format("Duplicate function registration of '{0}', old={1}, new={2}", demangleName(mangle), oldMi, mi));
            }

            Debug.Assert(LuaDll.lua_gettop(L) == top + 1);
        }

        /// <summary>
        /// Registers and pushes the given function delegate definition onto the lua stack.
        /// </summary>
        /// <param name="f">Function delegate</param>
        //private void pushFunction(Delegate f)
        //{
        //    pushFunction(f.Method);
        //}

        /// <summary>
        /// Registers the given function in lua as the global 'name'.
        /// </summary>
        /// <param name="name">Global name</param>
        /// <param name="f">Function to register</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void registerFunctionAs(System.Reflection.MethodInfo mi, string name)
        {
            // Push the function delgate
            pushFunction(mi, name);

            // Set the function as a global in the lua scope with the same name.
            LuaDll.lua_setglobal(L, name);
        }

        private static bool isBindable(System.Reflection.MethodInfo mi)
        {
            // Return true if the method's class includes the BindAll attribute or 
            // the methods includes the Bindable attribute.
            return  Attribute.IsDefined(mi.ReflectedType, typeof(BindAllAttribute)) ||
                    Attribute.IsDefined(mi, typeof(BindableAttribute));
        }

        private static string getBindableName(System.Reflection.MethodInfo mi, string def)
        {
            BindableAttribute a = (BindableAttribute)Attribute.GetCustomAttribute(mi, typeof(BindableAttribute));
            if (a != null && a.Name != null)
            {
                return a.Name;
            }
            else
            {
                return def;
            }
        }

        /// <summary>
        /// Registers the given type in lua.  Creating a metatable for the class containing its public methods, if not already defined, and pushing it onto the stack.
        /// </summary>
        /// <param name="t">Type to register</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void registerClass(Type t)
        {
            string className = t.FullName;
            if (LuaDll.luaL_newmetatable(L, className) == 1)
            {
                int top = LuaDll.lua_gettop(L);

                Console.WriteLine("Registering class " + className);

                foreach (System.Reflection.MethodInfo mi in t.GetMethods())
                {
                    if (mi.IsPublic)
                    {
                        // Check that this method has been declared as bindable?
                        if (isBindable(mi))
                        {
                            // If the name is set, then bind using that name.
                            string bindName = getBindableName(mi, mi.Name);

                            // Push the method
                            pushFunction(mi, bindName);

                            // Set the pushed closure as a member of our table.
                            // The method should be at the top of the stack (-1), and the table below that (-2)
                            // mt[mi.Name] = method;
                            LuaDll.lua_setfield(L, -2, bindName);
                        }
                    }
                }

                // Register some luaisms
                // __gc
                LuaDll.lua_pushcfunction(L, mgcAdapterDelegate);
                LuaDll.lua_setfield(L, -2, "__gc");

                // __tostring
                LuaDll.lua_pushcfunction(L, mtostringAdapterDelegate);
                LuaDll.lua_setfield(L, -2, "__tostring");

                // __eq
                // Comparison operator
                LuaDll.lua_pushcfunction(L, meqAdapterDelegate);
                LuaDll.lua_setfield(L, -2, "__eq");

                LuaDll.lua_pushstring(L, "__index");    // Push "__index"
                LuaDll.lua_pushvalue(L, -2);            // Copy the metatable to the top of the stack
                LuaDll.lua_settable(L, -3);             // mt["__index"] = mt

                // metatable should be at top of stack.
                Debug.Assert(top == LuaDll.lua_gettop(L), "Top is wrong");
            }
        }

        /// <summary>
        /// Pushes the metatable for the given class onto the stack
        /// Will currently register it if not already registered.
        /// </summary>
        /// <param name="t">Type to query</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void pushMetatable(Type t)
        {
            registerClass(t);
        }

        /// <summary>
        /// Pushes the object onto the stack.
        /// Will also register the class of the object within lua, if not already registered.
        /// </summary>
        /// <param name="o">Object to push</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void pushObject(Object o)
        {
            GCHandle gch = GCHandle.Alloc(o);

            // Store the GCHandle so that it won't get garbage collected.
            mLuaReferences.Add(gch);

            // Push a userdata onto the stack (we don't care about the returned address)
            // This is needed because we cannot create a metatable for a lightuserdata.
            // So we make the light userdata a member of it's metatable.
            IntPtr p = LuaDll.lua_newuserdata(L, 8);

            // Write the IntPtr of the object handle to the userdata.
            Marshal.WriteIntPtr(p, GCHandle.ToIntPtr(gch));

            // Pushes the metatable of the given class onto the stack
            pushMetatable(o.GetType());

            // We should have a metatable of this object's class on the top of the stack.
            // Now register the object, which is below that (-2)
            // o.__metatable = mt
            LuaDll.lua_setmetatable(L, -2);

            // The exported object should now be on the top of the stack.
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Panic funcition replacement for lua passed to lua_atpanic.
        /// </summary>
        private LuaDll.lua_CFunction mPanicFunctionDelegate = delegate(lua_State L)
        {
            string error = LuaDll.lua_tostring(L, -1);
            throw new Exception(error);
        };

        /// <summary>
        /// Returns the value at stack index i as a C# object.
        /// </summary>
        /// <param name="i">Index of object</param>
        /// <returns>Returns object at index i</returns>
        private object getValue(int i)
        {
            object result = null;

            int t = LuaDll.lua_type(L, i);
            switch (t)
            {
                case LuaDll.LUA_TSTRING:  /* strings */
                    result = LuaDll.lua_tostring(L, i);
                    break;

                case LuaDll.LUA_TBOOLEAN:  /* booleans */
                    result = LuaDll.lua_toboolean(L, i);
                    break;

                case LuaDll.LUA_TNUMBER:  /* numbers */
                    result = LuaDll.lua_tonumber(L, i);
                    break;

                case LuaDll.LUA_TUSERDATA:    /* user data */
                    IntPtr p = LuaDll.lua_touserdata(L, i);
                    result = GCHandle.FromIntPtr(Marshal.ReadIntPtr(p)).Target;
                    break;

                default:  /* other values */
                    Console.WriteLine("getValue({0}) : Unsupported type [{1}]", i, LuaDll.lua_typename(L, t));
                    break;

            }

            return result;
        }

        /// <summary>
        /// Returns count values on the lua stack, starting at index i. 
        /// </summary>
        /// <param name="i">Index of first value</param>
        /// <param name="count">Number of values to retrieve</param>
        /// <returns>Returns a MultiValue containing count arguments from the stack</returns>
        private MultiValue getValues(int i, int count)
        {
            MultiValue result = null;
            if (count > 0)
            {
                result = new MultiValue(count);
                for (int d = 0; d < count; ++d)
                {
                    result[d] = getValue(i + d);
                }
            }
            return result;
        }

        /// <summary>
        /// Pushes the object v into the lua stack and returns the number of values that were pushed.
        /// If v is a MultiValue then it might push more than one value.
        /// Will automatically register the class of the object if not yet registered
        /// </summary>
        /// <param name="v">Object to push onto the stack</param>
        /// <returns></returns>
        private int pushValue(object v)
        {
            if (v == null)
            {
                return 0;
            }

            Type t = v.GetType();
            if (t == typeof(int))
            {
                LuaDll.lua_pushinteger(L, (int)v);
                return 1;
            }
            else if ((t == typeof(double)) || (t == typeof(float)))
            {
                LuaDll.lua_pushnumber(L, (double)v);
                return 1;
            }
            else if (t == typeof(string))
            {
                LuaDll.lua_pushstring(L, (string)v);
                return 1;
            }
            else if (t == typeof(bool))
            {
                LuaDll.lua_pushboolean(L, ((bool)v)?1:0);
                return 1;
            }
            else if (t == typeof(MultiValue))
            {
                // Special case to push multiple values onto the stack
                MultiValue mv = (MultiValue)v;
                int result = 0;

                for (int i = 0; i < mv.Length; ++i)
                {
                    // Since this is recursive, we could end up with arrays inside arrays.
                    // Should we catch that?
                    result += pushValue(mv[i]);
                }

                return result;
            }
            else if (t.IsClass)
            {
                // If the is an object, then store a reference to it in the static
                // list of lua references.  It can be removed from this list once
                // lua is done with it.
                pushObject(v);
                return 1;
            }
            else
            {
                // Error, unexpected type, cannot push.
                Console.WriteLine("pushValue() Cannot push type [{0}]", t.FullName);
                return 0;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Private members
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// List of GCHandles that have been registered with lua.
        /// This is to prevent the C# garbage collector from releasing them before lua is done using them.
        /// </summary>
        private List<GCHandle>  mLuaReferences = new List<GCHandle>();

        /// <summary>
        /// Lua state
        /// </summary>
        private lua_State       L;

        //private HandleRef       mOutputRef;
        ///////////////////////////////////////////////////////////////////////
        // Static
        ///////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Replacement for global lua function "print".  Prints to the Console
        /// instead of stdout.
        /// </summary>
        private static LuaDll.lua_CFunction sLuaPrintDelegate = delegate(lua_State L)
        {
            int n = LuaDll.lua_gettop(L);  /* number of arguments */
            int i;
            LuaDll.lua_getglobal(L, "tostring");
            for (i = 1; i <= n; i++)
            {
                string s;
                LuaDll.lua_pushvalue(L, -1);  /* function to be called */
                LuaDll.lua_pushvalue(L, i);   /* value to print */
                LuaDll.lua_call(L, 1, 1);
                s = LuaDll.lua_tostring(L, -1);  /* get result */
                if (s == null)
                {
                    LuaDll.lua_pushstring(L, "'tostring' must return a string to 'print'");
                    return LuaDll.lua_error(L);
                }
                if (i > 1) Console.Write("\t");
                Console.Write(s);
                LuaDll.lua_pop(L, 1);  /* pop result */
            }
            Console.Write("\n");
            return 0;
        };

        ///////////////////////////////////////////////////////////////////////

        //[DllImport("Kernel32.dll", SetLastError = true)]
        //private static extern int SetStdHandle(int device, HandleRef handle);

        private const string kFunctionNameKey = "__key__";
    }
}
