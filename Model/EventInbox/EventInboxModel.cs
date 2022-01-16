using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Model
{
    public class EventInboxModel
    {
        List<EventInbox> items;

        public delegate void ItemAdedHandler(object sender, ItemEventArgs e);
        //public event ItemAdedHandler OnItemAdd;
        public event ItemAdedHandler OnItemDelete;
        public event ItemAdedHandler OnItemsAdd;
        public event ItemAdedHandler OnItemsDelete;

        public List<EventInbox> Items
        {
            get { return items; }
        }
        public EventInboxModel()
        {
            items = new List<EventInbox>();

            Debug.Print(items.GetHashCode().ToString());
        }
        public EventInboxModel(List<EventInbox> items)
        {
            items = new List<EventInbox>();
            Init(items);
        }

        public void Init(List<EventInbox> addItems)
        {
            items.AddRange(addItems);

            OnItemsAdd?.Invoke(this, new ItemEventArgs() { Item = addItems });
        }

        public void Add(List<EventInbox> addItems)
        {
            AddRange(addItems);

            OnItemsAdd?.Invoke(this, new ItemEventArgs() { Item = addItems });
        }
        
        private void AddRange(List<EventInbox> removeItems)
        {
            foreach (var item in removeItems)
            {
                items.Add(item);

                OnItemsAdd?.Invoke(this, new ItemEventArgs() { Item = item });
            }
        }

        public void Update(IEnumerable<EventInbox> updateItems)
        {

        }

        public void Delete(IList<EventInbox> removeItems)
        {
            RemoveRange(removeItems);

            OnItemsDelete?.Invoke(this, new ItemEventArgs() { Item = removeItems });
        }
        
        public void Refresh(List<EventInbox> newItems)
        {
            items = newItems;
        }
        private void RemoveRange(IList<EventInbox> removeItems)
        {
            foreach (var item in removeItems)
            {
                items.Remove(item);
                OnItemDelete?.Invoke(this, new ItemEventArgs() { Item = item });
            }
        }
        
    }
}
