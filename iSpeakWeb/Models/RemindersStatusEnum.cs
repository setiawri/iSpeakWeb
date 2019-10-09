using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public enum RemindersStatusEnum : byte
    {
        New,
        InProgress,
        OnHold,
        Waiting,
        Completed,
        Cancel
    }
}