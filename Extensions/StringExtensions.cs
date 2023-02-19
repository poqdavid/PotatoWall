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

namespace PotatoWall.Extensions;

// <copyright file="StringExtensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the StringExtensions class.</summary>

public static class StringExtensions
{
    /// <summary>
    /// Returns a new string with whitespaces removed.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string RemoveWhitespace(this string value)
    {
        return value.Replace(" ", string.Empty);
    }

    /// <summary>
    /// Returns a new string with whitespaces removed.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="lowercase">Converts string to lowercase</param>
    /// <returns></returns>
    public static string RemoveWhitespace(this string value, bool lowercase)
    {
        return lowercase switch
        {
            true => value.Replace(" ", string.Empty).ToLower(),
            false => value.Replace(" ", string.Empty),
        };
    }

    /// <summary>
    /// Converts CSV to String Array
    /// </summary>
    /// <param name="value"></param>
    /// <returns>An string Array converted from CSV</returns>
    public static string[] CSVToArray(this string value)
    {
        return value.Split(',');
    }

    /// <summary>
    /// Converts CSV to String Array
    /// </summary>
    /// <param name="value"></param>
    /// <param name="removewhitespace">Removes whitespace</param>
    /// <returns>An string Array converted from CSV</returns>
    public static string[] CSVToArray(this string value, bool removewhitespace)
    {
        return removewhitespace switch
        {
            true => value.RemoveWhitespace().Split(','),
            false => value.Split(','),
        };
    }

    /// <summary>
    /// Converts CSV to String Array
    /// </summary>
    /// <param name="value"></param>
    /// <param name="removewhitespace">Removes whitespace</param>
    /// <param name="lowercase">Converts string to lowercase</param>
    /// <returns>An string Array converted from CSV</returns>
    public static string[] CSVToArray(this string value, bool removewhitespace, bool lowercase)
    {
        return lowercase switch
        {
            true => value.ToLower().CSVToArray(removewhitespace),
            false => value.CSVToArray(removewhitespace),
        };
    }
}