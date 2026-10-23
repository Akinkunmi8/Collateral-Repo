using CollateralPortal.Infrastructure.Integrations.Mocks;
using FluentAssertions;
using Xunit;

namespace CollateralPortal.UnitTests;

public class MockLoanManagementClientTests
{
    [Fact]
    public async Task Known_loan_contract_returns_fixture()
    {
        var sut = new MockLoanManagementClient();
        var loan = await sut.GetLoanAsync("LN-2026-00123");

        loan.Should().NotBeNull();
        loan!.CustomerName.Should().Be("Adaeze Okafor Enterprises");
    }

    [Fact]
    public async Task Unknown_loan_contract_returns_null()
    {
        var sut = new MockLoanManagementClient();
        var loan = await sut.GetLoanAsync("does-not-exist");

        loan.Should().BeNull();
    }
}
