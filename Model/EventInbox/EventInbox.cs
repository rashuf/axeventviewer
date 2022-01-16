using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Model
{
    public class EventInbox : INotifyPropertyChanged
    {
        private long _inboxId;
        private DateTime _alertDateTime;
        private string _subject;
        private string _alertFor;
        private bool _isRead;
        private string _userId;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Int64 InboxId 
{
            get { return _inboxId; }
            set
            {
                _inboxId = value;
                OnPropertyChanged("InboxId");
            }
        }
        public DateTime AlertDateTime
        {
            get { return _alertDateTime; }
            set
            {
                _alertDateTime = value;
                OnPropertyChanged("AlertDateTime");
            }
        }
        public string Subject
        {
            get
            {
                return _subject;
            }

            set
            {
                _subject = value;
                OnPropertyChanged("Subject");
            }
        }
        public string AlertFor
        {
            get { return _alertFor; }
            set
            {
                _alertFor = value;
                OnPropertyChanged("AlertFor");
            }
        }
        public bool IsRead
        {
            get
            {
                return _isRead;
            }

            set
            {
                _isRead = value;
                OnPropertyChanged("IsRead");
            }
        }
        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged("UserId");
            }
        }
    }
}
