using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using SampleFileExplorer.ViewModels;
using SampleFileExplorer.Views;
using Serilog;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.FileExplorer.Core;
using Zafiro.Avalonia.FileExplorer.Core.Clipboard;
using Zafiro.Avalonia.FileExplorer.Core.Transfers;
using Zafiro.Avalonia.Mixins;
using Zafiro.Avalonia.Services;
using Zafiro.FileSystem.SeaweedFS.Filer.Client;
using FileSystem = Zafiro.FileSystem.Local.FileSystem;

namespace SampleFileExplorer;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        ConnectRealApp();
        //ConnectTest();
    }

    private void ConnectRealApp()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        this.Connect(
            () => new MainView(),
            mv =>
            {
                var topLevel = TopLevel.GetTopLevel(mv)!;
                var notificationService = new NotificationService();
                var handler = LoggerExtensions.GetHandler(Log.Logger);
                var seaweedfs = new Zafiro.FileSystem.SeaweedFS.FileSystem(new SeaweedFSClient(new System.Net.Http.HttpClient(handler)
                {
                    BaseAddress = new Uri("http://192.168.1.29:8888"),
                }));
                var dialogService = new DesktopDialog();
                ITransferManager transferManager = new TransferManager();
                List<FileSystemConnection> connections =
                [
                    new FileSystemConnection("local", "Local", new FileSystem(new System.IO.Abstractions.FileSystem())),
                    new FileSystemConnection("seaweedfs", "SeaweedFS", seaweedfs)
                ];
                var clipboardService = new ClipboardService(topLevel.Clipboard!, transferManager, connections);
                return new MainViewModel(connections, notificationService, dialogService, clipboardService, transferManager);
            }, () => new MainWindow());
    }

    private void ConnectTest()
    {
        this.Connect(
            () => new TestView(),
            mv =>
            {
                var topLevel = TopLevel.GetTopLevel(mv)!;
                var notificationService = new NotificationService(NotificationPosition.BottomRight);
                var fs = new Zafiro.FileSystem.SeaweedFS.FileSystem(new SeaweedFSClient(new System.Net.Http.HttpClient()
                {
                    BaseAddress = new Uri("http://192.168.1.29:8888"),
                    Timeout = TimeSpan.FromHours(12),
                }));
                return new TestViewModel(fs, notificationService);
            }, () => new MainWindow());
    }
}