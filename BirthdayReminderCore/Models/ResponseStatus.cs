using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BirthdayReminder.ViewModels
{
    public class ResponseStatus
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string IdentifierData { get; set; }
    }
}
