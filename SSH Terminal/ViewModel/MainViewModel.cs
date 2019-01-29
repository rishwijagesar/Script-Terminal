using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
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
        private string _pathScripts;
        private string _pathConnections;
        private System.Windows.Media.Color _color;
        public ShellStream _shellStreamSSH;
        private ScriptVM _scriptVM;
        private ObservableCollection<ScriptVM> _listScript;
        private OpenFileDialog _openFileDialog;
        public SshCommand command;
        public SshClient _sshClient;

        public MainViewModel()
        {

            _pathScripts = @"C:\ScriptTerminal\Scripts";
            _pathConnections = @"C:\ScriptTerminal\Connections";
            Color = Color.FromRgb(0, 0, 0);
            Status = "Status : Not Connected";
            ThreadStart threadStart = new ThreadStart(RecvSSHData);
            Thread thread = new Thread(threadStart)
            {
                IsBackground = true
            };

            thread.Start();
            ListScript = new ObservableCollection<ScriptVM>();
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            EnterCommand = new RelayCommand(Enter);
            SaveConnectionCommand = new RelayCommand(SaveConnection);
            OpenConnectionCommand = new RelayCommand(OpenConnection);
            BreakCommand = new RelayCommand(Break);

        }

        private void Break()
        {
            command = _sshClient.CreateCommand("\x03");
            command.Execute();
            Output += command.Result;
        }

        private void OpenConnection()
        {

            _openFileDialog = new OpenFileDialog
            {
                InitialDirectory = _pathConnections,
                Title = "Open Connection File",
                Filter = "Text|*.txt"
            };
            if (_openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileName = _openFileDialog.FileName;
                Address = File.ReadLines(fileName).ElementAtOrDefault(0);
                UserName = File.ReadLines(fileName).ElementAtOrDefault(1);
                PortNumber = File.ReadLines(fileName).ElementAtOrDefault(2);
            }
        }

        private void SaveConnection()
        {
            if (_address != null || _userName != null && _portNumber != null)
            {
                if (!Directory.Exists(_pathConnections))
                {  // if it doesn't exist, create
                    Directory.CreateDirectory(_pathConnections);
                }

                if (!File.Exists(_address + ".txt"))
                {
                    File.WriteAllText(Path.Combine(_pathConnections, _address + ".txt"), _address + "\n" + _userName + "\n" + _portNumber);
                    Output += "Connection " + _address + " is succesfully saved!";
                }
                else
                {
                    System.Windows.MessageBox.Show("File will be overwritten");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Some fields are empty");
            }
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

        // read script
        private void Script()
        {
            if (!Directory.Exists(_pathScripts))
            {  // if it doesn't exist, create
                Directory.CreateDirectory(_pathScripts);
            }

            DirectoryInfo fileDir = new DirectoryInfo(_pathScripts);
            FileInfo[] TXTFiles = fileDir.GetFiles("*.txt");

            // if there are any txt files
            NewMethod(TXTFiles);

        }

        // check if there is any file in the folder
        private void NewMethod(FileInfo[] TXTFiles)
        {
            if (TXTFiles.Length > 0)
            {
                // loop through all txt files
                foreach (var file in TXTFiles)
                {
                    if (file.Exists)
                    {
                        // create ScriptVM
                        ListScript.Add(_scriptVM = new ScriptVM(this)
                        {
                            Name = file.Name.Replace(".txt", ""),
                            Path = file.DirectoryName,
                            Content = File.ReadAllText(file.DirectoryName + "\\" + file.Name)
                        });
                    }
                }
            }
        }

        // disconnect method
        private void Disconnect()
        {
            try
            {
                _sshClient.Disconnect();
                Output += "Disconnected from " + _address + "\n";
                Color = Color.FromRgb(0, 0, 0);
                Status = "Status: Not connected";
                ListScript.Clear();
            }
            catch
            {
                System.Windows.MessageBox.Show("Error: There is no connection available to close");
            }
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
                Script();
            }
            catch (Exception exp)
            {
                Color = Color.FromRgb(255, 0, 0);
                _status = "Status: cannot connect to " + _address;
                System.Windows.MessageBox.Show("Error: " + exp.Message);
            }
        }

        // receive ssh data
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
        public RelayCommand SaveConnectionCommand { get; }
        public RelayCommand OpenConnectionCommand { get; }
        public RelayCommand BreakCommand { get; }
        public ObservableCollection<ScriptVM> ListScript
        {
            get => _listScript; set
            {
                _listScript = value;
                RaisePropertyChanged("ListScript");
            }
        }

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
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
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
            set
            {
                _portNumber = value;
                RaisePropertyChanged("PortNumber");
            }
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