using System;

namespace ImplioTeamsSetup.Auxiliary
{
    public static class Utilities
    {
        public static void DisplayHelp()
        {
            Console.WriteLine("Usage: implioteamssetup <command> [<parameters...>] <authToken>");
            Console.WriteLine("<command>:");
            Console.WriteLine("\tdeleterules <domainGUID>");
            Console.WriteLine("\tdeletelists <domainGUID>");
            Console.WriteLine("\tcopyrules <domainGUIDTo> <domainGUIDFrom>");
            Console.WriteLine("\tcopylists <domainGUIDTo> <domainGUIDFrom>");

            Console.ReadKey();
        }
    }
}
