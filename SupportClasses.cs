﻿using Microsoft.WindowsAzure.Storage.Table;

using System;
using System.Collections.Generic;
using System.Text;

namespace IotBackEnd
{

    public class MyTableEntity : TableEntity
    {
        public int humidity { get; set; }
        public double temperature { get; set; }
        public bool isFlameDetected { get; set; }
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