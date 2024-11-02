using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class JsonSections
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonSections(string filePath)
    {
        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public void AddSection<T>(string sectionName, T sectionData)
    {
        // Load existing data
        Dictionary<string, JsonElement> rootElement = new Dictionary<string, JsonElement>();

        if (File.Exists(_filePath))
        {
            // Read the existing file content as a string
            string jsonString = File.ReadAllText(_filePath);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                rootElement = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString, _jsonOptions);
            }
        }

        // Serialize the section data to a JsonElement
        var jsonData = JsonSerializer.Serialize(sectionData, _jsonOptions);
        rootElement[sectionName] = JsonDocument.Parse(jsonData).RootElement;

        // Write back to the file
        using (var writer = new StreamWriter(_filePath, false))
        {
            JsonSerializer.Serialize(writer.BaseStream, rootElement, _jsonOptions);
        }
    }

    public T ReadSection<T>(string sectionName)
    {
        using (var stream = File.OpenRead(_filePath))
        {
            using (var document = JsonDocument.Parse(stream))
            {
                var root = document.RootElement;
                if (root.TryGetProperty(sectionName, out JsonElement section))
                {
                    return JsonSerializer.Deserialize<T>(section.GetRawText());
                }
                throw new KeyNotFoundException($"Section '{sectionName}' not found in the configuration file.");
            }
        }
    }
}
