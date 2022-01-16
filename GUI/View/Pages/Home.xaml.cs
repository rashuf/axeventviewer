using GUI.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GUI.View.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();
            //DataContext = new HomeViewModel();
            DataContext = ((App)Application.Current).HomeViewModel; // URF, 18.09.2018, HomeViewModel уже инициализируется в App.OnStartup()
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void DataGridEvents_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count >= 1)
                {
                    Object[] rows = new Object[grid.SelectedItems.Count];
                    grid.SelectedItems.CopyTo(rows, 0);

                    foreach (var item in rows)
                    {
                        DataGridRow dgr = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

                        //if (!dgr.IsMouseOver)
                        {
                            (dgr as DataGridRow).IsSelected = false;
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HomeViewModel model = (HomeViewModel)DataContext;
        }
        
    }
}

