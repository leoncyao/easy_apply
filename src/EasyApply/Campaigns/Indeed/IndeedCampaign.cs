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

using EasyApply.Interfaces;
using EasyApply.Models;
using EasyAppy;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Web;
using System.Threading;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;



namespace EasyApply.Campaigns.Indeed
{
    public class IndeedCampaign : ICampaign
    {
        public IndeedCampaign(YmlConfiguration configuration)
            : base(configuration) { }

        /// <summary>
        /// Start job search campaign
        /// </summary>
        public override async Task StartCampaign()
        {
            //this.GetLogin();
            this.GetSearchPage(null);
            await this.StartJobSearch();
        }

        /// <summary>
        /// Log into Indeed if profile is not set with existing cookies
        /// </summary>
        public override void GetLogin()
        {
            // browser profile should have indeed cookies for bypass
            //if (!string.IsNullOrEmpty(Configuration.Browser.BrowserProfile)) return;

            // login to indeed
            WebDriver.Url = Constants.IndeedLoginUrl;

            var login = WebDriver.FindElement(By.Id(Constants.IndeedLoginCssID));
            login.SendKeys(Configuration.OpportunityConfiguration.Username);

            Thread.Sleep(1000);
            login.SendKeys(Keys.Return);
            Console.WriteLine("Hit any key after login & captcha");
            Console.ReadKey();
            Thread.Sleep(1000);
            var pass = WebDriver.FindElement(By.Id(Constants.IndeedPasswordCssID));
            pass.SendKeys(Configuration.OpportunityConfiguration.Password);
            Thread.Sleep(1000);
            pass.SendKeys(Keys.Return);
            Thread.Sleep(1000);
            // note : need user interaction to bypass captcha
            Console.WriteLine("Hit any key after login & captcha");
            Console.ReadKey();
        }

        /// <summary>
        /// Search Indeed to get search landing page
        /// </summary>
        public override void GetSearchPage(int? page)
        {
            System.Diagnostics.Debug.WriteLine(Configuration.OpportunityConfiguration.Position);
            System.Diagnostics.Debug.WriteLine(Configuration.OpportunityConfiguration.Location);
            // encode indeed search url string
            //var uri = "https://ca.indeed.com/jobs?q=junior+software+developer&l=Toronto%2C+ON&from=searchOnHP&vjk=4203b76ebedee612";
            var uri = "https://ca.indeed.com/jobs?q=entry+level+software+developer&l=Remote&from=searchOnHP&vjk=1dbe12f243c824bf";
            //var uri = String.Format("{0}/jobs?q={1}&l={2}",
            //    Constants.IndeedUrl,
            //    HttpUtility.UrlEncode(Configuration.OpportunityConfiguration.Position),
            //    HttpUtility.UrlEncode(Configuration.OpportunityConfiguration.Location));

            WebDriver.Url = uri;
        }

