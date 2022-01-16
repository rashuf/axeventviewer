using GUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Model;
using System.Diagnostics;
using GUI.Elements;
using GUI.Properties;
using System.IO;

namespace GUI.View.Pages
{
    /// <summary>
    /// Interaction logic for SettingsGeneral.xaml
    /// </summary>
    public partial class SettingsGeneral : UserControl
    {
        SettingsGeneralViewModel viewModel;
        public SettingsGeneral()
        {
            InitializeComponent();

            viewModel = new SettingsGeneralViewModel();

            DataContext = viewModel;
            
            if (viewModel.UserPass != null && viewModel.UserPass.Length > 0)
            {
                Pass.Password = "0000000";
            }

        }

        private void ApplyPass_Click(object sender, RoutedEventArgs e)
        {
            if(Pass.Password == "0000000")
            {
                using (var secureString = viewModel.UserPass.DecryptString())
                {
                    Debug.Print(secureString.ToInsecureString());
                }
                Notificator.Current.Show("Введите нновый пароль. Пароль не был изменен", TypesNotification.ShowError);
                return;
            }

            if (String.IsNullOrEmpty(Pass.Password))
            {
                viewModel.UserPass = null;
            }
            else
            {
                using (var secureString = Pass.Password.ToSecureString())
                {
                    viewModel.UserPass = secureString.EncryptString();
                }

                ((App)Application.Current).Controller.ReInitUserContext(
                       new UserContext(
                           UserSettings.Default.UserDomain,
                           UserSettings.Default.UserAccount,
                           UserSettings.Default.UserPass,
                           true));
            }
        }
        private void OpenLog_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(PathHelper.LogPath()))
            {
                if(!File.Exists(PathHelper.LogPath()))
                {
                    File.WriteAllText(PathHelper.LogPath(), "");
                }
                Process.Start(PathHelper.LogPath());
            }
        }

    }
}
