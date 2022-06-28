// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Microsoft.AspNetCore.Authentication.JwtBearer.Tools;

internal sealed record JwtAuthenticationSchemeSettings(string SchemeName, List<string> Audiences, string ClaimsIssuer)
{
    private const string AuthenticationKey = "Authentication";
    private const string DefaultSchemeKey = "DefaultScheme";
    private const string SchemesKey = "Schemes";

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
    };

    public void Save(string filePath)
    {
        using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var config = JsonSerializer.Deserialize<JsonObject>(reader, _jsonSerializerOptions);
        reader.Close();

        var settingsObject = new JsonObject
        {
            [nameof(Audiences)] = new JsonArray(Audiences.Select(aud => JsonValue.Create(aud)).ToArray()),
            [nameof(ClaimsIssuer)] = ClaimsIssuer
        };

        if (config[AuthenticationKey] is JsonObject authentication)
        {
            if (authentication[SchemesKey] is JsonObject schemes)
            {
                // If a scheme with the same name has already been registered, we
                // override with the latest token's options
                schemes[SchemeName] = settingsObject;
            }
            else
            {
                authentication.Add(SchemesKey, new JsonObject
                {
                    [SchemeName] = settingsObject
                });
            }
        }
        else
        {
            config[AuthenticationKey] = new JsonObject
            {
                [SchemesKey] = new JsonObject
                {
                    [SchemeName] = settingsObject
                }
            };
        }

        // Set the DefaultScheme if it has not already been set
        // and only a single scheme has been configured thus far
        if (config[AuthenticationKey][DefaultSchemeKey] is null
            && config[AuthenticationKey][SchemesKey] is JsonObject setSchemes
            && setSchemes.Count == 1)
        {
            config[AuthenticationKey][DefaultSchemeKey] = SchemeName;
        }

        using var writer = new FileStream(filePath, FileMode.Open, FileAccess.Write);
        JsonSerializer.Serialize(writer, config, _jsonSerializerOptions);
    }

    public static void RemoveScheme(string filePath, string name)
    {
        using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var config = JsonSerializer.Deserialize<JsonObject>(reader);
        reader.Close();

        if (config[AuthenticationKey] is JsonObject authentication &&
            authentication[SchemesKey] is JsonObject schemes)
        {
            schemes.Remove(name);
            if (authentication[DefaultSchemeKey] is JsonValue defaultScheme
                && defaultScheme.GetValue<string>() == name)
            {
                authentication.Remove(DefaultSchemeKey);
            }
        }

        using var writer = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(writer, config, _jsonSerializerOptions);
    }
}
