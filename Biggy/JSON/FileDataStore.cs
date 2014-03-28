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
    public class FileDataStore<T> : InMemoryList<T> where T : new()
    {
        public string FileName { get; set; }

        public string FullPath { get; private set; }

        public FileDataStore(string baseDirectory, string overriddenDBName = null, bool autoLoad = true)
        {
            // Ensure the datastore directory exists
            Directory.CreateDirectory(baseDirectory);

            var type = typeof(T);

            // Use or infer dbName
            var name = overriddenDBName ?? Inflector.Inflector.Pluralize(type.Name).ToLower();

            // Resolve the db path
            this.FullPath = Path.Combine(baseDirectory, name);

            // Load the datastore
            if (autoLoad)
            {
                this.Load();
            }
        }

        public void Load()
        {
            var items = new List<T>();
            if (File.Exists(this.FullPath))
            {
                //format for the deserializer...
                var json = "[" + File.ReadAllText(this.FullPath).Replace(Environment.NewLine, ",") + "]";
                items = JsonConvert.DeserializeObject<List<T>>(json);
            }

            _items = items;

            FireLoadedEvents();
        }

        public void Update(T item)
        {
            base.Update(item);
            this.Save();
        }

        public override void Add(T item)
        {
            var json = JsonConvert.SerializeObject(item);

            //append the to the file
            using (var writer = File.AppendText(this.FullPath))
            {
                writer.WriteLine(json);
            }
            base.Add(item);
        }

        public override void Clear()
        {
            base.Clear();
            this.Save();
        }

        public override bool Remove(T item)
        {
            var removed = base.Remove(item);
            this.Save();
            return removed;
        }

        public bool Save()
        {
            var completed = false;

            // Serialize json directly to the output stream
            using (var outstream = new StreamWriter(this.FullPath))
            {
                var writer = new JsonTextWriter(outstream);
                var serializer = JsonSerializer.CreateDefault();

                // Invoke custom serialization in BiggyListSerializer
                serializer.Serialize(writer, this);

                completed = true;
            }

            return completed;

        }

    }
}
