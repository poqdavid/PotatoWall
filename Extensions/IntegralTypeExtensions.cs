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

// <copyright file="IntegralTypeExtensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the IntegralTypeExtensions class.</summary>

public static class IntegralTypeExtensions
{
    public static ushort SWPOrder(this ushort num)
    {
        return (ushort)IPAddress.HostToNetworkOrder((short)num);
    }

    /// <summary>
    /// Cast's a long to T
    /// </summary>
    /// <typeparam name="T">Type to cast to</typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T CastTo<T>(this long value)
    {
        if (typeof(T) == typeof(double))
        {
            return (T)(object)Convert.ToDouble(value);
        }

        if (typeof(T) == typeof(int))
        {
            return (T)(object)Convert.ToInt32(Math.Round((double)value));
        }

        if (value is T t) return t;

        throw new Exception($"Casting from {value.GetType().FullName} to {typeof(T).FullName} wasn't successful.");
    }

    /// <summary>
    /// Cast's a double to T
    /// </summary>
    /// <typeparam name="T">Type to cast to</typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T CastTo<T>(this double value)
    {
        if (typeof(T) == typeof(Int32))
        {
            return (T)(object)Convert.ToInt32(Math.Round((double)value));
        }

        if (value is T t) return t;

        throw new Exception($"Casting from {value.GetType().FullName} to {typeof(T).FullName} wasn't successful.");
    }

    /// <summary>
    /// Returns a IBaseTheme of the int
    /// </summary>
    /// <param name="value"></param>
    /// <returns>IBaseTheme</returns>
    public static IBaseTheme ToTheme(this int value)
    {
        return value == 0 ? Theme.Dark : Theme.Light;
    }

    /// <summary>
    /// Returns a IBaseTheme of the uint
    /// </summary>
    /// <param name="value"></param>
    /// <returns>IBaseTheme</returns>
    public static IBaseTheme ToTheme(this uint value)
    {
        return value == 0 ? Theme.Dark : Theme.Light;
    }
}