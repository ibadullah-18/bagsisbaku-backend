namespace bagsisbaku.Application.Dashboard;

public interface IAdminDashboardQuery
{
    Task<AdminDashboardModel> GetAsync(
        CancellationToken cancellationToken = default);
}

public sealed record AdminDashboardModel(
    int ProductCount,
    int PendingOrderCount,
    int OnDeliveryOrderCount,
    int DeliveredOrderCount,
    int CancelledOrderCount,
    decimal DeliveredOrderTotal,
    int WhatsAppInquiryCount,
    int PageViews,
    int UniqueVisitors);