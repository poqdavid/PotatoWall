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

namespace PotatoWall.MVVM.Model;

// <copyright file="PotatoParagraph.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the PotatoParagraph class.</summary>
public class PotatoParagraph
{
    [JsonPropertyName("Text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("Background")]
    public Brush Background { get; set; } = null;

    [JsonPropertyName("Foreground")]
    public Brush Foreground { get; set; } = null;

    [JsonPropertyName("Bold")]
    public bool Bold { get; set; } = false;

    [JsonPropertyName("Italic")]
    public bool Italic { get; set; } = false;

    [JsonPropertyName("Underline")]
    public bool Underline { get; set; } = false;

    public PotatoParagraph()
    {
        this.Text = string.Empty;
        this.Background = null;
        this.Foreground = null;
        this.Bold = false;
        this.Italic = false;
        this.Underline = false;
    }

    public PotatoParagraph(string Text, Brush Background, Brush Foreground, bool Bold, bool Italic, bool Underline)
    {
        this.Text = Text;
        this.Background = Background;
        this.Foreground = Foreground;
        this.Bold = Bold;
        this.Italic = Italic;
        this.Underline = Underline;
    }

    public Paragraph ToParagraph()
    {
        Inline linep = new Run(Text);
        if (Background != null)
        {
            linep.Background = Background;
        }

        if (Foreground != null)
        {
            linep.Foreground = Foreground;
        }

        if (Bold)
        {
            linep = new Bold(linep);
        }

        if (Italic)
        {
            linep = new Italic(linep);
        }

        if (Underline)
        {
            linep = new System.Windows.Documents.Underline(linep);
        }

        return new Paragraph(linep);
    }

    public Run ToRun()
    {
        return new(Text + "\r\n")
        {
            Foreground = Foreground,
            Background = Background,
            FontWeight = GetFontWeight(),
            FontStyle = GetFontStyle()
        };
    }

    public FontWeight GetFontWeight()
    {
        if (Bold)
        {
            return FontWeights.Bold;
        }
        else
        {
            return FontWeights.Normal;
        }
    }

    public FontStyle GetFontStyle()
    {
        if (Italic)
        {
            return FontStyles.Italic;
        }
        else
        {
            return FontStyles.Normal;
        }
    }
}