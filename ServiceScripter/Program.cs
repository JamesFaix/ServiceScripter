using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fun;
using ServiceScripter.Control;

namespace ServiceScripter
{
    static class Program
    {
        static void Main(string[] args)
        {
            var path = ValidateArgs(args);

            var repository = new ServiceRepository();
            var parser = new ScriptParser();
            var runner = new ScriptRunner(repository);

            Task.Run(async () =>
                {
                    var result = await Result
                        .Try(() => File.ReadAllText(path))
                        .Map(parser.Parse)
                        .MapAsync(runner.Run);

                    Console.WriteLine(result.HasValue
                        ? string.Format("Script ran successfully.")
                        : result.Error.Message);
                })
                .GetAwaiter()
                .GetResult();

            Console.Read();
        }

        private static string ValidateArgs(string[] args)
        {
            if (args.Length > 2)
                throw new ArgumentException(
                    "There is only one optional argument, the path to a script file." + Environment.NewLine +
                    "If no path is given, script.json in the application directory is assumed.");

            string path;
            if (args.Any())
            {
                path = args[0];
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                var dir = Path.GetDirectoryName(assembly.Location);
                path = $"{dir}\\script.json";
            }

            return path;
        }
    }
}
