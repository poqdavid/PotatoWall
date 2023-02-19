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

// <copyright file="ObjectExtensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the ObjectExtensions class.</summary>

public static class ObjectExtensions
{
    /// <summary>
    /// Cast's a System.Object to T
    /// </summary>
    /// <typeparam name="T">Type to cast to</typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T CastTo<T>(this object value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (value is T t) return t;

        throw new Exception($"Casting from {value.GetType().FullName} to {typeof(T).FullName} wasn't successful.");
    }

    /// <summary>
    /// Cast's a System.Object to T
    /// </summary>
    /// <typeparam name="T">Type to cast to</typeparam>
    /// <param name="value"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    public static T TryCastTo<T>(this object value, out bool success)
    {
        success = true;

        if (value == null)
        {
            success = false;
            return default;
        }

        if (value is T t)
            return t;

        success = false;
        return default;
    }
}