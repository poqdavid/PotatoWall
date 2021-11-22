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

using System.Windows.Input;

namespace PotatoWall.MVVM.ViewModel
{
    public class AddIPViewModel : ViewModelBase
    {
        public ICommand AddIPBlackListCommand { get; }
        public ICommand AddIPWhiteListCommand { get; }
        public ICommand AddIPAutoWhiteListCommand { get; }

        public event EventHandler<EventArgs> AddIPBlackListEvent;

        public event EventHandler<EventArgs> AddIPWhiteListEvent;

        public event EventHandler<EventArgs> AddIPAutoWhiteListEvent;

        private string defaultIP;

        public string IP
        {
            get => defaultIP;
            set => SetProperty(ref defaultIP, value);
        }

        public AddIPViewModel()
        {
            AddIPBlackListCommand = new DelegateCommand(OnAddIPBlackList);
            AddIPWhiteListCommand = new DelegateCommand(OnAddIPWhiteList);
            AddIPAutoWhiteListCommand = new DelegateCommand(OnAddIPAutoWhiteList);
        }

        private void OnAddIPBlackList(object _)
        {
            EventHandler<EventArgs> raiseEvent = AddIPBlackListEvent;
            raiseEvent?.Invoke(this, new EventArgs());
        }

        private void OnAddIPWhiteList(object _)
        {
            EventHandler<EventArgs> raiseEvent = AddIPWhiteListEvent;
            raiseEvent?.Invoke(this, new EventArgs());
        }

        private void OnAddIPAutoWhiteList(object _)
        {
            EventHandler<EventArgs> raiseEvent = AddIPAutoWhiteListEvent;
            raiseEvent?.Invoke(this, new EventArgs());
        }
    }
}