using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using CSharpFunctionalExtensions;

namespace Zafiro.Avalonia.Misc;

public class ApplicationDataTemplatesRegistration : AvaloniaObject
{
    public static readonly StyledProperty<Uri?> SourceProperty = AvaloniaProperty.Register<ApplicationDataTemplatesRegistration, Uri?>(
        nameof(Source));

    static readonly object gate = new();
    static readonly HashSet<string> registeredSources = new();

    static ApplicationDataTemplatesRegistration()
    {
        SourceProperty.Changed.AddClassHandler<ApplicationDataTemplatesRegistration>((registration, args) =>
        {
            registration.OnSourceChanged(args);
        });
    }

    public Uri? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    void OnSourceChanged(AvaloniaPropertyChangedEventArgs change)
    {
        Register(change.GetNewValue<Uri?>());
    }

    static void Register(Uri? source)
    {
        Maybe.From(source)
            .Bind(uri => Maybe.From(Application.Current).Map(app => (app, uri)))
            .Bind(tuple => LoadTemplates(tuple.uri).Map(templates => (tuple.app, templates)))
            .Map(tuple =>
            {
                foreach (var template in tuple.templates)
                {
                    tuple.app.DataTemplates.Add(template);
                }

                return true;
            });
    }

    static Maybe<DataTemplates> LoadTemplates(Uri uri)
    {
        if (!ShouldRegister(uri))
        {
            return Maybe<DataTemplates>.None;
        }

        return Maybe.From(AvaloniaXamlLoader.Load(uri) as DataTemplates);
    }

    static bool ShouldRegister(Uri uri)
    {
        lock (gate)
        {
            return registeredSources.Add(uri.ToString());
        }
    }
}
