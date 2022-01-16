using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Navigation;
using FirstFloor.ModernUI.Windows.Controls;
using System.Reflection;

namespace GUI.View
{
    /// <summary>
    /// Логика взаимодействия для About.xaml
    /// </summary>
    public partial class About : ModernDialog
    {
        public About()
        {
            InitializeComponent();

            CloseButton.Content = "OK";            
            AboutApplicationName.Text = Application.ProductName;
            AboutApplicationVersion.Text = $"Версия {Application.ProductVersion} от {new System.IO.FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToShortDateString()}";
            AboutApplicationCopyright.Text = "© ООО \"Руссоль\", Иванченко А.Ю., Усманов Р.Ф.";
            BottomBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(Properties.UserSettings.Default.ThemeColor);
        }
        void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
