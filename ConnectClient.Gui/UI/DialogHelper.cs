using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace ConnectClient.Gui.UI
{
    public class DialogHelper : IDialogHelper
    {
        public void ShowException(Exception e)
        {
            var page = new TaskDialogPage
            {
                Caption = "Fehler",
                Heading = "Ein Fehler ist aufgetreten",
                Icon = TaskDialogIcon.ShieldErrorRedBar,
                Text = e.Message
            };

            if(e.InnerException != null)
            {
                page.Expander.Text = e.InnerException.Message;
                page.Expander.Expanded = true;
            }

            ShowDialogPage(page);
        }

        public void ShowSuccess(int adds, int updates, int removals)
        {
            var page = new TaskDialogPage
            {
                Caption = "Erfolg",
                Heading = "Änderungen erfolgreich angewendet",
                Text = $"{adds} neue Benutzer\n{updates} Benutzer aktualisiert\n{removals} Benutzer gelöscht",
                Icon = TaskDialogIcon.ShieldSuccessGreenBar
            };

            ShowDialogPage(page);
        }

        public bool Confirm(string header, string message)
        {
            var confirmButton = new TaskDialogButton("Bestätigen");
            var cancelButton = new TaskDialogButton("Abbrechen");

            var page = new TaskDialogPage
            {
                Caption = "Bestätigung",
                Heading = header,
                Text = message,
                Icon = TaskDialogIcon.Warning,
                Buttons = [ confirmButton, cancelButton ]
            };

            var result = ShowDialogPage(page);

            return result == confirmButton;
        }

        private TaskDialogButton ShowDialogPage(TaskDialogPage page)
        {
            var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            if (activeWindow != null)
            {
                return TaskDialog.ShowDialog(new WindowInteropHelper(activeWindow).Handle, page);
            }
            else
            {
                return TaskDialog.ShowDialog(page);
            }
        }
    }
}
