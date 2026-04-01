using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace IosMobileConfigGen;

public static class PlistWriter
{
    public static void Write(Dictionary<string, object> root, string path)
    {
        var doc = new XDocument(
            new XDeclaration(
                "1.0", 
                "utf-8", 
                null),
            new XDocumentType(
                "plist",
                "-//Apple//DTD PLIST 1.0//EN",
                "https://www.apple.com/DTDs/PropertyList-1.0.dtd", 
                null),
            new XElement(
                "plist",
                new XAttribute("version", "1.0"),
                Serialize(root)));

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false,
            NewLineHandling = NewLineHandling.Replace,
        };
        
        using var stream = File.Create(path);
        using var writer = XmlWriter.Create(stream, settings);
        doc.WriteTo(writer);
        writer.Flush();
        stream.WriteByte((byte)'\n');
    }

    private static XElement Serialize(object value) => value switch
    {
        Dictionary<string, object> dict => SerializeDict(dict),
        List<object> list => SerializeArray(list),
        string s => new XElement("string", s),
        int n => new XElement("integer", n),
        bool b => new XElement(b ? "true" : "false"),
        byte[] data => new XElement("data", Convert.ToBase64String(data)),
        _ => throw new NotSupportedException($"Unsupported plist value type: {value.GetType().Name}")
    };

    private static XElement SerializeDict(Dictionary<string, object> dict)
    {
        var element = new XElement("dict");
        // sort keys alphabetically:
        foreach (var key in dict.Keys.Order(StringComparer.Ordinal))
        {
            element.Add(new XElement("key", key));
            element.Add(Serialize(dict[key]));
        }
        return element;
    }

    private static XElement SerializeArray(List<object> list)
    {
        var element = new XElement("array");
        // preserve insertion order for arrays:
        foreach (var item in list)
        {
            element.Add(Serialize(item));
        }
        return element;
    }
}