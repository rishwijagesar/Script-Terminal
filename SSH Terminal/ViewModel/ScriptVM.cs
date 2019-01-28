using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSH_Terminal.ViewModel
{
    public class ScriptVM
    {

        private string _path;
        private string _content;
        private string _name;
        private MainViewModel _mainViewModel;

        public ScriptVM(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            ScriptCommand = new RelayCommand(Script);
        }

        private void Script()
        {
           
            _mainViewModel.command = _mainViewModel._sshClient.CreateCommand(_content);
            _mainViewModel.command.Execute();
            _mainViewModel.Output += _mainViewModel.command.Result;
        }

        public RelayCommand ScriptCommand { get; }

        public string Path { get => _path; set => _path = value; }
        public string Content { get => _content; set => _content = value; }
        public string Name { get => _name; set => _name = value; }
    }
}
