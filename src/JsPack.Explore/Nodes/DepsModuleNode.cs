using System.Collections.Generic;

namespace JsPack.Explore
{
    public class DepsModuleNode : INode
    { 
        public DepsModuleNode(DepsNode root)
        {
            Root = root;
            Use = new DepsSummaryNode() { Name = "Use", Mode = SummaryMode.Use };
            IsUsed = new DepsSummaryNode() { Name = "Is Used", Mode = SummaryMode.IsUsed };
        }

        public IEnumerable<INode> GetChildren()
        {
            yield return Use;
            yield return IsUsed;
        }

        public string Name { get; set; }

        public string FullPath { get; set; }

        public float Size { get; set; }

        public DepsSummaryNode Use { get;  }

        public DepsSummaryNode IsUsed { get; }

        public DepsNode Root { get;  }
    }
}