        /// <summary>
        /// Start job search
        /// </summary>
        public override async Task StartJobSearch()
        {

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            void OnProcessExit(object sender, EventArgs e)
            {
                WebDriver.Quit();
            }

            OpportunityCounter counter = new();

            // HARD LIMIT 
            int num_pages_to_search = 100;
            // Get the current window handle
            string currentHandle = WebDriver.CurrentWindowHandle;

            // Get all window handles
            List<string> allHandles = WebDriver.WindowHandles.ToList();

            // Iterate through all window handles
            foreach (string handle in allHandles)
            {
                Thread.Sleep(100);
                // Switch to the window
                try
                {
                    WebDriver.SwitchTo().Window(handle);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Switched windows too quickly");
                }
                Thread.Sleep(100);
                // Close the window if it's not the current window
                if (handle != currentHandle)
                {
                    WebDriver.Close();
                }
            }

            // Switch back to the original window
            WebDriver.SwitchTo().Window(currentHandle);

            while (counter.Pagination < num_pages_to_search)
            {
                // load page source
                var doc = new HtmlDocument();
                doc.LoadHtml(WebDriver.PageSource);

                // close popup if present
                var popup = doc.DocumentNode.SelectSingleNode(".//button[contains(@class, 'popover-x-button-close')]");
                if (popup != null)
                {
                    WebDriver.FindElement(By.XPath(".//button[contains(@class, 'popover-x-button-close')]")).Click();
                }

                // get job containers from search page

                // Waits for 1 second
                //Thread.Sleep(1000);
                var jobContainers = this.GetOpportunityContainers(doc);
                foreach (var container in jobContainers)
                {
                    // check for opportunity in database
                    if (!await DataRepository.CheckIndeedOpportunity(this.ParseOpportunityLink(container.SelectSingleNode(Constants.IndeedContainerLinkClass))))
                    //if (!await DataRepository.CheckIndeedOpportunity(this.ParseOpportunityLink(container)))
                    {
                        counter.Opportunities++;
                        IndeedOpportunity opportunity = null;

                        // parse html for opportunity info
                        opportunity = await this.ParseOpportunityContainer(container);
                        if (opportunity.Id == -1)
                        {
                            Console.WriteLine($"[*] Rejected : \t{opportunity.Company} - {opportunity.Position}");
                        }
                        //else if (true || (!opportunity.Applied && opportunity.EasyApply))
                        else if ((!opportunity.Applied && opportunity.EasyApply))
                        {
                            //opportunity.Link = "https://www.indeed.com/viewjob?jk=cba79e809ed569cd&tk=1hteea6srir2s8c3&from=iaBackPress";
                            // submit an easy apply applicaiton
                            opportunity = await this.SubmitEasyApply(opportunity);
                            // null on error, reporting from exception
                            if (opportunity == null) continue;

                            counter.EasyAppliedTo++;
                            counter.AppliedTo++;
                            Console.WriteLine($"[*][{counter.EasyAppliedTo,3}] Applied:\t{opportunity.Company} - {opportunity.Position}");
                        }
                        else if (opportunity.Applied)
                        {
                            // already applied
                            counter.AppliedTo++;
                            Console.WriteLine($"[*][{counter.AppliedTo,3}] Synced:\t{opportunity.Company} - {opportunity.Position}");
                        }
                        else
                        {
                            // saved for off-site application
                            counter.Saved++;
                            Console.WriteLine($"[*][{counter.Saved,3}] Saved:\t\t{opportunity.Company} - {opportunity.Position}");
                        }
                    }
                }

                //// Print the HTML code to the console
                //Console.WriteLine(htmlCode);
                // move to next page
                counter.Pagination++;
                Thread.Sleep(1000);
                ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");

                try
                {
                    WebDriver.FindElement(By.XPath(Constants.IndeedNextSearchLink)).Click();
                }
                catch
                {
                    Debug.WriteLine("Ran out of jobs to apply for currently with this search");
                    break;
                }
                Thread.Sleep(1000);
                Console.WriteLine($"[*] Pages Scraped :{counter.Pagination} \n[*] Opportunities Parsed : {counter.Opportunities}");
            }
        }

        /// <summary>
        /// Checks job for easy apply tags
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private bool ParseEasyResumeTags(HtmlNode container)
        {
            // check job container for easy apply tags
            if (container.SelectSingleNode(Constants.IndeedXpathEasilyApply) != null) return true;
            else if (container.SelectNodes(Constants.IndeedXpathEasyResumeButton) != null) return true;
            else return false;
        }

        /// <summary>
        /// Parse and create opportunity link
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string ParseOpportunityLink(HtmlNode node)
        {
            //Console.WriteLine(node.InnerHtml);
            //Console.WriteLine(node.Attributes["data-jk"].Value);
            //Console.WriteLine(node.Attributes["data-mobtk"].Value);
            // parse/create opportunity link
            return String.Format("https://www.indeed.com/viewjob?jk={0}&tk={1}",
                node.Attributes["data-jk"].Value,
                node.Attributes["data-mobtk"].Value);
        }

        /// <summary>
        /// Scrape opportunity containers for parsing
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private HtmlNodeCollection GetOpportunityContainers(HtmlDocument document)
        {
            //Console.WriteLine(document.DocumentNode);
            //HtmlNodeCollection allNodes = document.DocumentNode.SelectNodes("//*");
            ////Console.WriteLine(allNodes.Count);
            //foreach (HtmlNode node in allNodes)
            //{
            //    if (node.NodeType == HtmlNodeType.Element)
            //    {
            //        var classAttribute = node.Attributes["class"];
            //        if (classAttribute != null)
            //        {
            //            Console.WriteLine("Element Tag: " + node.Name);
            //            Console.WriteLine("CSS Classes: " + classAttribute.Value);
            //            //Console.WriteLine("Node HTML: " + node.OuterHtml);
            //            Console.WriteLine(); // Add a blank line for clarity
            //        }
            //    }
            //}
            return document.DocumentNode.SelectNodes(Constants.IndeedContainerCssClass);
        }

