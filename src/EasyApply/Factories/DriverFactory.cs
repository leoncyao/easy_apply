/*

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

using EasyApply.Enums;
using EasyApply.Interfaces;
using EasyApply.Models;
using EasyApply.Utilities;
using EasyAppy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using System.Drawing;
using System.Text;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace EasyApply.Factories
{
    /// <summary>
    /// Driver factory
    /// </summary>
    public class DriverFactory : IDriverFactory
    {
        /// <summary>
        /// Create web driver
        /// </summary>
        /// 

        static List<Cookie> LoadCookiesFromFile(string filePath)
        {
            List<Cookie> cookies = new List<Cookie>();

            // Read cookies from a JSON file
            using (StreamReader file = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                cookies = (List<Cookie>)serializer.Deserialize(file, typeof(List<Cookie>));
            }

            return cookies;
        }
        static void LogCookies(IWebDriver driver)
        {
            // Get all cookies
            var cookies = driver.Manage().Cookies.AllCookies;

            // Log cookies to the console
            Console.WriteLine("Cookies:");
            foreach (var cookie in cookies)
            {
                Console.WriteLine($"{cookie.Name}: {cookie.Value}");
            }
        }
        public override IWebDriver CreateDriver(Browser browser)
        {
            IWebDriver driver = null;
            switch (browser?.BrowserType)
            {
                case BrowserType.Chrome:
                    {
                        //if (!DependencyCheck.CheckChrome() ||
                        //    !DependencyCheck.CheckGeckoDriver())
                        //if (!DependencyCheck.CheckChrome())
                        //    {
                        //    if (Program.VerboseMode)
                        //    {
                        //        Debug.WriteLine("[*] Do not meet Chrome \\ Gecko driver requirements:");
                        //        Debug.WriteLine(@"[*] Check : C:\Program Files\Mozilla Firefox\firefox.exe");
                        //        Debug.WriteLine(@"[*] Check : C:\tools\selenium\geckodriver.exe");
                        //        Debug.WriteLine("[*] Aborting");
                        //        Environment.Exit(1);
                        //    }
                        //}

                        var chromeOptions = new ChromeOptions();
                        //chromeOptions.AddArgument("excludeSwitches=enable-automation");
                        //chromeOptions.AddArgument("--user-agent=Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 125.0.0.0 Safari / 537.36");
                        //chromeOptions.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36");
                        //chromeOptions.AddAdditionalCapability("useAutomationExtension", false);
                        //chromeOptions.AddAdditionalCapability("excludeSwitches", new string[] { "enable-logging" });
                        //chromeOptions.AddAdditionalCapability("excludeSwitches", new string[] { "enable-automation" });

                        //string userProfilePath = @"C:\\Users\\leony\\AppData\\Local\\Google\\Chrome\\User Data";
                        chromeOptions.AddArgument("--disable-clear-browsing-data");

                        string userProfilePath = browser.profile_path;
                        chromeOptions.AddArgument($"--user-data-dir={userProfilePath}");

                        if ((bool)browser?.Headless)
                            chromeOptions.AddArguments("--headless");

                        if ((bool)browser?.Incognito)
                            chromeOptions.AddArguments("--incognito");

                        if (!string.IsNullOrEmpty(browser?.WindowWidth))
                            chromeOptions.AddArgument($"--width={browser?.WindowWidth}");

                        if (!string.IsNullOrEmpty(browser?.WindowHeight))
                            chromeOptions.AddArgument($"--height={browser?.WindowHeight}");

                        //chromeOptions.DebuggerAddress = "127.0.0.1:9222";

                        //Console.WriteLine(string.Join(", ", chromeOptions.Arguments));
                        //foreach (var process in Process.GetProcessesByName("chrome"))
                        //{
                        //    process.Kill();
                        //}
                        //return new ChromeDriver("C:\\Users\\leony\\Desktop\\projects\\easy_apply", chromeOptions);
                        Console.WriteLine(userProfilePath);
                        Debug.WriteLine(browser.driver_path);
                        driver = new ChromeDriver(browser.driver_path, chromeOptions);

                        //List<Cookie> cookies = LoadCookiesFromFile(browser.driver_path + "/cookies.json");

                        //// Add each cookie to the WebDriver instance
                        //foreach (var cookie in cookies)
                        //{
                        //    driver.Manage().Cookies.AddCookie(cookie);
                        //}

                        //LogCookies(driver);

                        return driver;
                        //return new ChromeDriver("C:\\Users\\leony\\Downloads\\chromedriver-win64\\chromedriver-win64", chromeOptions);
                    }
                case BrowserType.Firefox:
                    {
                        if (!DependencyCheck.CheckFireFox() ||
                            !DependencyCheck.CheckGeckoDriver())
                        {
                            if (Program.VerboseMode)
                            {
                                Debug.WriteLine("[*] Do not meet Firefox \\ Gecko driver requirements:");
                                Debug.WriteLine(@"[*] Check : C:\Program Files\Mozilla Firefox\firefox.exe");
                                Debug.WriteLine(@"[*] Check : C:\tools\selenium\geckodriver.exe");
                                Debug.WriteLine("[*] Aborting");
                                Environment.Exit(1);
                            }
                        }

                        CodePagesEncodingProvider.Instance.GetEncoding(437);
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                        foreach (var process in Process.GetProcessesByName("firefox"))
                            process.Kill();

                        var firefoxOptions = new FirefoxOptions();
                        // set window open to tab
                        firefoxOptions.SetPreference("browser.link.open_newwindow", "2");

                        if (!string.IsNullOrEmpty(browser.BrowserProfile))
                        {
                            // create profile win+r firefox -p

                            var profile = new FirefoxProfile();
                            var profileManager = new FirefoxProfileManager();
                            profile = profileManager.GetProfile(browser.BrowserProfile);

                            profile.WriteToDisk();

                            firefoxOptions.Profile = profileManager.GetProfile(browser.BrowserProfile);
                        }

                        if (browser.Headless)
                            firefoxOptions.AddArguments("--headless");

                        if (browser.Incognito)
                            firefoxOptions.SetPreference("browser.privatebrowsing.autostart", "true");

                        driver = new FirefoxDriver(firefoxOptions);

                        if (!string.IsNullOrEmpty(browser.WindowWidth) &&
                            !string.IsNullOrEmpty(browser.WindowHeight))
                        {
                            driver.Manage().Window.Size =
                                new Size(int.Parse(browser.WindowWidth), int.Parse(browser.WindowHeight));
                        }

                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

                        return driver;
                    }
                default:
                    {
                        if (Program.VerboseMode)
                        {
                            Debug.WriteLine($"No browser type selected");
                        }

                        Environment.Exit(1);
                        return null;
                    }
            }
        }
    }
}