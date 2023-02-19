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

namespace PotatoWall.ValidationRules;

// <copyright file="CSVValidationRule.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the CSVValidationRule class.</summary>

public partial class CSVValidationRule : ValidationRule
{
    [GeneratedRegex("^[a-z0-9]+(?:, ?[a-z0-9]+)*$", RegexOptions.Multiline)]
    private static partial Regex CSVStrRegEX();

    [GeneratedRegex("^[0-9]+(?:, ?[0-9]+)*$", RegexOptions.Multiline)]
    private static partial Regex CSVIntRegEX();

    public String TypeName { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return TypeName switch
        {
            "UInt" => CSVIntRegEX().Match(value.CastTo<string>()).Success ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of CSV"),
            "Int" => CSVIntRegEX().Match(value.CastTo<string>()).Success ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of CSV"),
            "Str" => CSVStrRegEX().Match(value.CastTo<string>()).Success ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of CSV"),
            _ => throw new InvalidCastException($"{TypeName} is not supported"),
        };
    }
}