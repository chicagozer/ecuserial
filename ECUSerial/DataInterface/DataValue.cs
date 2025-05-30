﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RX7Interface
{
    public enum ParameterLength
    {
        OneByte,
        TwoBytes
    }

    public class DataValue
    {
        private uint baseAddress;

        public DateTime UpdateTime
        {
            get;
            set;
        }

        public uint[] Addresses
        {
            get;
            private set;
        }

        private byte[] rawValues;

        private double offset;
        private double conversion;
        
        public DataValue(uint address, ParameterLength length, double conversion, double offset)
        {
            UpdateTime = DateTime.MinValue;

            baseAddress = address;

            uint byteCount = 0;
            if (length == ParameterLength.OneByte)
            {
                byteCount = 1;
            }
            else if (length == ParameterLength.TwoBytes)
            {
                byteCount = 2;
            }

            Addresses = new uint[byteCount];
            rawValues = new byte[byteCount];

            for (int i = 0; i < byteCount; i++)
            {
                Addresses[i] = baseAddress + (uint)i;
            }

            this.conversion = conversion;
            this.offset = offset;
        }

        public void SetRawValue(uint address, byte value)
        {
            rawValues[address - baseAddress] = value;
        }

        public double GetValue()
        {
            ulong rawValue = 0;

            foreach (byte b in rawValues)
            {
                rawValue <<= 8;
                rawValue += b;
            }

            return ((double)rawValue * conversion) + offset;
        }
    }
}
