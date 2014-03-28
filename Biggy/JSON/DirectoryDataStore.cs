using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Biggy.JSON
{
    [JsonConverter(typeof(BiggyListSerializer))]
    public class DirectoryDataStore<T> : InMemoryList<T> where T : new()
    {
        public string FileName { get; set; }

        public string FullPath { get; private set; }

        public Func<T, string> GetFileKey { get; set; }

        public Func<List<T>, T, bool> ItemExists { get; set; }

        public DirectoryDataStore(string baseDirectory, Func<T, string> getFileKey = null, Func<List<T>, T, bool> itemExists = null,  string overriddenDBName = null, bool autoLoad = true)
        {
            this.GetFileKey = getFileKey;

            this.ItemExists = itemExists;

            // Ensure the datastore directory exists
            Directory.CreateDirectory(baseDirectory);

            var type = typeof(T);

            // Use or infer dbName
            var name = overriddenDBName ?? Inflector.Inflector.Pluralize(type.Name).ToLower();

            // Resolve the db path
            this.FullPath = Path.Combine(baseDirectory, name);

            // Ensure the db directory exists
            Directory.CreateDirectory(this.FullPath);

            // Load the datastore
            if (autoLoad)
            {
                this.Load();
            }
        }

        public void Load()
        {
            var items = new List<T>();

            foreach (string file in Directory.GetFiles(this.FullPath, "*.json"))
            {
                items.Add(JsonConvert.DeserializeObject<T>(File.ReadAllText(file)));
            }

            _items = items;

            FireLoadedEvents();
        }

        public void Update(T item)
        {
            base.Update(item);
            this.SaveItem(item);
        }

        public override void Add(T item)
        {
            if (ItemExists(_items, item))
            {
                throw new ArgumentException("Item already exists");
            }

            base.Add(item);

            SaveItem(item);
        }

        public override void Clear()
        {
            base.Clear();

            foreach (var file in Directory.GetFiles(this.FullPath, "*.json"))
            {
                File.Delete(file);
            }
        }

        public override bool Remove(T item)
        {
            var removed = base.Remove(item);
            File.Delete(GetFilePath(item));
            return removed;
        }

        private void SaveItem(T item)
        {
            string filePath = GetFilePath(item);

            // Serialize json directly to the output stream
            using (var outstream = new StreamWriter(filePath, false))
            {
                var writer = new JsonTextWriter(outstream);
                var serializer = JsonSerializer.CreateDefault();

                // Serialize the item to disk
                serializer.Serialize(writer, item);
            }
        }

        private string GetFilePath(T item)
        {
            return Path.Combine(this.FullPath, GetFileKey(item)) + ".json";
        }

    }
}
