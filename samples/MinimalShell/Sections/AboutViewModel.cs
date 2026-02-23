using System.Reactive.Linq;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section(icon: "fa-circle-info", sortIndex: 3)]
public class AboutViewModel : IHaveHeader
{
    public IObservable<object> Header => Observable.Return<object>("About This App");
    public string Version => "1.0.0";
    public string Framework => "Avalonia + Zafiro";
}
