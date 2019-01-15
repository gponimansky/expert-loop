using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ExpertLoop
{

    /// <summary>
    /// Test Module
    /// </summary>
    public class Test : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Sends back a youtube video link of "It's working" and exclaims it's working.
        /// </summary>
        /// <returns> Task </returns>
        [Command("test")]
        public async Task TestAsync()
        {
            await Task.Run(async () =>
            {
                var messageSend = await Context.Channel.SendMessageAsync("It's working. " + Context.User.Mention + "\nhttps://youtu.be/AXwGVXD7qEQ");
                await Task.Delay(10000).ConfigureAwait(false);
                await Context.Message.DeleteAsync();
                await messageSend.DeleteAsync();
            });
        }
    }

    /// <summary>
    /// Tools Module
    /// </summary>
    public class Tools : ModuleBase<SocketCommandContext>
    {
        // CommandService to list all avaliable commands
        private readonly CommandService _service;

        public Tools(CommandService service)
        {
            _service = service;
        }

        /// <summary>
        /// Help Async fetches a list of avaliable commands
        /// </summary>
        /// <returns> Task </returns>
        [Command("help")]
        [Summary("Shows a list of avaliable commands.")]
        public async Task HelpAsync()
        {
            string prefix = "!";
            var builder = new EmbedBuilder
            {
                Color = new Color(85, 171, 85),
                Description = "These are the commands you can use..."
            };

            foreach (var module in _service.Modules)
            {
                if (!module.Name.Equals("Test"))
                {
                    StringBuilder description = new StringBuilder();
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                        {
                            description.Append($"{prefix}{cmd.Aliases.First()}\n");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(description.ToString()))
                    {
                        builder.AddField(x =>
                        {
                            x.Name = module.Name.First().ToString().ToUpper() + module.Name.Substring(1);
                            x.Value = description.ToString();
                            x.IsInline = false;
                        });
                    }
                }
            }
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        /// <summary>
        /// Shows what a specific command given does
        /// </summary>
        /// <param name="command"> Given command </param>
        /// <returns> Task </returns>
        [Command("help")]
        [Summary("Shows what a specific command does.")]
        public async Task HelpAsync([Remainder][Summary("A given command")] string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            var builder = new EmbedBuilder
            {
                Color = new Color(85, 171, 85),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Name = x.Name.ToUpper();
                    x.Value = (cmd.Parameters.Count != 0) ? $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Summary))}.\n" + $"Description: {cmd.Summary}" : $"Description: {cmd.Summary}";
                    x.IsInline = false;
                });
            }
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        /// <summary>
        /// Clear a given number of commands
        /// </summary>
        /// <param name="numToDelete"> Given number of commands to delete</param>
        /// <returns> Task </returns>
        [Command("clear")]
        [Summary("Clears commands.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ClearAsync([Remainder] [Summary("Number of messages to delete")] int numToDelete = 0)
        {
            await Task.Run(async () =>
            {
                Discord.Rest.RestUserMessage messageSend;
                if (numToDelete <= 100)
                {
                    var messagesToDelete = await Context.Channel.GetMessagesAsync(numToDelete + 1).Flatten();
                    await Context.Channel.DeleteMessagesAsync(messagesToDelete);
                    messageSend = await Context.Channel.SendMessageAsync((numToDelete == 1) ? $"{Context.User.Mention} deleted 1 message" : $"{Context.User.Mention} deleted {numToDelete} messages");
                }
                else
                {
                    messageSend = await Context.Channel.SendMessageAsync("you cannot delete more than 100 messages");
                }

                await Task.Delay(4000).ConfigureAwait(false);
                await messageSend.DeleteAsync();
            });
        }
    }

    /// <summary>
    /// Info Module
    /// </summary>
    public class Info : ModuleBase<SocketCommandContext>
    {
        [Command("profile")]
        [Summary("Returns profile information.")]
        public async Task ProfileAsync([Remainder] [Summary("Avatar")] SocketUser avatar = null)
        {
            if (avatar == null) { avatar = Context.User; }
            await ReplyAsync("Username: " + avatar.Username + "\nStatus: " + avatar.Status + "\nCreated at: " + avatar.CreatedAt + "\nCurrent playing: " + avatar.Game);
        }

        [Command("bot")]
        [Summary("Returns information about the bot.")]
        public async Task BotAsync()
        {
            await ReplyAsync("`IN PROGRESS`");
        }

        [Command("server")]
        [Summary("Returns information about the bot.")]
        public async Task ServerAsync()
        {
            await ReplyAsync("Server name: "+ Context.Guild.Name + "\nOwner: " + Context.Guild.Owner + "\nMember Count: " + Context.Guild.MemberCount);
        }
    }

    /// <summary>
    /// Fun Module
    /// </summary>
    public class Fun : ModuleBase<SocketCommandContext>
    {
        // Random Variable
        readonly Random rand = new Random();

        // Prediction Array for 8ball
        readonly string[] predictionsTexts = new []
        {
            "It is very unlikely.",
            "I don't think so...",
            "Yes!",
            "I don't know",
            "No",
            "It is likely."
        };

        [Command("say")]
        [Summary("Echos a message.")]
        [Alias("repeat")]
        public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
        {
            await ReplyAsync(echo);
        }

        [Command("flipcoin")]
        [Summary("Flips a coin, returning heads or tails.")]
        [Alias("f", "flip", "coin")]
        public async Task FlipCoinAsync()
        {
            await ReplyAsync("The coin landed on " + ((rand.Next(2) == 1) ? "heads" : "tails") + ", " + Context.User.Mention);
        }

        [Command("rolldice")]
        [Summary("Rolls a dice, returning a number between the two numbers given.")]
        [Alias("roll", "die", "dice", "rolldie")]
        public async Task RollDiceAsync([Summary("Max/Min value to roll")] int valueOne = 6, [Summary("Min/Max value to roll")] int valueTwo = 1)
        {
            // declare max and min to check which one passed value is bigger than the other.
            int max, min;
            if (valueOne >= valueTwo)
            {
                min = valueTwo;
                max = valueOne;
            } else {
                min = valueOne;
                max = valueTwo;
            }
            await ReplyAsync("The " + new Emoji(":game_die:") + " rolled a " + rand.Next(min, max+1).ToString() + ", " + Context.User.Mention);
        }

        [Command("8ball")]
        [Summary("Gives a prediction.")]
        public async Task EightBallAsync([Remainder] [Summary("Given question")] string input)
        {
            await ReplyAsync(Context.User.Mention + " " + predictionsTexts[rand.Next(predictionsTexts.Length)]);
        }

        [Command("dog")]
        [Summary("Fetches dog.")]
        [Alias("doge")]
        public async Task ImageAsync()
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                string websiteurl = "https://random.dog/woof.json";
                client.BaseAddress = new Uri(websiteurl);
                HttpResponseMessage response = client.GetAsync("").Result;
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                await ReplyAsync(json["url"].ToString());
            }
        }

        [Command("poll")]
        [Summary("Polls the chat with the attached question.")]
        public async Task PollAsync([Remainder] [Summary("The question to poll")] string question)
        {
            await Context.Message.AddReactionAsync(new Emoji("👍"));
            await Context.Message.AddReactionAsync(new Emoji("👎"));
            await Context.Message.AddReactionAsync(new Emoji("🤷"));
        }

    }

    /// <summary>
    /// Game Module
    /// with a Prefix of Game.
    /// </summary>
    [Group("game")]
    public class Game : ModuleBase<SocketCommandContext>
    {
        // Random Variable
        readonly Random rand = new Random();

        // Game Variables
        static string gameState = "no-game";
        static int playerOneHP, playerTwoHP;
        static SocketUser playerOne, playerTwo;

        [Command("start")]
        [Summary("Checks if there is a game currently playing, (if not) starts a game with the opponent.")]
        public async Task StartGameAsync([Summary("Opponent")] SocketUser avatarTwo)
        {
            await Task.Run(async () =>
            {
                string response;

                switch (gameState)
                {
                    case "no-game":
                        playerOne = Context.User;
                        playerTwo = avatarTwo;
                        playerTwoHP = playerOneHP = 100;
                        gameState = (rand.Next(2) == 1) ? "turn-one" : "turn-two";
                        response = $"Started a game between {playerOne.Username} and {playerTwo.Username}.\nIt is {((gameState == "turn-one") ? playerOne.Username : playerTwo.Username)}'s turn.";
                        break;
                    case "turn-one":
                        response = $"There is currently a game going on between {playerOne.Username} and {playerTwo.Username}.\nIt is {playerOne.Username}'s turn.";
                        break;
                    case "turn-two":
                        response = $"There is currently a game going on between {playerOne.Username} and {playerTwo.Username}.\nIt is {playerTwo.Username}'s turn.";
                        break;
                    default:
                        response = "game state failed. Reset to no-game";
                        gameState = "no-game";
                        break;
                }
                var messageSend = await ReplyAsync(response);
                await Task.Delay(50000).ConfigureAwait(false);
                await Context.Message.DeleteAsync();
                await messageSend.DeleteAsync();
            });
        }

        [Command("damage")]
        [Summary("Checks whose turn it is, (if it is the user's turn) damage the opponent.")]
        [Alias("attack", "hit")]
        public async Task AttackGameAsync()
        {
            await Task.Run(async () =>
            {
                string response;
                int damage = 0;

                switch (gameState)
                {
                    case "no-game":
                        response = "There is no game currently running.";
                        break;
                    case "turn-one":
                        if (Context.User == playerOne)
                        {
                            damage = rand.Next(1, 20);
                            playerTwoHP -= damage;

                            response = $"{playerOne.Username}";
                            if (damage > 19)
                            {
                                damage = 30;
                                response += " gets a critical hit on ";
                            }
                            else if (damage > 15)
                            {
                                response += " stabs ";
                            }
                            else if (damage > 10)
                            {
                                response += " slashes ";
                            }
                            else if (damage > 5)
                            {
                                response += " punches ";
                            }
                            else
                            {
                                response += " throws a pebble at ";
                            }
                            response += $"{playerTwo.Username}.\n";

                            response += $"{playerTwo.Username}.\n{playerTwo.Username} has taken {damage.ToString()} in damage, and has went down to {playerTwoHP.ToString()}";
                            if (playerTwoHP <= 0)
                            {
                                gameState = "no-game";
                                response += " therefore losing the game.";
                            }
                            else
                            {
                                gameState = "turn-two";
                                response += $".\nIt is now {playerTwo.Username}'s turn.";
                            }
                        }
                        else
                        {
                            response = $"There is currently a game going on between {playerOne.Username} and {playerTwo.Username}.\nIt is {playerOne.Username}'s turn.";
                        }
                        break;
                    case "turn-two":
                        if (Context.User == playerTwo)
                        {
                            damage = rand.Next(1, 20);
                            playerOneHP -= damage;

                            response = $"{playerTwo.Username}";
                            if (damage > 19)
                            {
                                damage = 30;
                                response += " gets a critical hit on ";
                            }
                            else if (damage > 15)
                            {
                                response += " stabs ";
                            }
                            else if (damage > 10)
                            {
                                response += " slashes ";
                            }
                            else if (damage > 5)
                            {
                                response += " punches ";
                            }
                            else
                            {
                                response += " throws a pebble at ";
                            }
                            response += $"{playerOne.Username}.\n{playerOne.Username} has taken {damage.ToString()} in damage, and has went down to {playerOneHP.ToString()}";

                            if (playerOneHP <= 0)
                            {
                                gameState = "no-game";
                                response += " therefore losing the game.";
                            }
                            else
                            {
                                gameState = "turn-one";
                                response += $".\nIt is now {playerOne.Username}'s turn.";
                            }
                        }
                        else
                        {
                            response = $"There is currently a game going on between {playerOne.Username} and {playerTwo.Username}.\nIt is {playerTwo.Username}'s turn.";
                        }
                        break;
                    default:
                        response = "game state failed. Reset to no-game";
                        gameState = "no-game";
                        break;
                }
                var messageSend = await ReplyAsync(response);
                await Task.Delay(20000).ConfigureAwait(false);
                await Context.Message.DeleteAsync();
                await messageSend.DeleteAsync();
            });
        }

        [Command("end")]
        [Summary("Checks if the user is enrolled in the game, (if the user is) end game.")]
        [Alias("surrender")]
        public async Task EndGameAsync()
        {
            await Task.Run(async () =>
            {
                string response;

                switch (gameState)
                {
                    case "no-game":
                        response = "There is no game currently running.";
                        break;
                    case "turn-one":
                    case "turn-two":
                        if (Context.User == playerOne || Context.User == playerTwo)
                        {
                            gameState = "no-game";
                            response = $"The game going on between {playerOne.Username} and {playerTwo.Username} has ended.";
                        }
                        else
                        {
                            response = "No ending other people's games.";
                        }
                        break;
                    default:
                        response = "game state failed. Reset to no-game";
                        gameState = "no-game";
                        break;
                }
                var messageSend = await ReplyAsync(response);
                await Task.Delay(50000).ConfigureAwait(false);
                await Context.Message.DeleteAsync();
                await messageSend.DeleteAsync();
            });
        }
    }
}
