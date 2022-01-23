using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Explore
{

    public class DepsNode : INode
    {
        Dictionary<string, DepsModuleNode> _rootModules;

        public DepsNode()
        {
            _rootModules = new Dictionary<string, DepsModuleNode>();
        }

        public void Load(string path)
        {
            var parser = new JsModuleParser();
            Root = parser.Parse(path, true);
            Refresh();
        }

        public void Refresh()
        {
            _rootModules.Clear();

            GetOrCreateView(Root);

            UpdateCount();
        }

        public void UpdateCount()
        {
            foreach (var module in _rootModules.Values)
                UpdateCount(module);
        }

        protected void UpdateCount(DepsModuleNode moduleView)
        {
            UpdateIndicators(moduleView, a => a.Use);
            UpdateIndicators(moduleView, a => a.IsUsed);

        }

        protected void UpdateIndicators(DepsModuleNode moduleView, Func<DepsModuleNode, DepsSummaryNode> action)
        {
            var processed = new HashSet<DepsModuleNode>();

            UpdateIndicators(moduleView, action, processed); 
            
            action(moduleView).Count = processed.Count - 1;
            action(moduleView).OnPropertyChanged("Count");

            action(moduleView).Size = processed.Sum(a => a.Size);
            action(moduleView).OnPropertyChanged("Size");

        }

        protected void UpdateIndicators(DepsModuleNode moduleView, Func<DepsModuleNode, DepsSummaryNode> action, HashSet<DepsModuleNode> processed)
        {
            if (processed.Contains(moduleView))
                return;
            processed.Add(moduleView);
            foreach (var item in action(moduleView).Modules)
                UpdateIndicators(item.Module, action, processed);
        }

        protected DepsModuleNode GetOrCreateView(JsParsedModule module)
        {
            if (!_rootModules.TryGetValue(module.Path, out var moduleView))
            {
                moduleView = new DepsModuleNode(this)
                {
                    Name = GetModuleName(module),
                    FullPath = GetModuleFullPath(module),
                    Size = new FileInfo(module.Path).Length / 1024
                };
                _rootModules[module.Path] = moduleView;
                ProcessModule(module, moduleView);
                Debug.WriteLine(module.Path);
            }

            return moduleView;
        }

        protected string GetModuleName(JsParsedModule module)
        {
            return module.Name ?? Path.GetFileNameWithoutExtension(module.Path);
        }

        protected string GetModuleFullPath(JsParsedModule module)
        {
            return Path.GetRelativePath(Path.GetDirectoryName(Root.Path), module.Path);
        }

        protected void ProcessModule(JsParsedModule module, DepsModuleNode moduleView)
        {
            if ((module.Flags & JsParsedModuleFlags.Invalid) != 0)
                return;

            var allRefs = module.Imports.Values.Union(module.Exports.Values).Union(module.ModuleImports);
            foreach (var import in allRefs.Where(a => (a.Flags & JsReferenceFlags.Internal) == 0).GroupBy(a=> a.FromModule))
            {
                if (import.Key == null)
                    continue;
                
                var importView = GetOrCreateView(import.Key);

                importView.IsUsed.Modules.Add(new ModuleRefNode(moduleView, SummaryMode.IsUsed, importView.IsUsed.Modules));

                moduleView.Use.Modules.Add(new ModuleRefNode(importView, SummaryMode.Use, moduleView.Use.Modules));
            }
        }

        public IEnumerable<INode> GetChildren()
        {
            return _rootModules.Values.OrderByDescending(a => a.Use.Count);
        }

        public JsParsedModule Root { get; set; }

    }
}
