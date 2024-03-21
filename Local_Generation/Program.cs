using System.Text.Encodings.Web;
using System.Text.Json;

using Colossal;
using Tree_Controller;
using Tree_Controller.Settings;

namespace I18nEveryWhere.LocaleGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var setting = new TreeControllerSettings(new TreeControllerMod());
            var locale = new LocaleEN(setting);
            var e = new Dictionary<string, string>(
                locale.ReadEntries(new List<IDictionaryEntryError>(), new Dictionary<string, int>()));
            var str = JsonSerializer.Serialize(e, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText("C:\\Users\\TJ\\source\\repos\\Tree_Controller\\Tree_Controller\\lang\\en-US.json", str);
        }
    }
}