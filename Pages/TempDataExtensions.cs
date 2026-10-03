using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace _10_project_webiste.Pages;

public static class TempDataExtensions
{
    public static void Put<T>(this ITempDataDictionary tempData, string key, T value)
    {
        tempData[key] = System.Text.Json.JsonSerializer.Serialize(value);
    }

    public static T? Get<T>(this ITempDataDictionary tempData, string key)
    {
        return tempData[key] is string value
            ? System.Text.Json.JsonSerializer.Deserialize<T>(value)
            : default;
    }
}