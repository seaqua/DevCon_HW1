using Microsoft.ProjectOxford.Linguistics;
using Microsoft.ProjectOxford.Linguistics.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace HomeWork1
{
    public class Data
    {

        private LinguisticsClient Client = new LinguisticsClient("96fc9641c5934292be66b62d51fdde5c");
        private string sampleText = "I've got a sister. Her name is Kate. She's three. She has got many toys: four balls - red, green, blue, yellow; three puppies - black, white and grey; two brown monkeys, a yellow giraffe, a white bear and two nice dolls. Do you think she likes to play with her toys? No, she doesn't! What she does like is - she likes to play hide-and-seek with me. Do you know the way she does it? She hides under the table and says: Helen, where an I? I say: Are you on the chair? No, I'm not. Are you under the chair? No, try again! Oh, Kate, I try, and try, and try, but I can't see you! Then she runs up to me and says: Here I am! This is the way my little sister Kate plays hide-and-seek.Isn't she funny!";
        private Dictionary<string, List<string>> pos;

        public async Task<string> AnalyzeString(string string4analyze)
        {
            var Analyzers = await Client.ListAnalyzersAsync();

            var Req = new AnalyzeTextRequest();
            Req.AnalyzerIds = new Guid[] { Analyzers[0].Id };

            Req.Language = "en";
            Req.Text = string4analyze;

            var Res = await Client.AnalyzeTextAsync(Req);
            return Res[0].Result.ToString();
        }

        public async Task<Dictionary<string, List<string>>> CreatePOS()
        {
            //List<string> sample = sampleText.Split('[^A-Z]').ToList(); 
            List < string > sample = Regex.Split(sampleText, "[^a-zA-Z0-9-']").ToList();
            List<string> sample1 = Regex.Split(sampleText, "\\W").ToList();


            string analyzedString = await AnalyzeString(sampleText);
            List<string> posList = Regex.Replace(analyzedString, @"[^A-Z,]+", String.Empty).Split(',').ToList();

            pos = new Dictionary<string, List<string>>();

            for (int i = 0; i < sample.Count; i++)
            {
                List<string> l = new List<string>();
                if (pos.ContainsKey(posList[i]))
                {
                    l = pos[posList[i]];
                }
                l.Add(sample[i]);

                pos[posList[i]] = l;
            }

            return pos;
        }

        public async Task<string> ReplaceString(string message)
        {
            if (pos == null)
            {
                pos = await CreatePOS();
            }

            //List<string> sample = message.Split(' ').ToList();
            List<string> sample = Regex.Split(message, "[^a-zA-Z0-9-']").ToList();

            string analyzedString = await AnalyzeString(message);
            List<string> posList = Regex.Replace(analyzedString, @"[^A-Z,.]+", String.Empty).Split(',').ToList();

            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();

            for (int i = 0; i < posList.Count; i++)
            {
                List<string> l = new List<string>();
                if (pos.ContainsKey(posList[i]))
                {
                    sb.Append(pos[posList[i]][rnd.Next(pos[posList[i]].Count)]);
                } else
                {
                    sb.Append(sample[i]);
                }
                if (i+1 < posList.Count)
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }


        static async Task Work()
        {
            var Client = new LinguisticsClient("96fc9641c5934292be66b62d51fdde5c");

            var Analyzers = await Client.ListAnalyzersAsync();
            Console.WriteLine("ANALYZERS");

            foreach (var a in Analyzers)
            {
                Console.WriteLine($" > {a.Implementation}");
            }

            var f = File.OpenText(@"Data\wap.txt");
            StringBuilder sb = new StringBuilder();
            int c = 0;

            while (!f.EndOfStream)
            {
                var s = await f.ReadLineAsync();
/*                if (s.Contains("CHAPTER") || s.Contains("BOOK"))
                {
                    Console.WriteLine(s);
                    c++;
                    if (c > 10) break;
                    continue;
                }
*/
                if (s.Trim() == string.Empty)
                {
                    if (sb.Length > 5)
                    {
                        var Req = new AnalyzeTextRequest();
                        Req.Language = "en";
                        Req.Text = sb.ToString();
                        // Req.AnalyzerIds = (from x in Analyzers select x.Id).ToArray();
                        Req.AnalyzerIds = new Guid[] { Analyzers[1].Id };
                        var Res = await Client.AnalyzeTextAsync(Req);
                        Console.WriteLine(Res[0].Result);
                        //ShowAdj(Res[0].Result.ToString());
                        await Task.Delay(1000);
                        // Console.ReadKey();
                    }
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(s);
                }
            }
        }
        public static void ShowAdj(string s)
        {
            Regex ItemRegex = new Regex(@"\(JJ (\w+)\) \(NN (\w+)\)", RegexOptions.Compiled);
            foreach (Match ItemMatch in ItemRegex.Matches(s))
            {
                Console.WriteLine($"{ItemMatch.Groups[1].ToString()} {ItemMatch.Groups[2].ToString()}");
            }
        }

    }
}