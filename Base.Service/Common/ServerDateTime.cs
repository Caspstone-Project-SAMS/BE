using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Common
{
    public class ServerDateTime
    {
        private int Minutes { get; set; }
        private bool IsUp { get; set; }
        DateTime serverDateTime;
        public DateTime GetLocalMachineDateTime()
        {
            return DateTime.Now;
        }

        public DateTime GetServerDateTime() 
        {
            
            return serverDateTime;
        }

        public void SetTime(int minutes, bool isUp)
        {
            Minutes = minutes;
            IsUp = isUp;
        }
    }
}
