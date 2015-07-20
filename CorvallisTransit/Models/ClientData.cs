using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CorvallisTransit.Models
{
    public class ClientData
    {
        public string num { get; set; }

        public DateTime updateTime { get; set; }

        public string warningClasses{ get; set; }

        public object stops { get; set; }
    }
}
