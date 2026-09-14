using System.Collections.Frozen;

namespace bagsisbaku.Application.Security;

public static class PermissionNames
{
    public static class Dashboard
    {
        public const string View = "dashboard.view";
    }

    public static class Products
    {
        public const string View = "products.view";
        public const string Create = "products.create";
        public const string Update = "products.update";
        public const string Delete = "products.delete";
        public const string ManageStock = "products.manage-stock";
    }

    public static class Orders
    {
        public const string View = "orders.view";
        public const string UpdateStatus = "orders.update-status";
    }

    public static class Customers
    {
        public const string View = "customers.view";
        public const string Manage = "customers.manage";
    }

    public static class Catalog
    {
        public const string View = "catalog.view";
        public const string Manage = "catalog.manage";
    }

    public static class Promotions
    {
        public const string View = "promotions.view";
        public const string Manage = "promotions.manage";
    }

    public static class Content
    {
        public const string View = "content.view";
        public const string Manage = "content.manage";
    }

    public static class StoreSettings
    {
        public const string View = "store-settings.view";
        public const string Manage = "store-settings.manage";
    }

    public static class Announcements
    {
        public const string Send = "announcements.send";
    }

    public static class Admins
    {
        public const string View = "admins.view";
        public const string Create = "admins.create";
        public const string Update = "admins.update";
        public const string ManagePermissions = "admins.manage-permissions";
    }

    public static class AuditLogs
    {
        public const string View = "audit-logs.view";
    }

    private static readonly FrozenSet<string> AllPermissions =
        new[]
        {
            Dashboard.View,

            Products.View,
            Products.Create,
            Products.Update,
            Products.Delete,
            Products.ManageStock,

            Orders.View,
            Orders.UpdateStatus,

            Customers.View,
            Customers.Manage,

            Catalog.View,
            Catalog.Manage,

            Promotions.View,
            Promotions.Manage,

            Content.View,
            Content.Manage,

            StoreSettings.View,
            StoreSettings.Manage,

            Announcements.Send,

            Admins.View,
            Admins.Create,
            Admins.Update,
            Admins.ManagePermissions,

            AuditLogs.View
        }
        .ToFrozenSet(StringComparer.Ordinal);

    public static IReadOnlySet<string> All => AllPermissions;
}
