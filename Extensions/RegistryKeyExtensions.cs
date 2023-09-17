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

// <copyright file="RegistryKeyExtensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the RegistryKeyExtensions class.</summary>

public static class RegistryKeyExtensions
{
    /// <summary>
    /// Cast's Registry value to T
    /// </summary>
    /// <typeparam name="T">Type to cast to</typeparam>
    /// <param name="regKey"></param>
    /// <param name="name"></param>
    /// <param name="defaultvalue"></param>
    /// <param name="registryValueOptions"></param>
    /// <returns></returns>
    public static T GetValue<T>(this RegistryKey regKey, string name, object defaultvalue, RegistryValueOptions registryValueOptions)
    {
        object tempout = regKey.GetValue(name, defaultvalue, registryValueOptions);

        if (tempout.GetType() == typeof(int))
        {
            return (T)Convert.ChangeType(tempout, typeof(T));
        }
        else
        {
            return (T)tempout;
        }
    }

    /// <summary>
    /// Cast's Registry value to T
    /// </summary>
    /// <typeparam name="T">Type to cast to</typeparam>
    /// <param name="regKey"></param>
    /// <param name="name"></param>
    /// <param name="defaultvalue"></param>
    /// <returns></returns>
    public static T GetValue<T>(this RegistryKey regKey, string name, object defaultvalue)
    {
        return GetValue<T>(regKey, name, defaultvalue, RegistryValueOptions.None);
    }

    /// <summary>
    /// Cast's Registry value to uint
    /// </summary>
    /// <param name="regKey"></param>
    /// <param name="name"></param>
    /// <param name="defaultvalue"></param>
    /// <param name="registryValueOptions"></param>
    /// <returns>uint</returns>
    public static uint GetValueUint(this RegistryKey regKey, string name, object defaultvalue)
    {
        return (uint)regKey.GetValue<int>(name, defaultvalue);
    }
}