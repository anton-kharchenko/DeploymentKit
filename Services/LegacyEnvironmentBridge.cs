using DeploymentKit.Interfaces;

namespace DeploymentKit.Services;

/// <summary>
/// Applies temporary process-level environment overrides and restores previous values on dispose.
/// </summary>
public sealed class LegacyEnvironmentBridge : ILegacyEnvironmentBridge
{
    public IDisposable Apply(IDictionary<string, string?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return new Scope(values);
    }

    private sealed class Scope : IDisposable
    {
        private readonly Dictionary<string, string?> _previousValues;
        private bool _disposed;

        public Scope(IDictionary<string, string?> values)
        {
            _previousValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var (key, value) in values)
            {
                if (!_previousValues.ContainsKey(key))
                {
                    _previousValues[key] = Environment.GetEnvironmentVariable(key);
                }

                Environment.SetEnvironmentVariable(key, value);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var (key, value) in _previousValues)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            _disposed = true;
        }
    }
}

