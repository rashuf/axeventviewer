using FirstFloor.ModernUI.Windows.Controls;
using Model;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AutoUpdaterDotNET;
using System.Windows.Input;
using System;
using System.Collections.Generic;

namespace GUI.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Root : ModernWindow
    {
        private EventInboxController controller;
        //string daxUserId;
        private void OnItemsChange(object sender, ItemEventArgs e)
        {
            SetDisplayName();
        }
        private void SetDisplayName()
        {
            int counter = controller.EventInboxModel.Count(i => i.IsRead == false);
            linkUser.DisplayName = $"{controller.UserContext.UserDisplayName}  -  Непрочитанных оповещений: {counter}";
            ((App)Application.Current).SetIcon(counter > 0);
        }

        public Root()
        {
            InitializeComponent();
            AutoUpdater.ShowRemindLaterButton = false;
            AutoUpdater.Start("http://develop.russalt.local/MicrosoftDynamicsAXAif50/App/version_info.xml");
            controller = ((App)Application.Current).Controller;

            controller.OnModelAdd += OnItemsChange;
            controller.OnModelDel += OnItemsChange;
            controller.OnModelUpd += OnItemsChange;
            controller.OnInitModel += OnItemsChange;

            SetDisplayName();
        }

        // + URF, 27.09.2018
        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var frame = VisualTreeHelperFindChildren<ModernFrame>(this).FirstOrDefault();
            if (frame != null)
            {
                frame.Navigating += Frame_Navigating;
            }
        }
        private void Frame_Navigating(object sender, FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            string dialog = "dialog:";
            if (e.Source.OriginalString.StartsWith(dialog))
            {
                // Show dialog
                //var dialogName = e.Source.OriginalString.Remove(0, dialog.Length);
                //MessageBox.Show($"Show Dialog '{dialogName}'");
                About about = new About();
                about.ShowDialog();
                e.Cancel = true;                
            }
        }
        public static List<T> VisualTreeHelperFindChildren<T>(DependencyObject parent) where T : class
        {
            List<T> list = new List<T>();

            if (parent != null)
            {
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    // Get object at index i
                    DependencyObject dobj = VisualTreeHelper.GetChild(parent, i);

                    if (dobj is T)
                    {
                        list.Add(dobj as T);
                    }

                    // Loop through its children
                    list.AddRange(VisualTreeHelperFindChildren<T>(dobj));
                }
            }
            return list;
        }
        // - URF, 27.09.2018
    }
}
