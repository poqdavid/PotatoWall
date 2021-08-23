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

namespace PotatoWall
{
    [Flags]
    public enum LogLevel
    {
        Info,
        Error,
        Trace
    }

    // <copyright file="Logger.cs" company="POQDavid">
    // Copyright (c) POQDavid. All rights reserved.
    // </copyright>
    // <author>POQDavid</author>
    // <summary>This is the PotatoWallLogger class.</summary>
    public class PotatoWallLogger
    {
        private readonly NLog.Logger logger;

        public PotatoWallLogger()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void WriteLog(Exception ex, string message, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    Info(ex, message);
                    break;

                case LogLevel.Error:
                    Error(ex, message);
                    break;

                case LogLevel.Trace:
                    Trace(ex, message);
                    break;

                default:
                    break;
            }
        }

        public void WriteLog(string message, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    Info(message);
                    break;

                case LogLevel.Error:
                    Error(message);
                    break;

                case LogLevel.Trace:
                    Trace(message);
                    break;

                default:
                    break;
            }
        }

        private void Trace(Exception ex, string message)
        {
            logger.Trace(ex, message + ":\n" + ex.StackTrace);
        }

        private void Trace(string message)
        {
            logger.Trace(message);
        }

        private void Error(Exception ex, string message)
        {
            logger.Error(ex, message + ":\n" + ex.StackTrace);
        }

        private void Error(string message)
        {
            logger.Error(message);
        }

        private void Info(Exception ex, string message)
        {
            logger.Info(ex, message);
        }

        private void Info(string message)
        {
            logger.Info(message);
        }
    }
}