using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Dialogs.Views;

public class DialogControl : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<DialogControl, string?>(
        nameof(Title));

    public static readonly StyledProperty<IEnumerable<IOption>?> OptionsProperty = AvaloniaProperty.Register<DialogControl, IEnumerable<IOption>?>(
        nameof(Options),
        defaultValue: []);

    private static readonly DirectProperty<DialogControl, IEnumerable<IOption>> PrimaryOptionsProperty =
        AvaloniaProperty.RegisterDirect<DialogControl, IEnumerable<IOption>>(
            nameof(PrimaryOptions),
            o => o.PrimaryOptions);

    private static readonly DirectProperty<DialogControl, IEnumerable<IOption>> CancelOptionsProperty =
        AvaloniaProperty.RegisterDirect<DialogControl, IEnumerable<IOption>>(
            nameof(CancelOptions),
            o => o.CancelOptions);

    private static readonly DirectProperty<DialogControl, IEnumerable<IOption>> DestructiveOptionsProperty =
        AvaloniaProperty.RegisterDirect<DialogControl, IEnumerable<IOption>>(
            nameof(DestructiveOptions),
            o => o.DestructiveOptions);

    private static readonly DirectProperty<DialogControl, IEnumerable<IOption>> SecondaryOptionsProperty =
        AvaloniaProperty.RegisterDirect<DialogControl, IEnumerable<IOption>>(
            nameof(SecondaryOptions),
            o => o.SecondaryOptions);

    private static readonly DirectProperty<DialogControl, IEnumerable<IOption>> InfoOptionsProperty =
        AvaloniaProperty.RegisterDirect<DialogControl, IEnumerable<IOption>>(
            nameof(InfoOptions),
            o => o.InfoOptions);

    public static readonly DirectProperty<DialogControl, bool> HasCustomFooterProperty = AvaloniaProperty.RegisterDirect<DialogControl, bool>(
        nameof(HasCustomFooter), o => o.HasCustomFooter);

    public static readonly DirectProperty<DialogControl, object?> FooterProperty = AvaloniaProperty.RegisterDirect<DialogControl, object?>(
        nameof(Footer), o => o.Footer);

    private IEnumerable<IOption> cancelOptions = [];

    private IEnumerable<IOption> destructiveOptions = [];

    private object? footer;

    private bool hasCustomFooter;

    private IEnumerable<IOption> infoOptions = [];

    private IEnumerable<IOption> primaryOptions = [];

    private IEnumerable<IOption> secondaryOptions = Enumerable.Empty<IOption>();

    public DialogControl()
    {
        // Suscribe al cambio de Options
        this.GetObservable(OptionsProperty).Subscribe(options => { UpdateDerivedProperties(options); });

        this.GetObservable(ContentProperty)
            .Select(c => c as IHaveFooter)
            .Select(c => c?.Footer ?? Observable.Return<object?>(null))
            .Switch()
            .Subscribe(f =>
            {
                Footer = f;
                HasCustomFooter = f is not null;
            });
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IEnumerable<IOption>? Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public IEnumerable<IOption> PrimaryOptions
    {
        get => primaryOptions;
        private set => SetAndRaise(PrimaryOptionsProperty, ref primaryOptions, value);
    }

    public IEnumerable<IOption> CancelOptions
    {
        get => cancelOptions;
        private set => SetAndRaise(CancelOptionsProperty, ref cancelOptions, value);
    }

    public IEnumerable<IOption> DestructiveOptions
    {
        get => destructiveOptions;
        private set => SetAndRaise(DestructiveOptionsProperty, ref destructiveOptions, value);
    }

    public IEnumerable<IOption> SecondaryOptions
    {
        get => secondaryOptions;
        private set => SetAndRaise(SecondaryOptionsProperty, ref secondaryOptions, value);
    }

    public IEnumerable<IOption> InfoOptions
    {
        get => infoOptions;
        private set => SetAndRaise(InfoOptionsProperty, ref infoOptions, value);
    }

    public bool HasCustomFooter
    {
        get => hasCustomFooter;
        private set => SetAndRaise(HasCustomFooterProperty, ref hasCustomFooter, value);
    }

    public object? Footer
    {
        get => footer;
        private set => SetAndRaise(FooterProperty, ref footer, value);
    }

    private void UpdateDerivedProperties(IEnumerable<IOption>? options)
    {
        var safeOptions = options ?? [];

        var opts = safeOptions.ToList();

        PrimaryOptions = opts.Where(o => o.Role == OptionRole.Primary).ToList();
        CancelOptions = opts.Where(o => o.Role == OptionRole.Cancel).ToList();
        DestructiveOptions = opts.Where(o => o.Role == OptionRole.Destructive).ToList();
        SecondaryOptions = opts.Where(o => o.Role == OptionRole.Secondary).ToList();
        InfoOptions = opts.Where(o => o.Role == OptionRole.Info).ToList();
    }
}