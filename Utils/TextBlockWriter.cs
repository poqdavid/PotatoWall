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

using PotatoWall.MVVM.Model;

namespace PotatoWall.Utils;

internal class TextBlockWriter : TextWriter
{
    private readonly TextBlock textBlock;
    private readonly ScrollViewer scrollViewer;
    private String content = "";

    public TextBlockWriter(TextBlock textBlock, ScrollViewer scrollViewer)
    {
        this.textBlock = textBlock;
        this.scrollViewer = scrollViewer;
    }

    public override void Write(char value)
    {
        base.Write(value);
        content += value;

        if (value == '\n')
        {
            content = content.TrimEnd('\r', '\n');
            Action append = () =>
            {
                Brush br1 = Brushes.Green;

                string stout = "";
                string msg = "";

                if (content.Contains("<msgplaceholder>"))
                {
                    string[] data = content.Split("<msgplaceholder>");
                    msg = data[1];
                }
                else
                {
                    br1 = Brushes.Yellow;
                    msg = content;
                }

                if (content.ToLower().Contains(" [dbg] ") || content.ToLower().Contains(" [debug] ")) { br1 = Brushes.Cyan; }
                if (content.ToLower().Contains(" [wrn] ") || content.ToLower().Contains(" [warning] ")) { br1 = Brushes.Yellow; }
                if (content.ToLower().Contains(" [err] ") || content.ToLower().Contains(" [error] ")) { br1 = Brushes.Red; }

                if (!String.IsNullOrEmpty(msg) || !string.IsNullOrWhiteSpace(msg))
                {
                    stout = content.Replace("<msgplaceholder>", "");
                }

                PotatoParagraph pp = new(stout, Brushes.Transparent, br1, true, false, false);
                textBlock.Inlines.Add(pp.ToRun());
                scrollViewer.ScrollToEnd();
                content = "";
            };

            textBlock.Dispatcher.Invoke(DispatcherPriority.Normal, append);
        }
    }

    public override Encoding Encoding
    {
        get { return Encoding.UTF8; }
    }
}