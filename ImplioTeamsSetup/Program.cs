using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ImplioTeamsSetup.Auxiliary;
using ImplioTeamsSetup.Enums;
using ImplioTeamsSetup.TwilioSpecific;

namespace ImplioTeamsSetup
{
    class Program
    {

        static async Task Main(string[] args)
        {
            if (args.Length < 3 || args[0].Contains("help"))
            {
                Utilities.DisplayHelp();
                return;
            }

            if (!Enum.TryParse(args[0], ignoreCase: true, out Commands command))
            {
                Console.WriteLine($"Unrecognised command [{args[0]}]. Exiting...");
                return;
            }

            TwilioClient twilioClient = TwilioClient.Create(args[^1]);

            if(twilioClient == null)
                return;

            switch (command)
            {
                case Commands.DeleteRules:
                    Console.WriteLine("Attempting deleteion of the rules...");
                    await twilioClient.DeleteAllRulesAsync(args[1]);
                    break;
                case Commands.DeleteLists:
                    Console.WriteLine("Attempting deleteion of the lists...");
                    await twilioClient.DeleteAllListsAsync(args[1]);
                    break;
                case Commands.CopyRules:
                    Console.WriteLine("Attempting copying the rules...");
                    await twilioClient.CopyRulesAsync(args[2], args[1]);
                    break;
                case Commands.CopyLists:
                    Console.WriteLine("Attempting copying the lists...");
                    await twilioClient.CopyListsAsync(args[2], args[1]);
                    break;
                default:
                    Console.WriteLine($"Unrecognised command [{args[0]}]. Exiting...");
                    break;
            }
            
            Console.ReadKey();
        }
    }
}
