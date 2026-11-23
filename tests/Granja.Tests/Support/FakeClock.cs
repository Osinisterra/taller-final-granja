using Granja.Domain.Interfaces;

namespace Granja.Tests.Support;

public class FakeClock : IClock
{
    public FakeClock(DateOnly today) => Today = today;
    public DateOnly Today { get; set; }
}
