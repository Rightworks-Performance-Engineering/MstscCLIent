using CommandLine;
using CommandLine.Text;
using Destructurama.Attributed;
using System.Text.RegularExpressions;

namespace MstscCLIent
{
    public class Options
    {
        [Option('d', "Domain", Required = true, HelpText = "Domain for the connection.")]
        public string Domain { get; set; }

        [Option('u', "Username", Required = true, HelpText = "Username for the connection.")]
        public string Username { get; set; }

        [LogMasked(PreserveLength = true)]
        [Option('p', "Password", Required = true, HelpText = "Password for the connection.")]
        public string Password { get; set; }

        [Option('f', "FQDN", Required = true, HelpText = "Host to connect to.")]
        public string Host { get; set; }

        [Option('s', "Syslog", Required = false, HelpText = "Syslog server to send UDP logs.")]
        public string Syslog { get; set; }

        [Option('z', "Debug", Required = false, HelpText = "Enable Debug Mode.")]
        public bool Debug { get; set; }

        public static void DisplayBanner()
        {
            Console.WriteLine("");
            Console.WriteLine("██╗  ██╗███████╗██╗     ██████╗ ");
            Console.WriteLine("██║  ██║██╔════╝██║     ██╔══██╗");
            Console.WriteLine("███████║█████╗  ██║     ██████╔╝");
            Console.WriteLine("██╔══██║██╔══╝  ██║     ██╔═══╝ ");
            Console.WriteLine("██║  ██║███████╗███████╗██║     ");
            Console.WriteLine("╚═╝  ╚═╝╚══════╝╚══════╝╚═╝     ");
            Console.WriteLine("");
        }
        public static bool HelpRequired(string param)
        {
            return param == "--help" || param == "/?" || param == "/help";
        }
        public static void HelpCatcher(string[] args, Parser parser)
        {
            if (args.Length == 1 && Options.HelpRequired(args[0]))
            {
                Options.DisplayBanner();
                var ParserResult = parser.ParseArguments<Options>([""]);
                var helpText = HelpText.AutoBuild(
                    ParserResult,
                    h =>
                    {
                        h.Copyright = "";
                        h.Heading = "";
                        h.MaximumDisplayWidth = 160;
                        return HelpText.DefaultParsingErrorsHandler(ParserResult, h);
                    }, e => e, true);
                helpText.Copyright = "";
                Console.WriteLine(Regex.Replace(helpText.ToString(), "(Required option (.*)|ERROR\\(S\\):)", ""));
                Environment.Exit(0);
            }
        }
        public static void DisplayErrors(string[] args, Parser parser)
        {
            Options.DisplayBanner();
            var ParserResult = parser.ParseArguments<Options>(args);
            var helpText = HelpText.AutoBuild(
                   ParserResult,
                   h =>
                   {
                       h.Copyright = "";
                       h.Heading = "";
                       h.MaximumDisplayWidth = 160;
                       return HelpText.DefaultParsingErrorsHandler(ParserResult, h);
                   }, e => e, true);
            helpText.Copyright = "";
            Console.WriteLine(helpText);
        }
        public static int HandleParseError(IEnumerable<Error> errs, string[] args, Parser parser)
        {
            if (errs.Any(x => x is HelpRequestedError))
            {
                HelpCatcher(args, parser);
                return -1;
            }
            DisplayErrors(args, parser);
            return -1;
        }
    }
}
