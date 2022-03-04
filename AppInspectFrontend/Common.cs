using System.Reflection;
using AppInspectFrontend.Resources;
using System.Text.Json;
using AppInspectDataModels;
using System.Web;

namespace AppInspectFrontend
{
    public static class Common
    {
        public static string GetAPKSignatureText(AppFileRecord appFileRecord)
        {
            string fingerprintsText = string.Empty;

            if (appFileRecord.SignatureVerified != null)
                fingerprintsText += "Verified: " + (appFileRecord.SignatureVerified.Value? "Yes" : "No") + Environment.NewLine + Environment.NewLine;

            if (!string.IsNullOrEmpty(appFileRecord.SignedByCertificate))
                fingerprintsText += appFileRecord.SignedByCertificate;

            return fingerprintsText;
        }

        public static List<string> GetAppIdsFromUrlMultilineText(string text)
        {
            var appIdList = new List<string>();

            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Replace("\r", string.Empty));

            foreach (var line in lines)
            {
                string? idInUrl = Common.GetAppIdFromGooglePlayLink(line);
                if (!string.IsNullOrEmpty(idInUrl))
                {
                    appIdList.Add(idInUrl);
                }
                else
                {
                    var appId = line.Replace("\t", string.Empty).Trim();
                    if (FormattingUtility.IsAppIdValid(appId))
                        appIdList.Add(appId);
                }
            }

            return appIdList;
        }

        public static string? GetAppIdFromGooglePlayLink(string url)
        {
            Uri? resultUri;

            if (!Uri.TryCreate(url, UriKind.Absolute, out resultUri))
                return null;

            if (resultUri == null)
                return null;

            if (resultUri.Host.ToLower() != "play.google.com")
                return null;

            if (string.IsNullOrEmpty(resultUri.Query))
                return null;

            var queryDictionary = HttpUtility.ParseQueryString(resultUri.Query);

            if (queryDictionary == null)
                return null;

            if (!queryDictionary.AllKeys.Contains("id"))
                return null;

            var appId = queryDictionary["id"];

            if (string.IsNullOrEmpty(appId))
                return null;

            if (!FormattingUtility.IsAppIdValid(appId))
                return null;

            return appId;
        }

        public static string EncodeBase64(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string? DecodeBase64(string base64EncodedData)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch { }

            return null;
        }

        public static string[] ConvertJsonElementsToKeyValueLines(string argumentsInJson)
        {
            var attributes = new List<string>();
            var args = JsonDocument.Parse(argumentsInJson);

            foreach (var arg in args.RootElement.EnumerateObject())
            {
                string argumentName = arg.Name;
                string value = arg.Value.ToString();

                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    try
                    {
                        var nestedValues = ConvertJsonElementsToKeyValueLines(value);

                        if (nestedValues.Any())
                        {
                            attributes.AddRange(nestedValues);
                            continue;
                        }
                    }
                    catch { }
                }

                attributes.Add($"{argumentName}: {value}");
            }

            return attributes.ToArray();
        }

        public static string? GetPrettifiedAppStoreQueryTaskArguments(AppStoreQueryTaskArguments? taskArguments)
        {
            if (taskArguments != null)
            {
                if (taskArguments.QueryInJson != null)
                {
                    var storeArguments = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(taskArguments.QueryInJson);
                    if (storeArguments != null)
                    {
                        string argumentsInLines = $"method: {taskArguments.QueryMethod!}";
                        foreach (var key in storeArguments.Keys)
                        {
                            argumentsInLines += $"{Environment.NewLine}{key}: {storeArguments[key].ToString()}";
                        }
                        argumentsInLines += $"{Environment.NewLine}automatic_apk_retrieval: {taskArguments.AutomaticAPKRetrieval}";

                        return argumentsInLines;
                    }
                }
            }

            return null;
        }

        public static Stream? GetEmbeddedResourceStream(string ResourceId)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(ResourceId);
        }

        public static string? GetEmbeddedResourceText(string ResourceId)
        {
            Stream? stream = GetEmbeddedResourceStream(ResourceId);
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            return null;
        }

        public static List<CountryCodeEntry> GetCountryCodes()
        {
            var countryCodeJsonString = GetEmbeddedResourceText("AppInspectFrontend.Resources.CountryCodes_ISO3166-1.json");
            if (countryCodeJsonString != null)
            {
                var countryCodeList = JsonSerializer.Deserialize<List<CountryCodeEntry>>(countryCodeJsonString);
                if (countryCodeList != null)
                    return countryCodeList.OrderBy(cc => cc.Name).ToList();
            }
            return new List<CountryCodeEntry>() { new CountryCodeEntry() { Code = "US", Name = "United States" } };
        }

        public static List<LanguageCodeEntry> GetLanguageCodes()
        {
            var languageCodeJsonString = GetEmbeddedResourceText("AppInspectFrontend.Resources.LanguageCodes_ISO639-1.json");
            if (languageCodeJsonString != null)
            {
                var languageCodeList = JsonSerializer.Deserialize<List<LanguageCodeEntry>>(languageCodeJsonString);
                if (languageCodeList != null)
                    return languageCodeList.OrderBy(cc => cc.Name).ToList();
            }
            return new List<LanguageCodeEntry>() { new LanguageCodeEntry() { Code = "en", Name = "English" } };
        }
    }
}
