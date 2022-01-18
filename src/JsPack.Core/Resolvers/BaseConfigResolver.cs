using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Core
{

    public class ConfigRef<T>
    {
        public T Value { get; set; }

        public string Path { get; set; }

        public DateTime ParseTime { get; set; }
    }

    public abstract class BaseConfigResolver<TConfig> : IJsModuleResolver
    {
        readonly string _fileName;
        IDictionary<string, ConfigRef<TConfig>> _pathCache;
        List<ConfigRef<TConfig>> _configList;


        public BaseConfigResolver(string fileName)
        {
            _fileName = fileName;
            _pathCache = new Dictionary<string, ConfigRef<TConfig>>();
            _configList = new List<ConfigRef<TConfig>>();
        }

        public ConfigRef<TConfig> FindConfig(string basePath)
        {
            basePath = Path.GetFullPath(basePath);

            if (_pathCache.TryGetValue(basePath, out var cacheConfig))
                return cacheConfig;

            foreach (var config in _configList.OrderByDescending(a => a.Path.Length))
            {
                if (basePath.StartsWith(config.Path))
                {
                    _pathCache[basePath] = config;
                    return config;
                }
            }

            var curPath = basePath;
            
            string filePath;

            while (true)
            {
                filePath = Path.Combine(curPath, _fileName);

                if (File.Exists(filePath))
                    break;

                if (curPath.Length <= 3)
                {
                    _pathCache[basePath] = null;
                    return null;
                }
                
                curPath = Path.GetFullPath(Path.Combine(curPath, ".."));
            }

            var result = new ConfigRef<TConfig>()
            {
                Value = JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(filePath)),
                ParseTime = DateTime.Now,
                Path = curPath
            };
            
            if (!_configList.Any(a=> a.Path == curPath))
                _configList.Add(result);

            _pathCache[basePath] = result; 

            return result;
        }

        public abstract JsModule Resolve(JsModuleResolveContext context, string module);

        public int Priority { get; set; }
    }
}
