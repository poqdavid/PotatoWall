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

// <copyright file="ListBoxExtensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the ListBoxExtensions class.</summary>

public static class ListBoxExtensions
{
    public static void AddItem(this ListBox lb, string text, int index, bool waitUntilReturn = false)
    {
        ListBoxItem lbit = new()
        {
            Content = text,
            Tag = index
        };
        Action additem = () => lb.Items.Add(lbit);
        if (lb.CheckAccess())
        {
            additem();
        }
        else if (waitUntilReturn)
        {
            lb.Dispatcher.Invoke(additem);
        }
        else
        {
            _ = lb.Dispatcher.BeginInvoke(additem);
        }
    }

    public static void RemoveItem(this ListBox lb, bool waitUntilReturn = false)
    {
        Action additem = () => lb.Items.Remove("");
        if (lb.CheckAccess())
        {
            additem();
        }
        else if (waitUntilReturn)
        {
            lb.Dispatcher.Invoke(additem);
        }
        else
        {
            _ = lb.Dispatcher.BeginInvoke(additem);
        }
    }

    public static void AddRange()
    {
    }
}