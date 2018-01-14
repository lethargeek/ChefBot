using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChefBot.Properties;

namespace ChefBot
{
    /// <summary>
    ///     This is the main bot worker class which handles most bot operations
    /// </summary>
    public static class Bot
    {
        private static readonly Random Rng = new Random();

        /// <summary>
        ///     This is the crucial auto submission method used to primarily handle the bot's auto submission work
        /// </summary>
        /// <param name="browser">WebBrowser to operate on</param>
        /// <param name="currentlySolving">Label to show the currently solving problem's name</param>
        /// <param name="linkList">List of problem links to solve</param>
        /// <param name="awaitTime">Awaiting time after each successful submission</param>
        /// <returns>Bool value addressing the success of the method</returns>
        public static async Task AutoSubmission(WebBrowser browser, Label currentlySolving, List<string> linkList,
            int awaitTime)
        {
            while (linkList.Any())
                try
                {
                    var falseSubmission = Rng.Next(3); // Total false submissions for each problem

                    // Take out a link and remove it from the list afterwards

                    var indexToRemove = Rng.Next(linkList.Count);
                    var link = linkList[indexToRemove];
                    linkList.RemoveAt(indexToRemove);
                    string solutionListPageHtml;

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", Resources.UserAgent);

                        solutionListPageHtml = await
                            client.GetStringAsync(
                                new Uri(StringManupulation.LinkToSolutions(link.Replace("\r", string.Empty))));

                        currentlySolving.Text = link.Replace("https://www.codechef.com/problems/", string.Empty);
                    }

                    for (var count = 1; count <= falseSubmission; count++)
                    {
                        // False submissions

                        browser.Navigate(StringManupulation.LinkToSubmission(link));
                        await SubmitAsync(browser, StringManupulation.FakeSourceCodeGenerator());
                    }

                    // Navigate to submission page and finally submit the correct solution

                    browser.Navigate(StringManupulation.LinkToSubmission(link));
                    await SubmitAsync(browser, StringManupulation.GetExtractedSolution(solutionListPageHtml));
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    // Wait for awaitTime before jumping for the next submission

                    await Task.Delay(awaitTime);
                }
        }


        /// <summary>
        ///     This is the primary submit method used to submit solution for a problem
        /// </summary>
        /// <param name="browser">WebBrowser to operate on</param>
        /// <param name="solution">Solution to submit</param>
        /// <returns>Bool value addressing the success of the method</returns>
        private static async Task<bool> SubmitAsync(WebBrowser browser, string solution)
        {
            try
            {
                //Initially wait for 10 seconds and let the browser complete loading and start operation afterwards

                await Task.Delay(10000);

                browser.Document.GetElementById("edit-program").InnerText = solution;
                browser.Document.GetElementById("edit-submit").InvokeMember("Click");
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                //Wait for 10 seconds before next submission

                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            return true;
        }


        /// <summary>
        ///     This method is used to fetch the user submission stats
        /// </summary>
        /// <param name="problemsSolved">Label to print the total fetched problems solved stats</param>
        /// <param name="solutionsSubmitted">Label to print the total fetched solutions submitted stats</param>
        /// <param name="username">Username of the user to fetch stats for</param>
        /// <returns>Bool value addressing the success of the method</returns>
        public static async Task FetchStatsAsync(Label problemsSolved, Label solutionsSubmitted, string username)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //Download the html document data of the profile page

                    var html =
                        await
                            client.GetStringAsync
                            (new Uri(string.Format(@"https://codechef.com/users/" + username),
                                UriKind.Absolute));

                    var statsPattern = new Regex(@"(?<=<td>)\d+(?=</td>)");

                    //Parse the data and print them on the appropriate labels

                    problemsSolved.Text = statsPattern.Matches(html)[0].ToString();
                    solutionsSubmitted.Text = statsPattern.Matches(html)[2].ToString();
                }
            }
            catch (Exception)
            {
                //ignored
                //Exceptions are ignored as this method will be called periodically
            }
        }


        /// <summary>
        ///     This method is used to asynchronously login to CodeChef
        /// </summary>
        /// <param name="username">Username for the login credential</param>
        /// <param name="password">Password for the login credential</param>
        /// <param name="browser">WebBrowser to operate on</param>
        /// <returns>Bool value addressing the success of the method</returns>
        public static async Task<bool> LoginAsync(string username, string password, WebBrowser browser)
        {
            try
            {
                //Input username and password in the proper input boxes and hit submit

                browser.Document.GetElementById("edit-name").InnerText = username;
                browser.Document.GetElementById("edit-pass").InnerText = password;
                browser.Document.GetElementById("edit-submit").InvokeMember("Click");
            }

            catch (Exception)
            {
                await Task.Delay(1000);
                return false;
            }

            await Task.Delay(1000);
            return true;
        }
    }
}