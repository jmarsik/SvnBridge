using SvnBridge.Presenters;
using System.Windows.Forms;

namespace SvnBridge.Views
{
    public interface ISettingsView
    {
        SettingsViewPresenter Presenter { set; }
        DialogResult DialogResult { get; }
        void Show();
    }
}
