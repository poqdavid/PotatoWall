/*
 *      This file is part of PotatoWall distribution (https://github.com/poqdavid/PotatoWall or http://poqdavid.github.io/PotatoWall/).
 *  	Copyright (c) 2021 POQDavid
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace PotatoWall
{
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
                _ = JToken.Parse(json_string);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets property from json string.
        /// </summary>
        internal static string Get_str(string json_string, string key)
        {
            string temp = "";
            try
            {
                if (IsValid(json_string))
                {
                    JToken token = JObject.Parse(json_string);
                    token = token[key];

                    temp = token.ToString();
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }
            return temp;
        }

        /// <summary>
        /// Gets property from json int.
        /// </summary>
        internal static int Get_int(string json_string, string key)
        {
            int temp = 0;
            try
            {
                if (IsValid(json_string))
                {
                    JToken token = JObject.Parse(json_string);
                    token = token[key];

                    temp = int.Parse(token.ToString(), Thread.CurrentThread.CurrentCulture);
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }
            return temp;
        }

        public static string GenerateString(object jsonobj)
        {
            JsonSerializerSettings s = new()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace // without this, you end up with duplicates.
            };

            return JsonConvert.SerializeObject(jsonobj, Formatting.None, s);
        }
    }
}