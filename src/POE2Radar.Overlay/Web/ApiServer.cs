using System.Net;
using System.Text;
using System.Text.Json;
using POE2Radar.Core.Game;

namespace POE2Radar.Overlay.Web;

/// <summary>
/// Tiny read-only HTTP API for live troubleshooting (the PoE2 stand-in for POEMCP). Serves the
/// latest <see cref="RadarState"/> published by <see cref="RadarApp"/> each world tick.
///
/// Endpoints (localhost:7777):
///   GET /state                    — player, area, map visibility, entity counts by category
///   GET /entities                 — all entities (id, category, metadata, pos, hp, dist)
///       ?category=Monster         — filter by category (case-insensitive)
///       &amp;alive=true               — only entities with HP &gt; 0
///       &amp;radius=80                — only within N grid units of the player
///       &amp;limit=50                 — cap results (default 500)
/// </summary>
public sealed class ApiServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly Func<RadarState> _state;
    private volatile bool _running;

    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ApiServer(Func<RadarState> state, int port = 7777)
    {
        _state = state;
        _listener.Prefixes.Add($"http://localhost:{port}/");
    }

    public void Start()
    {
        _listener.Start();
        _running = true;
        var t = new Thread(Loop) { IsBackground = true, Name = "POE2Radar.Api" };
        t.Start();
    }

    private void Loop()
    {
        while (_running)
        {
            HttpListenerContext ctx;
            try { ctx = _listener.GetContext(); }
            catch { return; } // listener stopped
            try { Handle(ctx); }
            catch (Exception ex) { TryWrite(ctx, 500, JsonSerializer.Serialize(new { error = ex.Message }, Json)); }
        }
    }

    private void Handle(HttpListenerContext ctx)
    {
        var path = ctx.Request.Url?.AbsolutePath ?? "/";
        var q = ctx.Request.QueryString;
        var s = _state();

        switch (path)
        {
            case "/" or "/health":
                Write(ctx, 200, JsonSerializer.Serialize(new { ok = true, inGame = s.InGame }, Json));
                break;

            case "/state":
            {
                var counts = s.Entities.GroupBy(e => e.Category)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());
                Write(ctx, 200, JsonSerializer.Serialize(new
                {
                    s.InGame, areaCode = s.AreaCode, areaHash = s.AreaHash, areaLevel = s.AreaLevel,
                    character = s.CharName, charLevel = s.CharLevel,
                    mapVisible = s.MapVisible, zoom = s.Zoom,
                    hpPct = s.HpPct, manaPct = s.ManaPct, autoFlask = s.AutoFlask, flask = s.FlaskNote,
                    player = new { x = s.Player.X, y = s.Player.Y },
                    entityCount = s.Entities.Count,
                    poiCount = s.Entities.Count(e => e.Poi),
                    landmarkCount = s.Landmarks.Count,
                    counts,
                }, Json));
                break;
            }

            case "/landmarks":
            {
                var list = s.Landmarks
                    .OrderBy(l => Dist(l.Center, s.Player))
                    .Select(l => new
                    {
                        name = l.Name, path = l.Path, tiles = l.TileCount,
                        x = l.Center.X, y = l.Center.Y, dist = (int)Dist(l.Center, s.Player),
                    });
                Write(ctx, 200, JsonSerializer.Serialize(list, Json));
                break;
            }

            case "/entities":
            {
                var category = q["category"];
                var aliveOnly = string.Equals(q["alive"], "true", StringComparison.OrdinalIgnoreCase);
                _ = float.TryParse(q["radius"], out var radius);
                _ = int.TryParse(q["limit"], out var limit);
                if (limit <= 0) limit = 500;

                IEnumerable<Poe2Live.EntityDot> q2 = s.Entities;
                if (!string.IsNullOrEmpty(category))
                    q2 = q2.Where(e => string.Equals(e.Category.ToString(), category, StringComparison.OrdinalIgnoreCase));
                if (aliveOnly) q2 = q2.Where(e => e.HpCur > 0);
                if (radius > 0) q2 = q2.Where(e => Dist(e.Grid, s.Player) <= radius);

                var list = q2
                    .OrderBy(e => Dist(e.Grid, s.Player))
                    .Take(limit)
                    .Select(e => new
                    {
                        id = e.Id, addr = $"0x{e.Address:X}", category = e.Category.ToString(), metadata = e.Metadata,
                        poi = e.Poi, reaction = e.Reaction, friendly = e.IsFriendly, rarity = e.Rarity.ToString(),
                        x = e.Grid.X, y = e.Grid.Y, hpCur = e.HpCur, hpMax = e.HpMax,
                        alive = e.HpMax <= 0 || e.HpCur > 0,
                        dist = (int)Dist(e.Grid, s.Player),
                    });
                Write(ctx, 200, JsonSerializer.Serialize(list, Json));
                break;
            }

            default:
                Write(ctx, 404, JsonSerializer.Serialize(new { error = "not found", path }, Json));
                break;
        }
    }

    private static float Dist(System.Numerics.Vector2 a, System.Numerics.Vector2 b)
        => (a - b).Length();

    private static void Write(HttpListenerContext ctx, int status, string body)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        ctx.Response.ContentLength64 = bytes.Length;
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.OutputStream.Close();
    }

    private static void TryWrite(HttpListenerContext ctx, int status, string body)
    {
        try { Write(ctx, status, body); } catch { /* client gone */ }
    }

    public void Dispose()
    {
        _running = false;
        try { _listener.Stop(); } catch { }
        _listener.Close();
    }
}

/// <summary>Immutable snapshot published by the tick loop for the API to serve.</summary>
public sealed record RadarState(
    bool InGame,
    uint AreaHash,
    int AreaLevel,
    bool MapVisible,
    float Zoom,
    System.Numerics.Vector2 Player,
    IReadOnlyList<Poe2Live.EntityDot> Entities,
    IReadOnlyList<Poe2Live.Landmark> Landmarks,
    float HpPct,
    float ManaPct,
    bool AutoFlask,
    string FlaskNote,
    string AreaCode,
    string CharName,
    int CharLevel)
{
    public static readonly RadarState Empty =
        new(false, 0, 0, false, 0, System.Numerics.Vector2.Zero,
            Array.Empty<Poe2Live.EntityDot>(), Array.Empty<Poe2Live.Landmark>(), 100, 100, false, "", "", "", 0);
}
