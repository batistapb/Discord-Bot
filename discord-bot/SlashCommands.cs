using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Services; // Adicione esta linha para incluir o namespace correto

public class SlashCommands : ApplicationCommandModule
{
    private readonly ConfigService _configService = new ConfigService();
    private readonly ItemService _itemService = new ItemService();
    private readonly StoreService _storeService = new StoreService();

    [SlashCommand("setchannel", "Configura o canal padrão para envio de mensagens.")]
    public async Task SetDefaultChannel(InteractionContext ctx, [Option("channel", "Canal para configurar como padrão")] DiscordChannel channel)
    {
        if (channel.Guild.Id != ctx.Guild.Id)
        {
            await ctx.CreateResponseAsync("O canal fornecido não pertence ao servidor atual.", true);
            return;
        }

        _configService.SetDefaultChannelId(channel.Id);
        await ctx.CreateResponseAsync($"Canal padrão configurado para: {channel.Name}");
    }

    [SlashCommand("createbuy", "Cria uma nova lista de compra.")]
    public async Task CreateBuy(InteractionContext ctx, [Option("store", "Nome da loja")] string storeName)
    {
        _storeService.AddStore(storeName);
        await ctx.CreateResponseAsync($"Loja '{storeName}' criada com sucesso!");
    }

    [SlashCommand("configbuy", "Configura uma lista de compra existente.")]
    public async Task ConfigBuy(InteractionContext ctx, [Option("store", "Nome da loja")] string storeName, [Option("item", "Nome do item")] string itemName, [Option("price", "Preço do item")] long price)
    {
        _storeService.AddOrUpdateItem(storeName, itemName, (int)price);
        await ctx.CreateResponseAsync($"Item '{itemName}' configurado na loja '{storeName}' com preço {price}.");
    }

    [SlashCommand("sendstore", "Envia a mensagem fixa no canal da loja.")]
    public async Task SendStore(InteractionContext ctx)
    {
        var channelId = _configService.DefaultChannelId;
        if (channelId == null)
        {
            await ctx.CreateResponseAsync("Nenhum canal padrão configurado. Use /setchannel para configurar.", true);
            return;
        }

        var channel = await ctx.Client.GetChannelAsync(channelId.Value);
        if (channel == null)
        {
            await ctx.CreateResponseAsync("O canal configurado não foi encontrado.", true);
            return;
        }

        var embed = new DiscordEmbedBuilder
        {
            Title = "Loja - Itens Disponíveis",
            Description = string.Join("\n", _itemService.Items.Select(i => $"{i.Name} - {i.Price} moedas")),
            Color = DiscordColor.Gold
        };

        var dropdown = new DiscordSelectComponent(
            "item_select",
            "Selecione um item para abrir um ticket",
            _itemService.Items.Select(i => new DiscordSelectComponentOption(i.Name, i.Name)).ToList()
        );

        var messageBuilder = new DiscordMessageBuilder()
            .WithEmbed(embed)
            .AddComponents(dropdown);

        await channel.SendMessageAsync(messageBuilder);
        await ctx.CreateResponseAsync("Mensagem da loja enviada com sucesso!");
    }

    [SlashCommand("display", "Exibe a lista de itens disponíveis de uma loja no canal atual.")]
    public async Task Display(InteractionContext ctx, [Option("store", "Nome da loja")] string storeName)
    {
        if (!_storeService.Stores.ContainsKey(storeName))
        {
            await ctx.CreateResponseAsync($"A loja '{storeName}' não existe. Use /createbuy para criar uma loja.", true);
            return;
        }

        var storeItems = _storeService.Stores[storeName];
        var embed = new DiscordEmbedBuilder
        {
            Title = $"Loja - {storeName}",
            Description = string.Join("\n", storeItems.Select(i => $"{i.Name} - {i.Price} moedas")),
            Color = DiscordColor.Gold
        };

        var dropdown = new DiscordSelectComponent(
            "item_select",
            "Selecione um item para abrir um ticket",
            storeItems.Select(i => new DiscordSelectComponentOption(i.Name, i.Name)).ToList()
        );

        var messageBuilder = new DiscordMessageBuilder()
            .WithEmbed(embed)
            .AddComponents(dropdown);

        await ctx.Channel.SendMessageAsync(messageBuilder);
        await ctx.CreateResponseAsync("Mensagem da loja exibida com sucesso!", true);
    }
}
