using Granja.Domain.Interfaces;

namespace Granja.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
