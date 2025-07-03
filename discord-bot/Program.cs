using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands; 
using DotNetEnv; // Biblioteca para carregar variáveis de ambiente
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Carregue as variáveis de ambiente do arquivo .env
            Env.Load();

            var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token do bot não encontrado. Configure a variável de ambiente 'DISCORD_BOT_TOKEN'.");
                return;
            }

            var guildId = Environment.GetEnvironmentVariable("DISCORD_GUILD_ID");
            if (!ulong.TryParse(guildId, out var parsedGuildId))
            {
                Console.WriteLine("ID do servidor inválido. Configure a variável de ambiente 'DISCORD_GUILD_ID'.");
                return;
            }

            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "!" }
            });

            commands.RegisterCommands<BotCommands>();

            var slash = discord.UseSlashCommands();
            slash.RegisterCommands<SlashCommands>(guildId: parsedGuildId);

            discord.ComponentInteractionCreated += OnComponentInteraction;

            discord.Ready += OnReady;

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar o bot: {ex.Message}");
        }
    }

    private static Task OnReady(DiscordClient sender, ReadyEventArgs e)
    {
        Console.WriteLine("Bot está online!");
        return Task.CompletedTask;
    }

    private static async Task OnComponentInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (e.Id == "item_select")
        {
            var selectedItem = e.Values.FirstOrDefault();
            var guild = e.Guild;
            var user = e.User;
            var member = await guild.GetMemberAsync(user.Id);

            var ticketChannel = await guild.CreateChannelAsync($"ticket-{selectedItem}-{user.Username}", DSharpPlus.ChannelType.Text);

            await ticketChannel.AddOverwriteAsync(member, Permissions.AccessChannels | Permissions.SendMessages);
            await ticketChannel.AddOverwriteAsync(guild.EveryoneRole, Permissions.None);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Ticket Criado",
                Description = $"Você selecionou o item: **{selectedItem}**.\nUm administrador irá atendê-lo em breve.",
                Color = DiscordColor.Green
            };

            await ticketChannel.SendMessageAsync(embed);
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Ticket criado com sucesso!"));
        }
    }
}
