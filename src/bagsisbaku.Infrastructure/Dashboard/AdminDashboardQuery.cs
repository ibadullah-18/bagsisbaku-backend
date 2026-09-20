using bagsisbaku.Application.Dashboard;
using bagsisbaku.Domain.Orders;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Dashboard;

public sealed class AdminDashboardQuery(
    ApplicationDbContext dbContext)
    : IAdminDashboardQuery
{
    public async Task<AdminDashboardModel> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var productCount =
            await dbContext.Products
                .AsNoTracking()
                .CountAsync(cancellationToken);

        var pendingOrderCount =
            await dbContext.Orders
                .AsNoTracking()
                .CountAsync(
                    order => order.Status == OrderStatus.Pending,
                    cancellationToken);

        var onDeliveryOrderCount =
            await dbContext.Orders
                .AsNoTracking()
                .CountAsync(
                    order => order.Status == OrderStatus.OnDelivery,
                    cancellationToken);

        var deliveredOrderCount =
            await dbContext.Orders
                .AsNoTracking()
                .CountAsync(
                    order => order.Status == OrderStatus.Delivered,
                    cancellationToken);

        var cancelledOrderCount =
            await dbContext.Orders
                .AsNoTracking()
                .CountAsync(
                    order => order.Status == OrderStatus.Cancelled,
                    cancellationToken);

        var deliveredOrderTotal =
            await dbContext.Orders
                .AsNoTracking()
                .Where(
                    order => order.Status == OrderStatus.Delivered)
                .Select(order => (decimal?)order.Total)
                .SumAsync(cancellationToken) ?? 0m;

        var whatsAppInquiryCount =
            await dbContext.WhatsAppProductInquiries
                .AsNoTracking()
                .CountAsync(cancellationToken);

        var pageViews =
            await dbContext.SiteVisits
                .AsNoTracking()
                .CountAsync(cancellationToken);

        var uniqueVisitors =
            await dbContext.SiteVisits
                .AsNoTracking()
                .Select(visit => visit.VisitorId)
                .Distinct()
                .CountAsync(cancellationToken);

        return new AdminDashboardModel(
            productCount,
            pendingOrderCount,
            onDeliveryOrderCount,
            deliveredOrderCount,
            cancelledOrderCount,
            deliveredOrderTotal,
            whatsAppInquiryCount,
            pageViews,
            uniqueVisitors);
    }
}