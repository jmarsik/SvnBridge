using System;
using System.Windows.Forms;

namespace SvnBridge.Utility
{
    public class WaitCursor : IDisposable
    {
        // Fields

        Form form;
        Cursor oldCursor;

        // Lifetime

        public WaitCursor(Form form)
        {
            this.form = form;

            oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;
        }

        public void Dispose()
        {
            form.Cursor = oldCursor;
        }
    }
}