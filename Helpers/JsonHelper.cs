using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace CSharpWpfShazam.Helpers
{
    public static class JsonHelper
    {
        public static string SerializeObjectAsJsonString(object objectToSerialize)
        {
            string jsonString = JsonSerializer.Serialize(objectToSerialize,
                        options: new JsonSerializerOptions
                        {
                            // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-character-encoding
                            // Could use specific languages with multiples of UnicodeRanges
                            // Without UnicodeRanges.All, song title could look like:
                            // "\u4F60\u662F\u6211\u6700\u7231\u7684\u5973\u4EBA",
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            WriteIndented = true,
                        });
            return jsonString;
        }

        public static void SaveAsJsonToFile(object objectToSerialize, string filePath)
        {
            string jsonString = SerializeObjectAsJsonString(objectToSerialize);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                File.WriteAllText(filePath, jsonString);
            }
        }

        public static T? DeserializeToClass<T>(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return default(T);

            T? @class = JsonSerializer.Deserialize<T>(jsonString);
            return @class;
        }

        public static T? DeserializeFromFile<T>(string jsonfilePath)
        {
            string jsonString = File.ReadAllText(jsonfilePath);
            if (string.IsNullOrWhiteSpace(jsonString))
                return default(T);

            T? @class = JsonSerializer.Deserialize<T>(jsonString);
            return @class;
        }
    }
}
