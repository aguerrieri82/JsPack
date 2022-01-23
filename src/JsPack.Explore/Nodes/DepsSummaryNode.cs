using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace JsPack.Explore
{
    public enum SummaryMode
    {
        Use,
        IsUsed
    }

    public class DepsSummaryNode : BaseViewModel, INode
    {
        public DepsSummaryNode()
        {
            Modules = new List<ModuleRefNode>();
        }
        public IEnumerable<INode> GetChildren()
        {
            return Modules.OrderByDescending(a=> a.Count);
        }

        public string Name { get; set; }

        public object Icon { get; set; }

        public int Count { get; set; }

        public float Size { get; set; }

        public string FullPath { get; set; }

        public SummaryMode Mode { get; set; }

        public IList<ModuleRefNode> Modules { get; }
    }
}
