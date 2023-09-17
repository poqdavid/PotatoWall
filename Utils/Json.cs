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

namespace PotatoWall.Utils;

// <copyright file="Json.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the Json class.</summary>
public class Json
{
    public Json()
    {
    }

    /// <summary>
    /// Given the JSON string, validates if it's a correct
    /// JSON string.
    /// </summary>
    /// <param name="json_string">JSON string to validate.</param>
    /// <returns>true or false.</returns>
    internal static bool IsValid(string json_string)
    {
        try
        {
            _ = JsonNode.Parse(json_string);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets property from json string.
    /// </summary>
    internal static string Get_str(string json_string, string key)
    {
        try
        {
            if (IsValid(json_string))
            {
                JsonNode jsonNode = JsonNode.Parse(json_string)!;

                return jsonNode![key].ToJsonString();
            }
            else
            {
                return string.Empty;
            }
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets property from json int.
    /// </summary>
    internal static int Get_int(string json_string, string key)
    {
        try
        {
            if (IsValid(json_string))
            {
                JsonNode jsonNode = JsonNode.Parse(json_string)!;

                return jsonNode![key].GetValue<int>();
            }
            else
            {
                return 0;
            }
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static string GenerateString(object jsonobj)
    {
        JsonSerializerOptions options = new() { WriteIndented = false };

        return JsonSerializer.Serialize(jsonobj, options);
    }

    public static T Read<T>(string filepath, string defaultdata = "{}")
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        try
        {
            string json_string = File.ReadAllText(filepath);
            if (Json.IsValid(json_string))
            {
                return JsonSerializer.Deserialize<T>(json_string, options);
            }
            else
            {
                PotatoWallClient.Logger.Error("Error loading JSON, data is not valid!!");
                return JsonSerializer.Deserialize<T>(defaultdata, options);
            }
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error("Error loading JSON", ex);
            return JsonSerializer.Deserialize<T>(defaultdata, options);
        }
    }

    public static void Write(string filepath, object value)
    {
        JsonSerializerOptions options = new() { WriteIndented = true };

        File.WriteAllText(filepath, JsonSerializer.Serialize(value, options));
    }
}