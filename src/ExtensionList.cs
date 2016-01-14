using System.Collections.Generic;

namespace WebExtensionPack
{
    class ExtensionList
    {
        public static IDictionary<string, string> Products()
        {
            return new Dictionary<string, string> {
                { "5fb7364d-2e8c-44a4-95eb-2a382e30fec9", "Web Essentials" },
                { "148ffa77-d70a-407f-892b-9ee542346862", "Web Compiler"},
                { "36bf2130-106e-40f2-89ff-a2bdac6be879", "Web Analyzer"},
                { "bf95754f-93d3-42ff-bfe3-e05d23188b08", "Image optimizer"},
                { "950d05f7-bb25-43ce-b682-44b377b5307d", "Glyphfriend"},
                { "6ed6c371-5815-407f-9148-f64b3a025dd9", "Bootstrap Snippet Pack"},
                { "f4ab1e64-5d35-4f06-bad9-bf414f4b3bbb", "Open Command Line"},
                { "fdd64809-376e-4542-92ce-808a8df06bcc", "Package Installer"},
                { "d7f89ba3-815c-4feb-89b9-68d1654e2138", "NPM Scripts Task Runner"},
                { "10d9b3af-1338-4c45-bc99-4ec38c3a11fb", "Browser Sync"},
                { "2a20580c-7be4-4440-bcd6-8dcf5aa2004e", "JavaScript Snippet Pack" },
                { "a7dff10f-3592-429c-9dc1-622fe517921d", "React Snippet Pack" },
            };
        }
    }
}
