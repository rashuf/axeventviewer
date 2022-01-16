using AxService;
using Model.Extensions;
using Model.SqlLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Model
{
    public enum EventInboxControllerStatus
    {
        [Description("Создание")]
        Inited,
        [Description("Старт")]
        Starting,
        [Description("Загрузка локальных данных")]
        InitModel,
        [Description("Определение пользователя Dax")]
        InitUser,
        [Description("Локальные данные загружены")]
        ModelReady,
        [Description("Пользователь Dax определен")]
        UserReady,
        [Description("Ожидание")]
        Ready,
        [Description("Проверка статуса")]
        CheckStatus,
        [Description("Загрузка новых оповещений")]
        GetEvents,
        [Description("Остановлено")]
        Stoped,
        [Description("Ошибка")]
        Error
    }

    public class EventInboxController
    {
        public delegate void ItemAdedHandler(object sender, ItemEventArgs e);
        public delegate void ExceptionHandler(object sender, ErrorMessageEventArgs e);

        public event ItemAdedHandler OnInitModel;
        public event ItemAdedHandler OnIniDaxUser;
        public event ItemAdedHandler OnStatusChange;
       
        public event ItemAdedHandler OnModelAdd;
        public event ItemAdedHandler OnModelDel;
        public event ItemAdedHandler OnModelUpd;

        public event ExceptionHandler OnException;

        UserContext userContext;
        EventInboxControllerStatus status;
        string dbConnectionString;
        int timerInterval = 30000;
       
        ObservableRangeCollection<EventInbox> eventInboxModel;
        SynchronizationContext mainThred;

        System.Timers.Timer timer;
        string daxUserId;
        bool modelLoaded, isModelLoading;

        Object statusSynchronization = new Object();

        public UserContext UserContext { get => userContext; }
        public SynchronizationContext MainThred { get => mainThred; set => mainThred = value; }
        public ObservableRangeCollection<EventInbox> EventInboxModel { get => eventInboxModel; }
        public EventInboxControllerStatus Status { get => status; }
        public string DaxUserId { get => daxUserId; }
        public bool IsModelLoading { get => isModelLoading;}
        public int TimerInterval { get => timerInterval; set => timerInterval = value; }

        public EventInboxController(UserContext userContext, string dbConnectionString)
        {
            this.userContext = userContext;
            this.dbConnectionString = dbConnectionString;
            eventInboxModel = new ObservableRangeCollection<EventInbox>();

            SetStatus(EventInboxControllerStatus.Inited);
        }

        public void ReInitUserContext(UserContext userContext)
        {
            this.userContext = userContext;
            daxUserId = "";
            InitUserIdAsync();

        }

        protected void SetStatus(EventInboxControllerStatus status)
        {
            lock (statusSynchronization)
            {
                if (status == EventInboxControllerStatus.ModelReady || status == EventInboxControllerStatus.UserReady)
                {
                    if (modelLoaded && !String.IsNullOrEmpty(daxUserId))
                    {
                        this.status = EventInboxControllerStatus.Ready;
                        if(timer != null)
                        {
                            timer.Interval = 1;
                            timer.Start();
                        }
                    }
                }
                else
                {
                    this.status = status;
                }
                OnStatusChange?.Invoke(this, new ItemEventArgs() { Item = status });
            }
        }

        public void Start()
        {
            if (timer == null)
            {
                SetStatus(EventInboxControllerStatus.Starting);
                timer = new System.Timers.Timer();
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                timer.Enabled = false;
               
                OnTimedEvent(this, null);
            }
            else if(!timer.Enabled)
            {
                timer.Interval = timerInterval;
                timer.Start();
            }
            else
            {
                timer.Interval = timerInterval;
                timer.Start();
            }
        }
        public void Stop()
        {
            if(timer != null)
            {
                timer.Stop();
            }
        }
        protected void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            RunOperation();
            Start();
        }
        protected void RunOperation()
        { 
            if(String.IsNullOrEmpty(daxUserId))
            {
                InitUserIdAsync();
            }

            if(!modelLoaded && !isModelLoading)
            {
                InitModelDataAsync();
            }

            if(status == EventInboxControllerStatus.Ready)
            {
                UpdateEventsAsync();
                CreateEventsAsync();
            }    
        }
        public void InitUserIdAsync()
        {
            SetStatus(EventInboxControllerStatus.InitUser);
            Task<string> task = Task.Factory.StartNew(() => ReadDaxUserId());

            task.ContinueWith(ts =>
            {
                string result = ts.Result;

                if (!String.IsNullOrEmpty(result))
                {
                    mainThred.Send((object state) => { daxUserId = (result); }, null);

                    SetStatus(EventInboxControllerStatus.UserReady);
                    OnIniDaxUser?.Invoke(this, new ItemEventArgs() { Item = daxUserId });
                }
            });
        }
        public void InitModelDataAsync()
        {
            isModelLoading = true;
            SetStatus(EventInboxControllerStatus.InitModel);
           
            Task<List<EventInbox>> task = Task.Factory.StartNew(() => ReadEventsFromDataBase());

            task.ContinueWith(ts =>
            {
                List<EventInbox> result = ts.Result;

                if(result != null)
                {
                    mainThred.Send((object state) =>
                    {
                    
                        EventInboxModel.AddRangeExt(result);
                        isModelLoading = false;
                        modelLoaded = true;
                        SetStatus(EventInboxControllerStatus.ModelReady);
                        OnInitModel?.Invoke(this, new ItemEventArgs() { Item = true });
                    },
                    null);
                }
            });
        }
        public void CreateEventsAsync()
        {
            SetStatus(EventInboxControllerStatus.GetEvents);
            List<EventInbox> result = ReadNewEventsFromAx();

            if (result != null && result.Count > 0)
            {
                mainThred.Send(state =>
                {
                    EventInboxModel.AddRangeExt(result);
                    OnModelAdd?.Invoke(this, new ItemEventArgs() { Item = result });
                },
                null);
            }

            SetStatus(EventInboxControllerStatus.Ready);
        }
        public void UpdateEventsAsync()
        {
            SetStatus(EventInboxControllerStatus.CheckStatus);
            
            List<EventInbox> readed = ReadedEventsFromAx();
            if(readed != null && readed.Count > 0)
            {
                mainThred.Send(state => 
                {
                    UpdateIsRead(readed);
                    OnModelUpd?.Invoke(this, new ItemEventArgs() { Item = readed });
                }, 
                null);
            }
            SetStatus(EventInboxControllerStatus.Ready);
        }
        public void DeleteEventsAsync(List<EventInbox> deleteEvents)
        {
            if (deleteEvents != null && deleteEvents.Count > 0)
            {
                eventInboxModel.RemoveRange(deleteEvents);
                OnModelDel.Invoke(this, new ItemEventArgs() { Item = deleteEvents });

                Task<bool> task = Task.Factory.StartNew(() => DeleteEvent(deleteEvents));

                task.ContinueWith(ts =>
                {
                    bool result = ts.Result;
                    if (!result)
                    {
                        mainThred.Send(state =>
                        {
                            eventInboxModel.AddRangeExt(deleteEvents);
                        },
                        null);
                    }
                });
            }
         }
        public void ChangeStatusAsync(List<EventInbox> events, bool isRead)
        {
            if (events != null && events.Count > 0)
            {
                foreach (EventInbox eventInbox in events)
                {
                    eventInbox.IsRead = isRead;
                }
                OnModelUpd?.Invoke(this, new ItemEventArgs() { Item = events });

                Task<bool> task = Task.Factory.StartNew(() => ChangeStatus(events, isRead));
                task.ContinueWith(ts =>
                {
                    bool result = ts.Result;
                    if (!result)
                    {
                        mainThred.Send(state =>
                        {
                            foreach (EventInbox eventInbox in events)
                            {
                                eventInbox.IsRead = !isRead;
                            }
                            OnModelUpd?.Invoke(this, new ItemEventArgs() { Item = events });
                        },
                        null);
                    }
                });
            }
        }
        protected string ReadDaxUserId()
        {
            try
            {
                using (AxEventService ax = new AxEventService(userContext))
                {
                    return ax.GetUser();
                }
            }
            catch (Exception exception)
            {
                SetStatus(EventInboxControllerStatus.Error);
                OnException?.Invoke(this, new ErrorMessageEventArgs(exception, "Произошел сбой при определении пользователя Dax"));
            }
            return "";
        }
        protected List<EventInbox> ReadEventsFromDataBase()
        {
            try
            {
                using (SqliteDbContext dbContext = new SqliteDbContext(dbConnectionString))
                {
                    return new Repository<EventInbox>(dbContext).Get();
                }
            }
            catch(Exception exception)
            {
                SetStatus(EventInboxControllerStatus.Error);
                OnException?.Invoke(this, new ErrorMessageEventArgs(exception, "Произошел сбой при чтении локальных данных"));
            }
            return null;
        }
        protected List<EventInbox> ReadNewEventsFromAx()
        {
            List<EventInbox> newEvents;
            long? maxInBox;

            if (EventInboxModel != null && EventInboxModel.Count > 0)
            {
                maxInBox = EventInboxModel.Max(e => e.InboxId);
            }
            else
            {
                maxInBox = 0;
            }
            try
            {

                using (AxEventService axService = new AxEventService(UserContext))
                {
                    newEvents = axService.GetList(QueryCriterias.UserNotReadEventId(daxUserId, maxInBox));
                }
                if (newEvents != null && newEvents.Count > 0)
                {
                    using (SqliteDbContext dbContext = new SqliteDbContext(dbConnectionString))
                    {
                        new Repository<EventInbox>(dbContext).Create(newEvents);
                    }
                    //OnModelAdd?.Invoke(this, new ItemEventArgs() { Item = newEvents }); //URF, закомментировал, т.к. возникает несколько оповещений на 1 событие. Еще вызывается в CreateEventsAsync
                }
                return newEvents;
            }
            catch (Exception exception)
            {
                SetStatus(EventInboxControllerStatus.Error);
                OnException?.Invoke(this, new ErrorMessageEventArgs(exception, "Произошел сбой при чтении новых оповещений"));
            }
            return null;
        }
        protected List<EventInbox> ReadedEventsFromAx()
        {
            List<EventInbox> notReadEvents = EventInboxModel.Where((e) => e.IsRead == false).ToList();
            try
            {
                if (notReadEvents != null && notReadEvents.Count > 0)
                {
                    List<EventInbox> newEvents;

                    using (AxEventService axService = new AxEventService(UserContext))
                    {
                        newEvents = axService.FindList(notReadEvents);
                    }

                    notReadEvents = newEvents.Where(e => e.IsRead == true).ToList<EventInbox>();

                    if (notReadEvents != null && notReadEvents.Count > 0)
                    {
                        using (SqliteDbContext dbContext = new SqliteDbContext(dbConnectionString))
                        {
                            new Repository<EventInbox>(dbContext).Update(notReadEvents);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                SetStatus(EventInboxControllerStatus.Error);
                OnException?.Invoke(this, new ErrorMessageEventArgs(ex, "Произошел сбой при обновлении статуса прочитанных оповещений"));
            }
            return notReadEvents;
        }
        protected void UpdateIsRead(List<EventInbox> items)
        {
            foreach (EventInbox item in items)
            {
                EventInboxModel.FirstOrDefault(i => i.InboxId == item.InboxId).IsRead = item.IsRead;
            }
            OnModelUpd?.Invoke(this, new ItemEventArgs() { Item = items });
        }
        protected bool DeleteEvent(List<EventInbox> items)
        {
            try
            {
                using (SqliteDbContext dbContext = new SqliteDbContext(dbConnectionString))
                {
                    new Repository<EventInbox>(dbContext).Remove(items);
                }

                using (AxEventService axService = new AxEventService(UserContext))
                {
                    axService.DeleteInboxes(items);
                }
                return true;
            }
            catch(Exception exception)
            {
                SetStatus(EventInboxControllerStatus.Error);
                OnException?.Invoke(this, new ErrorMessageEventArgs(exception, "Произошел сбой при при удалении данных"));
            }
            return false;
        }
        protected bool ChangeStatus(List<EventInbox> items, bool isRead)
        {
            try
            {
                using (SqliteDbContext dbContext = new SqliteDbContext(dbConnectionString))
                {
                    new Repository<EventInbox>(dbContext).Update(items);
                }

                using (AxEventService axService = new AxEventService(UserContext))
                {
                    axService.SetRead(items, isRead);
                }

                return true;
            }
            catch (Exception exception)
            {
                SetStatus(EventInboxControllerStatus.Error);
                OnException?.Invoke(this, new ErrorMessageEventArgs(exception, "Произошел сбой при изменении статуса"));
            }
            return false;
        }
        public void Refresh()
        {
            if (String.IsNullOrEmpty(DaxUserId) || modelLoaded == false)
            {
                SetStatus(EventInboxControllerStatus.Inited);
            }
            else
            {
                SetStatus(EventInboxControllerStatus.Ready);
            }
            timer.Interval = 1;
            timer.Start();
        }
    }
}
