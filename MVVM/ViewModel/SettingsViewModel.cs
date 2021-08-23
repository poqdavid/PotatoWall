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

using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PotatoWall.MVVM.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public List<ColorDataList> SomeCollection { get; set; }
        public ColorData IColorData { get; set; } = new ColorData();
        public ICommand SelectionChangedCommand { get; set; }

        public event EventHandler<ComboBoxSelectionChangedEventArgs> SelectionChangedEvent;

        public SettingsViewModel()
        {
            IColorData.Load();
            SelectionChangedCommand = new DelegateCommand((objects) =>
            {
                ComboBoxSelectionChangedEventArgs e = new();
                EventHandler<ComboBoxSelectionChangedEventArgs> raiseEvent = SelectionChangedEvent;

                object[] values = (object[])objects;
                e.SelectedItem = (ColorDataList)values[0];
                e.SelectedIndex = (int)values[1];

                raiseEvent?.Invoke(this, e);
            });
        }
    }

    public class ComboBoxSelectionChangedEventArgs : EventArgs
    {
        public ColorDataList SelectedItem { get; set; }
        public int SelectedIndex { get; set; }
    }
}