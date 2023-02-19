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

namespace PotatoWall.Utils;

// <copyright file="PotatoTimer.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the PotatoTimer class.</summary>
public class PotatoTimer
{
    public enum TimerStates
    {
        Running,
        Stopped,
        NONE
    }

    private Task timerTask;
    private CancellationTokenSource cancellationTokenSource = new();
    private bool IsCTSDisposed = false;

    public void Start(TimeSpan timeSpan)
    {
        async Task StartTimer()
        {
            try
            {
                PeriodicTimer periodicTimer = new(timeSpan);

                if (IsCTSDisposed)
                {
                    cancellationTokenSource = new();
                }

                while (await periodicTimer.WaitForNextTickAsync(cancellationTokenSource.Token))
                {
                    OnPotatoTimerEvent(new PotatoTimerEventEventArgs { TimerState = TimerStates.Running });
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        timerTask = StartTimer();
    }

    public async void Stop()
    {
        cancellationTokenSource.Cancel();
        await timerTask;
        cancellationTokenSource.Dispose();
        IsCTSDisposed = true;

        OnPotatoTimerEvent(new PotatoTimerEventEventArgs { TimerState = TimerStates.Stopped });
    }

    protected virtual void OnPotatoTimerEvent(PotatoTimerEventEventArgs e)
    {
        PotatoTimerEvent?.Invoke(this, e);
    }

    public event EventHandler<PotatoTimerEventEventArgs> PotatoTimerEvent;
}

public class PotatoTimerEventEventArgs : EventArgs
{
    public TimerStates TimerState { get; set; }
}