// See https://aka.ms/new-console-template for more information

using System.ComponentModel;  // for BackgroundWorker
using Console = System.Console;
using RX7Interface.Gauges;
namespace RX7Interface
{


    class Program
    {

        public static string AppName { get; set; } = "My .NET Application";



        private static Parameter[] parameters = new Parameter[]
           {
            new Parameter("Intake Air Pressure", "mmHg", new DataValue(0x0021, ParameterLength.TwoBytes, 500d/256, -1000), null),
            new Parameter("Throttle Angle (Narrow)", "V", new DataValue(0x0023, ParameterLength.OneByte, 5d/256, 0), new GaugeInfo(0, 5)),
            new Parameter("Throttle Angle (Wide)", "V", new DataValue(0x0024, ParameterLength.OneByte, 5d/256, 0), new GaugeInfo(0, 5)),
            new Parameter("Oxygen Sensor Voltage", "V",new DataValue(0x0025, ParameterLength.OneByte, 5d/256, 0), new GaugeInfo(0, 5)),
            new Parameter("MOP Position", "V", new DataValue(0x0026, ParameterLength.OneByte, 5d/256, 0), null),
            new Parameter("Battery Voltage", "V", new DataValue(0x0027, ParameterLength.OneByte, 20d/256, 0), new GaugeInfo(8, 20, 11, 15)),
            new Parameter("Water Temperature", "°C", new DataValue(0x0028, ParameterLength.OneByte, 160d/256, -40), new GaugeInfo(0, 140, 80, 100)),
            new Parameter("Fuel Temperature", "°C", new DataValue(0x0029, ParameterLength.OneByte, 160d/256, 0), null),
            new Parameter("Intake Air Temperature", "°C",new DataValue(0x002a, ParameterLength.OneByte, 160d/256, -40), null),
            new Parameter("Engine Speed", "rpm", new DataValue(0x002c, ParameterLength.TwoBytes, 500d/256, 0), new GaugeInfo(0, 9000)),
            new Parameter("Vehicle Speed", "km/h", new DataValue(0x002e, ParameterLength.OneByte, 356d/256, 0), new GaugeInfo(0, 320)),
            new Parameter("Injector On Time", "ms", new DataValue(0x0800, ParameterLength.TwoBytes, 1d/256, 0), null),
            new Parameter("Injector Period", "ms", new DataValue(0x0802, ParameterLength.TwoBytes, 1d/256, 0), null),
            new Parameter("Ignition Advance (Leading)", "degrees", new DataValue(0x0804, ParameterLength.OneByte, 90d/256, -25), null),
            new Parameter("Ignition Advance (Trailling)", "degrees", new DataValue(0x0805, ParameterLength.OneByte, 90d/256, -25), null),
            new Parameter("Idle Speed Control", "%", new DataValue(0x0806, ParameterLength.TwoBytes, 800d/256, 0), null),
            new Parameter("Unknown 1", "?", new DataValue(0x0808, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Turbo Pre-Control", "%", new DataValue(0x0809, ParameterLength.OneByte, 100d/256, 0), null),
            new Parameter("Wastegate Control", "%", new DataValue(0x080a, ParameterLength.OneByte, 100d/256, 0), null),


            new Parameter("Unknown 2", "?", new DataValue(0x080b, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Unknown 3", "?", new DataValue(0x080c, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Unknown 4", "?", new DataValue(0x080d, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Unknown 5", "?", new DataValue(0x080e, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Unknown 6", "?", new DataValue(0x080f, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Unknown 7", "?", new DataValue(0x0810, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Unknown 8", "?", new DataValue(0x0811, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Unknown 9", "?", new DataValue(0x0812, ParameterLength.OneByte, 1, 0), null),
            new Parameter("Unknown 10", "?", new DataValue(0x0813, ParameterLength.OneByte, 1, 0), null)
           };

        /* Remaining Paramters (addresses unknown):
        MOP 	Metering Oil Pump Position 	V
        TPC 	Turbo Pre-Control Valve 	%
        WGC 	Wastegate Control Valve 	%
        CRFSV 	Charge Relief Valve 	ON . OFF
        CCNTSV 	Charge Control Valve 	ON . OFF
        TCNTSV 	Turbo Control Valve 	ON . OFF
        RAD-FAN1 	Radiator Fan Relay 1 	ON . OFF
        RAD-FAN2 	Radiator Fan Relay 2 	ON . OFF
        RAD-FAN3 	Radiator Fan Relay 3 	ON . OFF
        AC-SW   	AC Switch  	ON . OFF 
        AC-RLY 	AC Relay 	ON . OFF
        PRCSV 	Pressure Regulator Control Valve 	ON . OFF
        DTCNTSV 	Double Throttle Control Valve 	ON . OFF
        FP-RLY 	Fuel Pump Relay 	ON . OFF
        Diagnostic Code 	OK.NG CODE
         */

        static void Main(string[] args)
        {
            IDataStream dataStream;


            foreach (string arg in args)
            {
                Console.WriteLine($"Argument: {arg}");
            }

            dataStream = new Rx7DataStream(args[0]);
            dataStream.Open();
            Console.WriteLine("Hello, World!");




            BackgroundWorker worker = new();
            worker.DoWork += worker_DoWork!;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted!;
            //worker.ProgressChanged += worker_ProgressChanged!;
            worker.WorkerReportsProgress = false;
            worker.WorkerSupportsCancellation = true;

            Console.WriteLine("Starting worker... (any key to cancel/exit)");

            worker.RunWorkerAsync(argument: dataStream);

            Console.ReadKey(true);  // event loop

            if (worker.IsBusy)
            {
                Console.WriteLine("Interrupting the worker...");
                worker.CancelAsync();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (worker.IsBusy && sw.ElapsedMilliseconds < 5000)
                    Thread.Sleep(1);
            }

            dataStream.Close();

        }





      


        static void worker_RunWorkerCompleted(object _, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("Worker: I was busy!");
                return;
            }

            Console.WriteLine("Worker: I worked {0:D} times.", e.Result);
            Console.WriteLine("Worker: Done now!");
        }

        static void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;

            IDataStream dataStream = (IDataStream)e.Argument!;

            while (!e.Cancel)
            {

                foreach (Parameter p in parameters)
                {
                    if (p.Enabled)
                    {
                        foreach (uint address in p.DataValue.Addresses)
                        {
                            //Console.WriteLine($"cancel? {e.Cancel}");
                            if (worker!.CancellationPending)
                            {
                                e.Cancel = true;
                                break;
                            }
                            Thread.Sleep(200);
                            try
                            {

                                byte received = dataStream.ReadByte(address);
                                p.DataValue.SetRawValue(address, received);
                                p.DataValue.UpdateTime = DateTime.Now;
                                Console.WriteLine($"param: {p.Name} val:{p.DataValue.GetValue()}");
                            }
                            catch { }
                            ;


                           // worker.ReportProgress(0);
                        }


                    }
                }


            }
        }
    }

}