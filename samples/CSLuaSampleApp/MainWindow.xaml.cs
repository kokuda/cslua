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
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace CSLuaSampleApp
{
    // [CSLua.BindAll is to allow all public methods to be bound so they
    // don't have to each have the CSLua.Bindable attribute.
    [CSLua.BindAll]
    public class Squarer
    {
        public Squarer(string name)
        {
            mName = name;
        }

        public static int square(int i)
        {
            return i * i;
        }

        public string getName()
        {
            return mName;
        }

        private string mName;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CSLua.LuaState L;
        private static MainWindow sWindow1;

        public MainWindow()
        {
            InitializeComponent();

            L = new CSLua.LuaState();

            CSLua.LuaDll.lua_getglobal(L._L, "print");
            CSLua.LuaDll.lua_pushfstring(L._L, "End of Window1() and testing {0}", "pushfstring");
            CSLua.LuaDll.lua_pcall(L._L, 1, 0, 0);

            sWindow1 = this;
        }

        ~MainWindow()
        {
        }

        private static void outputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (comboBox1.Text == "exit")
                {
                    Close();
                }

                CSLua.MultiValue result = L.dostring(comboBox1.Text);
                comboBox1.Text = "";
            }
        }
    }
}
