using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Explore
{
    public class AsyncCommand : BaseCommand
    {
        readonly Func<Task> _command;

        public AsyncCommand(Func<Task> command)
        {
            _command = command;
        }

        public override async void Execute(object parameter)
        {
            await _command().ConfigureAwait(true);
        }
    }
}
