namespace bagsisbaku.Application.Security;

public sealed record PermissionDefinition(
    string Name,
    string Group,
    string DisplayName);

public static class PermissionCatalog
{
    private static readonly IReadOnlyList<
        PermissionDefinition> Definitions =
        Array.AsReadOnly<PermissionDefinition>(
        [
            new(
                PermissionNames.Dashboard.View,
                "Dashboard",
                "Dashboard-a baxmaq"),

            new(
                PermissionNames.Products.View,
                "Məhsullar",
                "Məhsullara baxmaq"),

            new(
                PermissionNames.Products.Create,
                "Məhsullar",
                "Məhsul yaratmaq"),

            new(
                PermissionNames.Products.Update,
                "Məhsullar",
                "Məhsulu dəyişmək"),

            new(
                PermissionNames.Products.Delete,
                "Məhsullar",
                "Məhsulu silmək"),

            new(
                PermissionNames.Products.ManageStock,
                "Məhsullar",
                "Stoku idarə etmək"),

            new(
                PermissionNames.Orders.View,
                "Sifarişlər",
                "Sifarişlərə baxmaq"),

            new(
                PermissionNames.Orders.UpdateStatus,
                "Sifarişlər",
                "Sifariş statusunu dəyişmək"),

            new(
                PermissionNames.Customers.View,
                "Müştərilər",
                "Müştərilərə baxmaq"),

            new(
                PermissionNames.Customers.Manage,
                "Müştərilər",
                "Müştəriləri idarə etmək"),

            new(
                PermissionNames.Catalog.View,
                "Kataloq",
                "Kataloqa baxmaq"),

            new(
                PermissionNames.Catalog.Manage,
                "Kataloq",
                "Kateqoriya, brend, rəng və ölçüləri idarə etmək"),

            new(
                PermissionNames.Promotions.View,
                "Kampaniyalar",
                "Kampaniyalara baxmaq"),

            new(
                PermissionNames.Promotions.Manage,
                "Kampaniyalar",
                "Kampaniyaları idarə etmək"),

            new(
                PermissionNames.Content.View,
                "Kontent",
                "Sayt kontentinə baxmaq"),

            new(
                PermissionNames.Content.Manage,
                "Kontent",
                "Sayt kontentini idarə etmək"),

            new(
                PermissionNames.StoreSettings.View,
                "Mağaza ayarları",
                "Mağaza ayarlarına baxmaq"),

            new(
                PermissionNames.StoreSettings.Manage,
                "Mağaza ayarları",
                "Mağaza ayarlarını dəyişmək"),

            new(
                PermissionNames.Announcements.Send,
                "Bildirişlər",
                "Müştərilərə bildiriş göndərmək"),

            new(
                PermissionNames.Admins.View,
                "Adminlər",
                "Adminlərə baxmaq"),

            new(
                PermissionNames.Admins.Create,
                "Adminlər",
                "Yeni admin yaratmaq"),

            new(
                PermissionNames.Admins.Update,
                "Adminlər",
                "Admin məlumatlarını dəyişmək"),

            new(
                PermissionNames.Admins.ManagePermissions,
                "Adminlər",
                "Admin permission-larını dəyişmək"),

            new(
                PermissionNames.AuditLogs.View,
                "Audit jurnalı",
                "Audit qeydlərinə baxmaq")
        ]);

    public static IReadOnlyList<
        PermissionDefinition> All =>
        Definitions;
}

