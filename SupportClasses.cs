using Microsoft.WindowsAzure.Storage.Table;

using System;
using System.Collections.Generic;
using System.Text;

namespace IotBackEnd
{

    public class MyTableEntity : TableEntity
    {
        public double humidity { get; set; }
        public double temperature { get; set; }
        public double pressure { get; set; }
        public int AQI { get; set; }
    }

    public class ResponseItem : TableEntity
    {
        public int humidity { get; set; }
        public double temperature { get; set; }
        public string Date { get; set; }
        public int AQI { get;  set; }
    }
}
