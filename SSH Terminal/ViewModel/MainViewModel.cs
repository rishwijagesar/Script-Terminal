using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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

        private SshClient _sshClient;
        private ShellStream _shellStreamSSH;


        public MainViewModel()
        {
            _sshClient = null;
            _shellStreamSSH = null;
            _address = "143.176.230.43";
            _userName = "rishwi";
            _portNumber = "22";
            Status = "Status : Not Connected";
            ThreadStart threadStart = new ThreadStart(recvSSHData);
            Thread thread = new Thread(threadStart)
            {
                IsBackground = true
            };

            thread.Start();
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            EnterCommand = new RelayCommand(Enter);

        }

        private void Enter()
        {
            try
            {
                _shellStreamSSH.Write(Output+ "\n");
                _shellStreamSSH.Flush();

                Output = "";
            }
            catch
            {

            }
        }

        private void Disconnect()
        {
            _sshClient.Disconnect();
            Output = "Disconnected from " + _address;
            Status = "Status: Not connected";
        }

        private void Connect()
        {
            try
            {
                _sshClient = new SshClient(_address, Convert.ToInt32(_portNumber), _userName, _password);

                this._sshClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(120);
                this._sshClient.Connect();

                this._shellStreamSSH = this._sshClient.CreateShellStream("vt100", 80, 60, 800, 600, 65536);
                Status = "Status: Connected to " + Address;
            }
            catch (Exception exp)
            {
                this._status = "Status: cannot connect to " + _address;
                System.Windows.MessageBox.Show("Error: " + exp.Message);
            }
        }

        private void recvSSHData()
        {
            while (true)
            {
                try
                {
                    if (_shellStreamSSH != null && _shellStreamSSH.DataAvailable)
                    {
                        // read data from shellstream
                        string data = this._shellStreamSSH.Read();
                        data = data.Replace("[01;34m", "");
                        data = data.Replace("[00m", "");
                        data = data.Replace("[m", "");
                        data = data.Replace("[01;32m", "");
                        data = data.Replace(" ", "");

                        Output = data;
                        
                    }
                }
                catch
                {

                }
                System.Threading.Thread.Sleep(500);
            }
        }


        #region Properties

        public RelayCommand ConnectCommand { get; }
        public RelayCommand DisconnectCommand { get; }
        public RelayCommand EnterCommand { get; }

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
            set => _password = value;
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