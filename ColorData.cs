/*
 *      This file is part of PotatoWall distribution (https://github.com/poqdavid/PotatoWall or http://poqdavid.github.io/PotatoWall/).
 *  	Copyright (c) 2016-2020 POQDavid
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

using MaterialDesignColors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Media;

namespace PotatoWall
{
    // <copyright file="ColorData.cs" company="POQDavid">
    // Copyright (c) POQDavid. All rights reserved.
    // </copyright>
    // <author>POQDavid</author>
    // <summary>This is the ColorData class.</summary>
    public class ColorDataList
    {
        public ColorDataList(string colorname, string colormetadata, bool colorenabled)
        {
            ColorName = colorname;
            ColorMetadata = colormetadata;
            ColorEnabled = colorenabled;
        }

        [JsonProperty("ColorName")]
        public string ColorName { get; set; } = "";

        [JsonProperty("ColorMetadata")]
        public string ColorMetadata { get; set; } = "";

        [JsonProperty("ColorEnabled")]
        public bool ColorEnabled { get; set; } = true;
    }

    public class ColorData : INotifyPropertyChanged
    {
        [JsonProperty("ColorDataList")]
        public List<ColorDataList> ColorDataList { get; set; } = new List<ColorDataList>() { };

        /// <summary>
        /// Saves the Color data in selected path.
        /// </summary>
        public static void Save()
        {
            if (!Directory.Exists(PotatoWallClient.AppDataPath))
            {
                _ = Directory.CreateDirectory(PotatoWallClient.AppDataPath);
            }

            JsonSerializerSettings s = new()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace // without this, you end up with duplicates.
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
                Color cx = (Color)c.GetValue(null);
                cd.ColorDataList.Add(new ColorDataList(c.Name, cx.ToString(Thread.CurrentThread.CurrentCulture), true));
            }
            cd.ColorDataList.Add(new ColorDataList("------------Others------------", "SEP", false));

            File.WriteAllText(PotatoWallUI.ColorDataPath, JsonConvert.SerializeObject(cd, Formatting.Indented, s));
        }

        public void Load()
        {
            try
            {
                string json_string = File.ReadAllText(PotatoWallUI.ColorDataPath);
                if (Json.IsValid(json_string))
                {
                    JsonSerializerSettings s = new()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ObjectCreationHandling = ObjectCreationHandling.Replace // without this, you end up with duplicates.
                    };

                    PotatoWallUI.IColorData = JsonConvert.DeserializeObject<ColorData>(json_string, s);
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

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies objects registered to receive this event that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}