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


namespace CSLua
{
    /// <summary>
    /// Represents multiple lua values.
    /// If a function returns this type then it will be converted into
    /// multiple return values on the stack.
    /// It can also represent multiple parameters to a function call.
    /// This is used instead of a simple array so that we can pass arrays
    /// as native types.
    /// </summary>
    public class MultiValue
    {
        /// <summary>
        /// Initialize with an array of objects
        /// </summary>
        /// <param name="values">Initial values in the MultiValue</param>
        public MultiValue(object[] values)
        {
            mValues = values;
        }

        /// <summary>
        /// Initialize a specific size
        /// </summary>
        /// <param name="count">Initial size of MultiValue</param>
        public MultiValue(int count)
        {
            mValues = new object[count];
        }

        /// <summary>
        /// Overload the [] so that this can be accessed like "object[]"
        /// </summary>
        /// <param name="i">Index of element to access</param>
        /// <returns>Returns object at index i</returns>
        public object this[int i]
        {
            get { return this.mValues[i]; }
            set { this.mValues[i] = value; }
        }

        /// <summary>
        /// Property containing the number of elements in the MultiValue
        /// </summary>
        public int Length
        {
            get { return mValues.Length; }
        }

        /// <summary>
        /// Array of objects stored within the MultiValue
        /// </summary>
        public object[] mValues;
    };
}
