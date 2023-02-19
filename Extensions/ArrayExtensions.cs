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

// <copyright file="ArrayExtensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the ArrayExtensions class.</summary>

public static class ArrayExtensions
{
    /// <summary>
    /// Returns a CSV string of the int array
    /// </summary>
    /// <param name="array"></param>
    /// <returns>CSV string</returns>
    public static string ToCSV(this int[] array)
    {
        return String.Join(", ", array);
    }

    /// <summary>
    /// Returns a CSV string of the uint array
    /// </summary>
    /// <param name="array"></param>
    /// <returns>CSV string</returns>
    public static string ToCSV(this uint[] array)
    {
        return String.Join(", ", array);
    }

    /// <summary>
    /// Returns a CSV string of the int array
    /// </summary>
    /// <param name="array"></param>
    /// <param name="withoutspace">CSV without whitespace</param>
    /// <returns>CSV string</returns>
    public static string ToCSV(this int[] array, bool withoutspace)
    {
        return withoutspace switch
        {
            true => String.Join(",", array).RemoveWhitespace(),
            false => String.Join(", ", array),
        };
    }
}