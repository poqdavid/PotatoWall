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

// <copyright file="ParagraphExtentions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the ParagraphExtentions class.</summary>

public static class ParagraphExtentions
{
    public static void Append(this Paragraph paragraph, string value = "", Brush background = null, Brush foreground = null, bool bold = false, bool italic = false, bool underline = false, bool waitUntilReturn = false)
    {
        Action append = () =>
        {
            Inline run = new Run(value);

            if (background != null) { run.Background = background; }
            if (foreground != null) { run.Foreground = foreground; }
            if (bold) { run = new Bold(run); }
            if (italic) { run = new Italic(run); }
            if (underline) { run = new System.Windows.Documents.Underline(run); }

            paragraph.Inlines.Add(run);
        };
        if (paragraph.CheckAccess())
        {
            append();
        }
        else if (waitUntilReturn)
        {
            paragraph.Dispatcher.Invoke(append);
        }
        else
        {
            _ = paragraph.Dispatcher.BeginInvoke(append);
        }
    }
}