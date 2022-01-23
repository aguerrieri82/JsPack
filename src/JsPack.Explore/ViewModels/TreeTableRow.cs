using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JsPack.Explore
{
    public interface INode
    {
        IEnumerable<INode> GetChildren();
    }

    public interface IDeleteHandler
    {
        void NotifyDeleted();

        bool CanDelete { get; }
    }

    public class TreeTableRow : BaseViewModel
    {
        protected IList<TreeTableRow> _children;
        protected bool _isExpanded;
        protected Visibility _visibility;
        protected int _level;
        protected int _index;
        TreeTableView _container;

        public TreeTableRow(TreeTableView container, INode value)
        {
            _container = container;
            Value = value;
            ToggleExpandCommand = new Command(ToggleExpand);
            DeleteCommand = new Command(Delete);
            DeleteVisibility = Value is IDeleteHandler deleteHandler && deleteHandler.CanDelete ? Visibility.Visible : Visibility.Collapsed;
        }

        public ICommand DeleteCommand { get; }

        public void Delete()
        {
            _container.Rows.Remove(this);

            if (Value is IDeleteHandler deleteHandler)
                deleteHandler.NotifyDeleted();

            DeleteChildren();

            if (Parent != null)
            {
                Parent.Children.Remove(this);
                Parent?.UpdateComputed();
            }
        }

        protected void DeleteChildren()
        {
            if (_children == null)
                return;

            foreach (var child in _children)
                child.Delete();
        }

        public void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
        }

        public void UpdateVisibility(bool isVisible)
        {
            if (_children == null)
                return;

            foreach (var child in _children)
            {
                child.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                if (child.IsExpanded)
                    child.UpdateVisibility(isVisible);
            }
        }

        public void RefreshChildren()
        {
             DeleteChildren();

            if (_children == null)
                _children = new List<TreeTableRow>();
            else
                _children.Clear();

            foreach (var childNode in Value.GetChildren())
            {
                var childRow = new TreeTableRow(_container, childNode);
                childRow.Parent = this;
                childRow.Level = _level + 1;

                if (Parent == null)
                    childRow._index = _container.Rows.Count;
                else
                {
                    if (_children.Count == 0)
                        childRow._index = _container.Rows.IndexOf(this) + 1;
                    else
                        childRow._index = _children[_children.Count - 1]._index + 1;
                }
                _children.Add(childRow);
                _container.Rows.Insert(childRow._index, childRow);
            }
            UpdateComputed();
        }

        protected void UpdateComputed()
        {
            ExpandVisibility = _children != null && _children.Count == 0 ? Visibility.Hidden : Visibility.Visible;
            OnPropertyChanged(nameof(ExpandVisibility));
        }

        public IList<TreeTableRow> Children
        {
            get
            {
                if (_children == null)
                    RefreshChildren();
                return _children;
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                    return;
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
                if (_isExpanded && _children == null)
                    RefreshChildren();
                UpdateVisibility(_isExpanded);
            }
        }

        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility == value)
                    return;
                _visibility = value;
                OnPropertyChanged(nameof(Visibility));
            }
        }

        public int Level
        {
            get => _level;
            set
            {
                if (_level == value)
                    return;
                _level = value;
                OnPropertyChanged(nameof(Level));
                OnPropertyChanged(nameof(Margin));
            }
        }

        public Visibility DeleteVisibility { get; }

        public Visibility ExpandVisibility { get; set; }

        public TreeTableRow Parent { get; internal set; }

        public ICommand ToggleExpandCommand { get; }

        public Thickness Margin => new Thickness(_level * 10, 0, 0, 0);

        public INode Value { get; }

    }
}
