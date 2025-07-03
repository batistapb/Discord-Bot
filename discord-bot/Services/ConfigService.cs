using System.IO;
using System.Text.Json;

public class ConfigService
{
    private const string ConfigFilePath = "Config.json";
    private BotConfig _config;

    public ConfigService()
    {
        LoadConfig();
    }

    public ulong? DefaultChannelId => _config.DefaultChannelId;

    public void SetDefaultChannelId(ulong channelId)
    {
        _config.DefaultChannelId = channelId;
        SaveConfig();
    }

    private void LoadConfig()
    {
        try
        {
            var json = File.ReadAllText(ConfigFilePath);
            _config = JsonSerializer.Deserialize<BotConfig>(json) ?? new BotConfig();
        }
        catch
        {
            _config = new Config();
        }
    }

    private void SaveConfig()
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigFilePath, json);
    }
}

public class BotConfig
{
    public List<Item> Items { get; set; } = new List<Item>();
    public ulong? DefaultChannelId { get; set; }

  public static implicit operator BotConfig(Config v)
  {
    throw new NotImplementedException();
  }
}
