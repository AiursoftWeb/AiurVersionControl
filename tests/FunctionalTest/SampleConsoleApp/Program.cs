using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using SampleWebApp.Data;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleConsoleApp
{
    class Program
    {
        public async static Task Main(string[] args)
        {
            Console.WriteLine("Please enter the ares endpoint URL.");
            var endpointUrl = Console.ReadLine();

            var repo = new Repository<LogItem>();
            await repo.AddRemoteAsync(new WebSocketRemote<LogItem>(endpointUrl, autoPush: true, autoPull: true));

            while (true)
            {
                var command = Console.ReadLine();
                switch (command)
                {
                    default:
                    case "help":
                        Console.WriteLine("Try help, log, commit, push, pull");
                        break;
                    case "log":
                        PrintRepo(repo);
                        break;
                    case "commit":
                        await repo.CommitAsync(new LogItem
                        {
                            Message = DateTime.Now.ToString()
                        });
                        break;
                    case "pull":
                        await repo.PullAsync();
                        break;
                    case "push":
                        await repo.PushAsync();
                        break;
                }

            }
        }

        public static void PrintRepo<T>(Repository<T> repo)
        {
            foreach (var commit in repo.Commits)
            {
                Console.WriteLine(JsonSerializer.Serialize(commit.Item));
            }
        }
    }
}
