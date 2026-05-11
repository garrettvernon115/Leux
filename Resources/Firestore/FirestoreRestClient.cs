using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Google.Cloud.Firestore;

namespace Leux.Resources.Firestore
{
    public class FirestoreRestClient
    {
        private readonly HttpClient _http;
        private const string Project = "leux-new";
        private const string Base = "https://firestore.googleapis.com/v1/projects/" + Project + "/databases/(default)/documents";

        public FirestoreRestClient(HttpClient http) => _http = http;

        public async Task<Dictionary<string, object>?> GetDocumentAsync(string path, string token)
        {
            using var req = Req(HttpMethod.Get, $"{Base}/{path}", token);
            using var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;
            var doc = JsonSerializer.Deserialize<JsonElement>(await res.Content.ReadAsStringAsync());
            return doc.TryGetProperty("fields", out var f) ? ParseFields(f) : null;
        }

        public async Task<bool> SetDocumentAsync(string path, Dictionary<string, object?> data, string token)
        {
            using var req = Req(HttpMethod.Patch, $"{Base}/{path}", token, new { fields = ToFields(data) });
            using var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> AddDocumentAsync(string collectionPath, Dictionary<string, object?> data, string token)
        {
            using var req = Req(HttpMethod.Post, $"{Base}/{collectionPath}", token, new { fields = ToFields(data) });
            using var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }

        public async Task<List<(string Id, Dictionary<string, object> Fields)>> ListDocumentsAsync(string collectionPath, string token)
        {
            using var req = Req(HttpMethod.Get, $"{Base}/{collectionPath}", token);
            using var res = await _http.SendAsync(req);
            var result = new List<(string, Dictionary<string, object>)>();
            if (!res.IsSuccessStatusCode) return result;
            var root = JsonSerializer.Deserialize<JsonElement>(await res.Content.ReadAsStringAsync());
            if (!root.TryGetProperty("documents", out var docs)) return result;
            foreach (var doc in docs.EnumerateArray())
            {
                var name = doc.GetProperty("name").GetString()!;
                var id = name.Split('/').Last();
                var fields = doc.TryGetProperty("fields", out var f) ? ParseFields(f) : new();
                result.Add((id, fields));
            }
            return result;
        }

        public async Task<bool> ArrayUnionAsync(string docPath, string fieldPath, Dictionary<string, object?> value, string token)
        {
            var body = new
            {
                writes = new[]
                {
                    new
                    {
                        transform = new
                        {
                            document = $"projects/{Project}/databases/(default)/documents/{docPath}",
                            fieldTransforms = new[]
                            {
                                new
                                {
                                    fieldPath,
                                    appendMissingElements = new
                                    {
                                        values = new[] { new { mapValue = new { fields = ToFields(value) } } }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            using var req = Req(HttpMethod.Post, $"{Base}:commit", token, body);
            using var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }

        public async Task<List<(string Id, Dictionary<string, object> Fields)>> RunQueryAsync(
            string parentPath, object structuredQuery, string token)
        {
            using var req = Req(HttpMethod.Post, $"{Base}/{parentPath}:runQuery", token, new { structuredQuery });
            using var res = await _http.SendAsync(req);
            var result = new List<(string, Dictionary<string, object>)>();
            if (!res.IsSuccessStatusCode) return result;
            var arr = JsonSerializer.Deserialize<JsonElement>(await res.Content.ReadAsStringAsync());
            foreach (var item in arr.EnumerateArray())
            {
                if (!item.TryGetProperty("document", out var doc)) continue;
                var name = doc.GetProperty("name").GetString()!;
                var id = name.Split('/').Last();
                var fields = doc.TryGetProperty("fields", out var f) ? ParseFields(f) : new();
                result.Add((id, fields));
            }
            return result;
        }

        private HttpRequestMessage Req(HttpMethod method, string url, string token, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (body != null)
                req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            return req;
        }

        private static Dictionary<string, object> ToFields(Dictionary<string, object?> data)
        {
            var fields = new Dictionary<string, object>();
            foreach (var (k, v) in data)
                fields[k] = ToValue(v);
            return fields;
        }

        private static object ToValue(object? v) => v switch
        {
            null => new { nullValue = "NULL_VALUE" },
            string s => new { stringValue = s },
            bool b => new { booleanValue = b },
            int i => new { integerValue = i.ToString() },
            long l => new { integerValue = l.ToString() },
            double d => new { doubleValue = d },
            float f => new { doubleValue = (double)f },
            DateTime dt => new { timestampValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToString("o") },
            Timestamp ts => new { timestampValue = ts.ToDateTime().ToString("o") },
            _ => new { stringValue = v.ToString() ?? "" }
        };

        internal static Dictionary<string, object> ParseFields(JsonElement fields)
        {
            var d = new Dictionary<string, object>();
            foreach (var p in fields.EnumerateObject())
                d[p.Name] = ParseValue(p.Value);
            return d;
        }

        internal static object ParseValue(JsonElement v)
        {
            if (v.TryGetProperty("stringValue", out var s)) return s.GetString() ?? "";
            if (v.TryGetProperty("integerValue", out var i))
                return i.ValueKind == JsonValueKind.String ? long.Parse(i.GetString()!) : i.GetInt64();
            if (v.TryGetProperty("doubleValue", out var d)) return d.GetDouble();
            if (v.TryGetProperty("booleanValue", out var b)) return b.GetBoolean();
            if (v.TryGetProperty("timestampValue", out var t))
                return DateTime.Parse(t.GetString()!, null, System.Globalization.DateTimeStyles.RoundtripKind);
            if (v.TryGetProperty("nullValue", out _)) return DBNull.Value;
            if (v.TryGetProperty("arrayValue", out var av))
            {
                var list = new List<object>();
                if (av.TryGetProperty("values", out var vals))
                    foreach (var item in vals.EnumerateArray())
                        list.Add(ParseValue(item));
                return list;
            }
            if (v.TryGetProperty("mapValue", out var mv))
                return mv.TryGetProperty("fields", out var mf) ? ParseFields(mf) : new Dictionary<string, object>();
            return "";
        }
    }
}
