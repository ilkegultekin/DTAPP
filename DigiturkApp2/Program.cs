using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigiturkApp2
{
    class Program
    {
        static string folderToWrite = ConfigurationManager.AppSettings["folderToWrite"];
        static string folderToRead = ConfigurationManager.AppSettings["folderToRead"];
        static string logFilePath = ConfigurationManager.AppSettings["logFilePath"];
        static BackgroundWorker bgWorker = new BackgroundWorker();
        static string curFileName;
        static int logCount;
        static int lineCount;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            logCount = 0;
            lineCount = 0;

            InitializeBackgroundWorker();
            bgWorker.RunWorkerAsync();

            if (File.Exists(logFilePath))
            {
                using (StreamReader sr = new StreamReader(logFilePath))
                {
                    curFileName = sr.ReadLine();
                    logCount = Int32.Parse(sr.ReadLine());
                    lineCount = Int32.Parse(sr.ReadLine());
                }

                splitFile();
            }
            File.Delete(logFilePath);

            string[] fileList = Directory.GetFiles(folderToRead,"*.log");
            foreach (var fileName in fileList)
            {
                //string fileNameWoExt = fileName.Substring(fileName.LastIndexOf('\\')+1, fileName.IndexOf('.')- fileName.LastIndexOf('\\')-1);
                string fileNameWExt = Path.GetFileName(fileName);
                string fileNameWoExt = fileNameWExt.Substring(0, fileNameWExt.IndexOf('.'));
                curFileName = fileNameWoExt;
                logCount = 0;
                lineCount = 0;
                splitFile();

            }
            bgWorker.CancelAsync();
        }

        static void InitializeBackgroundWorker()
        {
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new DoWorkEventHandler(listenForShutDown);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerCompleted);
        }

        static void bgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool shutDownSignal = (bool)e.Result;
            if (shutDownSignal)
            {
                Environment.Exit(0);
            }
        }

        static void listenForShutDown(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("Press Escape to shut down the program!");
            //while (!Console.KeyAvailable)
            //    Thread.Sleep(100);
            while (Console.ReadKey().Key != ConsoleKey.Escape || bgWorker.CancellationPending)
            {
                Thread.Sleep(10);
            }

            if (bgWorker.CancellationPending)
            {
                e.Result = false;
                return;
            }
            else
            {
                Console.WriteLine("You pressed Escape! Program shutting down!");
                e.Result = true;
            }
        }

        static void splitFile()
        {
            int fileLineCounter = 0;
            string line = string.Empty;
            StreamWriter sw = null;
            string curLogFileName, curLogFilePathString;
            string filePathString = String.Concat(folderToRead, curFileName, ".log");

            if (lineCount > 0)
            {
                curLogFileName = String.Concat(curFileName, "-", logCount.ToString().PadLeft(4, '0'));
                curLogFilePathString = String.Concat(folderToWrite, curLogFileName, ".log");
                sw = File.AppendText(curLogFilePathString);
            }
            using (StreamReader sr = new StreamReader(filePathString))
            {
                while (fileLineCounter < lineCount)
                {
                    sr.ReadLine();
                    fileLineCounter++;
                }
                if (fileLineCounter == 0)
                {
                    sr.ReadLine();
                    fileLineCounter++; lineCount++;
                }
                
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    fileLineCounter++; lineCount++;
                    if (fileLineCounter % 2300 == 2)
                    {   
                        Thread.Sleep(3000);
                        Console.WriteLine("Woke up\n");
                        if (sw != null)
                        {
                            sw.Flush();
                            sw.Close();
                            sw.Dispose();
                        }
                        logCount++;
                        curLogFileName = String.Concat(curFileName, "-", logCount.ToString().PadLeft(4, '0'));
                        curLogFilePathString = String.Concat(folderToWrite, curLogFileName, ".log");
                        sw = File.CreateText(curLogFilePathString);
                    }
                    Thread.Sleep(10);
                    sw.WriteLine(line);
                }
            }

            sw.Flush();
            sw.Close();
            sw.Dispose();
            Console.WriteLine("Finished splitting file {0}", curFileName);
            File.Delete(filePathString);
            Console.WriteLine("Created {0} log files", logCount);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            using (StreamWriter sw = File.CreateText(logFilePath))
            {
                sw.WriteLine(curFileName);
                sw.WriteLine(logCount);
                sw.WriteLine(lineCount);
            }

            Console.WriteLine("Process exiting\n");
        }
    }
}
