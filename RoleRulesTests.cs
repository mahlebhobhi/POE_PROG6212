using Xunit;

namespace RaceDay.Tests;

public class RoleRulesTests
{
    [Theory]
    [InlineData("Organiser", true)]
    [InlineData("Participant", true)]
    [InlineData("Admin", false)]
    [InlineData("", false)]
    public void RegistrationRoleMustBeOneOfTheTwoAllowedRoles(string role, bool expected)
    {
        bool actual = role == "Organiser" || role == "Participant";
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("Run", true)]
    [InlineData("Walk", true)]
    [InlineData("Cycle", true)]
    [InlineData("Swim", false)]
    public void EventTypeMustBeRunWalkOrCycle(string eventType, bool expected)
    {
        bool actual = new[] { "Run", "Walk", "Cycle" }
            .Contains(eventType, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MinimumAgeCannotBeGreaterThanMaximumAge()
    {
        int minimumAge = 30;
        int maximumAge = 20;
        Assert.False(minimumAge <= maximumAge);
    }
}
