using SvnBridge.Presenters;
using SvnBridge.Views;

namespace SvnBridge.Stubs
{
    public class StubSettingsView : ISettingsView
    {
        internal delegate void ShowDelegate();
        
        internal SettingsViewPresenter Set_Presenter;
        internal bool Show_Called;
        internal ShowDelegate Show_Delegate;

        #region ISettingsView Members

        public SettingsViewPresenter Presenter
        {
            set { Set_Presenter = value; }
            get { return Set_Presenter; }
        }

        public void Show()
        {
            if (Show_Delegate != null)
                Show_Delegate();
                
            Show_Called = true;
        }

        #endregion
    }
}