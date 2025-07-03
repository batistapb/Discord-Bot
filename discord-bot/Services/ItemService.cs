
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class ItemService
{
    public List<Item> Items { get; private set; }

    public ItemService()
    {
        LoadItems();
    }

    private void LoadItems()
    {
        var json = File.ReadAllText("Config.json");
        var config = JsonSerializer.Deserialize<Config>(json);
        Items = config.ItemList;
    }
}

public class Item
{
    public string Name { get; set; }
    public int Price { get; set; }
}

public class Config
{
    public List<Item> ItemList { get; set; }
}