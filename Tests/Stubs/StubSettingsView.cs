using SvnBridge.Presenters;
using SvnBridge.Views;
using System.Windows.Forms;

namespace SvnBridge.Stubs
{
    public class StubSettingsView : ISettingsView
    {
        public delegate void ShowDelegate();

        public SettingsViewPresenter PresenterProperty;
        public bool Show_Called;
        public ShowDelegate Show_Delegate;
        public DialogResult DialogResult_Return;

        #region ISettingsView Members

        public SettingsViewPresenter Presenter
        {
            set { PresenterProperty = value; }
            get { return PresenterProperty; }
        }

        public void Show()
        {
            if (Show_Delegate != null)
                Show_Delegate();
                
            Show_Called = true;
        }

        public DialogResult DialogResult
        {
            get { return DialogResult_Return; }
        }

        #endregion
    }
}