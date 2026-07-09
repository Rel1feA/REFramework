using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace RECode.REFramework
{
    public class CsvReader :NormalSingleton<CsvReader>
    {
        public List<string[]> ReadCsv(string filePath,Encoding encoding=null)
        {
            if(encoding == null) encoding = new UTF8Encoding(true);
            List<string[]> lines = new List<string[]>();
            using (StreamReader reader = new StreamReader(filePath, encoding))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    string[] values = ParsecCsvLine(line);
                    lines.Add(values);
                }
            }
            return lines;
        }

        public string[] ParsecCsvLine(string line)
        {
            List<string> result=new List<string>();
            bool inQuotes=false;//判断是否在引号里面
            int start = 0;
            for(int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c=='"')
                {
                    inQuotes = !inQuotes;
                }
                else if(c==','&&!inQuotes)
                {
                    result.Add(line.Substring(start, i - start).Trim('"'));
                    start = i + 1;
                }
            }
            result.Add(line.Substring(start).Trim('"'));
            return result.ToArray();
        }

        public List<string[]> LoadFromStreamingAssets(string fileName)
        {
            string path=Path.Combine(Application.streamingAssetsPath,"Configs",fileName+".csv");
            if(!File.Exists(path))
            {
                Debug.LogError($"CSV文件不存在:{path}");
                return null;
            }
            return ReadCsv(path);
        }
    }
}

