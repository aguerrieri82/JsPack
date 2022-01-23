using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JsPack.Explore
{

    public class TreeTableView : BaseViewModel
    {
        TreeTableRow _row;

        public TreeTableView()
        {
            Rows = new ObservableCollection<TreeTableRow>();
        }

        public void Load(INode root)
        {
            Rows.Clear();
            _row = new TreeTableRow(this, root);
            _row.Level = -1;
            _row.IsExpanded = true;
        }

        public INode Root { get; set; }

        public ObservableCollection<TreeTableRow> Rows { get; }
    }
}