﻿using System.Net.Http;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Gadget.Cli.Commands
{
    [Command("services restart", Description = "Attempts to restart requested service")]
    public class RestartServiceCommand : Command, ICommand
    {
        [CommandParameter(0, Description = "agent name")]
        public string Agent { get; set; }

        [CommandParameter(1, Description = "service name")]
        public string Service { get; set; }

        public RestartServiceCommand(HttpClient httpClient) : base(httpClient)
        {
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            var response = await HttpClient.PostAsync($"agents/{Agent}/{Service}/restart", null!);
            if (!response.IsSuccessStatusCode)
            {
                await console.Output.WriteLineAsync("could not execute this command successfully");
                return;
            }

            await console.Output.WriteLineAsync("command executed successfully");
        }
    }
}