using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace Word_Snake
{
    class WordList
    {
        StorageFile file;
        IList<String> words;
        
        public WordList()
        {
        }

        public async Task<List<String>> GetWords(int max_len,int count)
        {
            if (file == null)
            {
                var uri = new System.Uri("ms-appx:///Assets/Words.txt");
                file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            }
     
            if (words == null)
                words = await FileIO.ReadLinesAsync(file);

            List<String> result = new List<string>();
         
            foreach (String w in words)
            {
                if(w.Length<=max_len)
                    result.Add(w);

                if (result.Count == count)
                    break;
            }

            return result;
        }
    }
}
