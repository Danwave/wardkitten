using Shouldly;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Tests.Domain;

public class CriticalityTests
{
    [Theory]
    [InlineData(Severity.Low, false)]
    [InlineData(Severity.Medium, false)]
    [InlineData(Severity.High, true)]
    [InlineData(Severity.Critical, true)]
    public void HighGrades_BypassQuietHours(Severity severity, bool bypass)
    {
        CriticalityCatalog.For(severity).BypassQuietHours.ShouldBe(bypass);
    }

    [Fact]
    public void EveryGrade_HasLabelAndColor()
    {
        foreach (var severity in Enum.GetValues<Severity>())
        {
            var policy = CriticalityCatalog.For(severity);
            policy.Label.ShouldNotBeNullOrWhiteSpace();
            policy.Color.ShouldStartWith("#");
        }
    }
}
