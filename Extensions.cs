/*
 *      This file is part of PotatoWall distribution (https://github.com/poqdavid/PotatoWall or http://poqdavid.github.io/PotatoWall/).
 *  	Copyright (c) 2016-2020 POQDavid
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

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

// <copyright file="Extensions.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the Extensions class.</summary>
namespace PotatoWall
{
    public static class IntegralTypeExtensions
    {
        public static ushort SWPOrder(this ushort num)
        {
            return (ushort)IPAddress.HostToNetworkOrder((short)num);
        }
    }

    public static class ListBoxExtension
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
    }

    public static class WindowExtensions
    {
        public static void SetTitle(this Window winx, string text, bool waitUntilReturn = false)
        {
            Action settitle = () => winx.Title = "SAPP Remote" + text;
            if (winx.CheckAccess())
            {
                settitle();
            }
            else if (waitUntilReturn)
            {
                winx.Dispatcher.Invoke(settitle);
            }
            else
            {
                _ = winx.Dispatcher.BeginInvoke(settitle);
            }
        }
    }

    public static class ListBoxExtention
    {
        public static void AddRange()
        {
        }
    }

    public static class ParagraphExtention
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
                if (underline) { run = new Underline(run); }

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

    public static class FlowDocumentExtensions
    {
        public static void CheckAppendText(this FlowDocument fDoc, Paragraph msg, bool waitUntilReturn = false)
        {
            Action append = () =>
            {
                fDoc.Blocks.Add(msg);
            };
            if (fDoc.CheckAccess())
            {
                append();
            }
            else if (waitUntilReturn)
            {
                fDoc.Dispatcher.Invoke(append);
            }
            else
            {
                _ = fDoc.Dispatcher.BeginInvoke(append);
            }
        }

        public static void SetText(this FlowDocument fDoc, Paragraph msg, bool waitUntilReturn = false)
        {
            Action append = () =>
            {
                fDoc.Blocks.Clear();
                fDoc.Blocks.Add(msg);
            };
            if (fDoc.CheckAccess())
            {
                append();
            }
            else if (waitUntilReturn)
            {
                fDoc.Dispatcher.Invoke(append);
            }
            else
            {
                _ = fDoc.Dispatcher.BeginInvoke(append);
            }
        }
    }

    /*    public static class TextBoxExtensions
        {
            public static void CheckAppendText(this TextBoxBase textBox, string msg, bool waitUntilReturn = false)
            {
                Action append = () => textBox.AppendText(msg);
                if (textBox.CheckAccess())
                {
                    append();
                }
                else if (waitUntilReturn)
                {
                    textBox.Dispatcher.Invoke(append);
                }
                else
                {
                    textBox.Dispatcher.BeginInvoke(append);
                }
            }
        }*/

    public static class TextBlockExtensions
    {
        public static void SetText(this TextBlock textBlock, string text, bool waitUntilReturn = false)
        {
            Action append = () => textBlock.Text = text;
            if (textBlock.CheckAccess())
            {
                append();
            }
            else if (waitUntilReturn)
            {
                textBlock.Dispatcher.Invoke(append);
            }
            else
            {
                _ = textBlock.Dispatcher.BeginInvoke(append);
            }
        }
    }

    public static class DispatcherExtensions
    {
        public static void InvokeOrExecute(this Dispatcher dispatcher, Action action)
        {
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _ = dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
        }
    }

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
}