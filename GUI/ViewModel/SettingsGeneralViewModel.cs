using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace GUI.ViewModel
{
    class SettingsGeneralViewModel : NotifyPropertyChanged
    {
        private string _daxExe;
        private string _daxConfig;
        private string _daxPipeName;

        private int _eventShowQty;
        private int _eventShowTime;
        private int _eventReadTime;

        private string _userAccount;
        private string _userPass;
        private string _userDomain;

        private bool _firstRun;

        private string _updatePath;

        public string DaxExe
        {
            get
            {
                return _daxExe;
            }
            set
            {
                _daxExe = value;
                OnPropertyChanged("DaxExe");
                SettingChange("DaxExe", value);
            }
        }
        public string DaxConfig
        {
            get
            {
                return _daxConfig;
            }
            set
            {
                _daxConfig = value;
                OnPropertyChanged("DaxConfig");
                SettingChange("DaxConfig", value);
            }
        }
        public string DaxPipeName
        {
            get
            {
                return _daxPipeName;
            }
            set
            {
                _daxPipeName = value;
                OnPropertyChanged("DaxPipeName");
                SettingChange("DaxPipeName", value);
            }
        }
        public int EventShowQty
        {
            get
            {
                return _eventShowQty;
            }
            set
            {
                _eventShowQty = value;
                OnPropertyChanged("EventShowQty");
                SettingChange("EventShowQty", value);
            }
        }
        public int EventShowTime
        {
            get
            {
                return _eventShowTime;
            }
            set
            {
                _eventShowTime = value;
                OnPropertyChanged("EventShowTime");
                SettingChange("EventShowTime", value);
            }
        }
        public int EventReadTime
        {
            get
            {
                return _eventReadTime;
            }
            set
            {
                _eventReadTime = value;
                OnPropertyChanged("EventReadTime");
                SettingChange("EventReadTime", value);
            }
        }
        public string UserAccount
        {
            get
            {
                return _userAccount;
            }
            set
            {
                _userAccount = value;
                OnPropertyChanged("UserAccount");
                SettingChange("UserAccount", value);
            }
        }
        public string UserDomain
        {
            get
            {
                return _userDomain;
            }
            set
            {
                _userDomain = value;
                OnPropertyChanged("UserDomain");
                SettingChange("UserDomain", value);
            }
        }
        public bool FirstRun
        {
            get
            {
                return _firstRun;
            }
            set
            {
                _firstRun = value;
                OnPropertyChanged("FirstRun");
                SettingChange("FirstRun", value);
            }
        }
        public string UpdatePath
        {
            get
            {
                return _updatePath;
            }
            set
            {
                _updatePath = value;
                OnPropertyChanged("UpdatePath");
                SettingChange("UpdatePath", value);
            }
        }

        public string UserPass
        {
            get
            {
                return _userPass;
            }
            set
            {
                _userPass = value;
                OnPropertyChanged("UserPass");
                SettingChange("UserPass", value);
            }
        }

        private void SettingChange(string name, Object value)
        {
            Properties.UserSettings.Default[name] = value;
            Properties.UserSettings.Default.Save();
        }

        public SettingsGeneralViewModel()
        {
            DaxExe = Properties.UserSettings.Default.DaxExe;
            DaxConfig = Properties.UserSettings.Default.DaxConfig;
            DaxPipeName = Properties.UserSettings.Default.DaxPipeName;

            EventShowQty = Properties.UserSettings.Default.EventShowQty;
            EventShowTime = Properties.UserSettings.Default.EventShowTime;
            EventReadTime = Properties.UserSettings.Default.EventReadTime;

            UserAccount = Properties.UserSettings.Default.UserAccount;
            UserPass = Properties.UserSettings.Default.UserPass;
            UserDomain = Properties.UserSettings.Default.UserDomain;

            FirstRun = Properties.UserSettings.Default.FirstRun;

            UpdatePath = Properties.UserSettings.Default.UpdatePath;
        }

        private string DialogOpenFile(string initialPath, string filter)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = filter;

            if (File.Exists(initialPath))
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(initialPath);
            }

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return null;

            return openFileDialog.FileName;
        }
        private string DialogOpenDir(string initialPath)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (Directory.Exists(initialPath))
            {
                folderBrowserDialog.SelectedPath = initialPath;
            }
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return null;
            }
            return folderBrowserDialog.SelectedPath;
        }

        private RelayCommand openFileClientCommand;
        public ICommand OpenFileClientCommand
        {
            get
            {
                if (openFileClientCommand == null)
                {
                    openFileClientCommand = new RelayCommand((c) => { OpenFileClien(); });
                }
                return openFileClientCommand;
            }
        }
        public void OpenFileClien()
        {
            string fileName = DialogOpenFile(DaxExe, "Executable files(*.exe) | *.exe; | All files(*.*) | *.*");
            if (!String.IsNullOrEmpty(fileName))
            {
                DaxExe = fileName;
            }
        }

        private RelayCommand openFileConfigCommand;
        public ICommand OpenFileConfigComman
        {
            get
            {
                if (openFileConfigCommand == null)
                {
                    openFileConfigCommand = new RelayCommand((c) => { OpenFileConfig(); });
                }
                return openFileConfigCommand;
            }
        }
        public void OpenFileConfig()
        {
            string fileName = DialogOpenFile(DaxConfig, "Config files(*.axc) | *.axc; | All files(*.*) | *.*");

            if(!String.IsNullOrEmpty(fileName))
            {
                DaxConfig = fileName;
            }
        }

        private RelayCommand openUpdateFolderCommand;
        public ICommand OpenUpdateFolderCommand
        {
            get
            {
                if (openUpdateFolderCommand == null)
                {
                    openUpdateFolderCommand = new RelayCommand((c) => { OpenUpdateFolder(); });
                }
                return openUpdateFolderCommand;
            }
        }
        public void OpenUpdateFolder()
        {
            string folderName = DialogOpenDir(UpdatePath);

            if (!String.IsNullOrEmpty(folderName))
            {
                UpdatePath = folderName;
            }
        }
    }
}
