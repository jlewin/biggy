using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy;
using Xunit;
using Biggy.JSON;
using System.IO;
using Newtonsoft.Json;

namespace Tests {
  [Trait("JSON","DirectoryDataStore")]
  public class DirectoryDataStoreTests {

    private string dbpath;

    public string KeySelector(Film film)
    {
        return film.Film_ID.ToString();
    }

    public bool ItemExists(List<Film> items, Film film)
    {
        return items.FirstOrDefault(f => f.Film_ID == film.Film_ID) != null;
    }

    public DirectoryDataStoreTests()
    {

        this.dbpath = Path.Combine(Directory.GetCurrentDirectory(), "data");

        var ds = new DirectoryDataStore<Film>(dbpath, null, null, null, false);

        ds.Clear();


    }

    [Fact(DisplayName = "Create")]
    public void Create() {

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented };

        var ds = new DirectoryDataStore<Film>(dbpath, this.KeySelector, this.ItemExists);

        var item = new Film()
        {
            Film_ID = 123,
            Description = "Hello World",
            FullText = "Something",
            Length = 5,
            ReleaseYear = 1975,
            Title = "Hello world"
        };

        ds.Add(item);

        ds.FirstOrDefault<Film>(g => g.Film_ID == 1);

        var temp = new DirectoryDataStore<Film>(dbpath, this.KeySelector);

        Assert.True(temp.Count == 1);

        var matched = temp.FirstOrDefault(f => f.Film_ID == 123);

        Assert.True(matched.Film_ID == item.Film_ID);
        Assert.True(matched.Description == item.Description);
    }


    [Fact(DisplayName = "Prevent Dupes")]
    public void PreventDupes()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented };

        var ds = new DirectoryDataStore<Film>(dbpath, this.KeySelector, this.ItemExists);

        ds.ItemExists = ItemExists;

        var item = new Film()
        {
            Film_ID = 123,
            Description = "Hello World",
            FullText = "Something",
            Length = 5,
            ReleaseYear = 1975,
            Title = "Hello world"
        };

        // Add the item
        ds.Add(item);

        // Attempt to add again
        Assert.Throws<ArgumentException>(() => ds.Add(item));

        Assert.True(ds.Count == 1);
    }

    [Fact(DisplayName = "Update")]
    public void Update()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented };

        var ds = new DirectoryDataStore<Film>(dbpath, this.KeySelector, this.ItemExists);

        ds.ItemExists = ItemExists;

        var item = new Film()
        {
            Film_ID = 123,
            Description = "Hello World",
            FullText = "Something",
            Length = 5,
            ReleaseYear = 1975,
            Title = "Hello world"
        };

        // Add the item
        ds.Add(item);

        string newValue = "Item updated!";

        item.Description = newValue;
        ds.Update(item);

        var temp = new DirectoryDataStore<Film>(dbpath, this.KeySelector, this.ItemExists);
        var matched = temp.FirstOrDefault(f => f.Film_ID == 123);


        // Attempt to add again
        Assert.Equal(matched.Description, newValue);
        Assert.True(ds.Count == 1);
    }

    [Fact(DisplayName = "Delete")]
    public void Delete()
    {
        System.Diagnostics.Debugger.Launch();

        var ds = new DirectoryDataStore<Film>(dbpath, this.KeySelector, this.ItemExists);

        ds.ItemExists = ItemExists;

        var item = new Film()
        {
            Film_ID = 123,
            Description = "Hello World",
            FullText = "Something",
            Length = 5,
            ReleaseYear = 1975,
            Title = "Hello world"
        };

        // Add the item
        ds.Add(item);

        var item2 = new Film()
        {
            Film_ID = 345,
            Description = "Hello World",
            FullText = "Something",
            Length = 5,
            ReleaseYear = 1975,
            Title = "Hello world"
        };

        ds.Add(item2);

        Assert.Equal(ds.Count, 2);

        ds.Remove(item2);

        var temp = new DirectoryDataStore<Film>(dbpath, this.KeySelector, this.ItemExists);
        var matched = temp.FirstOrDefault(f => f.Film_ID == 123);

        Assert.Equal(temp.Count, 1);

        // Attempt to add again
        Assert.Equal(matched.Film_ID, 123);
    }
  }
}