        /// <summary>
        /// Parse and create opportunity object
        /// </summary>
        /// <param name="node"></param>
        /// <param name="easyApply"></param>
        /// <returns></returns>
        private async Task<IndeedOpportunity> ParseOpportunityContainer(HtmlNode node)
        {

            //cons
            var opportunity = new IndeedOpportunity();
            opportunity.Created = DateTime.Now;
            opportunity.Position = node.SelectSingleNode(Constants.IndeedXpathJobTitle)?.InnerText;
            opportunity.Company = node.SelectSingleNode(Constants.IndeedXpathCompany)?.InnerText;
            opportunity.Location = node.SelectSingleNode(Constants.IndeedXpathLocation)?.InnerText;
            opportunity.Salary = node.SelectSingleNode(Constants.IndeedXpathSalary)?.InnerText;
            opportunity.Description = node.SelectSingleNode(Constants.IndeedXpathJobSnippet)?.InnerText;

            // black & white lists
            bool isBlackWhiteListed = false;
            //opportunity.Link = this.ParseOpportunityLink(node);

            //Console.WriteLine(node.InnerHtml);
            //Console.WriteLine(node.SelectSingleNode(Constants.IndeedContainerLinkClass).InnerHtml);
            opportunity.Link = this.ParseOpportunityLink(node.SelectSingleNode(Constants.IndeedContainerLinkClass));
            opportunity.Applied = (node.SelectSingleNode(Constants.IndeedXpathAppliedAlready) != null) ? true : false;
            opportunity.EasyApply = this.ParseEasyResumeTags(node);

            // syncronize local database with indeed
            if (!await DataRepository.CheckIndeedOpportunity(opportunity.Link))
            {
                await DataRepository.AddIndeedOpportunity(opportunity);
            }

            return opportunity;
        }

        /// <summary>
        /// Submit an easily apply opportunity
        /// </summary>
        /// <param name="opportunity"></param>
        /// <returns></returns> 
        private async Task<IndeedOpportunity> SubmitEasyApply(IndeedOpportunity opportunity)
        {
            Console.WriteLine("Applying for " + opportunity.Link);
            try
            {
                // open new window and switch to handle
                ((IJavaScriptExecutor)WebDriver).ExecuteScript($"window.open('{opportunity.Link}', 'NewTab')");
                var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(2));
                wait.Until(x => WebDriver.SwitchTo().Window(WebDriver.WindowHandles[1]));

                // click to apply
                wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(2));
                Thread.Sleep(2000);
                try
                {
                    WebDriver.FindElement(By.XPath(Constants.IndeedXpathApplyButton)).Click();
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    //WebDriver.Close();
                }
                catch (OpenQA.Selenium.ElementClickInterceptedException)
                {
                    Debug.WriteLine("Something was covering the apply button");
                }


                try
                {
                    // look for indeed bug, say applied after already applying
                    WebDriver.FindElement(By.XPath(Constants.IndeedXpathAppliedBugTag));
                    Debug.WriteLine($"Indeed applied already bug");
                    return opportunity;
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    //WebDriver.Close();
                }

