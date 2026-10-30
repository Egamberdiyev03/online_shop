using Application.Extentions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public static class NotificationFactory
    {
        // Umumiy yordamchi: takrorlanadigan qismlarni bir joyda yig'adi
        private static NotificationRequest Build(
            int orderId, int branchId, NotificationType type, string title, string body,
            NotificationPriority priority = NotificationPriority.Normal, string? link = null) => new(
                Type: type,
                Title: title,
                Body: body,
                EntityType: NotificationEntityType.Order,
                EntityId: orderId,
                BranchId: branchId,
                Priority: priority,
                Link: link ?? $"/orders/{orderId}");

        // ---------- MIJOZGA ----------

        public static NotificationRequest Created(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.OrderCreated,
                "Buyurtma qabul qilindi",
                $"Siz yangi buyurtma berdingiz. Buyurtma ID: {orderId}",
                NotificationPriority.High);

        public static NotificationRequest Confirmed(int orderId, int branchId, string branchName) =>
            Build(orderId, branchId, NotificationType.OrderConfirmed,
                "Buyurtma tasdiqlandi",
                $"#{orderId} buyurtmangiz {branchName} filiali tomonidan tasdiqlandi");

        public static NotificationRequest Preparing(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.OrderPreparing,
                "Buyurtma tayyorlanmoqda",
                $"#{orderId} buyurtmangiz tayyorlanmoqda");

        public static NotificationRequest ReadyForPickup(int orderId, int branchId, string branchName) =>
            Build(orderId, branchId, NotificationType.OrderReady,
                "Buyurtma tayyor",
                $"#{orderId} buyurtmangizni {branchName} filialidan olib ketishingiz mumkin",
                NotificationPriority.High);

        public static NotificationRequest ReadyForCourier(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.OrderReady,
                "Buyurtma tayyor",
                $"#{orderId} buyurtmangiz kuryerga topshirishga tayyor");

        public static NotificationRequest OnTheWay(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.OrderOnTheWay,
                "Buyurtma yo'lda",
                $"#{orderId} buyurtmangiz yo'lga chiqdi",
                NotificationPriority.High);

        public static NotificationRequest Completed(int orderId, int branchId, bool isPickup) =>
            Build(orderId, branchId, NotificationType.OrderCompleted,
                isPickup ? "Buyurtma topshirildi" : "Buyurtma yetkazildi",
                $"#{orderId} buyurtma yakunlandi. Xaridingiz uchun rahmat!");

        public static NotificationRequest CancelledForCustomer(int orderId, int branchId, string? reason = null) =>
            Build(orderId, branchId, NotificationType.OrderCancelled,
                "Buyurtma bekor qilindi",
                string.IsNullOrWhiteSpace(reason)
                    ? $"#{orderId} buyurtmangiz bekor qilindi"
                    : $"#{orderId} buyurtmangiz bekor qilindi. Sabab: {reason}",
                NotificationPriority.High);

        // ---------- FILIAL ADMINIGA ----------

        public static NotificationRequest NewForBranch(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.NewBranchOrder,
                "Yangi buyurtma tushdi",
                $"#{orderId} raqamli yangi buyurtma keldi",
                NotificationPriority.High);

        public static NotificationRequest CancelledByCustomer(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.OrderCancelledByCustomer,
                "Mijoz buyurtmani bekor qildi",
                $"#{orderId} buyurtma mijoz tomonidan bekor qilindi",
                NotificationPriority.High);

        public static NotificationRequest CompletedForBranch(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.OrderCompleted,
                "Buyurtma yakunlandi",
                $"#{orderId} buyurtma yakunlandi");

        // ---------- TO'LOV ----------

        public static NotificationRequest PaymentSucceeded(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.PaymentSucceeded,
                "To'lov qabul qilindi",
                $"#{orderId} buyurtma uchun to'lov muvaffaqiyatli amalga oshdi");

        public static NotificationRequest PaymentSucceededForBranch(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.PaymentSucceeded,
                "Buyurtma to'landi",
                $"#{orderId} buyurtma uchun to'lov qabul qilindi");

        public static NotificationRequest PaymentFailed(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.PaymentFailed,
                "To'lov amalga oshmadi",
                $"#{orderId} buyurtma uchun to'lov amalga oshmadi. Qayta urinib ko'ring",
                NotificationPriority.High);

        public static NotificationRequest PaymentRefunded(int orderId, int branchId) =>
            Build(orderId, branchId, NotificationType.PaymentRefunded,
                "To'lov qaytarildi",
                $"#{orderId} buyurtma uchun to'lov qaytarildi");
    }
}
