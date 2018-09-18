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
using System;
using CSLua;

namespace CSLua.Tests
{
    // [CSLua.BindAll is to allow all public methods to be bound so they
    // don't have to each have the CSLua.Bindable attribute.
    [CSLua.BindAll]
    public class Multiplier
    {
        public Multiplier(string name)
        {
            mName = name;
        }

        public int multiply(int a, int b)
        {
            return a * b;
        }

        public string getName()
        {
            return mName;
        }

        private string mName;
    }

    public class SetterGetter
    {
        [CSLua.Bindable]
        public void SetNumber(double value)
        {
            mNumber = value;
        }

        [CSLua.Bindable]
        public double GetNumber()
        {
            return mNumber;
        }

        [CSLua.Bindable]
        public void SetString(string s)
        {
            mString = s;
        }

        [CSLua.Bindable]
        public string GetString()
        {
            return mString;
        }

        private string mString;
        private double mNumber;
    }

    [TestClass]
    public class LuaStateTests
    {
        [TestMethod]
        public void numberTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            CSLua.MultiValue result = L.dostring("return 1");
            Assert.AreEqual(1.0, result[0]);
        }

        [TestMethod]
        public void stringTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            CSLua.MultiValue result = L.dostring("return \"This is a string\"");
            Assert.AreEqual("This is a string", result[0]);
        }

        [TestMethod]
        public void callTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            L.dostring("square = function(x) return x*x end");
            CSLua.MultiValue result = L.call("square", 12.0);
            Assert.AreEqual(144.0, result[0]);
        }

        [TestMethod]
        [Ignore]
        public void unRegisterObjectTest()
        {
            // unregisterObject is not yet implemented.

            CSLua.LuaState L = new CSLua.LuaState();
            Multiplier multiplier = new Multiplier("MyMult");
            L.registerObject("testObject", multiplier);

            {
                var result = L.dostring("return testObject:multiply(1, 1)");
                Assert.AreEqual(1.0, result[0]);
            }

            // This is not yet implemented
            L.unregisterObject(multiplier);

            {
                var result = L.dostring("return testObject:multiply(1, 1)");
                Assert.AreEqual(null, result[0]);
            }
        }

        [CSLua.Bindable(Name = "Square")]
        public static double NOTSquare(double d)
        {
            return d * d;
        }

        [TestMethod]
        public void registerFunctionDelegateFuncTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            L.registerFunction((Delegate)(Func<double, double>)NOTSquare);
            var result = L.dostring("return Square(32.623)");
            var expected = 1064.260129;
            Assert.AreEqual(expected, (double)result[0], 0.00001);
        }

        private static bool mTestActionCalled;
        [CSLua.Bindable]
        public static void TestAction()
        {
            mTestActionCalled = true;
        }

        [TestMethod]
        public void registerFunctionActionTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            mTestActionCalled = false;
            L.registerFunction((Action)TestAction);
            L.call("TestAction");
            Assert.IsTrue(mTestActionCalled);
        }

        public static string mTestActionOneParamParam;
        [CSLua.Bindable]
        public static void TestActionOneParam(string param)
        {
            mTestActionOneParamParam = param;
        }
        [TestMethod]
        public void registerFunctionActionOneParamTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            mTestActionCalled = false;
            L.registerFunction((Action<string>)TestActionOneParam);
            L.call("TestActionOneParam", "This is a param");
            Assert.AreEqual("This is a param", mTestActionOneParamParam);
        }

        [TestMethod]
        public void registerFunctionAsFuncTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            L.registerFunctionAs((Func<double, double>)NOTSquare, "sqr");
            var result = L.dostring("return sqr(3.3)");
            Assert.AreEqual(10.89, (double)result[0], 0.00001);
        }

        [TestMethod]
        [Ignore]
        public void registerFunctionAsLambdaTest()
        {
            // This test is disabled because lambda expressions don't register correctly.
            // This is due to LuaState only assuming that all non-static methods are members of an object
            // and must be called from lua using the : syntax (passing the object as the first argument).
            // Lambda functions are not static, and they contain their object, but this is not yet supported.

            CSLua.LuaState L = new CSLua.LuaState();
            L.registerFunctionAs((Func<double, double>)(x => x * x), "sqr");
            var result = L.dostring("return sqr(4.0)");
            Assert.AreEqual(16.0, (double)result[0], 0.00001);
        }

        [TestMethod]
        public void registerObjectTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            Multiplier multiplier = new Multiplier("MyMult");
            L.registerObject("testObject", multiplier);

            CSLua.MultiValue result = L.dostring("return testObject:multiply(7, 13)");
            Assert.AreEqual(91.0, result[0]);

            result = L.dostring("return testObject:multiply(1, 1)");
            Assert.AreEqual(1.0, result[0]);

            result = L.dostring("return testObject:multiply(100001, 7)");
            Assert.AreEqual(700007.0, result[0]);
        }

        [TestMethod]
        public void classTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            var setterGetter = new SetterGetter();
            setterGetter.SetNumber(1234.567);
            setterGetter.SetString("this is a test");
            L.registerObject("testObject", setterGetter);
            {
                var result = L.dostring("return testObject:GetNumber()");
                Assert.AreEqual(1234.567, result[0]);
            }

            {
                var result = L.dostring("return testObject:GetString()");
                Assert.AreEqual("this is a test", result[0]);
            }

            {
                L.dostring("testObject:SetNumber(394856.32)");
                Assert.AreEqual(394856.32, setterGetter.GetNumber());
            }
            {
                L.dostring("testObject:SetString(\"Yet Another String\")");
                Assert.AreEqual("Yet Another String", setterGetter.GetString());
            }

        }

        [TestMethod()]
        public void dostringTest()
        {
            CSLua.LuaState L = new CSLua.LuaState();
            CSLua.MultiValue result = L.dostring("return \"Hello World\"");
            Assert.AreEqual("Hello World", result[0]);
        }

    }
}
