using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GSCFieldApp.Services
{
    internal class ErrorLogToFile
    {
        private const string Directory = "C:\\AppLogs";
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public string DefaultPath
        {
            get
            {
                var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                var folder = Path.Combine(ApplicationData.Current.LocalFolder.Path);

                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }

                var fileName = $"GSCFieldAppLog {DateTime.Today:yyyy-MM-dd}.txt";
                return $"{folder}\\{fileName}";
            }
        }

        public ErrorLogToFile(string message)
        {
            Message = message;
        }

        public ErrorLogToFile(Exception ex)
        {
            Exception = ex;
        }

        public bool WriteToFile(string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = DefaultPath;
            }

            try
            {
                using (var writer = new StreamWriter(path, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    writer.WriteLine();

                    if (Exception != null)
                    {
                        writer.WriteLine(Exception.GetType().FullName);
                        writer.WriteLine("Source : " + Exception.Source);
                        writer.WriteLine("Message : " + Exception.Message);
                        writer.WriteLine("StackTrace : " + Exception.StackTrace);
                        writer.WriteLine("InnerException : " + Exception.InnerException?.Message);
                    }

                    if (!string.IsNullOrEmpty(Message))
                    {
                        writer.WriteLine(Message);
                    }

                    writer.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

    }
}
