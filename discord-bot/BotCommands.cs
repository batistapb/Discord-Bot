using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System;
using System.Threading.Tasks;

public class BotCommands : BaseCommandModule
{
    private readonly ConfigService _configService = new ConfigService();
    private readonly ItemService _itemService = new ItemService();

    [Command("embed")]
    public async Task SendEmbed(CommandContext ctx, string title, string description)
    {
        var embed = new DiscordEmbedBuilder
        {
            Title = title,
            Description = description,
            Color = DiscordColor.Azure
        };

        embed.WithFooter("Enviado por " + ctx.User.Username, ctx.User.AvatarUrl);

        await ctx.Channel.SendMessageAsync(embed);
    }

    [Command("ticket")]
    public async Task CreateTicket(CommandContext ctx, string itemName)
    {
        var item = _itemService.Items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (item == null)
        {
            await ctx.RespondAsync($"Item '{itemName}' não encontrado. Use !list para ver os itens disponíveis.");
            return;
        }

        try
        {
            var guild = ctx.Guild;
            var ticketChannel = await guild.CreateChannelAsync($"ticket-{ctx.User.Username}", DSharpPlus.ChannelType.Text);

            await ticketChannel.AddOverwriteAsync(ctx.Member, DSharpPlus.Permissions.AccessChannels | DSharpPlus.Permissions.SendMessages);
            await ticketChannel.AddOverwriteAsync(guild.EveryoneRole, DSharpPlus.Permissions.None);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Ticket Criado",
                Description = $"Você escolheu o item: **{item.Name}**.\nPreço: **{item.Price}**.\nUm administrador irá atendê-lo em breve.",
                Color = DiscordColor.Green
            };

            await ticketChannel.SendMessageAsync(embed);
            await ctx.RespondAsync("Seu ticket foi criado!");
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"Erro ao criar o ticket: {ex.Message}");
        }
    }

    [Command("list")]
    public async Task ListItems(CommandContext ctx)
    {
        var embed = new DiscordEmbedBuilder
        {
            Title = "Itens Disponíveis",
            Description = string.Join("\n", _itemService.Items.Select(i => $"{i.Name} - {i.Price} moedas")),
            Color = DiscordColor.Azure
        };

        await ctx.RespondAsync(embed);
    }

    [Command("setchannel")]
    public async Task SetDefaultChannel(CommandContext ctx, DiscordChannel channel)
    {
        if (channel.Guild.Id != ctx.Guild.Id)
        {
            await ctx.RespondAsync("O canal fornecido não pertence ao servidor atual.");
            return;
        }

        _configService.SetDefaultChannelId(channel.Id);
        await ctx.RespondAsync($"Canal padrão configurado para: {channel.Name}");
    }

    [Command("sendmessage")]
    public async Task SendMessageToDefaultChannel(CommandContext ctx, string message)
    {
        var channelId = _configService.DefaultChannelId;
        if (channelId == null)
        {
            await ctx.RespondAsync("Nenhum canal padrão configurado. Use !setchannel para configurar.");
            return;
        }

        var channel = await ctx.Client.GetChannelAsync(channelId.Value);
        if (channel == null)
        {
            await ctx.RespondAsync("O canal configurado não foi encontrado.");
            return;
        }

        await channel.SendMessageAsync(message);
        await ctx.RespondAsync("Mensagem enviada ao canal padrão.");
    }

    [Command("openbuy")]
    public async Task OpenBuy(CommandContext ctx)
    {
        var embed = new DiscordEmbedBuilder
        {
            Title = "Itens Disponíveis para Compra",
            Description = string.Join("\n", _itemService.Items.Select(i => $"{i.Name} - {i.Price} moedas")),
            Color = DiscordColor.Gold
        };

        await ctx.RespondAsync(embed);
    }
}
