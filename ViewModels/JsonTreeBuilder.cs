using System.Text;
using System.Text.Json;

namespace MmLogView.ViewModels;

public static class JsonTreeBuilder
{
    private const string IndentString = "  ";
    
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static (string FormattedText, JsonNodeViewModel RootNode) Build(string jsonString)
    {
        var rootNode = new JsonNodeViewModel { Name = "Root", IsExpanded = true };
        var sb = new StringBuilder();

        try
        {
            using var doc = JsonDocument.Parse(jsonString);
            BuildNode(doc.RootElement, rootNode, sb, 0, isLastItemInParent: true);
        }
        catch
        {
            return (jsonString, rootNode); // Fallback on parse failure
        }

        return (sb.ToString(), rootNode);
    }

    private static void BuildNode(JsonElement element, JsonNodeViewModel node, StringBuilder sb, int indentLevel, bool isLastItemInParent)
    {
        int startPos = sb.Length;
        string indent = new string(' ', indentLevel * 2);
        string childIndent = new string(' ', (indentLevel + 1) * 2);

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                sb.AppendLine("{");
                node.Value = "{ ... }";
                
                var objEnumerator = element.EnumerateObject();
                var objList = objEnumerator.ToList();
                
                for (int i = 0; i < objList.Count; i++)
                {
                    var prop = objList[i];
                    bool isLast = i == objList.Count - 1;
                    
                    sb.Append(childIndent);
                    int childStartPos = sb.Length;
                    sb.Append(JsonSerializer.Serialize(prop.Name, _serializerOptions)).Append(": ");
                    
                    var childNode = new JsonNodeViewModel
                    {
                        Name = prop.Name,
                        Parent = node,
                        TextStart = childStartPos
                    };

                    BuildNode(prop.Value, childNode, sb, indentLevel + 1, isLast);
                    node.Children.Add(childNode);
                }
                
                sb.Append(indent).Append('}');
                if (!isLastItemInParent) sb.Append(',');
                sb.AppendLine();
                break;

            case JsonValueKind.Array:
                sb.AppendLine("[");
                node.Value = "[ ... ]";

                var arrEnumerator = element.EnumerateArray();
                var arrList = arrEnumerator.ToList();

                for (int i = 0; i < arrList.Count; i++)
                {
                    var item = arrList[i];
                    bool isLast = i == arrList.Count - 1;

                    sb.Append(childIndent);
                    int childStartPos = sb.Length;

                    var childNode = new JsonNodeViewModel
                    {
                        Name = $"[{i}]",
                        Parent = node,
                        TextStart = childStartPos
                    };

                    BuildNode(item, childNode, sb, indentLevel + 1, isLast);
                    node.Children.Add(childNode);
                }
                
                sb.Append(indent).Append(']');
                if (!isLastItemInParent) sb.Append(',');
                sb.AppendLine();
                break;

            case JsonValueKind.String:
                var strVal = element.GetString() ?? "";
                var serializedStr = JsonSerializer.Serialize(strVal, _serializerOptions);
                node.Value = serializedStr;
                sb.Append(serializedStr);
                if (!isLastItemInParent) sb.Append(',');
                sb.AppendLine();
                break;

            case JsonValueKind.Null:
                node.Value = "null";
                sb.Append("null");
                if (!isLastItemInParent) sb.Append(',');
                sb.AppendLine();
                break;

            default:
                var rawVal = element.ToString() ?? "";
                node.Value = rawVal;
                sb.Append(rawVal);
                if (!isLastItemInParent) sb.Append(',');
                sb.AppendLine();
                break;
        }

        if (node.TextStart == 0) node.TextStart = startPos;
        node.TextLength = sb.Length - node.TextStart;
        // Trim trailing newline/carriage returns from the node's length representation so selection is tide
        while (node.TextLength > 0 && (sb[node.TextStart + node.TextLength - 1] == '\n' || sb[node.TextStart + node.TextLength - 1] == '\r'))
        {
            node.TextLength--;
        }
    }
}
