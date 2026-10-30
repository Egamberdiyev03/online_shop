using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum NotificationType
    {
       
        OrderCreated = 1,
        OrderConfirmed = 2,
        OrderReady = 3,
        OrderOnTheWay = 4,
        OrderCompleted = 5,
        OrderCancelled = 6,
        NewBranchOrder = 7,
        CourierAssigned = 8,   // keyin ishlatiladi

        CourierArrived = 9,
        HandoverToCourierReady = 10,
        HandedToCourier = 11,
        CourierIssueReported = 12,
        PaymentSucceeded = 13,
        PaymentFailed = 14,
        PaymentRefunded = 15,
        OrderPreparing = 16,
        OrderCancelledByCustomer = 17
    }

    public enum NotificationPriority { Low = 1, Normal = 2, High = 3 }

    public enum NotificationEntityType { Order = 1, Product = 2, Branch = 3 }
}
