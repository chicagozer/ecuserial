using System;
using System.Collections.Generic;
using System.Text;

namespace RX7Interface
{
    interface IDataStream
    {
        void Open();

        bool IsOpen
        {
            get;
        }

        void Close();

        byte ReadByte(uint address);

        byte ReadByte(uint address, int retryCount);
    }
}
