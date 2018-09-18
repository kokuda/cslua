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

namespace CSLua
{
    /// <summary>
    /// Declare a method as bindable to Lua and optionally give it a different name.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple=false)]
    public class BindableAttribute : System.Attribute
    {
        /// <summary>
        /// Bind the method as this name in lua
        /// </summary>
        public string Name;
    }

    /// <summary>
    /// Bind all public methods of a class when bound to a Lua state.
    /// Eliminates the need to bind individual methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BindAllAttribute : System.Attribute
    {
    }
}
