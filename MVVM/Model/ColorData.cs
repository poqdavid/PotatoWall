/*
 *      This file is part of PotatoWall distribution (https://github.com/poqdavid/PotatoWall or http://poqdavid.github.io/PotatoWall/).
 *  	Copyright (c) 2023 POQDavid
 *      Copyright (c) contributors
 *
 *      PotatoWall is free software: you can redistribute it and/or modify
 *      it under the terms of the GNU General Public License as published by
 *      the Free Software Foundation, either version 3 of the License, or
 *      (at your option) any later version.
 *
 *      PotatoWall is distributed in the hope that it will be useful,
 *      but WITHOUT ANY WARRANTY; without even the implied warranty of
 *      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *      GNU General Public License for more details.
 *
 *      You should have received a copy of the GNU General Public License
 *      along with PotatoWall.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace PotatoWall.MVVM.Model;

// <copyright file="ColorData.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the ColorData class.</summary>
public class ColorDataList
{
    public ColorDataList()
    {
        ColorName = string.Empty;
        ColorMetadata = string.Empty;
        ColorEnabled = true;
    }

    public ColorDataList(string colorname, string colormetadata, bool colorenabled)
    {
        ColorName = colorname;
        ColorMetadata = colormetadata;
        ColorEnabled = colorenabled;
    }

    [JsonPropertyName("ColorName")]
    public string ColorName { get; set; } = string.Empty;

    [JsonPropertyName("ColorMetadata")]
    public string ColorMetadata { get; set; } = string.Empty;

    [JsonPropertyName("ColorEnabled")]
    public bool ColorEnabled { get; set; } = true;
}

public class ColorData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private List<ColorDataList> defaultColorDataList = new() { };

    [JsonPropertyName("ColorDataList")]
    public List<ColorDataList> ColorDataList
    { get => defaultColorDataList; set { defaultColorDataList = value; OnPropertyChanged(); } }

    /// <summary>
    /// Saves the Color data in selected path.
    /// </summary>
    public static void Save()
    {
        if (!Directory.Exists(PotatoWallClient.AppDataPath))
        {
            _ = Directory.CreateDirectory(PotatoWallClient.AppDataPath);
        }

        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        SwatchesProvider swatchesProvider = new();
        List<string> ColorsList = swatchesProvider.Swatches.Select(a => a.Name).ToList();

        ColorData cd = new();

        cd.ColorDataList.Add(new ColorDataList("------------Recommended------------", "SEP", false));
        foreach (string st in ColorsList)
        {
            cd.ColorDataList.Add(new ColorDataList(st, "REC", true));
        }
        cd.ColorDataList.Add(new ColorDataList("------------Recommended------------", "SEP", false));

        cd.ColorDataList.Add(new ColorDataList("------------Others------------", "SEP", false));
        foreach (PropertyInfo c in typeof(Colors).GetProperties())
        {
            Color cx = c.GetValue(null).CastTo<Color>();
            cd.ColorDataList.Add(new ColorDataList(c.Name, cx.ToString(Thread.CurrentThread.CurrentCulture), true));
        }
        cd.ColorDataList.Add(new ColorDataList("------------Others------------", "SEP", false));

        File.WriteAllText(PotatoWallClient.ColorDataPath, JsonSerializer.Serialize(cd, options));
    }

    public void Load()
    {
        try
        {
            string json_string = File.ReadAllText(PotatoWallClient.ColorDataPath);
            if (Json.IsValid(json_string))
            {
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true
                };

                ColorDataList = JsonSerializer.Deserialize<ColorData>(json_string, options).ColorDataList;
            }
            else
            {
                Save();
                Load();
            }
        }
        catch (Exception)
        {
            Save();
            Load();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}