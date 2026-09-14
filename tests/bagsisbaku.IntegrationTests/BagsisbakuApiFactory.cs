using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace bagsisbaku.IntegrationTests;

internal sealed class BagsisbakuApiFactory
    : WebApplicationFactory<Program>
{
    private static readonly string TestSigningKey =
        Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                new string('k', 64)));

    private readonly string _connectionString;

    public BagsisbakuApiFactory(
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        _connectionString = connectionString;
    }

    protected override IHost CreateHost(
        IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(
            configuration =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["environment"] = "Testing",

                        ["ConnectionStrings:DefaultConnection"] =
                            _connectionString,

                        ["Jwt:Issuer"] =
                            "bagsisbaku-tests",

                        ["Jwt:Audience"] =
                            "bagsisbaku-tests",

                        ["Jwt:SigningKey"] =
                            TestSigningKey,

                        ["Jwt:AccessTokenMinutes"] =
                            "15",

                        ["Jwt:RefreshTokenDays"] =
                            "30"
                    });
            });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
