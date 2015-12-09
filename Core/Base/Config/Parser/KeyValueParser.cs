using System;

namespace socket4net
{
    public class KeyValueParser : IKeyValueParser
    {
        private readonly IFileLoader _loader;
        public KeyValueParser(IFileLoader loader)
        {
            _loader = loader;
        } 

        public void Parse(IKeyValue container, string path)
        {
            var text = _loader.Read(path);
            if (string.IsNullOrEmpty(text)) 
            {
                Logger.Instance.FatalFormat("Parse {0} failed!", path);
                return;
            }

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                // # ... => comment
                if (line.TrimStart(' ')[0] == '#')
                    continue;

                var words = line.Split(new [] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length != 2)
                {
                    throw new Exception("Invalid key-value configuration file : " + path);
                }
                
               container.Add(words[0], words[1]);
            }
        }
    }
}