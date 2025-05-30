using System;
using System.Diagnostics;
using System.Threading;
using System.IO.Ports;

namespace RX7Interface
{
    class Rx7DataStream : IDataStream
    {
        // Read timeout in ms.
        private const int ReadTimeout = 500;

        private const byte ReadCommand = 0x40;

        private const byte ChecksumError = 0x14;
        private const int DefaultRetryCount = 3;

        private Rx7SerialPort serialPort;

        public Rx7DataStream(string port)
        {
            serialPort = new Rx7SerialPort(port);
        }

        public void Open()
        {
            serialPort.Open();
        }

        public bool IsOpen
        {
            get
            {
                return serialPort.IsOpen;
            }
        }

        public void Close()
        {
            serialPort.Close();
        }

        public byte ReadByte(uint address)
        {
            return ReadByte(address, DefaultRetryCount);
        }

        public byte ReadByte(uint address, int retryCount)
        {
            if (retryCount < 0)
            {
                throw new System.IO.IOException("Read failed.");
            }

            byte addressHigher = (byte)(address >> 8);
            byte addressLower = (byte)(address % 256);

            byte[] packet = new byte[] { ReadCommand, addressHigher, addressLower, 0 };

            CalculateChecksum(packet);

            serialPort.Write(packet);

            // Loop until data becomes available. 3 bytes are expected in reply.
            Stopwatch readTimer = Stopwatch.StartNew();
            while (serialPort.BytesAvailable < 3 && readTimer.ElapsedMilliseconds < ReadTimeout)
            {
                Thread.Sleep(0);
            }

            byte[] received = serialPort.Read();

            if (received.Length == 3&& IsChecksumOk(received) && received[0] == ReadCommand)
            {
                // Second byte is the actual data.
                return received[1];
            }
            else
            {
                if (received.Length == 2 && received[0] == 0x14)
                {
                    // Checksum error on sent packet.
                    Console.WriteLine("Checksum error reported by ECU.");
                }
                else
                {
                    // Unexpected reply.
                    Console.WriteLine(string.Format("Unexpected reply. Length {0}. Data: {1}", received.Length, BitConverter.ToString(received)));
                }

                return ReadByte(address, --retryCount);
            }
        }

        private bool IsChecksumOk(byte[] data)
        {
            int total = 0;

            for (int i = 0; i < data.Length - 1; i++)
            {
                total += data[i];
            }

            return data[data.Length - 1] == total % 256;
        }

        private void CalculateChecksum(byte[] data)
        {
            int total = 0;

            for (int i = 0; i < data.Length - 1; i++)
            {
                total += data[i];
            }

            data[data.Length - 1] = (byte)(total % 256);
        }
    }
}
