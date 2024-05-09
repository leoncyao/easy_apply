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

using EasyApply.Factories;
using EasyApply.Models;
using EasyApply.Repositories;
using OpenQA.Selenium;

namespace EasyApply.Interfaces
{
    /// <summary>
    /// Opportunity campaign interface
    /// </summary>
    public abstract class ICampaign
    {
        public YmlConfiguration Configuration { get; set; }

        public IWebDriver? WebDriver { get; set; }

        public IDataRepository DataRepository { get; set; }

        protected ICampaign(YmlConfiguration configuration)
        {
            this.Configuration = configuration;

            WebDriver = new DriverFactory()
                .CreateDriver(configuration.Browser);


            DataRepository = new DataRepositoryFactory()
                .CreateRepository(configuration.OpportunityConfiguration.Database);
        }

        /// <summary>
        /// Start opportunity campaign
        /// </summary>
        /// <returns></returns>
        public abstract Task StartCampaign();

        /// <summary>
        /// Login into opportunity board
        /// </summary>
        /// <returns></returns>
        public abstract void GetLogin();

        /// <summary>
        /// Login into opportunity board
        /// </summary>
        /// <returns></returns>
        public abstract void GetSearchPage(int? page);

        /// <summary>
        /// Start opportunity Searching
        /// </summary>
        public abstract Task StartJobSearch();
    }
}
