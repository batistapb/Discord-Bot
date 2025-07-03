using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands; // Adicionado para suporte a slash commands
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var token = File.ReadAllText("BotToken.txt").Trim(); // Carregue o token de um arquivo externo

            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "!" } // Prefixo para comandos
            });

            commands.RegisterCommands<BotCommands>();

            var slash = discord.UseSlashCommands(); // Adicionado para suporte a slash commands
            slash.RegisterCommands<SlashCommands>(guildId: 1231040307328585738); // Substitua YOUR_GUILD_ID pelo ID do servidor

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
            var member = await guild.GetMemberAsync(user.Id); // Retrieve the DiscordMember object

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
