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

// <copyright file="RegistryHelper.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the RegistryHelper class.</summary>

public static partial class RegistryHelper
{
    private const string Advapi32 = "advapi32.dll";

    public static readonly IntPtr HKEY_LOCAL_MACHINE = new(unchecked((int)0x80000002));
    public static readonly IntPtr HKEY_CURRENT_USER = new(unchecked((int)0x80000001));

    private const int KEY_READ = 0x20019;
    private const uint REG_DWORD = 4;

    [LibraryImport(Advapi32, EntryPoint = "RegOpenKeyExW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired, out IntPtr phkResult);

    [LibraryImport(Advapi32, EntryPoint = "RegQueryValueExW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, out uint lpType, byte[] lpData, ref uint lpcbData);

    [LibraryImport(Advapi32, SetLastError = true)]
    private static partial int RegCloseKey(IntPtr hKey);

    public static uint GetValue(string registryPath, string valueName)
    {
        return GetValue(HKEY_CURRENT_USER, registryPath, valueName);
    }

    public static uint GetValue(IntPtr hiveKey, string registryPath, string valueName)
    {
        uint regDwordValue = 0;

        int result = RegOpenKeyEx(hiveKey, registryPath, 0, KEY_READ, out IntPtr hKey);

        if (result != 0)
        {
            PotatoWallClient.Logger.Error($"Failed to open registry key. Error code: {result}");
        }

        byte[] lpData = new byte[4]; // DWORD size
        uint lpcbData = (uint)lpData.Length;

        result = RegQueryValueEx(hKey, valueName, 0, out uint lpType, lpData, ref lpcbData);

        if (result != 0 || lpType != REG_DWORD)
        {
            PotatoWallClient.Logger.Error($"Failed to read REG_DWORD value. Error code: {result}");
        }

        regDwordValue = BitConverter.ToUInt32(lpData, 0);

        RegCloseKey(hKey);

        return regDwordValue;
    }
}