                try
                {
                    WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

                    // double check for applied opportunity
                    WebDriver.FindElement(By.XPath(Constants.IndeedXpathAppliedAlready));

                    opportunity = null;
                }
                catch (NoSuchElementException)
                {
                    // auto application apply loop
                    // note: each loop detects application steps
                    bool isParsing = true;

                    // TODO Add counter for max autoapply steps ( say 20 )?
                    while (isParsing)
                    {
                        // check for application view
                        isParsing = AutoApplyStepCheck();
                    }

                    // mark opportunity applied
                    opportunity.Applied = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Easy apply exception : {ex.ToString()}");
                    if (Program.VerboseMode)
                    {
                        Debug.WriteLine($"Easy apply exception : {ex.ToString()}");
                    }
                }
                finally
                {
                    WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

                    if (opportunity != null)
                    {
                        await DataRepository.UpdateIndeedOpportunity(opportunity);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Easy apply exception : {ex.Message}");
                if (Program.VerboseMode)
                {
                    Debug.WriteLine($"Easy apply exception : {ex.Message}");
                }

                opportunity = null;
            }
            finally
            {
                // close application window & return to main search page
                if (WebDriver.WindowHandles.Count > 0)
                    WebDriver.Close();

                WebDriver.SwitchTo().Window(WebDriver.WindowHandles[0]);
            }

            return opportunity;
        }

        /// <summary>
        /// Step check for inputting form values
        /// </summary>
        /// <returns></returns>
        private bool AutoApplyStepCheck()
        {
            Thread.Sleep(3000);
            // check page header for application step
            //Console.WriteLine(WebDriver.PageSource);
            //Console.WriteLine(Constants.IndeedXpathHeader);
            var path = "/html/body/div[2]/div/div[1]/div/div/div[2]/div[2]/div/div/main/h1";
            var heading = WebDriver.FindElement(By.XPath(path)).Text;
            Console.WriteLine(heading);
            if (heading.Contains(Constants.IndeedResumeHeader)) ApplyResumeStep();
            else if (heading.Contains(Constants.IndeedQuestionsHeader)) ApplyQuestionsStep();
            else if (heading.Contains(Constants.IndeedPastExperienceHeader)) ApplyPastJobExperienceStep();
            else if (heading.Contains(Constants.IndeedQualificationsHeader)) ApplyBypassRequiredQualifications();
            else if (heading.Contains(Constants.IndeedCoverLetterHeader))
            {
                if (!Configuration.IndeedConfiguration.BypassRequirements) return false;
                ApplyAddCoverLetterStep();
            }
            else if (heading.Contains(Constants.IndeedLocationHeader)) ApplyLocationStep();
            else if (heading.Contains(Constants.IndeedVoluntaryIdentificationHeader)) ApplyVoluntaryIdentificationStep();
            else if (heading.Contains(Constants.IndeedRequestsCoverLetterHeader)) ApplyWriteCoverLetterStep();
            else if (heading.Contains(Constants.IndeedReviewApplicationHeader))
            {
                ApplyReviewStep();
                Thread.Sleep(1000);
                return false;
            }
            else
            {
                ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
                try
                {
                    var review_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathReviewButton));
                    review_button.Click();
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("no review button");
                }

