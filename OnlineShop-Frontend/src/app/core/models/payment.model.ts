export enum PaymentStatus {
  Pending = 0,     // Kutilmoqda (chek tekshirilmoqda)
  Completed = 1,   // Tasdiqlangan / Muvaffaqiyatli
  Failed = 2,      // Rad etilgan / Muvaffaqiyatsiz
  Refunded = 3,    // Qaytarilgan
  Cancelled = 4    // Bekor qilingan
}

export const PaymentStatusLabels: Record<PaymentStatus | number, { label: string; color: string; bg: string }> = {
  [PaymentStatus.Pending]: { label: 'To‘lov kutilmoqda', color: '#b45309', bg: '#fef3c7' },
  [PaymentStatus.Completed]: { label: 'To‘langan', color: '#15803d', bg: '#dcfce7' },
  [PaymentStatus.Failed]: { label: 'To‘lov rad etildi', color: '#b91c1c', bg: '#fee2e2' },
  [PaymentStatus.Refunded]: { label: 'Qaytarildi', color: '#4b5563', bg: '#f3f4f6' },
  [PaymentStatus.Cancelled]: { label: 'Bekor qilindi', color: '#6b7280', bg: '#e5e7eb' },
};

export interface Payment {
  id: number;
  orderId: number;
  amount: number;
  createdAt: string;
  status: PaymentStatus;
  paymentMethod?: string;
  chekUrl?: string;
  transactionNote?: string;
  confirmedByUserId?: number;
  confirmedAt?: string;
  rejectionReason?: string;
}

export interface CreatePaymentDto {
  orderId: number;
  userId: number;
  amount: number;
  paymentMethod?: string;
  chekUrl?: string;
  transactionNote?: string;
}

export interface RejectPaymentDto {
  paymentId: number;
  reason: string;
}
