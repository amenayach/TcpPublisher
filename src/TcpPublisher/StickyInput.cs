namespace System.Windows.Forms
{
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Linq;

    public static class StickyInput
    {

        private const string Filename = "appsettings.json";

        public static void Stick(this Form form, params Control[] controls)
        {
            if (form != null && (controls?.Any() ?? false))
            {
                form.Load += (sender, e) =>
                {
                    var json = GetAppSettings();

                    var formEntry = (json[form.Name] as JObject) ?? new JObject();

                    foreach (var control in controls)
                    {
                        control.Text = formEntry[control.Name]?.ToString();
                    }
                };

                form.FormClosing += (sender, e) =>
                {
                    var formEntry = new JObject();

                    foreach (var control in controls)
                    {
                        formEntry[control.Name] = control.Text;
                    }

                    SaveAppSettings(form.Name, formEntry);
                };
            }
        }

        private static JObject GetAppSettings()
        {
            var jsonFilename = Path.Combine(Directory.GetCurrentDirectory(), Filename);

            if (!File.Exists(jsonFilename))
            {
                return new JObject();
            }

            return JObject.Parse(File.ReadAllText(jsonFilename));
        }

        private static void SaveAppSettings(string entry, JObject json)
        {
            var jsonSettings = GetAppSettings();

            jsonSettings[entry] = json;

            var jsonFilename = Path.Combine(Directory.GetCurrentDirectory(), Filename);

            File.WriteAllText(jsonFilename, jsonSettings.ToString());
        }
    }
}
