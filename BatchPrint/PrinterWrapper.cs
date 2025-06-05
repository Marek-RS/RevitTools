using System;
using System.IO;
using System.Collections.Generic;
using pdfforge.PDFCreator.UI.ComWrapper;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TTTRevitTools.BatchPrint
{
    public static class PrinterWrapper
    {
        static Queue jobQueue;
        static bool _isTypeInitialized = false;

        public static string GetVersion(out bool allowed, string ourVersion)
        {
            string foundVersion = "0.0.0";
            string keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\pdfforge\PDFCreator\Program";
            try
            {
                var value = Registry.GetValue(keyName, "ApplicationVersion", null);
                foundVersion = value.ToString();
                allowed = CompareVersions(foundVersion, ourVersion);
            }
            catch (Exception)
            {
                allowed = false;
                foundVersion = "Version not found";
            }
            return foundVersion;
        }

        public static bool CompareVersions(string foundVersion, string implVersion)
        {
            if(implVersion.CompareTo(foundVersion) > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool CheckPDFCreatorInstances()
        {
            bool result = false;
            var pdfCreator = new PdfCreatorObj();
            result = pdfCreator.IsInstanceRunning;
            try
            {
                if (result)
                {
                    //MessageBox.Show("Instance is running");
                    var processes = Process.GetProcessesByName("PDFCreator");
                    foreach (Process p in processes)
                    {                       
                        p.Kill();
                        p.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            var pdfCreatorTest = new PdfCreatorObj();
            result = pdfCreatorTest.IsInstanceRunning;
            return result;
        }

        public static List<string> CheckPrinters()
        {
            List<string> result = new List<string>();
            Printers printers = new PdfCreatorObj().GetPDFCreatorPrinters;
            int noOfPrinters = printers.Count;
            for (int i = 0; i < noOfPrinters; i++)
            {
                string printerName = printers.GetPrinterByIndex(i);
                result.Add(printerName);
            }
            return result;
        }

        private static Queue CreateQueue()
        {
            // This needs to be done once to make the ComWrapper work reliably.
            if (!_isTypeInitialized)
            {
                Type queueType = Type.GetTypeFromProgID("PDFCreator.JobQueue");
                Activator.CreateInstance(queueType);
                _isTypeInitialized = true;
            }

            return new Queue();
        }

        public static void InitializeJobQueue()
        {
            //initialize PDFCreator Queue to catch print jobs
            jobQueue = CreateQueue();
            jobQueue.Initialize();
        }

        public static void CancelPrinting()
        {
            try
            {
                int jobCount = jobQueue.Count;
                if (jobCount != 0)
                {
                    jobQueue.WaitForJobs(jobCount, 1);
                    for (int i = 0; i < jobCount; i++)
                    {
                        jobQueue.DeleteJob(i);
                    }
                }
                jobQueue.Clear();
                jobQueue.ReleaseCom();
                CheckPDFCreatorInstances();
            }
            catch (Exception)
            {
                jobQueue.Clear();
                jobQueue.ReleaseCom();
                CheckPDFCreatorInstances();
            }
        }

        //must return some information
        public static void AwaitJobs(Dictionary<string, string> printNameDictionary, string directoryPath)
        {

            jobQueue.WaitForJob(3);
            while (jobQueue.Count > 0)
            {
                PrintJob printJob = jobQueue.NextJob;
                const string profile = "DefaultGuid";
                printJob.SetProfileByGuid(profile);
                foreach (var singleSetting in GetProfileSettings())
                {
                    printJob.SetProfileSetting(singleSetting.Key, singleSetting.Value);
                }
                string printFileName = Path.GetFileName(printJob.PrintJobInfo.PrintJobName) + "_not_found.pdf";
                if(printNameDictionary.ContainsKey(Path.GetFileName(printJob.PrintJobInfo.PrintJobName)))
                {
                    printFileName = printNameDictionary[Path.GetFileName(printJob.PrintJobInfo.PrintJobName)] + ".pdf";
                }
                printJob.ConvertTo(Path.Combine(directoryPath, printFileName));
                if (!printJob.IsFinished || !printJob.IsSuccessful)
                {
                    System.Windows.MessageBox.Show("Failed to process document: " + printJob.PrintJobInfo.PrintJobName); //temporary                                                                                                                               //write to log
                    continue;
                }
            }
                           
            jobQueue.Clear();
            jobQueue.ReleaseCom();
            CheckPDFCreatorInstances();
        }

        private static Dictionary<string, string> GetProfileSettings()
        {
            //create profile settings
            var result =  new Dictionary<string, string>();
            //result.Add("ShowProgress", "true");
            return result;
        }
    }
}
