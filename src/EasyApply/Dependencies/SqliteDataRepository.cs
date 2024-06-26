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

using EasyApply.Data;
using EasyApply.Models;
using EasyApply.Repositories;
using EasyAppy;
using System.Diagnostics;

namespace EasyApply.Dependencies
{
    /// <summary>
    /// Sqlite Data Repository injection
    /// </summary>
    public class SqliteDataRepository : IDataRepository
    {
        public void InitializeDatabase()
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    ctx.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        public async Task<IndeedOpportunity> AddIndeedOpportunity(IndeedOpportunity indeedOpportunity)
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    ctx.IndeedOpportunity.Add(indeedOpportunity);
                    return await Task.FromResult(indeedOpportunity);
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return null;
            }
        }

        public async Task<List<IndeedOpportunity>> GetIndeedOpportunities()
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    return await Task.FromResult(ctx.IndeedOpportunity.ToList());
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return null;
            }
        }

        public async Task<IndeedOpportunity> GetIndeedOpportunity(int id)
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    var indeedOpportunity = ctx.IndeedOpportunity.FirstOrDefault(x => x.Id == id);
                    return await Task.FromResult(indeedOpportunity);
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return null;
            }
        }

        public async Task<bool> CheckIndeedOpportunity(string link)
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    var exists = ctx.IndeedOpportunity.Any(x => x.Link == link);
                    return await Task.FromResult(exists);
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return false;
            }
        }

        public async Task<bool> UpdateIndeedOpportunity(IndeedOpportunity indeedOpportunity)
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    var update = ctx.IndeedOpportunity.Update(indeedOpportunity);
                    return await Task.FromResult((update != null));
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return false;
            }
        }

        public async Task<MonsterOpportunity> AddMonsterOpportunity(MonsterOpportunity MonsterOpportunity)
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    ctx.MonsterOpportunity.Add(MonsterOpportunity);
                    return await Task.FromResult(MonsterOpportunity);
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return null;
            }
        }

        public async Task<List<MonsterOpportunity>> GetMonsterOpportunities()
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    return await Task.FromResult(ctx.MonsterOpportunity.ToList());
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return null;
            }
        }

        public async Task<MonsterOpportunity> GetMonsterOpportunity(int id)
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    var MonsterOpportunity = ctx.MonsterOpportunity.FirstOrDefault(x => x.Id == id);
                    return await Task.FromResult(MonsterOpportunity);
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return null;
            }
        }

        public async Task<bool> CheckMonsterOpportunity(string link)
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    var exists = ctx.MonsterOpportunity.Any(x => x.Link == link);
                    return await Task.FromResult(exists);
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return false;
            }
        }

        public async Task<bool> UpdateMonsterOpportunity(MonsterOpportunity MonsterOpportunity)
        {
            try
            {
                using (var ctx = new SqliteContext())
                {
                    var update = ctx.MonsterOpportunity.Update(MonsterOpportunity);
                    return await Task.FromResult((update != null));
                }
            }
            catch (Exception ex)
            {
                if (Program.VerboseMode)
                {
                    Debug.WriteLine(ex);
                }
                return false;
            }
        }
    }
}