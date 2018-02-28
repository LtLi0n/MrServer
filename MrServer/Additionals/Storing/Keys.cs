using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Additionals.Storing
{
    public class Keys
    {
        public static async Task LoadAPIKeys(string path = "data/ApiKeys")
        {
            API_KEYS = new ApiDictionary();

            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                string Key = await sr.ReadToEndAsync();
                string Tag = Path.GetFileNameWithoutExtension(file);
                string[] pathChunks = Tag.Split('/');
                Tag = pathChunks[pathChunks.Length - 1];

                API_KEYS.Add(Tag, Key);

                sr.Close();
                fs.Close();
            }
        }

        public static ApiDictionary API_KEYS;

        public sealed class ApiDictionary : Dictionary<string, string>
        {
            public new void Add(string Tag, string Key) => base.Add(Tag, Key);
            public new string this[string Tag] => base[Tag];
        }
    }
}
