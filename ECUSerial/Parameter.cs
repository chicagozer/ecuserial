using System;
using System.Collections.Generic;
using System.Text;
using RX7Interface.Gauges;

namespace RX7Interface
{
    public class Parameter
    {
        public DataValue DataValue
        {
            get;
            private set;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public GaugeInfo GaugeInfo
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Units
        {
            get;
            private set;
        }

        public Parameter(string name, string units, DataValue dataValue, GaugeInfo info)
        {
            Enabled = true;

            this.Name = name;
            this.Units = units;

            DataValue = dataValue;
            GaugeInfo = info;
        }
    }
}
