using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JsPack.Explore
{
    public abstract class BaseCommand : ICommand, INotifyPropertyChanged
    {
        bool _isEnabled;
        string _errorMessage;
        bool _isVisible;

        public BaseCommand()
        {
            _isEnabled = true;
            _isVisible = true;
        }

        public string ErrorMessage
        {
            set
            {
                if (_errorMessage == value)
                    return;
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
            get
            {
                return _errorMessage;
            }
        }

        public bool IsEnabled
        {
            set
            {
                if (_isEnabled == value)
                    return;
                _isEnabled = value;
                OnCanExecuteChanged(null);
                OnPropertyChanged(nameof(IsEnabled));
            }
            get
            {
                return _isEnabled;
            }
        }

        public bool IsVisible
        {
            set
            {
                if (_isVisible == value)
                    return;
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
            get
            {
                return _isVisible;
            }
        }

        virtual protected void OnCanExecuteChanged(EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }

        public bool CanExecute(object parameter)
        {
            return _isEnabled;
        }

        public abstract void Execute(object parameter);


        public event EventHandler CanExecuteChanged;

        #region INotifyPropertyChanged

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
