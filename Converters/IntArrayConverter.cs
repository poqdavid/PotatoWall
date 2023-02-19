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

namespace PotatoWall.Converters;

// <copyright file="IntArrayConverter.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the IntArrayConverter class.</summary>

public class IntArrayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (targetType.FullName, value.GetType().FullName) switch
        {
            ("System.String", "System.Int32[]") => value.CastTo<int[]>().ToCSV(),
            ("System.String", "System.UInt32[]") => value.CastTo<uint[]>().ToCSV(),
            _ => throw new NotImplementedException("Not implemented."),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (targetType.FullName, value.GetType().FullName) switch
        {
            ("System.Int32[]", "System.String") => value.CastTo<string>().CSVToArray(true).Select(x => int.Parse(x)).ToArray(),
            ("System.UInt32[]", "System.String") => value.CastTo<string>().CSVToArray(true).Select(x => uint.Parse(x)).ToArray(),
            _ => throw new NotImplementedException("Not implemented."),
        };
    }
}