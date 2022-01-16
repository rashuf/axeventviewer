using DynamicsPipe;
using FirstFloor.ModernUI.Presentation;
using GUI.Elements;
using GUI.Properties;
using Model;
using Model.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GUI.ViewModel
{
    class HomeViewModel : NotifyPropertyChanged
    {
        #region Fields
        private EventInboxController controller;

        private EventInbox selectedItem;
        private IList selectedItems;
        #endregion
        #region Properties External
        public Double Opacity
        {
            get { return controller.IsModelLoading ? 0.5 : 1; }
            set { OnPropertyChanged("Opacity"); }
        }
        public bool IsLoading
        {
            get { return controller.IsModelLoading; }
            set { OnPropertyChanged("IsLoading"); }
        }

        public IEnumerable<EventInbox> Items
        {
            get { return controller.EventInboxModel; }
            set { if (value != null)
                OnPropertyChanged("Items");
            }
        }
        public Brush AccentBrush
        {
            get { return new SolidColorBrush(AppearanceManager.Current.AccentColor); }
            set { OnPropertyChanged("AccentBrush"); }
        }
        public string DaxUserId
        {
            get { return $"Пользователь Dax: {controller.DaxUserId}"; }
            set { OnPropertyChanged("DaxUserId"); }
        }

        public string Status
        {
            get { return $"Статус: {controller.Status.GetDescription()}"; }
            set { OnPropertyChanged("Status"); }
        }
        #endregion
        #region Properties Internal
        public EventInbox SelectedItem
        {
            get { return this.selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }
        public IList SelectedItems
        {
            get { return selectedItems; }
            set
            {
                selectedItems = value;
                OnPropertyChanged("SelectedItems");
            }
        }

        private bool singleSelect;

        public bool SingleSelect
        {
            get { return singleSelect; }
            set { singleSelect = value; OnPropertyChanged("SingleSelect"); }
        }


        #endregion
        #region Events
        private void OnItemsAdd(object sender, ItemEventArgs e)
        {
            OnPropertyChanged("Items");
        }
        private void OnLoading(object sender, ItemEventArgs e)
        {
            OnPropertyChanged("Items");
            OnPropertyChanged("IsLoading");
            OnPropertyChanged("Opacity");
        }
        private void OnIniDaxUser(object sender, ItemEventArgs e)
        {
            OnPropertyChanged("DaxUserId");
        }
        private void OnStatusChange(object sender, ItemEventArgs e)
        {
            OnPropertyChanged("Status");
        }
        private void OnAppearanceManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ThemeSource" || e.PropertyName == "AccentColor")
            {
                OnPropertyChanged("AccentBrush");
            }
        }
        private void OnInitNewEvents(object sender, ItemEventArgs e)
        {
            List<EventInbox> events = e.Item as List<EventInbox>;

            if (events != null && events.Count >= 1)
            {
                ShowNotifications(events);
            }
        }
        #endregion
        #region Ctor
        public HomeViewModel()
        {
            AppearanceManager.Current.PropertyChanged += OnAppearanceManagerPropertyChanged;

            controller = ((App)Application.Current).Controller;

            controller.OnInitModel += OnLoading;
            controller.OnIniDaxUser += OnIniDaxUser;
            controller.OnStatusChange += OnStatusChange;
            controller.OnModelAdd += OnInitNewEvents;
        }
        #endregion
        #region Commands

        private RelayCommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand((c) => { controller.DeleteEventsAsync(selectedItems.Cast<EventInbox>().ToList()); });
                }
                return deleteCommand;
            }
        }

        private RelayCommand setIsRead;
        public ICommand SetIsRead
        {
            get
            {
                if (setIsRead == null)
                {
                    setIsRead = new RelayCommand((c) => { controller.ChangeStatusAsync(selectedItems.Cast<EventInbox>().ToList(), true); });
                }
                return setIsRead;
            }
        }

        private RelayCommand setUnRead;
        public ICommand SetUnRead
        {
            get
            {
                if (setUnRead == null)
                {
                    setUnRead = new RelayCommand((c) => { controller.ChangeStatusAsync(selectedItems.Cast<EventInbox>().ToList(), false); });
                }
                return setUnRead;
            }
        }
        private RelayCommand openDaxCommand;
        public ICommand OpenDaxCommand
        {
            get
            {
                if (openDaxCommand == null)
                {
                    openDaxCommand = new RelayCommand((c) => { OpenDax(selectedItem); });
                }
                return openDaxCommand;
            }
        }
        private RelayCommand refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (refreshCommand == null)
                {
                    refreshCommand = new RelayCommand((c) => { controller.TimerInterval = UserSettings.Default.EventReadTime * 1000; controller.Refresh(); });
                }
                return refreshCommand;
            }
        }

        public EventInboxController Controller { get => controller; set => controller = value; }
        #endregion
        #region Methods
        public bool OpenDax(EventInbox item)
        {
            try
            {
                if (item == null)
                {
                    return false;
                }

                string pipeName = SendMessage.PipeName(UserSettings.Default.DaxPipeName);
                string command = "DrillDown_" + item.InboxId.ToString();

                if (command == null)
                {
                    return false;
                }

                if (SendMessage.CheckPipeName(pipeName))
                {
                    SendMessage.Send(pipeName, command);
                }
                else if (UserSettings.Default.DaxConfig != "" && UserSettings.Default.DaxExe != "")
                {
                    SendMessage.OpenClient(command, UserSettings.Default.DaxConfig, UserSettings.Default.DaxExe);
                }
                else if (UserSettings.Default.DaxConfig != "")
                {
                    SendMessage.OpenClient(command, UserSettings.Default.DaxConfig);
                }
                else
                {
                    SendMessage.OpenClient(command);
                }
                controller.ChangeStatusAsync(new List<EventInbox>() { item }, true);
            }
            catch (Exception ex)
            {
                Notificator.Current.ShowException("При открытии Dax произошёл сбой. Закройте Dax и повторите попытку", ex); // URF, 09.07.2018
                
                return false;
            }
          
            return true;
        }
        #endregion

        public void ShowNotifications(List<EventInbox> items)
        {
            if (items != null)
            {
                Notificator.Current.InitNotificator(((App)Application.Current).Dispatcher, UserSettings.Default.EventShowQty, UserSettings.Default.EventShowTime/* * 1000*/); // URF, время в миллисекунды переводить не надо

                if (items.Count > UserSettings.Default.EventShowQty)
                {
                    var parameter = Notificator.Current.ClickNotifier(() => { ((App)Application.Current).ShowMainWindowSimple(); });
                    Notificator.Current.Show($"Вам пришло {items.Count} оповещений.", TypesNotification.ShowSuccess, parameter);
                }
                else
                {
                    foreach (EventInbox item in items)
                    {
                        var parameter = Notificator.Current.ClickNotifier(() => { OpenDax(item); });
                        Notificator.Current.Show($"Вам пришло оповещение:\n{item.Subject}", TypesNotification.ShowInformation, parameter);
                    }
                }
            }
        }

    }
}

