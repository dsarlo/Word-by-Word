using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace WordByWord.Services
{
    public static class Dictionary
    {
        public static async Task<string> DefineAsync(string word)
        {
            char[] blacklist = "1234567890`~!@#$%^&*()_+-={}[]\\|;:'\",<.>/?".ToCharArray();
            if (blacklist.Contains(word[word.Length - 1]))
            {
                word = word.Substring(0, word.Length - 1);
            }

            string definition = string.Empty;
            using (var hc = new HttpClient())
            {
                HttpResponseMessage response = await hc.GetAsync("https://www.dictionaryapi.com/api/v1/references/collegiate/xml/" + word.ToLower() + "?key=951049dd-152a-458f-bd61-6ee2385dc5b2");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(content);

                    try
                    {
                        string result = xmldoc.DocumentElement.SelectSingleNode("entry/def/dt").InnerText;
                        definition = result.IndexOf(':') == -1 ? result : result.Split(':')[1];
                        definition = definition.First().ToString().ToUpper() + definition.Substring(1);
                    }
                    catch (Exception)
                    {
                        return definition;
                    }
                }
                return definition;
            }
        }
    }
}
