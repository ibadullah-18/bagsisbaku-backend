using System.Text;
using bagsisbaku.Application.Abstractions.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
                            "30",

                        ["EmailAnnouncements:DemoMode"] =
                            "false",

                        ["EmailAnnouncements:BatchSize"] =
                            "10",

                        ["EmailAnnouncements:ProcessingEnabled"] =
                            "false",

                        ["EmailAnnouncements:ProcessingIntervalSeconds"] =
                            "10"
                    });
            });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(
            "Testing");

        builder.ConfigureTestServices(
            services =>
            {
                services.RemoveAll<IEmailSender>();
                services.RemoveAll<TestEmailSender>();

                services.AddSingleton<
                    TestEmailSender>();

                services.AddSingleton<IEmailSender>(
                    serviceProvider =>
                        serviceProvider
                            .GetRequiredService<
                                TestEmailSender>());
            });
    }
}