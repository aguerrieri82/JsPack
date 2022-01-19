using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPack.Explore
{
    public class Command : BaseCommand
    {
        readonly Action _command;

        public Command(Action command)
        {
            _command = command;
        }

        public override void Execute(object parameter)
        {
            _command();
        }
    }
}
