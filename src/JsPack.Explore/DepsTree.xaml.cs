using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JsPack.Explore
{
    /// <summary>
    /// Interaction logic for DepsTree.xaml
    /// </summary>
    public partial class DepsTree : UserControl
    {
        public DepsTree()
        {
            InitializeComponent();

            var treeTable = new TreeTableView();
            var deps = new DepsNode();
            deps.Load(@"D:\Development\Personal\Git\DataLab\src\DataLab.Web\obj\js\DataLab\src\DataLab.Web\Scripts\index.js");
            treeTable.Load(deps);
            DataContext = treeTable;
        }
    }
}
