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

using Newtonsoft.Json;
using System.Windows.Documents;
using System.Windows.Media;

namespace PotatoWall
{
    // <copyright file="PotatoParagraph.cs" company="POQDavid">
    // Copyright (c) POQDavid. All rights reserved.
    // </copyright>
    // <author>POQDavid</author>
    // <summary>This is the PotatoParagraph class.</summary>
    public class PotatoParagraph
    {
        [JsonProperty("Text")]
        public string Text { get; set; } = "";

        [JsonProperty("Background")]
        public Brush Background { get; set; } = null;

        [JsonProperty("Foreground")]
        public Brush Foreground { get; set; } = null;

        [JsonProperty("Bold")]
        public bool Bold { get; set; } = false;

        [JsonProperty("Italic")]
        public bool Italic { get; set; } = false;

        [JsonProperty("Underline")]
        public bool Underline { get; set; } = false;

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
                linep = new Underline(linep);
            }

            return new Paragraph(linep);
        }
    }
}