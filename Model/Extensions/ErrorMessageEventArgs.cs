using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Model.Extensions
{
    public class ErrorMessageEventArgs : ErrorEventArgs
    {
        private string message;
        public ErrorMessageEventArgs(Exception exception, string message) : base(exception) 
        {
            this.message = message;
        }

        public string Message { get => message; }
    }
}
