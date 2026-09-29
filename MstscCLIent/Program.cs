using CommandLine;
using Serilog;
using System.Diagnostics;

namespace MstscCLIent;

static class Program
{
    private static KeyBoardRemapper? _remapper;

    [STAThread]
    static void Main(string[] args)
    {
        // Use https://github.com/commandlineparser/commandline for command line args
        var parser = new Parser(config => config.HelpWriter = null);
        Options.HelpCatcher(args, parser);
        var argResults = parser.ParseArguments<Options>(args).MapResult((opts) => ProcessArgs(opts),
         errs => Options.HandleParseError(errs, args, parser));
        if (argResults == -1)
        {
            Environment.Exit(-1);
        }
    }
    public static int ProcessArgs(Options options)
    {
        string dateTime = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");
        string appSessionID = Guid.NewGuid().ToString();
        try
        {

            Log.Logger = SerilogCreator.Init(options, "MstscCLIent", dateTime, appSessionID);
            Log.Information("Params: {@params}", options);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(@$"Error enabling logging {e.Message}");
            return -1;
        }
        string version = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion!;
        Log.Information("MstscCLIent version {version}", version);
        Log.Information("MstscCLIent session {session}", appSessionID);

        try
        {
            Log.Information("Initializing keyboard remapper.");
            _remapper = new KeyBoardRemapper();
            Log.Information("Keyboard remapper initialized successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred when initializing the keyboard remapper, {ex.Message}", ex.Message);
            Log.CloseAndFlush();
            Application.Exit();
        }

        ApplicationConfiguration.Initialize();
        try
        {
            Application.Run(new UI(options));
        }
        finally
        {
            _remapper?.Dispose();
            Log.CloseAndFlush();
        }
        return 1;
    }
}