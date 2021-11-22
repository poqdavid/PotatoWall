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

namespace PotatoWall.Extensions;

// <copyright file="FrameworkElementExtensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the FrameworkElementExtensions class.</summary>

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Cast is redundant", Justification = "The cast is needed")]
public static class FrameworkElementExtensions
{
    public static T GetTemplatedParent<T>(this FrameworkElement o) where T : DependencyObject
    {
        DependencyObject child = o, parent = null;

        while (child != null && (parent = LogicalTreeHelper.GetParent(child)) == null)
        {
            child = VisualTreeHelper.GetParent(child);
        }
        return parent is FrameworkElement frameworkParent ? frameworkParent.TemplatedParent as T : null;
    }
}