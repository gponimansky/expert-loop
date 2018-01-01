using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertLoop
{

    public class ExtraModule : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task TestAsync()
        {
            await Context.Channel.SendMessageAsync("It's working. " + Context.User.Mention);
        }
    }

    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [Summary("Echos a message.")]
        [Alias("repeat")]
        public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
        {
            await ReplyAsync(echo);
        }

        [Command("flipcoin")]
        [Summary("Flips coin.")]
        [Alias("f", "flip", "flip coin")]
        public async Task FlipCoinAsync()
        {
            await ReplyAsync((new Random().Next(100) >= 50) ? "Heads" : "Tails");
        }

        [Command("help")]
        [Summary("Help with commands.")]
        [Alias("h")]
        public async void HelpAsync()
        {
            await Context.Channel.SendMessageAsync("The following commands are listed.");
        }

    }
}