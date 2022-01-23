using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace JsPack.Explore
{
    public class ModuleRefNode : BaseViewModel, INotifyPropertyChanged, IDeleteHandler, INode
    {
        SummaryMode _mode;
        IList<ModuleRefNode> _container;

        public ModuleRefNode(DepsModuleNode module, SummaryMode mode, IList<ModuleRefNode> container)
        {
            _mode = mode;
            _container = container;
            Module = module;
        }


        public void NotifyDeleted()
        {
            _container.Remove(this);
            Module.Root.UpdateCount();
        }

        public IEnumerable<INode> GetChildren()
        {
            return Summary.Modules.Select(a => new ModuleRefNode(a.Module, _mode, Summary.Modules)).OrderByDescending(a=> a.Count);
        }

        public string Name => Module.Name;

        public string FullPath => Module.FullPath;

        public int Count => Summary.Count;

        public float Size => Summary.Size;

        public bool CanDelete => true;

        public DepsSummaryNode Summary => _mode == SummaryMode.Use ? Module.Use : Module.IsUsed;

        public DepsModuleNode Module { get; set; }
    }
}
