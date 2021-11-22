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

// <copyright file="RichTextBoxExtensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the RichTextBoxExtensions class.</summary>

public static class RichTextBoxExtensions
{
    public static void CheckAppendText(this RichTextBox richtextBox, Paragraph msg, bool waitUntilReturn = false)
    {
        //Action append = () =>
        Action append = () =>
        {
            richtextBox.Document.CheckAppendText(msg);
        };

        if (richtextBox.CheckAccess())
        {
            append();
        }
        else if (waitUntilReturn)
        {
            richtextBox.Dispatcher.Invoke(append);
        }
        else
        {
            _ = richtextBox.Dispatcher.BeginInvoke(append);
        }
    }

    public static void SetText(this RichTextBox richtextBox, Paragraph msg, bool waitUntilReturn = false)
    {
        //Action append = () =>
        Action append = () =>
        {
            richtextBox.Document.SetText(msg);
        };

        if (richtextBox.CheckAccess())
        {
            append();
        }
        else if (waitUntilReturn)
        {
            richtextBox.Dispatcher.Invoke(append);
        }
        else
        {
            _ = richtextBox.Dispatcher.BeginInvoke(append);
        }
    }
}