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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSLua;
using System;
using System.Runtime.InteropServices;

namespace CSLua.Tests
{
    using lua_Number = Double;
    using lua_Integer = Int32;
    using size_t = Int32;

    [TestClass]
    public class LuaDllTests
    {
        IntPtr Alloc(IntPtr ud, IntPtr ptr, size_t osize, size_t nsize)
        {
            if (nsize == 0)
            {
                Marshal.FreeHGlobal(ptr);
                return new IntPtr(0);
            }
            else
            {
                if (osize == 0)
                {
                    return Marshal.AllocHGlobal(nsize);
                }
                else
                {
                    return Marshal.ReAllocHGlobal(ptr, (IntPtr)nsize);
                }
            }
        }

        [TestMethod]
        public void lua_newstateTest()
        {
            lua_State L = LuaDll.lua_newstate(Alloc, new IntPtr());
            LuaDll.lua_close(L);

        }

        [TestMethod]
        public void lua_pcallTest()
        {
            var L = LuaDll.luaL_newstate();
            LuaDll.luaL_openlibs(L);

            int top = LuaDll.lua_gettop(L);
            LuaDll.lua_getglobal(L, "type");
            LuaDll.lua_pushnil(L);
            var result = LuaDll.lua_pcall(L, 1, LuaDll.LUA_MULTRET, 0);
            int newtop = LuaDll.lua_gettop(L);
            int index = newtop;
            int count = newtop - top;

            var errorString = LuaDll.lua_tostring(L, newtop);

            Assert.AreEqual(0, result);
            Assert.AreEqual(1, count);
            Assert.AreEqual(LuaDll.LUA_TSTRING, LuaDll.lua_type(L, index));
            Assert.AreEqual("nil", LuaDll.lua_tostring(L, index));
        }

        [TestMethod]
        public void lua_registerTest()
        {
            bool wasCalled = false;
            LuaDll.lua_CFunction testFunctionDelegate = delegate (lua_State state)
            {
                wasCalled = true;
                LuaDll.lua_pushstring(state, "Success!");
                return 1;
            };

            // Create a new state
            var L = LuaDll.luaL_newstate();

            // Register the given function by name
            LuaDll.lua_register(L, "test_function", testFunctionDelegate);

            // Call the function by name
            var result = LuaDll.luaL_dostring(L, "return test_function()");
            Assert.AreEqual(0, result);
            Assert.IsTrue(wasCalled);

            // Check that the function returned the same string.
            var resultIndex = LuaDll.lua_gettop(L);
            var resultString = LuaDll.lua_tostring(L, resultIndex);
            Assert.AreEqual("Success!", resultString);
        }

        [TestMethod]
        public void lua_pushcfunctionTest()
        {
            // Create a new state
            var L = LuaDll.luaL_newstate();
            lua_Number value = 0;

            LuaDll.lua_CFunction squareandstorenumber = delegate (lua_State _L)
            {
                value = LuaDll.lua_tonumber(_L, 1);
                LuaDll.lua_pushnumber(_L, value * value);
                return 1;
            };

            string functionName = "squareandstorenumber";
            string tableName = "TestTable";
            double testValue = 3.14159;

            var top = LuaDll.lua_gettop(L);
            LuaDll.lua_newtable(L);
            LuaDll.lua_pushcfunction(L, squareandstorenumber);
            LuaDll.lua_setfield(L, -2, functionName);
            LuaDll.lua_setglobal(L, tableName);

            Assert.AreEqual(top, LuaDll.lua_gettop(L), "top doesn't match after registering function");

            LuaDll.lua_getglobal(L, tableName);             // push TestTable
            LuaDll.lua_getfield(L, -1, functionName);       // push TestTable["squareandstorenumber"]
            LuaDll.lua_pushnumber(L, testValue);            // push parameter
            var callResult = LuaDll.lua_pcall(L, 1, 1, 0);  // call function (pops parameter and function and pushes result)
            var squareResult = LuaDll.lua_tonumber(L, -1);  // get result
            LuaDll.lua_pop(L, 2);                           // pop the result and table

            Assert.AreEqual(top, LuaDll.lua_gettop(L), "top doesn't match after calling function");

            Assert.AreEqual(0, callResult, "Error calling squareandstorenumber");
            Assert.AreEqual(testValue, value, double.Epsilon);
            Assert.AreEqual(testValue * testValue, squareResult, double.Epsilon);
        }

        [TestMethod()]
        public void luaL_registerTest()
        {
            // Create a new state
            var L = LuaDll.luaL_newstate();
            LuaDll.lua_CFunction func = delegate (lua_State state)
            {
                LuaDll.lua_pushstring(state, "Success!");
                return 1;
            };

            LuaDll.luaL_Reg[] testLib =
            {
                new LuaDll.luaL_Reg {name="square",  func = delegate (lua_State _L)
                    {
                        var value = LuaDll.lua_tonumber(_L, 1);
                        LuaDll.lua_pushnumber(_L, value * value);
                        return 1;
                    }
                },
                new LuaDll.luaL_Reg {name="add",  func = delegate (lua_State _L)
                    {
                        var value1 = LuaDll.lua_tonumber(_L, 1);
                        var value2 = LuaDll.lua_tonumber(_L, 2);
                        LuaDll.lua_pushnumber(_L, value1 + value2);
                        return 1;
                    }
                },
                // Must have one nulled entry to indicate the end of the array
                new LuaDll.luaL_Reg {name=null,  func = null }
            };
            LuaDll.luaL_register(L, "TestLibrary", testLib);
            LuaDll.lua_pop(L, 1);
            var result = LuaDll.luaL_dostring(L, "return TestLibrary.square(9) + TestLibrary.add(3.14, 4.13)");
            Assert.AreEqual(0, result);

            Assert.AreEqual(88.27, LuaDll.lua_tonumber(L, 1));
        }
    }
}
