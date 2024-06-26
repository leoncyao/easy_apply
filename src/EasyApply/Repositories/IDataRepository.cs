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

using EasyApply.Models;

namespace EasyApply.Repositories
{
    public interface IDataRepository
    {
        /// <summary>
        /// Initialize specific database startup sequence
        /// </summary>
        void InitializeDatabase();

        /// <summary>
        /// Add Indeed opportunity to satabase
        /// </summary>
        /// <param name="indeedOpportunity"></param>
        /// <returns></returns>
        Task<IndeedOpportunity> AddIndeedOpportunity(IndeedOpportunity indeedOpportunity);

        /// <summary>
        /// Get Indeed opportunity by database id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IndeedOpportunity> GetIndeedOpportunity(int id);

        /// <summary>
        /// Get all Indeed opportunities from database
        /// </summary>
        /// <returns></returns>
        Task<List<IndeedOpportunity>> GetIndeedOpportunities();

        /// <summary>
        /// Check for existing Indeed opportunity
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        Task<bool> CheckIndeedOpportunity(string link);

        /// <summary>
        /// Update an existing Indeed opportunity
        /// </summary>
        /// <param name="indeedOpportunity"></param>
        /// <returns></returns>
        Task<bool> UpdateIndeedOpportunity(IndeedOpportunity indeedOpportunity);

        /// <summary>
        /// Add Monster opportunity to satabase
        /// </summary>
        /// <param name="MonsterOpportunity"></param>
        /// <returns></returns>
        Task<MonsterOpportunity> AddMonsterOpportunity(MonsterOpportunity MonsterOpportunity);

        /// <summary>
        /// Get Monster opportunity by database id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MonsterOpportunity> GetMonsterOpportunity(int id);

        /// <summary>
        /// Get all Monster opportunities from database
        /// </summary>
        /// <returns></returns>
        Task<List<MonsterOpportunity>> GetMonsterOpportunities();

        /// <summary>
        /// Check for existing Monster opportunity
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        Task<bool> CheckMonsterOpportunity(string link);

        /// <summary>
        /// Update an existing Monster opportunity
        /// </summary>
        /// <param name="MonsterOpportunity"></param>
        /// <returns></returns>
        Task<bool> UpdateMonsterOpportunity(MonsterOpportunity MonsterOpportunity);
    }
}