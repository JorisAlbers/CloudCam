using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using ReactiveUI;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Splat;

namespace CloudCam
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ErrorObserver ErrorObserver { get; private set; }


        public App()
        {
            SetupLogging();

            SetupLoggingForUnhandledExceptions();

            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
        }

        private void SetupLoggingForUnhandledExceptions()
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            RxApp.DefaultExceptionHandler = Observer.Create(
                (Exception e) =>
                {
                    if (Debugger.IsAttached) Debugger.Break();
                    Log.Logger.Fatal(e, "An unhandled exception occurred in an reactive stream.");
                    RxApp.MainThreadScheduler.Schedule(() => { throw e; });
                },
                (Exception e) =>
                {
                    if (Debugger.IsAttached) Debugger.Break();
                    Log.Logger.Fatal(e, "An unhandled exception occurred in an reactive stream.");
                    RxApp.MainThreadScheduler.Schedule(() => { throw e; });
                },
                () =>
                {
                    if (Debugger.IsAttached) Debugger.Break();
                    RxApp.MainThreadScheduler.Schedule(() => { throw new NotImplementedException(); });
                });
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.Exception, "An unhandled exception occurred.");
        }

        private void SetupLogging()
        {
            string logFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CloudCam",
                "logs.txt");

            ErrorObserver = new ErrorObserver();

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .WriteTo.Console()
#endif
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Infinite)
                .WriteTo.Logger(lc=>lc.Filter.ByIncludingOnly((@event => @event.Level == LogEventLevel.Error || @event.Level == LogEventLevel.Fatal))
                    .WriteTo.Sink(ErrorObserver))
                .CreateLogger();

        }
    }

    public class ErrorObserver : ILogEventSink, IObservable<LogEvent>
    {
        private readonly Subject<LogEvent> _errorSubject = new Subject<LogEvent>();

        public void Emit(LogEvent logEvent)
        {
            switch (logEvent.Level)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                case LogEventLevel.Information:
                case LogEventLevel.Warning:
                    return;
                default:
                    _errorSubject.OnNext(logEvent);
                    break;
            }
            
        }

        public IDisposable Subscribe(IObserver<LogEvent> observer)
        {
            return _errorSubject.Subscribe(observer);
        }
    }
}
