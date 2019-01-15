using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ExpertLoop
{
    public class Program
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        public static void Main(string[] args) => 
            new Program().MainAsync().GetAwaiter().GetResult();

        /// <summary>
        /// MainAsync
        /// </summary>
        /// <returns>Returns a task that is blocked until the program is closed</returns>
        public async Task MainAsync()
        {
            // Sets up client Socket and LogLevel severity.
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            // Sets up command service
            _commands = new CommandService();

            // Adds log to client
            _client.Log += Log;

            _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .BuildServiceProvider();

            await InstallCommandsAsync().ConfigureAwait(false);

            // Login and Start the Bot.
            await _client.LoginAsync(TokenType.Bot, APIKey.BotToken);
            await _client.StartAsync();

            // When the bot is ready setup the GameStatus and return a task complete
            _client.Ready += () =>
            {
                // To make bot leave specific server, _client.GetGuild(########).LeaveAsync();
                // Sets Game Playing Status
                _client.SetGameAsync("01100100 01110010 01100101 01100001 01101101 01101001 01101110 01100111");
                return Task.CompletedTask;
            };

            
            // Block this task until the program is closed.
            await Task.Delay(-1).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads all commands and hooks MessageReceived Event to the Command Handler.
        /// </summary>
        /// <returns></returns>
        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into Command Handler
            _client.MessageReceived += HandleCommandAsync;

            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Processes Commands
        /// </summary>
        /// <param name="messageParam">Message Received</param>
        /// <returns>Command Response/Error Result/Nothing</returns>
        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) { return; }

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot)
            {
                // Checks if message correctly matches "no u" no matter the case.
                if (message.Content.ToLower().Contains("no u"))
                {
                    // Replies with no u privately 
                    await message.Author.SendMessageAsync("no u");
                }
                   
                return;
            }

            // Create a Command Context
            var context = new SocketCommandContext(_client, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
            {
                if (result.Error.Value != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }

        /// <summary>
        /// Writes a LogMessage to the console via certain colour based on Severity.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static Task Log(LogMessage message)
        {
            var fColor = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = fColor;
            return Task.CompletedTask;
        }
    }
}
