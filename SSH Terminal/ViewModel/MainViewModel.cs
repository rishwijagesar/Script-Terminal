using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Renci.SshNet;

namespace SSH_Terminal.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        private string _address;
        private string _userName;
        private string _password;
        private string _portNumber;
        private string _output;
        private string _input;
        private string _status;
        private System.Windows.Media.Color _color;

        private SshClient _sshClient;
        private ShellStream _shellStreamSSH;


        public MainViewModel()
        {
            _sshClient = null;
            _shellStreamSSH = null;
            _address = "143.176.230.43";
            _userName = "rishwi";
            _portNumber = "22";
            Color = Color.FromRgb(0, 0, 0);
            Status = "Status : Not Connected";
            ThreadStart threadStart = new ThreadStart(RecvSSHData);
            Thread thread = new Thread(threadStart)
            {
                IsBackground = true
            };

            thread.Start();
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            EnterCommand = new RelayCommand(Enter);
            ListDirectoryCommand = new RelayCommand(ListDirectory);

        }

        private void ListDirectory()
        {
            SshCommand command = _sshClient.CreateCommand("ls");
            command.Execute();
            Output += "\n";
            Output += command.Result;
        }

        private void Enter()
        {
            try
            {
                _shellStreamSSH.Write(Input + "\n");
                _shellStreamSSH.Flush();
                Input = "";
            }
            catch
            {

            }
        }

        private void Disconnect()
        {
            _sshClient.Disconnect();
            Output = "Disconnected from " + _address + "\n";
            Color = Color.FromRgb(0, 0, 0);
            Status = "Status: Not connected";
        }

        private void Connect()
        {
            try
            {
                _sshClient = new SshClient(_address, Convert.ToInt32(_portNumber), _userName, _password);

                _sshClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(120);
                _sshClient.Connect();
                Password = "";
                _shellStreamSSH = _sshClient.CreateShellStream("xterm-256color", 80, 160, 80, 160, 1024);
                Color = Color.FromRgb(0, 255, 0);
                Status = "Status: Connected to " + _address;
            }
            catch (Exception exp)
            {
                Color = Color.FromRgb(255, 0, 0);
                _status = "Status: cannot connect to " + _address;
                System.Windows.MessageBox.Show("Error: " + exp.Message);
            }
        }

        private void RecvSSHData()
        {
            while (true)
            {
                try
                {
                    if (_shellStreamSSH != null && _shellStreamSSH.DataAvailable)
                    {
                        // read data from shellstream
                        string data = this._shellStreamSSH.Read();

                        // remove ansi color codes
                        data = new Regex(@"\x1B\[[^@-~]*[@-~]").Replace(data, "");

                        Output += data;

                    }
                }
                catch
                {

                }
                System.Threading.Thread.Sleep(200);
            }
        }


        #region Properties

        public RelayCommand ConnectCommand { get; }
        public RelayCommand DisconnectCommand { get; }
        public RelayCommand EnterCommand { get; }
        public RelayCommand ListDirectoryCommand { get; }

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                RaisePropertyChanged("Address");
            }
        }

        public string UserName
        {
            get => _userName;
            set => _userName = value;
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string PortNumber
        {
            get => _portNumber;
            set => _portNumber = value;
        }
        public string Output
        {
            get => _output;
            set
            {
                _output = value;
                RaisePropertyChanged("Output");
            }
        }
        public string Input
        {
            get => _input;
            set
            {
                _input = value;
                RaisePropertyChanged("Input");
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }
        #endregion



    }
}