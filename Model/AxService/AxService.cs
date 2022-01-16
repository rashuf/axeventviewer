using Model;
using Model.EventInboxService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;


namespace AxService
{
    public class AxEventService : IDisposable
    {
        EventInboxServiceClient client;
        UserContext userContext;
        int packSize = 100;
        int packSizeBig = 500;
        #region ServiceMethods
        public AxEventService(UserContext userContext)
        {
            this.userContext = userContext;
            client = new EventInboxServiceClient();

            if (!String.IsNullOrWhiteSpace(userContext.DomainName) && !String.IsNullOrWhiteSpace(userContext.UserAccount))
            {
                client.ClientCredentials.Windows.ClientCredential.Domain = userContext.DomainName;
                client.ClientCredentials.Windows.ClientCredential.UserName = userContext.UserAccount;
                client.ClientCredentials.Windows.ClientCredential.SecurePassword = userContext.UserPassword;
            }

            client.Open();
        }
        public string GetUser()
        {
            if (client.State != CommunicationState.Opened)
            {
                client.Open();
            }
            return client.getUser(userContext.UserSID);
        }
        public bool SetRead(string inboxes, bool isRead)
        {
            if (client.State != CommunicationState.Opened)
            {
                client.Open();
            }
            return client.setRead(inboxes, isRead);
        }
        public string GetData(QueryCriteria queryCriteria)
        {
            if (client.State != CommunicationState.Opened)
            {
                client.Open();
            }
            return client.getData(queryCriteria);
        }
        public string FindData(string inboxes)
        {
            if (client.State != CommunicationState.Opened)
            {
                client.Open();
            }
            return client.find(inboxes);
        }
        public bool DeleteData(string inboxes)
        {
            if (client.State != CommunicationState.Opened)
            {
                client.Open();
            }
            return client.deleteData(inboxes);
        }
        public void Dispose()
        {
            client.Close();
        }
        #endregion
       
        #region ExtensionMethods
        protected List<EventInbox> ParceMessage(string message)
        {
            List<EventInbox> list = new List<EventInbox>();

            string[] sep = { "\n" };

            string[] lines = message.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in lines)
            {
                string[] sepFld = { ";" };
                string[] fields = item.Split(sepFld, StringSplitOptions.RemoveEmptyEntries);

                list.Add(new EventInbox()
                {
                    InboxId = long.Parse(fields[0]),
                    AlertDateTime = DateTime.Parse(fields[1], CultureInfo.CurrentCulture),
                    Subject = fields[2],
                    AlertFor = fields[3],
                    IsRead = int.Parse(fields[4]) == 0 ? false : true,
                    UserId = fields[5]
                });
            }
            return list;
        }
        public List<EventInbox> FindList(string inboxes)
        {
            return ParceMessage(FindData(inboxes));
        }
        public List<EventInbox> GetList(QueryCriteria queryCriteria)
        {
            return ParceMessage(GetData(queryCriteria));
        }
        public List<EventInbox> FindList(List<EventInbox> inboxes)
        {
            List<EventInbox> eventInbox = new List<EventInbox>();
            string queryString = String.Empty;
           
            for (int i = 0; i < inboxes.Count; i++)
            {
                EventInbox item = inboxes[i];
                queryString += (String.IsNullOrEmpty(queryString) ? "" : ", ") + item.InboxId.ToString();

                if (i % packSize == 0)
                {
                    eventInbox.AddRange(FindList(queryString));
                    queryString = "";
                }
            }

            if (!String.IsNullOrEmpty(queryString))
            {
                eventInbox.AddRange(FindList(queryString));
            }
            return eventInbox;
        }
        public bool SetRead(List<EventInbox> inboxes, bool isRead)
        {
            string querySting = String.Empty;
            bool ok = true;
            for (int i = 0; i < inboxes.Count; i++)
            {
                EventInbox item = inboxes[i];
                querySting += (String.IsNullOrEmpty(querySting) ? "" : ", ") + item.InboxId.ToString();

                if (i % packSizeBig == 0)
                {
                    ok = SetRead(querySting, isRead) && ok;
                    querySting = "";
                }
            }

            if (!String.IsNullOrEmpty(querySting))
            {
                ok = SetRead(querySting, isRead) && ok;
            }

            return ok;
        }
        public bool DeleteInboxes(List<EventInbox> inboxes)
        {
            string querySting = String.Empty;
            bool ok = true;
            for (int i = 0; i < inboxes.Count; i++)
            {
                EventInbox item = inboxes[i];
                querySting += (String.IsNullOrEmpty(querySting) ? "" : ", ") + item.InboxId.ToString();

                if (i % packSizeBig == 0)
                {
                    ok = DeleteData(querySting) && ok;
                    querySting = "";
                }
            }

            if (!String.IsNullOrEmpty(querySting))
            {
                ok = DeleteData(querySting) && ok;
            }

            return ok;
        }
        #endregion
    }
}
