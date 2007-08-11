using SvnBridge.Presenters;

namespace SvnBridge.Views
{
    public interface ISettingsView
    {
        SettingsViewPresenter Presenter { set; }
        
        void Show();
    }
}
