using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUpdate
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [Summary("Echos a message.")]
        [Alias("repeat")]
        public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
        {
            // ReplyAsync is a method on ModuleBase
            await ReplyAsync(echo);
        }

        [Command("flipcoin")]
        [Summary("Flips coin.")]
        [Alias("f", "flip", "flip coin")]
        public async Task FlipCoinAsync()
        {
            // ReplyAsync is a method on ModuleBase
            await ReplyAsync((new Random().Next(100) >= 50) ? "Heads" : "Tails");
   
        }
    }
}
