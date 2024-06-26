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

using EasyApply.Campaigns.Indeed;
using EasyApply.Campaigns.Monster;
using EasyApply.Interfaces;
using EasyApply.Parsers;
using EasyAppy;
using System.Diagnostics;

namespace EasyApply.Factories
{
    /// <summary>
    /// Campaign factory
    /// </summary>
    public class CampaignFactory : ICampaignFactory
    {
        public override ICampaign CreateCampaign(string path)
        {
            var yml = YmlConfigurationParser.LoadConfiguration(path);
            switch (yml.OpportunityConfiguration?.JobType)
            {
                case Enums.OpportunityType.Indeed:
                    {
                        return new IndeedCampaign(yml);
                    }
                case Enums.OpportunityType.Monster:
                    {
                        return new MonsterCampaign(yml);
                    }
                default:
                    {
                        if (Program.VerboseMode)
                        {
                            Debug.WriteLine($"No job type selected");
                        }

                        Environment.Exit(1);
                        return null;
                    }
            }
        }
    }
}