namespace DiscordBot.Services // Certifique-se de que o namespace está correto
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    public class StoreService
    {
        private const string StoreFilePath = "Stores.json";
        public Dictionary<string, List<Item>> Stores { get; private set; }

        public StoreService()
        {
            LoadStores();
        }

        public void AddStore(string storeName)
        {
            if (!Stores.ContainsKey(storeName))
            {
                Stores[storeName] = new List<Item>();
                SaveStores();
            }
        }

        public void AddOrUpdateItem(string storeName, string itemName, int price)
        {
            if (!Stores.ContainsKey(storeName))
                return;

            var store = Stores[storeName];
            var existingItem = store.Find(i => i.Name.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));

            if (existingItem != null)
            {
                existingItem.Price = price;
            }
            else
            {
                store.Add(new Item { Name = itemName, Price = price });
            }

            SaveStores();
        }

        private void LoadStores()
        {
            try
            {
                var json = File.ReadAllText(StoreFilePath);
                Stores = JsonSerializer.Deserialize<Dictionary<string, List<Item>>>(json) ?? new Dictionary<string, List<Item>>();
            }
            catch
            {
                Stores = new Dictionary<string, List<Item>>();
            }
        }

        private void SaveStores()
        {
            var json = JsonSerializer.Serialize(Stores, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StoreFilePath, json);
        }
    }
}
