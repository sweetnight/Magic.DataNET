namespace Magic.DataNET
{
    public class Logs
    {

        private string LogFilePath { get; set; } = string.Empty;

        public Logs(string logFilePath)
        {
            LogsGeneralEvents?.Invoke(new LogsGeneralEventArgs(1, "Start initiate Logs system. Create LogFilePath property."));

            if (!File.Exists(logFilePath))
            {
                LogsGeneralEvents?.Invoke(new LogsGeneralEventArgs(2, "Create new Log File."));

                File.Create(logFilePath).Close();
            }

            LogFilePath = logFilePath;

            LogsGeneralEvents?.Invoke(new LogsGeneralEventArgs(3, "Initiate Logs system done"));

        } // end of method

        public void Add(string content)
        {
            LogsGeneralEvents?.Invoke(new LogsGeneralEventArgs(4, "Prepare Add Log"));

            File.AppendAllText(this.LogFilePath, $"[{DateTime.Now.ToString()}] " + content + Environment.NewLine);

            LogsGeneralEvents?.Invoke(new LogsGeneralEventArgs(5, "Add Log done"));

        } // end of method

        public event Action<LogsGeneralEventArgs>? LogsGeneralEvents;

        public class LogsGeneralEventArgs
        {
            public int eventType { get; set; }
            public string eventMessage { get; set; } = string.Empty;

            private void eventSignature(int eventType, string eventMessage)
            {
                this.eventType = eventType;
                this.eventMessage = eventMessage;
            } // end of method

            public LogsGeneralEventArgs(int eventType, string eventMessage)
            {
                eventSignature(eventType, eventMessage);
            } // end of constructor method
        } // end of class

    } // end of class


} // end of namespace
