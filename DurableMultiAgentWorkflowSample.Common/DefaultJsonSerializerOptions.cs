using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace DurableMultiAgentWorkflowSample.Common;
public static class DefaultJsonSerializerOptions
{
    public static readonly System.Text.Json.JsonSerializerOptions Value = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };
}
