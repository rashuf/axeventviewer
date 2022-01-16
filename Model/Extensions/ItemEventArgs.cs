using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class ItemEventArgs : EventArgs
    {
        public Object Item { get; set; }
    }
}
