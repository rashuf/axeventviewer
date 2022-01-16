using FirstFloor.ModernUI.Presentation;
using GUI.Elements;
using GUI.Properties;
using Model;
using Model.Extensions;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        static Mutex mutex;
        private EventInboxController controller;
        private NotifyIcon notifyIcon;
        private ViewModel.HomeViewModel homeViewModel;

        public EventInboxController Controller { get => controller; }
        public NotifyIcon NotifyIcon { get => notifyIcon; }
        internal ViewModel.HomeViewModel HomeViewModel { get => homeViewModel; }

        protected override void OnStartup(StartupEventArgs e)
        {
            CheckIsRunning();

            base.OnStartup(e);
            InitUserSettings();

            InitController();
            InitNotificator();
            InitMainWindow();

            homeViewModel = new ViewModel.HomeViewModel(); // URF, 30.08.2018, при запуске в свернутом виде не показываются оповещения
        }

        private void InitUserSettings()
        {
            AppearanceManager.Current.AccentColor = UserSettings.Default.ThemeColor;
            AppearanceManager.Current.FontSize = UserSettings.Default.ThemeFontSize;
            AppearanceManager.Current.ThemeSource = UserSettings.Default.ThemeSource;
        }
        private void InitController()
        {
            UserContext userContext = new UserContext(
                UserSettings.Default.UserDomain, 
                UserSettings.Default.UserAccount, 
                UserSettings.Default.UserPass,
                true);

            userContext.OnException += OnExсeption;
                        
            controller = new EventInboxController(userContext, $"Data Source = {PathHelper.DataBasePath()}");
            controller.TimerInterval = UserSettings.Default.EventReadTime * 1000;
            controller.MainThred = SynchronizationContext.Current;
            controller.OnException += OnExсeption;
            controller.Start();
        }
        private void InitNotificator()
        {
            Notificator.Current.InitNotificator(this.Dispatcher, UserSettings.Default.EventShowQty, UserSettings.Default.EventShowTime);
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            MainWindow.Hide();
        }
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.Hide();
            }
        }
        private void InitMainWindow()
        {
            MainWindow = new View.Root();
            MainWindow.Closing += MainWindow_Closing;
            MainWindow.StateChanged += MainWindow_StateChanged;

            InitNotifyIcon();
        }
        private void InitNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = GUI.Properties.Resource.Icon;
            notifyIcon.Click += (s, e) => ShowMainWindow(s, e);
            notifyIcon.Visible = true;

            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Открыть").Click += (s, e) => ShowMainWindowSimple();
            notifyIcon.ContextMenuStrip.Items.Add("Выход").Click += (s, e) => ExitApplication();
        }
        private void ShowMainWindow(object sender, EventArgs e)
        {
            MouseEventArgs mouseEventArgs = e as MouseEventArgs;

            if (mouseEventArgs != null)
            {
                switch (mouseEventArgs.Button)
                {
                    case MouseButtons.Left :
                        ShowMainWindowSimple();
                        break;

                    case MouseButtons.Right:
                        notifyIcon.ContextMenuStrip.Visible = true;
                        break;
                }
            }
  
        }
        internal void ShowMainWindowSimple()
        {
            if (MainWindow.IsVisible)
            {
                MainWindow.Hide();
            }
            else
            {

                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Activate();
            }
        }
        private void ExitApplication()
        {
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }
            Environment.Exit(0);
        }
        private void OnExсeption(object sender, ErrorMessageEventArgs args) //URF, 13.09.2018, переименовал метод
        {
            Notificator.Current.ShowException(args.Message, args.GetException());
        }

        public void SetIcon(bool _notification)
        {
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Icon = _notification ? Resource.blue_bell : Resource.Icon;
                notifyIcon.Visible = true;
            }
        }
        private void CheckIsRunning()
        {
            bool createdNew;
            mutex = new Mutex(true, $"Mutex for {System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\\"," ")} DaxEvents", out createdNew);
            if (!createdNew)
            {
                System.Windows.MessageBox.Show("Приложение уже запущенно", "DaxEvents");
                ExitApplication();
            }
        }
    }
}
