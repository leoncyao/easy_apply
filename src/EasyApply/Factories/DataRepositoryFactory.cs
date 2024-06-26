﻿/*

      __ _/| _/. _  ._/__ /
    _\/_// /_///_// / /_|/
               _/
    
    sof digital 2021
    written by michael rinderle <michael@sofdigital.net>
    
    mit license
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

*/

using EasyApply.Dependencies;
using EasyApply.Enums;
using EasyApply.Interfaces;
using EasyApply.Repositories;
using EasyAppy;
using System.Diagnostics;

namespace EasyApply.Factories
{
    /// <summary>
    /// DataRespository factory
    /// </summary>
    public class DataRepositoryFactory : IDataRepositoryFactory
    {
        /// <summary>
        /// Create Repository
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override IDataRepository CreateRepository(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.Sqlite:
                    {
                        return new IData(new SqliteDataRepository());
                    }
                default:
                    {
                        if (Program.VerboseMode)
                        {
                            Debug.WriteLine($"No database selected or not implimented yet");
                        }

                        Environment.Exit(1);
                        return null;
                    }
            }
        }
    }
}