using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JsPack.Explore
{
    public enum SummaryMode
    {
        Use,
        IsUsed
    }

    public class ModuleRefView : INotifyPropertyChanged
    {
        public ModuleRefView(DepsModuleView module, DepsSummaryView parent)
        {
            DeleteCommand = new Command(Delete);
            Module = module;
            Parent = parent;
        }

        public void Delete()
        {
            Parent.Modules.Remove(this);
            Parent.OnPropertyChanged("Items");
            Child.OnPropertyChanged("Items");
            Module.Container.UpdateCount();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<ModuleRefView> Items => Child.Modules.OrderByDescending(a => a.Child.Count);

        public DepsSummaryView Child => Parent.Mode == SummaryMode.Use ? Module.Use : Module.IsUsed;

        public ICommand DeleteCommand { get; }

        public DepsModuleView Module { get; set; }

        public DepsSummaryView Parent { get; set; }
    }

    public class DepsModuleView
    {
        public DepsModuleView(DepsTreeViewModel container)
        {
            Container = container;

            Use = new DepsSummaryView() { Name = "Use", Mode = SummaryMode.Use, Parent = this };
            IsUsed = new DepsSummaryView() { Name = "Is Used", Mode = SummaryMode.IsUsed, Parent = this };
        }

        public string Name { get; set; }

        public string FullPath { get; set; }

        public DepsSummaryView Use { get;  }

        public DepsSummaryView IsUsed { get; }

        public DepsTreeViewModel Container { get; }

        public IEnumerable<DepsSummaryView> Items
        {
            get
            {
                yield return Use;
                yield return IsUsed;

            }
        }
    }

    public class DepsSummaryView : INotifyPropertyChanged
    {
        public DepsSummaryView()
        {
            Modules = new List<ModuleRefView>();
        }

        public string Name { get; set; }

        public object Icon { get; set; }

        public int Count { get; set; }

        public int Size { get; set; }

        public string FullPath { get; set; }

        public SummaryMode Mode { get; set; }

        public DepsModuleView Parent { get; set; }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<ModuleRefView> Items => Modules.OrderByDescending(a => a.Child.Count);

        public List<ModuleRefView> Modules { get; set; }
    }

    public class DepsTreeViewModel : INotifyPropertyChanged
    {
        Dictionary<string, DepsModuleView> _rootModules;

        public DepsTreeViewModel()
        {
            _rootModules = new Dictionary<string, DepsModuleView>();

            Load(@"D:\Development\Personal\Git\Eusoft\WebApp\src\Eusoft.WebApp\obj\js\index.js");
        }

        public void Load(string path)
        {
            var parser = new JsModuleParser();
            Root = parser.Parse(path, true);
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await Task.Run(() =>
            {
                _rootModules.Clear();
                
                GetOrCreateView(Root);

                UpdateCount();

                OnPropertyChanged(nameof(Modules));
            });
        }

        public void UpdateCount()
        {
            foreach (var module in _rootModules.Values)
                UpdateCount(module);
        }

        protected void UpdateCount(DepsModuleView moduleView)
        {
            moduleView.Use.Count = CountUse(moduleView, a => a.Use);
            moduleView.IsUsed.Count = CountUse(moduleView, a => a.IsUsed);

            moduleView.Use.OnPropertyChanged("Count");
            moduleView.IsUsed.OnPropertyChanged("Count");
        }

        protected int CountUse(DepsModuleView moduleView, Func<DepsModuleView, DepsSummaryView> action)
        {
            var processed = new HashSet<DepsModuleView>();
            CountUse(moduleView, action, processed); 
            return processed.Count - 1;
        }

        protected void CountUse(DepsModuleView moduleView, Func<DepsModuleView, DepsSummaryView> action, HashSet<DepsModuleView> processed)
        {
            if (processed.Contains(moduleView))
                return;
            processed.Add(moduleView);
            foreach (var item in action(moduleView).Modules)
                CountUse(item.Module, action, processed);
        }

        protected DepsModuleView GetOrCreateView(JsParsedModule module)
        {
            if (!_rootModules.TryGetValue(module.Path, out var moduleView))
            {
                moduleView = new DepsModuleView(this)
                {
                    Name = GetModuleName(module),
                    FullPath = GetModuleFullPath(module)
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

        protected void ProcessModule(JsParsedModule module, DepsModuleView moduleView)
        {
            if ((module.Flags & JsParsedModuleFlags.Invalid) != 0)
                return;

            var allRefs = module.Imports.Values.Union(module.Exports.Values).Union(module.ModuleImports);
            foreach (var import in allRefs.Where(a => (a.Flags & JsReferenceFlags.Internal) == 0).GroupBy(a=> a.FromModule))
            {
                if (import.Key == null)
                    continue;
                
                var importView = GetOrCreateView(import.Key);

                importView.IsUsed.Modules.Add(new ModuleRefView(moduleView, importView.IsUsed));

                moduleView.Use.Modules.Add(new ModuleRefView(importView, moduleView.Use));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public JsParsedModule Root { get; set; }

        public IEnumerable<DepsModuleView> Modules => _rootModules.Values.OrderByDescending(a => a.Use.Count);
    }
}