                try
                {
                    var review_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathContinueButton));
                    review_button.Click();
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("no continue button");

                }
            }

            return true;
        }

        /// <summary>
        /// Submit custom PDF to application
        /// </summary>
        private void ApplyResumeStep()
        {
            Thread.Sleep(2000);
            // picks uploaded pdf resume on indeed
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            wait.Until(x => x.FindElement(By.XPath(Constants.IndeedXpathPdfResume))).Click();

            ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
            var wait1 = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            Thread.Sleep(1000);
            wait1.Until(x => x.FindElement(By.XPath(Constants.IndeedXpathContinueButton))).Click();
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Submit employee questions
        /// </summary>
        private void ApplyQuestionsStep()
        {
            // missed questions
            int missedQuestions = 0;

            // load page source
            var doc = new HtmlDocument();
            doc.LoadHtml(WebDriver.PageSource);

            // get questions from container
            var questions = doc.DocumentNode.SelectNodes(Constants.IndeedXpathQuestions);
            //Console.WriteLine(Constants.IndeedXpathQuestions);
            //Console.WriteLine(Constants.IndeedXpathInputId);
            // loop over questions to answer
            int i = 0;
            if (questions != null)
            {
                foreach (var element in questions)
                {
                    // get input id, question, and possible answer from yml configuration
                    var inputIdElement = element.SelectSingleNode(Constants.IndeedXpathInputId);
                    var SelectIdElement = element.SelectSingleNode(Constants.IndeedXpathSelectSelector);
                    var TextAreaIdElement = element.SelectSingleNode(Constants.IndeedXpathTextAreaSelector);

                    if (inputIdElement != null)
                    {
                        // need to get the id, and then find the element
                        var inputId = inputIdElement.GetAttributeValue("id", string.Empty);
                        if (inputId == "") continue;
                        var input = WebDriver.FindElement(By.Id(inputId));


                        var type = input.GetAttribute("type");
                        if (type == "radio" || type == "checkbox")
                        {
                            ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].click();", input);
                        }
                        else if (type == "text" || type == "number")
                        {
                            var placeholder = inputIdElement.GetAttributeValue("placeholder", string.Empty);
                            var currentValue = inputIdElement.GetAttributeValue("value", string.Empty);
                            for (int j = 0; j < 10; j++)
                            {
                                input.SendKeys(Keys.Backspace);
                            }
                            if (currentValue.Length == 0)
                            {
                                if (placeholder != "")
                                {
                                    input.SendKeys(placeholder);
                                }
                                else
                                {
                                    input.SendKeys("10");
                                }
                            }
                            else
                            {
                                input.SendKeys(currentValue);
                            }
                        }
                        else if (type == "file")
                        {
                            input.SendKeys("C:\\Users\\leony\\Desktop\\projects\\easy_apply\\src\\cover_letter.pdf");
                        }
                        else if (type == "tel")
                        {
                            input.SendKeys("1111111111");
                        }
                        else
                        {
                            var placeholder = inputIdElement.GetAttributeValue("placeholder", string.Empty);
                            if (placeholder != "")
                            {
                                input.SendKeys(placeholder);
                            }
                        }
                    }
                    // see if there is a select element
                    //var selectNode = element.SelectSingleNode(Constants.IndeedXpathSelectSelector);
                    if (SelectIdElement != null)
                    {
                        var SelectId = SelectIdElement.GetAttributeValue("id", string.Empty);
                        if (SelectId == "") continue;
                        var Select = WebDriver.FindElement(By.Id(SelectId));
                        Select.SendKeys(Keys.Space);
                        // pressing down 3 times seems to go to postdoctorate for the education question quite frequently
                        Select.SendKeys(Keys.Down);
                        //Select.SendKeys(Keys.Down);
                        //Select.SendKeys(Keys.Down);
                        Select.SendKeys(Keys.Enter);
                    }

                    if (TextAreaIdElement != null)
                    {
                        var TextAreaId = TextAreaIdElement.GetAttributeValue("id", string.Empty);
                        if (TextAreaId == "") continue;
                        var TextArea = WebDriver.FindElement(By.Id(TextAreaId));
                        TextArea.SendKeys("100000");
                    }

                    // scroll to input id
                    //((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].scrollIntoView(true);", WebDriver.FindElement(By.Id(inputId)));
                    //var question = element.SelectSingleNode(Constants.IndeedXpathLabelQuestionValue);
                    //var answer = Configuration.OpportunityQuestions
                    //.FirstOrDefault(x => question.InnerText.IndexOf(x.Substring, 0, StringComparison.CurrentCultureIgnoreCase) != -1);
                    //object answer = null;

                    i = i + 1;

                    if (i == 50)
                    {
                        throw new Exception("Got stuck on a question for job " + WebDriver.Url);
                    }
                }
            }

            if (missedQuestions > 0)
            {
                throw new Exception("Missed Questions, moving on but got new questions for database");
            }

            ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
            try
            {
                var continue_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathContinueButton));
                continue_button.Click();
                Thread.Sleep(1000);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("no continue button");

            }
        }

        /// <summary>
        /// Skip auto populated job experience step
        /// </summary>
        private void ApplyPastJobExperienceStep()
        {
            Thread.Sleep(1000);

            var doc = new HtmlDocument();
            doc.LoadHtml(WebDriver.PageSource);

            // get questions from container
            var questions = doc.DocumentNode.SelectNodes(Constants.IndeedXpathInputId);
            //Console.WriteLine(Constants.IndeedXpathQuestions);
            //Console.WriteLine(Constants.IndeedXpathInputId);
            // loop over questions to answer
            if (questions != null)
            {

                int i = 0;
                String[] answers = { "Full Stack Developer", "Encircle" };

                // assuming there are two fields, job title and company
                foreach (var inputIdElement in questions)
                {
                    // get input id, question, and possible answer from yml configuration
                    //var inputIdElement = element.SelectSingleNode(Constants.IndeedXpathInputId);
                    var inputId = inputIdElement.GetAttributeValue("id", string.Empty);
                    if (inputId == "") continue;
                    var input = WebDriver.FindElement(By.Id(inputId));
                    for (int j = 0; j < 50; j++)
                    {
                        input.SendKeys(Keys.Backspace);
                    }
                    input.SendKeys(answers[i]);

                    i = i + 1;
                }
            }

            try
            {
                var continue_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathContinueButton));
                continue_button.Click();
                Thread.Sleep(1000);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("no continue button");

            }

            //// auto populated
            //var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            //wait.Until(x => x.FindElement(By.XPath(Constants.IndeedXpathContinueButton))).Click();
            //Thread.Sleep(2000);
        }

        /// <summary>
        /// Bypass employer qualification requirements
        /// </summary>
        private void ApplyBypassRequiredQualifications()
        {
            // bypass warnings about qualifications
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            wait.Until(x => x.FindElement(By.XPath(Constants.IndeedXpathContinueButton))).Click();
        }

        /// <summary>
        /// Add custom or default cover letter
        /// </summary>
        private void ApplyAddCoverLetterStep()
        {
            //try
            //{
            //    // add resume located at yml configuration path
            //    WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            //    var uploadFile = WebDriver.FindElement(By.Id("additionalDocuments"));
            //    uploadFile.SendKeys(Configuration.OpportunityConfiguration.CoverLetter);

            //    WebDriver.FindElement(By.XPath(Constants.IneedXpathWriteCoverLetterButton)).Click();
            //}
            //catch (NoSuchElementException)
            //{
            //    // select auto populated cover letter text
            //    var wait1 = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            //    wait1.Until(x => x.FindElement(By.XPath(Constants.IneedXpathWriteCoverLetterButton))).Click();
            //}

            //WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
            //((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
            //var wait2 = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            Thread.Sleep(1000);
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
            try
            {
                var review_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathReviewButton));
                review_button.Click();
                Thread.Sleep(1000);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("no continue button");

            }
        }

        /// <summary>
        /// Review, finalize & submit application
        /// </summary>
        /// 
        private void ApplyLocationStep()
        {
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
            Thread.Sleep(1000);
            try
            {
                var review_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathContinueButton));
                review_button.Click();
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("no continue button");
            }
            Thread.Sleep(1000);
        }

        private void ApplyWriteCoverLetterStep()
        {
            // load page source
            var doc = new HtmlDocument();
            doc.LoadHtml(WebDriver.PageSource);

            var TextAreaIdElement = doc.DocumentNode.SelectSingleNode(Constants.IndeedXpathTextAreaSelector);
            if (TextAreaIdElement != null)
            {
                var TextAreaId = TextAreaIdElement.GetAttributeValue("id", string.Empty);
                var TextArea = WebDriver.FindElement(By.Id(TextAreaId));
                TextArea.SendKeys("Dear Hiring Manager, I'm enthusiastic about this that I found on Indeed. My experience as a Software Developer at Encircle involved diverse tasks in web and mobile app development and testing, emphasizing both manual and automated testing methods. During an internship at Ecopia, I developed C++ programs for blending satellite images, refining my debugging and problem-solving skills. My undergraduate studies in Mathematics and Computer Science, particularly in Software Design and System Tools, honed my programming abilities. I am eager to discuss how my experiences align with the position's requirements. Thank you for considering my application.");
            }

            try
            {
                var review_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathReviewButton));
                review_button.Click();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("no continue button");
            }
            Thread.Sleep(1000);
        }
        private void ApplyVoluntaryIdentificationStep()
        {
            ApplyQuestionsStep();
            try
            {
                var review_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathReviewButton));
                review_button.Click();
                Thread.Sleep(2000);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("no review button");
            }

            try
            {
                var review_button = WebDriver.FindElement(By.XPath(Constants.IndeedXpathContinueButton));
                review_button.Click();
                Thread.Sleep(2000);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("no continue button");

            }
        }
        private void ApplyReviewStep()
        {
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            Thread.Sleep(1000);
            wait.Until(x => x.FindElement(By.XPath(Constants.IndeedXpathSubmitButton))).Click();
        }
    }
}