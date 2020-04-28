using CommandLine;

namespace ConnectClient.Cli
{
    public class Options
    {
        [Option("fullsync", HelpText = "Whether or not to perform a full sync (updates all existing users independent of their modify date.")]
        public bool FullSync { get; set; }
    }
}
