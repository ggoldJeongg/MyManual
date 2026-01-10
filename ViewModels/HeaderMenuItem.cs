using System.Windows.Input;

namespace MyManual.ViewModels
{
    public class HeaderMenuItem
    {
        public string Title { get; }
        public ICommand Command { get; }

        public HeaderMenuItem(string title, ICommand command)
        {
            Title = title;
            Command = command;
        }
    }
}
