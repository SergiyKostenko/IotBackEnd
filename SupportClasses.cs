using Microsoft.WindowsAzure.Storage.Table;

using System;
using System.Collections.Generic;
using System.Text;

namespace IotBackEnd
{

    public class MyTableEntity : TableEntity
    {
        public Int64 humidity { get; set; }
        public double temperature { get; set; }
        public bool isFlameDetected { get; set; }
        public double pressure { get; set; }
        public double voc { get; set; }
        public int AQI { get; internal set; }
    }

    public class ResponseItem
    {
        public int humidity { get; set; }
        public double temperature { get; set; }
        public bool isFlameDetected { get; set; }
        public string DeviceName { get; set; }
        public string Date { get; set; }

    }
}
