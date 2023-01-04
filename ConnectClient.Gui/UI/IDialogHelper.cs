using System;

namespace ConnectClient.Gui.UI
{
    public interface IDialogHelper
    {
        void ShowSuccess(int adds, int updates, int removals);

        void ShowException(Exception e);
    }
}
