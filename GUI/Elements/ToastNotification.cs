using System;
using System.IO;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace GUI.Elements
{
    public enum TypesNotification
    {
        ShowInformation,
        ShowError,
        ShowSuccess,
        ShowWarning
    }
    public class MessageParameters : MessageOptions
    {

    }
    public class Notificator : IDisposable
    {
        static Notificator current;

        private Notifier notifier;
        private Dispatcher dispatcher;
        private int eventShowTime;
        private int eventShowQty;

        private Notificator()
        {

        }
        static public Notificator Current
        {
            get
            {
                if(current == null)
                {
                    current = new Notificator();
                }
                return current;
            }
        }
        
        public void InitNotificator(Dispatcher dispatcher, int eventShowQty = 5, int eventShowTime = 30)
        {
            try
            {
                if(notifier == null || 
                   this.eventShowQty != eventShowQty ||
                   this.eventShowTime != eventShowTime ||
                   this.dispatcher != dispatcher)
                {
                    this.eventShowQty = eventShowQty;
                    this.eventShowTime = eventShowTime;
                    this.dispatcher = dispatcher;

                    notifier = new Notifier(cfg =>
                    {
                        cfg.PositionProvider = new PrimaryScreenPositionProvider(
                           corner: Corner.BottomRight,
                           offsetX: 10,
                           offsetY: 50);

                        cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                             notificationLifetime: TimeSpan.FromSeconds(eventShowTime),
                             maximumNotificationCount: ToastNotifications.Lifetime.MaximumNotificationCount.FromCount(eventShowQty));

                        cfg.Dispatcher = dispatcher;
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось инициализировать окно оповещения.\nПодробности см. в журнале ошибок.", ex);
            }
        }
        public MessageParameters ClickNotifier(Action clickNotifier)
        {
            var options = new MessageParameters
            {
                NotificationClickAction = n =>
                {
                    n.Close();
                    clickNotifier.Invoke();
                }
            };
            return options;
        }
        public void Show(string message, TypesNotification typeNotification, MessageParameters options = null)
        {
            switch (typeNotification)
            {
                case TypesNotification.ShowInformation:
                    notifier.ShowInformation(message, options);
                    break;
                case TypesNotification.ShowError:
                    notifier.ShowError(message, options);
                    break;
                case TypesNotification.ShowSuccess:
                    notifier.ShowSuccess(message, options);
                    break;
                case TypesNotification.ShowWarning:
                    notifier.ShowWarning(message, options);
                    break;
            }
        }
        public void ShowException(string message, Exception ex, MessageParameters options = null)
        {
            try
            {
                string logFile = PathHelper.LogPath();

                if (!String.IsNullOrEmpty(logFile))
                {
                    File.AppendAllLines(logFile, new string[]{ DateTime.Now.ToString(),  message, ex.ToString(), System.Environment.NewLine});
                }
            }
            catch
            {
                Show("Ошибка при записи лога Windows", TypesNotification.ShowError);
            }

            if (message != null)
            {
                notifier.ShowError(message, options);
            }
        }
        public void Dispose()
        {
            if (notifier != null)
            {
                notifier.Dispose();
            }
        }
    }
}
