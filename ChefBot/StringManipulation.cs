using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using ChefBot.Properties;

namespace ChefBot
{
    /// <summary>
    ///     This is a library class which handles string manipulation required in this program
    /// </summary>
    public static class StringManupulation
    {
        private static readonly Random Rng = new Random();

        /// <summary>
        ///     This method is used to extract source from the webpage of avalaible listed solutions
        /// </summary>
        /// <param name="html">Document of solution links' list webpage</param>
        /// <returns>Source code from a random solution</returns>
        public static string GetExtractedSolution(string html)
        {
            string extractedSolution;

            var solutionLinksList = Regex.Matches(html, @"(/viewsolution/)\w+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray();

            if (!solutionLinksList.Any()) return FakeSourceCodeGenerator();

            //Select a random solution's link from the list of avalaible solutions
            var selectedSolutionLink = @"https://www.codechef.com"
                                       + solutionLinksList[Rng.Next(solutionLinksList.Length - 1)]
                                           .Replace("viewsolution", "viewplaintext");

            using (var client = new HttpClient())
            {
                //Extract the source code from the solution link

                client.DefaultRequestHeaders.Add("User-Agent", Resources.UserAgent);
                extractedSolution = client.GetStringAsync(selectedSolutionLink).Result
                    .Replace("<pre>", string.Empty)
                    .Replace("</pre>", string.Empty);
            }

            //Obfuscate the source code to create uniqueness
            return ObfuscatedSourceCode(WebUtility.HtmlDecode(extractedSolution));
        }


        /// <summary>
        ///     This method returns a randomly generated alphanumeric string of custom length
        /// </summary>
        /// <param name="minLength">Minimum length of the string</param>
        /// <param name="maxLength">Maximum length of the string</param>
        /// <returns>Randomly generated alphanumeric string of given length</returns>
        public static string RandomStringGenerator(int minLength = 10, int maxLength = 100)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*() ";
            var generatedString = new string(Enumerable.Repeat(chars, Rng.Next(500, 2500))
                .Select(s => s[Rng.Next(s.Length)])
                .ToArray());

            return generatedString;
        }


        /// <summary>
        ///     This method returns a random source code, extracted from one of the link from FakeSC text file
        /// </summary>
        /// <returns>Fake source code</returns>
        public static string FakeSourceCodeGenerator()
        {
            try
            {
                var urlList = Resources.FakeSC.Split('\n');
                var url = urlList[new Random().Next(urlList.Length)];
                string html;

                var regex = new Regex(@"(?<=<textarea.*>)(\n|\r|\r\n)*.*(?=</textarea>)", RegexOptions.Singleline);


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Resources.UserAgent);
                    html = WebUtility.HtmlDecode(client.GetStringAsync(url).Result);
                }

                var sourceCode = regex.Match(html).ToString();

                //Either return the vanilla version or the obfuscated versio
                return new Random().Next(2) == 0
                    ? sourceCode.Substring(sourceCode.IndexOf('#'))
                    : ObfuscatedSourceCode(sourceCode.Substring(sourceCode.IndexOf('#')));
            }
            catch (Exception)
            {
                //If an exception rises, simply return a randomly generated string instead

                return RandomStringGenerator(1500, 2500);
            }
        }


        /// <summary>
        ///     This methos is used to obfuscate the argument data or source code
        /// </summary>
        /// <param name="data">A source code is taken as argument</param>
        /// <returns>Obfuscated version of the source code</returns>
        public static string ObfuscatedSourceCode(string data)
        {
            var newData = Regex.Replace(data, @";\s+", ";");
            newData = Regex.Replace(newData, @"}\s+", "}");
            newData = newData + "\n\t//" + "\t" + RandomStringGenerator();
            return newData;
        }


        /// <summary>
        ///     This is a simple method used to convert a problem link to its solutions page URL
        /// </summary>
        /// <param name="originalLink">Take the original problem link</param>
        /// <returns>Solutions page URL of the problem</returns>
        public static string LinkToSolutions(string originalLink)
        {
            var solutionLink = string.Format(originalLink.Replace("problems", "status")
                                             + @"?sort_by=All&sorting_order=asc&language=11&status=15&handle=&Submit=GO");

            return solutionLink;
        }


        /// <summary>
        ///     This simple method modifies the problem link and return the URL of the submit page of that problem
        /// </summary>
        /// <param name="originalLink">Original problem link</param>
        /// <returns>Submit page URL of the problem</returns>
        public static string LinkToSubmission(string originalLink)
        {
            return originalLink.Replace("problems", "submit");
        }
    }
}