using System;

namespace ConnectClient.Gui.Message
{
    public class ErrorDialogMessage : DialogMessage
    {
        public Exception Exception { get; set; }
    }
}
