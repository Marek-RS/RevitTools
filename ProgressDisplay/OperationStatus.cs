using System;
using System.ComponentModel;
using System.Threading;

namespace TTTRevitTools.ProgressDisplay
{
    public class OperationStatus : INotifyPropertyChanged
    {
        public delegate void OperationStatusEventHandler(object source, EventArgs args);
        public delegate void NewMessageEventHandler(object source, EventArgs args);
        public event OperationStatusEventHandler OperationEnded;
        public event NewMessageEventHandler MessageAdded;
        public ManualResetEvent SignalEvent = new ManualResetEvent(false);
        public bool UserAborted { get; set; }
        private bool _closeButtonEnabled;
        public bool CloseButtonEnabled
        {
            get => _closeButtonEnabled;
            set
            {
                if (_closeButtonEnabled != value)
                {
                    _closeButtonEnabled = value;
                    OnPropertyChanged(nameof(CloseButtonEnabled));
                }
            }
        }
        private bool _abortButtonEnabled;
        public bool AbortButtonEnabled
        {
            get => _abortButtonEnabled;
            set
            {
                if (_abortButtonEnabled != value)
                {
                    _abortButtonEnabled = value;
                    OnPropertyChanged(nameof(AbortButtonEnabled));
                }
            }
        }
        private string _textMessage;
        public string TextMessage
        {
            get => _textMessage;
            set
            {
                if (_textMessage != value)
                {
                    _textMessage = value;
                    OnPropertyChanged(nameof(TextMessage));
                }
            }
        }
        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private OperationStatus()
        {
            AbortButtonEnabled = true;
            CloseButtonEnabled = false;
        }
        public static OperationStatus Initialize(string[] initialMessage)
        {
            OperationStatus result = new OperationStatus();
            result.SetInitialTextMessage(initialMessage);
            result.UserAborted = false;
            return result;
        }
        public static OperationStatus Initialize(string initialMessage)
        {
            OperationStatus result = new OperationStatus();
            result.SetInitialTextMessage(initialMessage);
            result.UserAborted = false;
            return result;
        }
        public static OperationStatus Initialize()
        {
            OperationStatus result = new OperationStatus();
            result.UserAborted = false;
            result.SetInitialTextMessage();
            return result;
        }
        public void CloseOperationWindow()
        {
            OnOperationEnded();
        }
        public void AddLineToTextMessage(string messageLine)
        {
            TextMessage += messageLine + Environment.NewLine;
        }
        public void RemoveLineFromTextMessage(string messageLine)
        {
            string message = messageLine + Environment.NewLine;
            TextMessage = TextMessage.Replace(message, "");
        }
        public void DisableAbortButton()
        {
            AbortButtonEnabled = false;
        }
        public void EnableCloseButton()
        {
            CloseButtonEnabled = true;
        }
        public void ShowCountDown(int seconds)
        {
            int n = seconds;
            for (int i = 0; i < seconds; i++)
            {
                AddLineToTextMessage(string.Format("The window will be closed in {0} seconds", n));
                Thread.Sleep(1000);
                RemoveLineFromTextMessage(string.Format("The window will be closed in {0} seconds", n));
                n = n - 1;
            }
        }
        private void SetInitialTextMessage(string[] initialMessage)
        {
            foreach (string line in initialMessage)
            {
                TextMessage += line + Environment.NewLine;
            }
        }
        private void SetInitialTextMessage(string initialMessage)
        {
            TextMessage = initialMessage + Environment.NewLine;
        }
        private void SetInitialTextMessage()
        {
            TextMessage = "";
        }
        protected virtual void OnOperationEnded()
        {
            OperationEnded?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnMessageAdded()
        {
            MessageAdded?.Invoke(this, EventArgs.Empty);
        }
    }
}