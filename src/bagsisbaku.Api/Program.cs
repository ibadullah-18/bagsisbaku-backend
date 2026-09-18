using bagsisbaku.Api.Authentication;
using bagsisbaku.Api.OpenApi;
using bagsisbaku.Application;
using bagsisbaku.Infrastructure;
using bagsisbaku.Infrastructure.Promotions;
using bagsisbaku.Infrastructure.Promotions.Administration;
using bagsisbaku.Infrastructure.Favorites;
using bagsisbaku.Infrastructure.Orders;
using bagsisbaku.Infrastructure.Orders.Administration;
using bagsisbaku.Infrastructure.Baskets;
using bagsisbaku.Infrastructure.Customers;
using bagsisbaku.Infrastructure.Customers.Administration;
using bagsisbaku.Infrastructure.Administration.Admins;
using bagsisbaku.Infrastructure.Authentication;
using bagsisbaku.Infrastructure.Authorization;
using bagsisbaku.Infrastructure.Email;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddApiDocumentation();

builder.Services.AddApplication();

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection tapÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â±lmadÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¾ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â±.");

var cloudinarySettings =
    new CloudinarySettings(
        builder.Configuration["Cloudinary:CloudName"],
        builder.Configuration["Cloudinary:ApiKey"],
        builder.Configuration["Cloudinary:ApiSecret"]);

var jwtSettings =
    new JwtSettings(
        builder.Configuration["Jwt:Issuer"]
            ?? string.Empty,

        builder.Configuration["Jwt:Audience"]
            ?? string.Empty,

        builder.Configuration["Jwt:SigningKey"]
            ?? string.Empty,

        builder.Configuration.GetValue<int?>(
            "Jwt:AccessTokenMinutes")
            ?? 15,

        builder.Configuration.GetValue<int?>(
            "Jwt:RefreshTokenDays")
            ?? 30);

builder.Services.AddInfrastructure(
    connectionString,
    cloudinarySettings);

builder.Services.AddJwtAuthentication(
    jwtSettings);

var smtpEmailSettings =
    new SmtpEmailSettings(
        builder.Configuration[
            "Email:Smtp:Host"],
        builder.Configuration.GetValue<int?>(
            "Email:Smtp:Port") ?? 587,
        builder.Configuration[
            "Email:Smtp:Username"],
        builder.Configuration[
            "Email:Smtp:Password"],
        builder.Configuration[
            "Email:Smtp:FromEmail"],
        builder.Configuration[
            "Email:Smtp:FromName"]);

builder.Services.AddEmailDelivery(
    smtpEmailSettings);
var frontendBaseUrl =
    builder.Configuration[
        "Frontend:BaseUrl"]
    ?? "http://localhost:3000";

if (
    !Uri.TryCreate(
        frontendBaseUrl,
        UriKind.Absolute,
        out var frontendBaseUri)
)
{
    throw new InvalidOperationException(
        "Frontend:BaseUrl dÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼zgÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¼n URL deyil.");
}

var authenticationUrlSettings =
    new AuthenticationUrlSettings(
        frontendBaseUri);

builder.Services.AddAuthenticationEmails(
    authenticationUrlSettings);
builder.Services.AddBagsisbakuAuthentication();

var identityBootstrapSettings =
    new IdentityBootstrapSettings(
        builder.Configuration.GetValue<bool>(
            "Bootstrap:Identity:Enabled"),
        builder.Configuration[
            "Bootstrap:Identity:SuperAdmin:FullName"],
        builder.Configuration[
            "Bootstrap:Identity:SuperAdmin:Email"],
        builder.Configuration[
            "Bootstrap:Identity:SuperAdmin:Password"]);

builder.Services.AddIdentityBootstrap(
    identityBootstrapSettings);
builder.Services.AddPermissionAuthorization();

builder.Services.AddAdminManagement();

builder.Services.AddCurrentUser();
builder.Services.AddCustomerProfiles();
builder.Services.AddAdminCustomers();

builder.Services.AddBaskets();

builder.Services.AddFavorites();

builder.Services.AddCheckout();

builder.Services.AddCustomerOrders();

builder.Services.AddAdminOrders();

builder.Services.AddAdminPromotions();

builder.Services.AddPromoCodeValidation();

var app = builder.Build();

if (identityBootstrapSettings.Enabled)
{
    await using var bootstrapScope =
        app.Services.CreateAsyncScope();

    var identityBootstrapper =
        bootstrapScope.ServiceProvider
            .GetRequiredService<IIdentityBootstrapper>();

    await identityBootstrapper.InitializeAsync();
}
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "bagsisbaku api v1");

        options.RoutePrefix = "swagger";
        options.DocumentTitle = "bagsisbaku api";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/health",
    () => Results.Ok(
        new
        {
            name = "bagsisbaku",
            status = "healthy",
            utc = DateTimeOffset.UtcNow
        }));

app.Run();

public partial class Program;




