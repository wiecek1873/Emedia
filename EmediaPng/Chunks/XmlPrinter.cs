using System.Xml;

namespace EmediaPng
{
    public class XmpPrinter
    {
        private const string PrefixOfRow = "     | ";
        private const string DeepLevelPrefix = "  ";
        private const string NameValueSeparator = " => ";
        private const string ChildPrefix = "> ";

        public static string ParseAndPrint(string Text)
        {
            XmlDocument xml = ExtractXmp(Text);
            if (xml == null)
                return "Invalid XMP format";
            else
                return PrintXmlRecurrence(xml.DocumentElement, PrefixOfRow);
        }

        private static string PrintXmlRecurrence(XmlNode element, string prefix)
        {
            if (element == null) return "";

            string output = "";
            string newPrefix = prefix + DeepLevelPrefix;
            XmlNodeList children = element.ChildNodes;

            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    output += $"\n{prefix}{ChildPrefix}{children[i].Name}";
                    output += PrintXmlAttributes(element, prefix);
                    output += PrintXmlRecurrence(children[i], newPrefix);
                }
            }
            else
            {
                output += $"{NameValueSeparator}{element.InnerText}";
                output += PrintXmlAttributes(element, prefix);
            }

            return output;
        }

        private static string PrintXmlAttributes(XmlNode element, string prefix)
        {
            if (element == null) return "";
            if (element.Attributes == null) return "";

            string output = "";
            foreach (XmlAttribute att in element.Attributes)
            {
                if (AttributeFilter(att))
                    output += $"\n{prefix}| {att.Name} = {att.Value}";
            }
            return output;
        }

        private static bool AttributeFilter(XmlAttribute att)
        {
            if (att.Name.StartsWith("xmlns:")) return false;
            return true;
        }

        private static XmlDocument ExtractXmp(string asString)
        {
            var start = asString.IndexOf("<x:xmpmeta");
            var end = asString.IndexOf("</x:xmpmeta>") + 12;
            if (start == -1 || end == -1)
                return null;
            var justTheMeta = asString.Substring(start, end - start);
            var returnVal = new XmlDocument();
            returnVal.LoadXml(justTheMeta);
            return returnVal;
        }
    }
}