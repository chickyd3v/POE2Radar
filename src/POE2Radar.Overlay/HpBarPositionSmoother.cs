using POE2Radar.Core.Game;

namespace POE2Radar.Overlay;

/// <summary>Projects entity world positions to screen and eases discrete game-tick jumps in
/// <b>screen space</b>. While the camera is moving (player walking, pan) positions snap to the
/// live projection so bars stay glued to mobs; when the view is stable, screen coords ease in.</summary>
internal sealed class HpBarScreenSmoother
{
  // 1/s — higher catches up faster; ~20 bridges a ~30 Hz game tick in a few overlay frames.
  private const float SmoothRate = 20f;
  // Squared L2 delta on the 16-float camera matrix — above this we snap instead of ease.
  private const float CameraMotionThresholdSq = 4f;

  private readonly Dictionary<nint, (float X, float Y)> _screen = new();
  private readonly float[] _lastCam = new float[16];
  private bool _hasLastCam;

  /// <summary>True when the WorldToScreen matrix changed enough that easing would fight camera motion.</summary>
  public bool CameraMoved(float[] cam)
  {
    if (!_hasLastCam)
    {
      Array.Copy(cam, _lastCam, 16);
      _hasLastCam = true;
      return false;
    }

    float d = 0f;
    for (var i = 0; i < 16; i++)
    {
      var dd = cam[i] - _lastCam[i];
      d += dd * dd;
    }
    Array.Copy(cam, _lastCam, 16);
    return d > CameraMotionThresholdSq;
  }

  /// <summary>Project <paramref name="world"/> and return eased screen coords. Snaps when
  /// <paramref name="snap"/> is true (camera/player moving) or on first sight.</summary>
  public bool TryUpdate(nint entity, Vector3 world, float[] cam, float w, float h, float dt, bool snap,
    out float sx, out float sy)
  {
    sx = sy = 0f;
    if (!TryProject(world, cam, w, h, out var tx, out var ty)) return false;

    if (snap || !_screen.TryGetValue(entity, out var cur))
    {
      _screen[entity] = (tx, ty);
      sx = tx;
      sy = ty;
      return true;
    }

    var t = 1f - MathF.Exp(-SmoothRate * dt);
    var nx = cur.X + (tx - cur.X) * t;
    var ny = cur.Y + (ty - cur.Y) * t;
    _screen[entity] = (nx, ny);
    sx = nx;
    sy = ny;
    return true;
  }

  public void Prune(IReadOnlyCollection<nint> active)
  {
    if (_screen.Count == 0) return;
    List<nint>? dead = null;
    foreach (var k in _screen.Keys)
    {
      if (active.Contains(k)) continue;
      dead ??= new List<nint>();
      dead.Add(k);
    }
    if (dead is null) return;
    foreach (var k in dead) _screen.Remove(k);
  }

  public void Clear()
  {
    _screen.Clear();
    _hasLastCam = false;
  }

  private static bool TryProject(Vector3 world, float[] m, float w, float h, out float sx, out float sy)
  {
    var cw = world.X * m[3] + world.Y * m[7] + world.Z * m[11] + m[15];
    if (cw <= 0.0001f) { sx = sy = 0f; return false; }
    var cx = world.X * m[0] + world.Y * m[4] + world.Z * m[8] + m[12];
    var cy = world.X * m[1] + world.Y * m[5] + world.Z * m[9] + m[13];
    sx = (cx / cw / 2f + 0.5f) * w;
    sy = (0.5f - cy / cw / 2f) * h;
    return true;
  }
}
