namespace TaskManager.Tests.TestDoubles;

internal class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public static readonly DateTime DefaultUtcNow = new(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);

    private DateTimeOffset _now = now;

    public FixedTimeProvider()
        : this(new DateTimeOffset(DefaultUtcNow))
    {
    }

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan delta) => _now += delta;
}
