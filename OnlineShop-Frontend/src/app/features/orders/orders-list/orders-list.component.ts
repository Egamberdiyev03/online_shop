import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Order, OrderStatus } from '../../../core/models/order.model';
import { CompanyBranch } from '../../../core/models/company-branch.model';
import { Payment, PaymentStatus, PaymentStatusLabels } from '../../../core/models/payment.model';
import { Customer } from '../../../core/models/customer.model';
import { OrderService } from '../../../core/services/order.service';
import { CompanyService } from '../../../core/services/company.service';
import { PaymentService } from '../../../core/services/payment.service';
import { CustomerService } from '../../../core/services/customer.service';
import { AuthStateService } from '../../../core/services/auth-state.service';
import { NotificationService } from '../../../core/services/notification.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    StatusBadgeComponent, 
    LoadingSpinnerComponent, 
    EmptyStateComponent
  ],
  template: `
    <div class="container">
      <!-- Page Header (Matching UI Spec: # Mening buyurtmalarim, Mijoz #... hisobidagi buyurtmalar tarixi va holati) -->
      <div class="page-header">
        <div>
          <h1 class="page-title">
            {{ authState.isBranchManager()
                ? 'Filial buyurtmalari'
                : (viewMode === 'my' ? 'Mening buyurtmalarim' : "Kompaniyalar bo'yicha buyurtmalar") }}
          </h1>
          <p class="page-subtitle">
            {{ authState.isBranchManager()
                ? 'Filial #' + (authState.userBranchId() || '') + ' - buyurtmalar boshqaruvi'
                : (viewMode === 'my'
                    ? 'Mijoz #' + (authState.currentCustomerId() || 5) + ' hisobidagi buyurtmalar tarixi va holati'
                    : 'Tizimdagi filiallar va mijozlarning buyurtmalari boshqaruvi') }}
          </p>
        </div>

        <!-- Mode Toggle if admin/manager - BranchManager uchun yashirin -->
        <div class="mode-toggle" *ngIf="authState.isAdminMode() && !authState.isBranchManager()">
          <button
            class="toggle-btn"
            [class.active]="viewMode === 'my'"
            (click)="setViewMode('my')"
          >
            Mening Buyurtmalarim
          </button>
          <button
            class="toggle-btn"
            [class.active]="viewMode === 'all'"
            (click)="setViewMode('all')"
          >
            Barcha Buyurtmalar
          </button>
        </div>
      </div>

      <!-- Admin Filters -->
      <div class="filter-bar card" *ngIf="viewMode === 'all'">
        <!-- Filial filtri - BranchManager uchun yashirin -->
        <div class="filter-col" *ngIf="!authState.isBranchManager()">
          <label class="filter-label">Filial bo'yicha:</label>
          <select [(ngModel)]="filterBranchId" (ngModelChange)="loadOrders()" class="form-control">
            <option [ngValue]="null">Barcha filiallar</option>
            <option *ngFor="let branch of branches" [ngValue]="branch.id">
              {{ branch.name }} (#{{ branch.id }})
            </option>
          </select>
        </div>

        <div class="filter-col">
          <label class="filter-label">Status bo'yicha:</label>
          <select [(ngModel)]="filterStatus" (ngModelChange)="loadOrders()" class="form-control">
            <option [ngValue]="null">Barcha statuslar</option>
            <option [ngValue]="OrderStatus.Pending">Kutilmoqda (Pending)</option>
            <option [ngValue]="OrderStatus.Confirmed">Tasdiqlangan (Confirmed)</option>
            <option [ngValue]="OrderStatus.Shipped">Jo'natildi (Shipped)</option>
            <option [ngValue]="OrderStatus.Delivered">Yetkazildi (Delivered)</option>
            <option [ngValue]="OrderStatus.Cancelled">Bekor qilingan (Cancelled)</option>
          </select>
        </div>
      </div>

      <app-loading-spinner *ngIf="isLoading" message="Buyurtmalar yuklanmoqda..."></app-loading-spinner>

      <!-- Xato holati -->
      <app-empty-state
        *ngIf="!isLoading && hasError"
        icon="⚠️"
        title="Xatolik yuz berdi"
        [description]="errorMessage"
      ></app-empty-state>

      <!-- Bo'sh holat (xato yo'q, lekin buyurtmalar ham yo'q) -->
      <app-empty-state
        *ngIf="!isLoading && !hasError && orders.length === 0"
        icon="📋"
        title="Buyurtmalar mavjud emas"
        description="Hozircha hech qanday buyurtma ro'yxatga olinmagan."
      ></app-empty-state>

      <!-- Orders List -->
      <div class="orders-container" *ngIf="!isLoading && orders.length > 0">
        <div class="order-card card" *ngFor="let order of orders">
          <div class="order-header">
            <div class="order-id-block">
              <span class="order-number">Buyurtma #{{ order.id }}</span>
              <span class="order-date">{{ order.createdAt | date:'medium' }}</span>
            </div>

            <div class="header-right">
              <span class="customer-tag" *ngIf="viewMode === 'all'" title="Mijoz">
                👤 {{ getCustomerDisplayName(order) }}
              </span>
              <span class="branch-tag" *ngIf="order.companyBranchId">
                📍 {{ getBranchName(order.companyBranchId) }}
              </span>
              <app-status-badge [status]="order.status"></app-status-badge>
            </div>
          </div>

          <!-- Order Items Table -->
          <div class="order-items-wrapper">
            <table class="items-table" *ngIf="order.orderItems && order.orderItems.length > 0">
              <thead>
                <tr>
                  <th>Mahsulot ID</th>
                  <th>Nomi</th>
                  <th class="text-center">Miqdori</th>
                  <th class="text-right">Narxi</th>
                  <th class="text-right">Jami</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let item of order.orderItems">
                  <td>#{{ item.productId }}</td>
                  <td class="item-name">{{ item.productName || ('Mahsulot #' + item.productId) }}</td>
                  <td class="text-center">{{ item.quantity }} dona</td>
                  <td class="text-right">{{ item.unitPrice | number:'1.0-0' }} so'm</td>
                  <td class="text-right fw-bold">{{ (item.unitPrice * item.quantity) | number:'1.0-0' }} so'm</td>
                </tr>
              </tbody>
            </table>

            <div *ngIf="!order.orderItems || order.orderItems.length === 0" class="no-items-note">
              OrderItem ma'lumotlari mavjud emas.
            </div>
          </div>

          <!-- Order Footer & Actions -->
          <div class="order-footer">
            <div class="total-wrap">
              <span class="total-label">Jami to'lov:</span>
              <span class="total-amount">{{ order.totalPrice | number:'1.0-0' }} so'm</span>

              <!-- To'lov statusi ko'rinishi -->
              <div class="payment-badge-wrap" *ngIf="orderPayments.get(order.id) as payment">
                <span class="payment-pill" [style.color]="getPaymentBadge(payment.status).color" [style.background]="getPaymentBadge(payment.status).bg">
                  💳 {{ getPaymentBadge(payment.status).label }}
                </span>
                <span class="rejection-note" *ngIf="payment.status === PaymentStatus.Failed && payment.rejectionReason">
                  Sabab: {{ payment.rejectionReason }}
                </span>
              </div>
              <div class="payment-badge-wrap" *ngIf="!orderPayments.get(order.id) && order.status !== OrderStatus.Cancelled">
                <span class="payment-pill unpaid-pill">
                  ⚠️ To‘lov kutilmoqda
                </span>
              </div>
            </div>

            <div class="order-actions">
              <!-- Customer: Pay / Upload Receipt Button (Faqat mijoz uchun, Admin/BranchManager ga ko'rinmaydi) -->
              <button
                *ngIf="canCustomerPay(order)"
                class="btn btn-primary btn-sm pay-btn"
                (click)="openPayModal(order)"
              >
                💳 {{ orderPayments.get(order.id) ? 'Chekni yangilash' : 'To‘lov qilish' }}
              </button>

              <!-- View Receipt Button (Chek rasmi bo'lsa barchaga ko'rinadi) -->
              <button
                *ngIf="orderPayments.get(order.id)?.chekUrl"
                class="btn btn-secondary btn-sm"
                (click)="openViewReceiptModal(order)"
                title="Chek rasmini ko'rish"
              >
                🧾 Chekni ko‘rish
              </button>

              <!-- Admin/BranchManager: To'lovni tekshirish / qabul qilish -->
              <button
                *ngIf="canAdminReviewPayment(order)"
                class="btn btn-sm"
                [ngClass]="getAdminPaymentBtnClass(order)"
                (click)="openAdminPaymentModal(order)"
              >
                {{ getAdminPaymentBtnLabel(order) }}
              </button>

              <!-- Cancel Button: Opens dedicated modal -->
              <button 
                *ngIf="canCancel(order.status)"
                class="btn btn-danger btn-sm cancel-btn"
                (click)="openCancelModal(order)"
              >
                ✕ Buyurtmani bekor qilish
              </button>

              <!-- Admin Status Change Dropdown -->
              <div class="admin-status-changer" *ngIf="authState.isAdminMode() || authState.isAdminRole()">
                <label class="status-label">Statusni o'zgartirish:</label>
                <select 
                  [ngModel]="order.status" 
                  (ngModelChange)="updateStatus(order, $event)"
                  class="form-control status-select"
                >
                  <option [ngValue]="OrderStatus.Pending">Kutilmoqda</option>
                  <option [ngValue]="OrderStatus.Confirmed">Tasdiqlangan</option>
                  <option [ngValue]="OrderStatus.Shipped">Jo'natildi</option>
                  <option [ngValue]="OrderStatus.Delivered">Yetkazildi</option>
                  <option [ngValue]="OrderStatus.Cancelled">Bekor qilindi</option>
                </select>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Customer: Pay & Upload Receipt Modal -->
      <div class="modal-overlay" *ngIf="showPayModal" (click)="closePayModal()">
        <div class="modal-content pay-modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3 class="modal-title">💳 To‘lov qilish (#{{ currentOrderForPay?.id }}-buyurtma)</h3>
            <button class="modal-close" (click)="closePayModal()">&times;</button>
          </div>

          <div class="modal-body" *ngIf="currentOrderForPay">
            <!-- Compact Bank Card Box -->
            <div class="compact-card-box">
              <div class="card-top-row">
                <span class="card-brand-badge">💳 HUMO / UZCARD</span>
                <span class="card-brand-sub">Rasmiy to‘lov kartasi</span>
              </div>
              <div class="card-num-row">
                <span class="card-num-text">9860 3501 2345 6789</span>
                <button type="button" class="copy-pill-btn" (click)="copyCardNumber('9860350123456789')" title="Nusxa olish">
                  📋 Nusxa
                </button>
              </div>
              <div class="card-holder-text">
                OnlineShop Rasmiy Hisobi (Asadbek E.)
              </div>
            </div>

            <!-- Pay Amount & Concise Guide -->
            <div class="pay-summary-strip">
              <div class="amount-side">
                <span class="label">To‘lanadigan summa:</span>
                <span class="val">{{ currentOrderForPay.totalPrice | number:'1.0-0' }} so‘m</span>
              </div>
              <div class="guide-side">
                <span>💡 Karta raqamiga to‘lov qilib, chek skrinshotini yuklang.</span>
              </div>
            </div>

            <!-- File Upload -->
            <div class="form-group-compact">
              <label class="form-sub-label">To‘lov cheki rasmi (skrinshot) *</label>
              <div 
                class="file-drop-area-compact" 
                (click)="fileInput.click()"
                (dragover)="$event.preventDefault()"
                (drop)="onFileDrop($event)"
              >
                <input
                  #fileInput
                  type="file"
                  accept="image/jpeg,image/png,image/webp,application/pdf"
                  (change)="onFileSelected($event)"
                  style="display: none"
                />

                <div class="drop-empty" *ngIf="!selectedFilePreview">
                  <span class="drop-icon">📎</span>
                  <div class="drop-texts">
                    <strong>Chek rasmini tanlash uchun bosing</strong>
                    <small>(JPG, PNG, WEBP, maks 5 MB)</small>
                  </div>
                </div>

                <div class="drop-filled" *ngIf="selectedFilePreview">
                  <img [src]="selectedFilePreview" alt="Chek rasmi" class="receipt-thumb" />
                  <div class="filled-info">
                    <span class="file-name">{{ selectedReceiptFile?.name || 'Chek rasmi yuklangan' }}</span>
                    <span class="file-size" *ngIf="selectedReceiptFile">({{ (selectedReceiptFile.size / 1024).toFixed(0) }} KB)</span>
                    <span class="change-action">🔄 O‘zgartirish uchun bosing</span>
                  </div>
                  <button type="button" class="btn-remove-file" (click)="removeSelectedFile($event)" title="O‘chirish">
                    ✕
                  </button>
                </div>
              </div>
            </div>

            <!-- Transaction Note -->
            <div class="form-group-compact">
              <label class="form-sub-label">Qo‘shimcha izoh / Karta egasi ismi (ixtiyoriy)</label>
              <input
                type="text"
                class="form-control form-control-sm"
                [(ngModel)]="payTransactionNote"
                placeholder="Masalan: 9860 **** 1234 dan Aliyev V."
              />
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary btn-sm" (click)="closePayModal()" [disabled]="isSubmittingPay">
              Bekor qilish
            </button>
            <button
              type="button"
              class="btn btn-primary btn-sm submit-pay-btn"
              [disabled]="isSubmittingPay || (!selectedReceiptFile && !currentOrderExistingChekUrl)"
              (click)="submitPaymentReceipt()"
            >
              <span *ngIf="isSubmittingPay" class="btn-spinner"></span>
              {{ isSubmittingPay ? 'Yuborilmoqda...' : 'To‘lov chekini yuborish' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Admin / BranchManager: Review & Confirm/Reject Payment Modal -->
      <div class="modal-overlay" *ngIf="showAdminPaymentModal" (click)="closeAdminPaymentModal()">
        <div class="modal-content admin-pay-modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3 class="modal-title">🧾 To‘lovni tekshirish (#{{ adminReviewOrder?.id }}-buyurtma)</h3>
            <button class="modal-close" (click)="closeAdminPaymentModal()">&times;</button>
          </div>

          <div class="modal-body" *ngIf="adminReviewOrder">
            <!-- Review Summary -->
            <div class="admin-review-summary">
              <div class="summary-line">
                <span class="label">Mijoz:</span>
                <strong class="text-dark">👤 {{ getCustomerFullName(adminReviewOrder) }}</strong>
              </div>
              <div class="summary-line" *ngIf="getCustomerPhone(adminReviewOrder)">
                <span class="label">Telefon raqami:</span>
                <strong>📞 {{ getCustomerPhone(adminReviewOrder) }}</strong>
              </div>
              <div class="summary-line">
                <span class="label">Jami summa:</span>
                <strong class="total-highlight">{{ adminReviewOrder.totalPrice | number:'1.0-0' }} so‘m</strong>
              </div>
              <div class="summary-line">
                <span class="label">To‘lov holati:</span>
                <span 
                  *ngIf="adminReviewPayment" 
                  class="payment-pill" 
                  [style.color]="getPaymentBadge(adminReviewPayment.status).color" 
                  [style.background]="getPaymentBadge(adminReviewPayment.status).bg"
                >
                  {{ getPaymentBadge(adminReviewPayment.status).label }}
                </span>
                <span *ngIf="!adminReviewPayment" class="payment-pill unpaid-pill">
                  ⚠️ Hali chek yuklanmagan
                </span>
              </div>
              <div class="summary-line" *ngIf="adminReviewPayment?.paymentMethod">
                <span class="label">To‘lov usuli:</span>
                <span>{{ adminReviewPayment?.paymentMethod }}</span>
              </div>
              <div class="summary-line" *ngIf="adminReviewPayment?.transactionNote">
                <span class="label">Mijoz izohi:</span>
                <span class="note-text">{{ adminReviewPayment?.transactionNote }}</span>
              </div>
              <div class="summary-line" *ngIf="adminReviewPayment">
                <span class="label">Chek yuborilgan vaqt:</span>
                <span class="fw-bold">🕒 {{ formatPaymentDate(adminReviewPayment.createdAt, adminReviewOrder.createdAt) }}</span>
              </div>
              <div class="summary-line" *ngIf="adminReviewPayment?.confirmedAt">
                <span class="label">Tasdiqlangan vaqt:</span>
                <span>{{ formatPaymentDate(adminReviewPayment!.confirmedAt!) }} (Admin #{{ adminReviewPayment?.confirmedByUserId }})</span>
              </div>
              <div class="summary-line" *ngIf="adminReviewPayment?.rejectionReason">
                <span class="label text-danger">Rad etish sababi:</span>
                <span class="text-danger fw-bold">{{ adminReviewPayment?.rejectionReason }}</span>
              </div>
            </div>

            <!-- Case 1: Receipt Image Available -->
            <div class="receipt-section" *ngIf="adminReviewPayment?.chekUrl">
              <label class="form-sub-label">Yuklangan to‘lov cheki:</label>
              <div class="receipt-preview-card" (click)="openViewReceiptModal(adminReviewOrder)" title="Kattalashtirish">
                <img
                  [src]="paymentService.getFullImageUrl(adminReviewPayment?.chekUrl)"
                  alt="Chek rasmi"
                  class="admin-thumb-img"
                />
                <div class="receipt-overlay">
                  <span>🔍 Kattalashtirib ko‘rish</span>
                </div>
              </div>
              <div class="receipt-quick-actions">
                <button 
                  type="button" 
                  class="btn btn-secondary btn-sm"
                  (click)="openViewReceiptModal(adminReviewOrder)"
                >
                  🔍 Chekni to‘liq ko‘rish
                </button>
                <a 
                  [href]="paymentService.getFullImageUrl(adminReviewPayment?.chekUrl)" 
                  target="_blank" 
                  class="btn btn-secondary btn-sm"
                >
                  ↗ Yangi oynada ochish
                </a>
              </div>
            </div>

            <!-- Case 2: No Receipt yet but Order Pending (Manual Cash/Terminal confirmation at branch) -->
            <div class="manual-payment-box" *ngIf="!adminReviewPayment && adminReviewOrder.status === OrderStatus.Pending">
              <div class="manual-pay-title">
                <span>💵 Filialda to‘lovni qabul qilish</span>
              </div>
              <p class="manual-pay-desc">
                Agar mijoz filialda naqd pul yoki terminal orqali to‘lagan bo‘lsa, usulni tanlab to‘lovni tasdiqlashingiz mumkin:
              </p>
              <div class="method-selector">
                <label class="radio-option">
                  <input type="radio" [(ngModel)]="adminPaymentMethod" value="Cash" />
                  <span>💵 Naqd pul</span>
                </label>
                <label class="radio-option">
                  <input type="radio" [(ngModel)]="adminPaymentMethod" value="CardTransfer" />
                  <span>💳 Terminal / Bank kartasi</span>
                </label>
              </div>
              <div class="form-group-compact mt-2">
                <label class="form-sub-label">Kassa izohi (ixtiyoriy)</label>
                <input
                  type="text"
                  class="form-control form-control-sm"
                  [(ngModel)]="adminPaymentNote"
                  placeholder="Masalan: Kassa #1 ga qabul qilindi"
                />
              </div>
            </div>

            <!-- Rejection Reason Input (shown only if rejecting) -->
            <div class="reject-box" *ngIf="showRejectReasonInput">
              <label class="form-sub-label text-danger">Rad etish sababini kiriting *</label>
              <textarea
                class="form-control"
                rows="2"
                [(ngModel)]="adminRejectReason"
                placeholder="Masalan: Hisobga pul tushmadi yoki summa mos kelmadi..."
              ></textarea>
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary btn-sm" (click)="closeAdminPaymentModal()" [disabled]="isAdminProcessingPayment">
              Yopish
            </button>

            <!-- Case A: Payment Pending -> Reject or Approve -->
            <ng-container *ngIf="adminReviewPayment && adminReviewPayment.status === PaymentStatus.Pending">
              <button
                *ngIf="!showRejectReasonInput"
                type="button"
                class="btn btn-danger btn-sm"
                (click)="showRejectReasonInput = true"
                [disabled]="isAdminProcessingPayment"
              >
                ❌ Rad etish
              </button>
              <button
                *ngIf="showRejectReasonInput"
                type="button"
                class="btn btn-danger btn-sm"
                (click)="confirmRejectPayment()"
                [disabled]="isAdminProcessingPayment || !adminRejectReason.trim()"
              >
                {{ isAdminProcessingPayment ? 'Bajarilmoqda...' : 'Rad etishni tasdiqlash' }}
              </button>
              <button
                type="button"
                class="btn btn-success btn-sm"
                (click)="confirmApprovePayment()"
                [disabled]="isAdminProcessingPayment"
              >
                {{ isAdminProcessingPayment ? 'Tasdiqlanmoqda...' : '✅ To‘lovni tasdiqlash' }}
              </button>
            </ng-container>

            <!-- Case B: No payment record yet -> Direct cash confirmation -->
            <ng-container *ngIf="!adminReviewPayment && adminReviewOrder?.status === OrderStatus.Pending">
              <button
                type="button"
                class="btn btn-success btn-sm"
                (click)="confirmCashOrDirectPayment()"
                [disabled]="isAdminProcessingPayment"
              >
                {{ isAdminProcessingPayment ? 'Tasdiqlanmoqda...' : '✅ To‘lovni qabul qilib tasdiqlash' }}
              </button>
            </ng-container>
          </div>
        </div>
      </div>

      <!-- View Receipt Modal (Enlarged) -->
      <div class="modal-overlay" *ngIf="showReceiptViewerModal" (click)="closeReceiptViewerModal()">
        <div class="modal-content viewer-modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3 class="modal-title">🧾 To‘lov cheki</h3>
            <button class="modal-close" (click)="closeReceiptViewerModal()">&times;</button>
          </div>
          <div class="modal-body text-center">
            <img [src]="viewingReceiptUrl" alt="Chek rasmi" class="viewer-img" />
          </div>
          <div class="modal-footer">
            <a [href]="viewingReceiptUrl" target="_blank" download class="btn btn-secondary">
              📥 Yuklab olish
            </a>
            <button type="button" class="btn btn-primary" (click)="closeReceiptViewerModal()">
              Yopish
            </button>
          </div>
        </div>
      </div>

      <!-- Cancel Confirmation Modal -->
      <div class="modal-overlay" *ngIf="showCancelModal" (click)="closeCancelModal()">
        <div class="modal-content cancel-modal" (click)="$event.stopPropagation()">
          <div class="modal-header modal-header-danger">
            <h3 class="modal-title">⚠️ Buyurtmani bekor qilasizmi?</h3>
            <button class="modal-close" (click)="closeCancelModal()">&times;</button>
          </div>

          <div class="modal-body">
            <p class="cancel-modal-text">
              Haqiqatan ham <strong>#{{ orderToCancel?.id }}</strong>-raqamli buyurtmani bekor qilishni xohlaysizmi?
            </p>
            <div class="cancel-order-summary" *ngIf="orderToCancel">
              <div class="summary-row">
                <span>Jami summa:</span>
                <strong>{{ orderToCancel.totalPrice | number:'1.0-0' }} so'm</strong>
              </div>
              <div class="summary-row">
                <span>Sana:</span>
                <span>{{ orderToCancel.createdAt | date:'medium' }}</span>
              </div>
            </div>
            <p class="cancel-warning-note">
              ⚠️ Bekor qilingach, ushbu buyurtma qayta tiklanmaydi.
            </p>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeCancelModal()">
              Yo'q, qolsin
            </button>
            <button 
              type="button" 
              class="btn btn-danger" 
              [disabled]="isCancelling"
              (click)="confirmCancelOrder()"
            >
              {{ isCancelling ? 'Bekor qilinmoqda...' : 'Ha, bekor qilinsin' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem 1.25rem;
    }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .page-title {
      font-size: 1.85rem;
      font-weight: 800;
      color: #0f172a;
      letter-spacing: -0.02em;
    }
    .page-subtitle {
      color: #64748b;
      font-size: 0.9rem;
      margin-top: 0.25rem;
    }
    .mode-toggle {
      display: flex;
      background: #f1f5f9;
      padding: 0.25rem;
      border-radius: 10px;
      gap: 0.25rem;
      border: 1px solid #e2e8f0;
    }
    .toggle-btn {
      padding: 0.5rem 0.95rem;
      border-radius: 8px;
      font-size: 0.825rem;
      font-weight: 700;
      color: #64748b;
      background: transparent;
      border: none;
      cursor: pointer;
      transition: all 0.2s;
    }
    .toggle-btn.active {
      background: #ffffff;
      color: #4f46e5;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
    }

    .filter-bar {
      display: flex;
      gap: 1.5rem;
      padding: 1rem 1.25rem;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
      background: #ffffff;
      border: 1px solid #e2e8f0;
      border-radius: 12px;
    }
    .filter-col {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .filter-label {
      font-size: 0.825rem;
      font-weight: 700;
      color: #334155;
      white-space: nowrap;
    }

    .orders-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .order-card {
      padding: 0;
      overflow: hidden;
      background: #ffffff;
      border-radius: 16px;
      border: 1px solid #e2e8f0;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
      transition: box-shadow 0.2s;
    }
    .order-card:hover {
      box-shadow: 0 6px 16px rgba(0, 0, 0, 0.06);
    }
    .order-header {
      padding: 1.25rem 1.5rem;
      background: #f8fafc;
      border-bottom: 1px solid #e2e8f0;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .order-id-block {
      display: flex;
      align-items: baseline;
      gap: 0.75rem;
    }
    .order-number {
      font-size: 1.15rem;
      font-weight: 800;
      color: #0f172a;
    }
    .order-date {
      font-size: 0.8125rem;
      color: #64748b;
    }
    .header-right {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      flex-wrap: wrap;
    }
    .customer-tag, .branch-tag {
      font-size: 0.75rem;
      background: #ffffff;
      border: 1px solid #e2e8f0;
      padding: 0.25rem 0.6rem;
      border-radius: 6px;
      color: #475569;
      font-weight: 600;
    }

    .order-items-wrapper {
      padding: 1.25rem 1.5rem;
    }
    .items-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.875rem;
    }
    .items-table th {
      text-align: left;
      font-size: 0.75rem;
      font-weight: 700;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      padding-bottom: 0.75rem;
      border-bottom: 1px solid #e2e8f0;
    }
    .items-table td {
      padding: 0.85rem 0;
      border-bottom: 1px dashed #f1f5f9;
      color: #334155;
    }
    .items-table tr:last-child td {
      border-bottom: none;
    }
    .item-name {
      font-weight: 600;
      color: #0f172a;
    }
    .text-center { text-align: center !important; }
    .text-right { text-align: right !important; }
    .fw-bold { font-weight: 700; color: #0f172a; }
    .no-items-note {
      font-size: 0.825rem;
      color: #94a3b8;
      font-style: italic;
      padding: 0.5rem 0;
    }

    .order-footer {
      padding: 1.25rem 1.5rem;
      background: #fafafa;
      border-top: 1px solid #f1f5f9;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .total-wrap {
      display: flex;
      align-items: baseline;
      gap: 0.5rem;
    }
    .total-label {
      font-size: 0.9rem;
      color: #64748b;
      font-weight: 600;
    }
    .total-amount {
      font-size: 1.35rem;
      font-weight: 800;
      color: #4f46e5;
    }
    .order-actions {
      display: flex;
      align-items: center;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .cancel-btn {
      padding: 0.5rem 0.95rem;
      font-size: 0.825rem;
      border-radius: 8px;
    }
    .admin-status-changer {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .status-label {
      font-size: 0.75rem;
      color: #64748b;
      font-weight: 600;
    }
    .status-select {
      width: auto;
      padding: 0.35rem 0.65rem;
      font-size: 0.8125rem;
      border-radius: 6px;
    }

    /* Cancel Modal */
    .cancel-modal {
      max-width: 460px;
    }
    .modal-overlay {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(15, 23, 42, 0.45);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1050;
      padding: 1rem;
    }
    .modal-content {
      background: #ffffff;
      border-radius: 16px;
      width: 100%;
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
      overflow: hidden;
      animation: modalSlide 0.2s ease-out;
    }
    @keyframes modalSlide {
      from { transform: scale(0.95); opacity: 0; }
      to { transform: scale(1); opacity: 1; }
    }
    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.5rem;
    }
    .modal-header-danger {
      background: #fef2f2;
      border-bottom: 1px solid #fee2e2;
    }
    .modal-title {
      font-size: 1.15rem;
      font-weight: 700;
      color: #991b1b;
    }
    .modal-close {
      color: #94a3b8;
      font-size: 1.4rem;
      background: none;
      border: none;
      cursor: pointer;
    }
    .modal-body {
      padding: 1.5rem;
    }
    .cancel-modal-text {
      font-size: 0.95rem;
      color: #0f172a;
      line-height: 1.5;
    }
    .cancel-order-summary {
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      padding: 0.85rem;
      margin: 1rem 0;
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
      font-size: 0.85rem;
    }
    .summary-row {
      display: flex;
      justify-content: space-between;
      color: #475569;
    }
    .cancel-warning-note {
      font-size: 0.78rem;
      color: #b45309;
      margin: 0;
    }
    .modal-footer {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      padding: 1rem 1.5rem;
      border-top: 1px solid #f1f5f9;
      background: #f8fafc;
    }

    /* Payment Badges & UI */
    .payment-badge-wrap {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-top: 0.25rem;
      flex-wrap: wrap;
    }
    .payment-pill {
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.2rem 0.6rem;
      border-radius: 999px;
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
    }
    .unpaid-pill {
      color: #92400e;
      background: #fef3c7;
    }
    .rejection-note {
      font-size: 0.75rem;
      color: #dc2626;
      font-weight: 600;
    }
    .pay-btn {
      background: #4f46e5;
      color: #ffffff;
      border: none;
    }
    .pay-btn:hover {
      background: #4338ca;
    }
    .confirm-pay-btn {
      background: #059669;
      color: #ffffff;
      border: none;
    }
    .confirm-pay-btn:hover {
      background: #047857;
    }

    /* Pay Modal & Admin Modal common structure */
    .pay-modal, .admin-pay-modal {
      max-width: 500px;
      max-height: 90vh;
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }
    .pay-modal .modal-header, .admin-pay-modal .modal-header {
      padding: 1rem 1.25rem;
      border-bottom: 1px solid #e2e8f0;
      flex-shrink: 0;
    }
    .pay-modal .modal-body, .admin-pay-modal .modal-body {
      padding: 1rem 1.25rem;
      overflow-y: auto;
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 0.85rem;
    }
    .pay-modal .modal-footer, .admin-pay-modal .modal-footer {
      padding: 0.85rem 1.25rem;
      border-top: 1px solid #f1f5f9;
      background: #f8fafc;
      flex-shrink: 0;
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
    }

    /* Customer Pay Modal elements */
    .compact-card-box {
      background: linear-gradient(135deg, #1e1b4b 0%, #312e81 100%);
      color: #ffffff;
      border-radius: 12px;
      padding: 0.75rem 1rem;
      box-shadow: 0 4px 12px rgba(30, 27, 75, 0.15);
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    .card-top-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.75rem;
      opacity: 0.85;
      font-weight: 600;
    }
    .card-brand-badge {
      letter-spacing: 0.05em;
    }
    .card-brand-sub {
      font-size: 0.7rem;
      opacity: 0.75;
    }
    .card-num-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .card-num-text {
      font-family: monospace;
      font-size: 1.15rem;
      font-weight: 700;
      letter-spacing: 0.08em;
    }
    .copy-pill-btn {
      background: rgba(255, 255, 255, 0.18);
      border: 1px solid rgba(255, 255, 255, 0.3);
      color: #ffffff;
      font-size: 0.725rem;
      padding: 0.25rem 0.55rem;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 600;
      transition: background 0.2s;
    }
    .copy-pill-btn:hover {
      background: rgba(255, 255, 255, 0.3);
    }
    .card-holder-text {
      font-size: 0.78rem;
      opacity: 0.9;
    }

    .pay-summary-strip {
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      padding: 0.65rem 0.85rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 0.5rem;
      flex-wrap: wrap;
    }
    .amount-side {
      display: flex;
      align-items: baseline;
      gap: 0.4rem;
    }
    .amount-side .label {
      font-size: 0.825rem;
      color: #64748b;
      font-weight: 600;
    }
    .amount-side .val {
      font-size: 1.15rem;
      font-weight: 800;
      color: #4f46e5;
    }
    .guide-side {
      font-size: 0.75rem;
      color: #475569;
      background: #eff6ff;
      padding: 0.25rem 0.5rem;
      border-radius: 6px;
      border-left: 3px solid #3b82f6;
    }

    .form-group-compact {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    .form-sub-label {
      font-size: 0.78rem;
      font-weight: 700;
      color: #334155;
    }

    /* File Drop Area Compact */
    .file-drop-area-compact {
      border: 2px dashed #cbd5e1;
      border-radius: 10px;
      padding: 0.65rem 0.85rem;
      cursor: pointer;
      background: #f8fafc;
      transition: border-color 0.2s, background 0.2s;
    }
    .file-drop-area-compact:hover {
      border-color: #4f46e5;
      background: #f1f5f9;
    }
    .drop-empty {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      color: #475569;
      font-size: 0.825rem;
    }
    .drop-icon {
      font-size: 1.5rem;
    }
    .drop-texts {
      display: flex;
      flex-direction: column;
      line-height: 1.3;
    }
    .drop-texts small {
      color: #94a3b8;
      font-size: 0.72rem;
    }
    .drop-filled {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      text-align: left;
    }
    .receipt-thumb {
      width: 54px;
      height: 54px;
      object-fit: cover;
      border-radius: 8px;
      border: 1px solid #e2e8f0;
      flex-shrink: 0;
    }
    .filled-info {
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
      flex: 1;
      overflow: hidden;
    }
    .file-name {
      font-size: 0.825rem;
      font-weight: 600;
      color: #0f172a;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .file-size {
      font-size: 0.72rem;
      color: #64748b;
    }
    .change-action {
      font-size: 0.72rem;
      color: #4f46e5;
      font-weight: 600;
    }
    .btn-remove-file {
      background: #fee2e2;
      color: #dc2626;
      border-radius: 50%;
      width: 24px;
      height: 24px;
      border: none;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      cursor: pointer;
      flex-shrink: 0;
    }
    .submit-pay-btn {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
    }
    .btn-spinner {
      display: inline-block;
      width: 14px;
      height: 14px;
      border: 2px solid rgba(255, 255, 255, 0.4);
      border-radius: 50%;
      border-top-color: #ffffff;
      animation: spin 0.6s linear infinite;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* Admin Review Modal Elements */
    .admin-review-summary {
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      padding: 0.75rem 0.95rem;
      display: flex;
      flex-direction: column;
      gap: 0.45rem;
      font-size: 0.825rem;
    }
    .summary-line {
      display: flex;
      justify-content: space-between;
      align-items: center;
      color: #475569;
    }
    .total-highlight {
      font-size: 1.05rem;
      font-weight: 800;
      color: #4f46e5;
    }
    .note-text {
      color: #0f172a;
      font-weight: 600;
      max-width: 65%;
      text-align: right;
    }
    .receipt-section {
      display: flex;
      flex-direction: column;
      gap: 0.45rem;
    }
    .receipt-preview-card {
      position: relative;
      border-radius: 10px;
      overflow: hidden;
      border: 1px solid #cbd5e1;
      max-height: 200px;
      background: #0f172a;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .admin-thumb-img {
      max-height: 200px;
      max-width: 100%;
      object-fit: contain;
    }
    .receipt-overlay {
      position: absolute;
      bottom: 0;
      left: 0;
      right: 0;
      background: rgba(15, 23, 42, 0.75);
      color: #ffffff;
      font-size: 0.75rem;
      text-align: center;
      padding: 0.3rem;
      font-weight: 600;
    }
    .receipt-quick-actions {
      display: flex;
      gap: 0.5rem;
      justify-content: center;
    }
    .manual-payment-box {
      background: #f0fdf4;
      border: 1px solid #bbf7d0;
      border-radius: 10px;
      padding: 0.85rem;
    }
    .manual-pay-title {
      font-weight: 700;
      color: #166534;
      font-size: 0.9rem;
      margin-bottom: 0.25rem;
    }
    .manual-pay-desc {
      font-size: 0.78rem;
      color: #15803d;
      margin-bottom: 0.5rem;
      line-height: 1.4;
    }
    .method-selector {
      display: flex;
      gap: 1.25rem;
    }
    .radio-option {
      display: flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.825rem;
      font-weight: 700;
      color: #166534;
      cursor: pointer;
    }
    .reject-box {
      background: #fef2f2;
      border: 1px solid #fee2e2;
      border-radius: 8px;
      padding: 0.75rem;
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    /* Viewer Modal */
    .viewer-modal {
      max-width: 650px;
    }
    .viewer-img {
      max-width: 100%;
      max-height: 70vh;
      border-radius: 8px;
      object-fit: contain;
    }
  `]
})
export class OrdersListComponent implements OnInit {
  private orderService = inject(OrderService);
  private companyService = inject(CompanyService);
  public paymentService = inject(PaymentService);
  public customerService = inject(CustomerService);
  public authState = inject(AuthStateService);
  private notification = inject(NotificationService);

  OrderStatus = OrderStatus;
  PaymentStatus = PaymentStatus;
  viewMode: 'my' | 'all' = 'my';
  orders: Order[] = [];
  branches: CompanyBranch[] = [];
  customersMap: Map<number, Customer> = new Map();
  isLoading = true;
  hasError = false;
  errorMessage = '';

  filterBranchId: number | null = null;
  filterStatus: OrderStatus | null = null;

  // Payments map: orderId -> Payment
  orderPayments: Map<number, Payment> = new Map();

  // Customer Pay Modal state
  showPayModal = false;
  currentOrderForPay: Order | null = null;
  selectedReceiptFile: File | null = null;
  selectedFilePreview: string | null = null;
  payTransactionNote = '';
  isSubmittingPay = false;
  currentOrderExistingChekUrl: string | null = null;

  // Admin Review Modal state
  showAdminPaymentModal = false;
  adminReviewOrder: Order | null = null;
  adminReviewPayment: Payment | null = null;
  adminRejectReason = '';
  showRejectReasonInput = false;
  isAdminProcessingPayment = false;
  adminPaymentMethod: 'Cash' | 'CardTransfer' = 'Cash';
  adminPaymentNote = '';

  // Receipt Viewer Modal state
  showReceiptViewerModal = false;
  viewingReceiptUrl = '';

  // Cancel modal state (Matching UI Spec: ### Buyurtmani bekor qilasizmi?)
  showCancelModal = false;
  orderToCancel: Order | null = null;
  isCancelling = false;

  ngOnInit(): void { 
    this.loadBranches(); 
    this.loadCustomers();
    if (this.authState.isAdminMode() || this.authState.isAdminRole()) { 
      this.viewMode = 'all'; 
    } 
    if (this.authState.isBranchManager()) { 
      this.filterBranchId = this.authState.userBranchId(); 
      this.viewMode = 'all'; 
    } 
    this.loadOrders(); 
  }

  setViewMode(mode: 'my' | 'all'): void {
    this.viewMode = mode;
    this.loadOrders();
  }

  loadBranches(): void {
    this.companyService.getAllBranches().subscribe({
      next: (branches) => {
        if (this.authState.isCompanyAdmin()) {
          const cId = this.authState.userCompanyId();
          this.branches = branches.filter(b => b.companyId === cId);
        } else if (this.authState.isBranchManager()) {
          const bId = this.authState.userBranchId();
          this.branches = branches.filter(b => b.id === bId);
        } else {
          this.branches = branches;
        }
      },
      error: () => {}
    });
  }

  loadCustomers(): void {
    this.customerService.getAll().subscribe({
      next: (customers) => {
        (customers || []).forEach(c => {
          this.customersMap.set(c.id, c);
        });
      },
      error: () => {}
    });
  }

  loadOrders(): void {
    this.isLoading = true;
    this.hasError = false;
    this.errorMessage = '';

    if (this.viewMode === 'my') {
      const customerId = this.authState.currentCustomerId();
      this.orderService.getByCustomerId(customerId).subscribe({
        next: (orders) => {
          this.orders = orders || [];
          this.isLoading = false;
          this.loadPaymentsForOrders();
        },
        error: (err) => {
          console.error('[Orders] Error loading my orders:', err);
          this.isLoading = false;
          this.hasError = true;
          this.orders = [];
          if (err?.status === 403) {
            this.errorMessage = 'Buyurtmalarni ko\'rish uchun ruxsat yo\'q.';
          } else {
            this.errorMessage = 'Buyurtmalarni yuklashda xatolik yuz berdi. Qayta urinib ko\'ring.';
          }
          this.notification.error(this.errorMessage);
        }
      });
    } else {
      if (this.filterBranchId !== null) {
        this.orderService.getOrdersByBranchId(this.filterBranchId).subscribe({
          next: (orders) => {
            this.orders = orders || [];
            this.isLoading = false;
            this.loadPaymentsForOrders();
          },
          error: (err) => {
            console.error('[Orders] Error loading orders by branch:', err);
            this.isLoading = false;
            this.hasError = true;
            this.orders = [];
            this.errorMessage = err?.status === 403 ? 'Ruxsat yo\'q.' : 'Xatolik yuz berdi.';
          }
        });
      } else if (this.filterStatus !== null) {
        this.orderService.getOrdersByStatus(this.filterStatus).subscribe({
          next: (orders) => {
            this.orders = orders || [];
            this.isLoading = false;
            this.loadPaymentsForOrders();
          },
          error: (err) => {
            console.error('[Orders] Error loading orders by status:', err);
            this.isLoading = false;
            this.hasError = true;
            this.orders = [];
            this.errorMessage = err?.status === 403 ? 'Ruxsat yo\'q.' : 'Xatolik yuz berdi.';
          }
        });
      } else {
        this.orderService.getAll().subscribe({
          next: (orders) => {
            if (this.authState.isSuperAdmin()) {
              this.orders = orders || [];
            } else {
              const branchIds = this.branches.map(b => b.id);
              this.orders = (orders || []).filter(o => branchIds.includes(o.companyBranchId));
            }
            this.isLoading = false;
            this.loadPaymentsForOrders();
          },
          error: (err) => {
            console.error('[Orders] Error loading all orders:', err);
            this.isLoading = false;
            this.hasError = true;
            this.orders = [];
            this.errorMessage = err?.status === 403 ? 'Barcha buyurtmalarni ko\'rish uchun ruxsat yo\'q.' : 'Xatolik yuz berdi.';
          }
        });
      }
    }
  }

  loadPaymentsForOrders(): void {
    if (!this.orders || this.orders.length === 0) return;

    if (this.authState.isAdminMode() || this.authState.isAdminRole() || this.viewMode === 'all') {
      const branchId = this.authState.isBranchManager() ? (this.authState.userBranchId() || undefined) : (this.filterBranchId || undefined);
      this.paymentService.getAllPayments(undefined, branchId).subscribe({
        next: (payments) => {
          (payments || []).forEach(p => {
            this.orderPayments.set(p.orderId, p);
          });
        },
        error: (err) => {
          console.error('[Orders] Error loading payments:', err);
        }
      });
    } else {
      this.orders.forEach(order => {
        this.paymentService.getPaymentByOrderId(order.id).subscribe({
          next: (payment) => {
            if (payment) {
              this.orderPayments.set(order.id, payment);
            }
          },
          error: () => {}
        });
      });
    }
  }

  getPayment(orderId: number): Payment | undefined {
    return this.orderPayments.get(orderId);
  }

  getPaymentBadge(status: PaymentStatus): { label: string; color: string; bg: string } {
    return PaymentStatusLabels[status] || { label: 'Noma\'lum', color: '#6b7280', bg: '#f3f4f6' };
  }

  canCustomerPay(order: Order): boolean {
    // Agar login qilgan user admin, branch manager yoki company admin bo'lsa yoki viewMode 'all' bo'lsa, to'lov tugmasi CHIQMAYDI
    if (this.authState.isAdminRole() || this.authState.isAdminMode() || this.viewMode === 'all') {
      return false;
    }
    if (order.status === OrderStatus.Cancelled || order.status === OrderStatus.Delivered) {
      return false;
    }
    const payment = this.getPayment(order.id);
    return !payment || payment.status === PaymentStatus.Failed || payment.status === PaymentStatus.Pending;
  }

  canAdminReviewPayment(order: Order): boolean {
    const isAdmin = this.authState.isAdminRole() || this.authState.isAdminMode() || this.viewMode === 'all';
    if (!isAdmin) return false;
    if (order.status === OrderStatus.Cancelled) return false;
    return true;
  }

  getAdminPaymentBtnLabel(order: Order): string {
    const payment = this.getPayment(order.id);
    if (!payment) {
      return '💵 To‘lovni qabul qilish';
    }
    if (payment.status === PaymentStatus.Pending) {
      return '⏳ To‘lovni tekshirish';
    }
    if (payment.status === PaymentStatus.Completed) {
      return '✅ To‘lov tasdiqlangan';
    }
    if (payment.status === PaymentStatus.Failed) {
      return '⚠️ To‘lov rad etilgan';
    }
    return '💳 To‘lov ma’lumotlari';
  }

  getAdminPaymentBtnClass(order: Order): string {
    const payment = this.getPayment(order.id);
    if (!payment) {
      return 'btn-success';
    }
    if (payment.status === PaymentStatus.Pending) {
      return 'btn-warning';
    }
    if (payment.status === PaymentStatus.Completed) {
      return 'btn-secondary';
    }
    if (payment.status === PaymentStatus.Failed) {
      return 'btn-danger';
    }
    return 'btn-secondary';
  }

  getCustomerFullName(order?: Order | null): string {
    if (!order) return '—';
    const id = order.customerId || (order as any).userId;
    if (!id) return 'Nomaʼlum mijoz';

    const customer = this.customersMap.get(id);
    if (customer && customer.name) {
      return `${customer.name} (ID: #${id})`;
    }
    return `Mijoz #${id}`;
  }

  getCustomerDisplayName(order?: Order | null): string {
    if (!order) return '—';
    const id = order.customerId || (order as any).userId;
    if (!id) return 'Nomaʼlum mijoz';

    const customer = this.customersMap.get(id);
    if (customer && customer.name) {
      return `${customer.name} (#${id})`;
    }
    return `Mijoz #${id}`;
  }

  getCustomerPhone(order?: Order | null): string | null {
    if (!order) return null;
    const id = order.customerId || (order as any).userId;
    if (!id) return null;

    const customer = this.customersMap.get(id);
    return customer?.phoneNumber || null;
  }

  formatPaymentDate(dateStr?: string | null, fallbackDateStr?: string | null): string {
    const target = dateStr || fallbackDateStr;
    if (!target) return '—';

    const d = new Date(target);
    // Agar sana noto'g'ri bo'lsa yoki yil <= 1 bo'lsa (0001-01-01 default qiymat)
    if (isNaN(d.getTime()) || d.getFullYear() <= 1) {
      if (fallbackDateStr) {
        const fb = new Date(fallbackDateStr);
        if (!isNaN(fb.getTime()) && fb.getFullYear() > 1) {
          return this.formatDisplayDate(fb);
        }
      }
      return 'Yaqinda yuborilgan';
    }

    return this.formatDisplayDate(d);
  }

  formatDisplayDate(d: Date | string): string {
    try {
      const dateObj = typeof d === 'string' ? new Date(d) : d;
      return dateObj.toLocaleString('uz-UZ', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return String(d);
    }
  }

  // --- Customer Pay Modal Actions ---
  openPayModal(order: Order): void {
    this.currentOrderForPay = order;
    const existingPayment = this.getPayment(order.id);
    this.currentOrderExistingChekUrl = existingPayment?.chekUrl || null;
    this.selectedFilePreview = existingPayment?.chekUrl 
      ? this.paymentService.getFullImageUrl(existingPayment.chekUrl) 
      : null;
    this.payTransactionNote = existingPayment?.transactionNote || '';
    this.selectedReceiptFile = null;
    this.showPayModal = true;
  }

  closePayModal(): void {
    this.showPayModal = false;
    this.currentOrderForPay = null;
    this.selectedReceiptFile = null;
    this.selectedFilePreview = null;
    this.payTransactionNote = '';
    this.isSubmittingPay = false;
  }

  copyCardNumber(num: string): void {
    navigator.clipboard.writeText(num);
    this.notification.success('Karta raqami nusxalandi: ' + num);
  }

  onFileDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.handleReceiptFile(file);
    }
  }

  removeSelectedFile(event: Event): void {
    event.stopPropagation();
    this.selectedReceiptFile = null;
    this.selectedFilePreview = null;
    this.currentOrderExistingChekUrl = null;
  }

  private handleReceiptFile(file: File): void {
    if (file.size > 5 * 1024 * 1024) {
      this.notification.error('Fayl hajmi 5MB dan oshmasligi kerak!');
      return;
    }
    this.selectedReceiptFile = file;
    const reader = new FileReader();
    reader.onload = () => {
      this.selectedFilePreview = reader.result as string;
    };
    reader.readAsDataURL(file);
  }

  onFileSelected(event: any): void {
    const file = event?.target?.files?.[0];
    if (file) {
      this.handleReceiptFile(file);
    }
  }

  submitPaymentReceipt(): void {
    if (!this.currentOrderForPay) return;

    this.isSubmittingPay = true;
    const order = this.currentOrderForPay;
    const userId = this.authState.currentCustomerId() || 0;

    if (this.selectedReceiptFile) {
      this.paymentService.uploadReceipt(this.selectedReceiptFile).subscribe({
        next: (chekUrl) => {
          this.executeCreatePayment(order, userId, chekUrl);
        },
        error: (err) => {
          this.isSubmittingPay = false;
          this.notification.error(err?.error?.message || 'Chek rasmini yuklashda xatolik yuz berdi.');
        }
      });
    } else if (this.currentOrderExistingChekUrl) {
      this.executeCreatePayment(order, userId, this.currentOrderExistingChekUrl);
    } else {
      this.isSubmittingPay = false;
      this.notification.error('Iltimos, to‘lov cheki rasmini yuklang!');
    }
  }

  private executeCreatePayment(order: Order, userId: number, chekUrl: string): void {
    this.paymentService.createPayment({
      orderId: order.id,
      userId: userId,
      amount: order.totalPrice,
      chekUrl: chekUrl,
      transactionNote: this.payTransactionNote,
      paymentMethod: 'CardTransfer'
    }).subscribe({
      next: (payment) => {
        this.isSubmittingPay = false;
        this.orderPayments.set(order.id, payment);
        this.notification.success('To‘lov cheki muvaffaqiyatli yuborildi! Admin tasdiqlashi kutilmoqda.');
        this.closePayModal();
      },
      error: (err) => {
        this.isSubmittingPay = false;
        this.notification.error(err?.error?.message || 'To‘lovni yuborishda xatolik yuz berdi.');
      }
    });
  }

  // --- Admin Review Actions ---
  openAdminPaymentModal(order: Order): void {
    this.adminReviewOrder = order;
    this.adminReviewPayment = this.getPayment(order.id) || null;
    this.adminRejectReason = '';
    this.adminPaymentNote = '';
    this.adminPaymentMethod = 'Cash';
    this.showRejectReasonInput = false;
    this.showAdminPaymentModal = true;

    const cId = order.customerId || (order as any).userId;
    if (cId && !this.customersMap.has(cId)) {
      this.customerService.getById(cId).subscribe({
        next: (cust) => {
          if (cust) this.customersMap.set(cust.id, cust);
        },
        error: () => {}
      });
    }
  }

  closeAdminPaymentModal(): void {
    this.showAdminPaymentModal = false;
    this.adminReviewOrder = null;
    this.adminReviewPayment = null;
    this.showRejectReasonInput = false;
    this.adminRejectReason = '';
    this.adminPaymentNote = '';
    this.isAdminProcessingPayment = false;
  }

  confirmApprovePayment(): void {
    if (!this.adminReviewPayment) return;

    this.isAdminProcessingPayment = true;
    const paymentId = this.adminReviewPayment.id;
    const adminId = this.authState.currentUser()?.userId || this.authState.currentCustomerId() || 1;

    this.paymentService.confirmPayment(paymentId, adminId).subscribe({
      next: (updatedPayment) => {
        this.isAdminProcessingPayment = false;
        this.orderPayments.set(updatedPayment.orderId, updatedPayment);
        if (this.adminReviewOrder) {
          this.adminReviewOrder.status = OrderStatus.Confirmed;
        }
        this.notification.success('To‘lov muvaffaqiyatli tasdiqlandi! Buyurtma holati yangilandi.');
        this.closeAdminPaymentModal();
      },
      error: (err) => {
        this.isAdminProcessingPayment = false;
        this.notification.error(err?.error?.message || 'To‘lovni tasdiqlashda xatolik yuz berdi.');
      }
    });
  }

  confirmRejectPayment(): void {
    if (!this.adminReviewPayment || !this.adminRejectReason.trim()) {
      this.notification.error('Iltimos, rad etish sababini kiriting!');
      return;
    }

    this.isAdminProcessingPayment = true;
    const paymentId = this.adminReviewPayment.id;
    const adminId = this.authState.currentUser()?.userId || this.authState.currentCustomerId() || 1;

    this.paymentService.rejectPayment({
      paymentId: paymentId,
      reason: this.adminRejectReason.trim()
    }, adminId).subscribe({
      next: (updatedPayment) => {
        this.isAdminProcessingPayment = false;
        this.orderPayments.set(updatedPayment.orderId, updatedPayment);
        this.notification.warning('To‘lov rad etildi.');
        this.closeAdminPaymentModal();
      },
      error: (err) => {
        this.isAdminProcessingPayment = false;
        this.notification.error(err?.error?.message || 'To‘lovni rad etishda xatolik yuz berdi.');
      }
    });
  }

  confirmCashOrDirectPayment(): void {
    if (!this.adminReviewOrder) return;
    this.isAdminProcessingPayment = true;
    const order = this.adminReviewOrder;
    const adminId = this.authState.currentUser()?.userId || this.authState.currentCustomerId() || 1;

    this.paymentService.createPayment({
      orderId: order.id,
      userId: order.customerId,
      amount: order.totalPrice,
      paymentMethod: this.adminPaymentMethod,
      transactionNote: this.adminPaymentNote || (this.adminPaymentMethod === 'Cash' ? 'Filialda naqd pul to‘landi' : 'Filialda terminal orqali to‘landi')
    }).subscribe({
      next: (createdPayment) => {
        this.paymentService.confirmPayment(createdPayment.id, adminId).subscribe({
          next: (confirmedPayment) => {
            this.isAdminProcessingPayment = false;
            this.orderPayments.set(order.id, confirmedPayment);
            order.status = OrderStatus.Confirmed;
            this.notification.success(`Buyurtma #${order.id} uchun to‘lov qabul qilindi va tasdiqlandi!`);
            this.closeAdminPaymentModal();
          },
          error: (err) => {
            this.isAdminProcessingPayment = false;
            this.notification.error(err?.error?.message || 'To‘lovni tasdiqlashda xatolik yuz berdi.');
          }
        });
      },
      error: (err) => {
        this.isAdminProcessingPayment = false;
        this.notification.error(err?.error?.message || 'To‘lov yaratishda xatolik yuz berdi.');
      }
    });
  }

  // --- Viewer Modal Actions ---
  openViewReceiptModal(order: Order): void {
    const payment = this.getPayment(order.id);
    if (!payment?.chekUrl) return;
    this.viewingReceiptUrl = this.paymentService.getFullImageUrl(payment.chekUrl);
    this.showReceiptViewerModal = true;
  }

  closeReceiptViewerModal(): void {
    this.showReceiptViewerModal = false;
    this.viewingReceiptUrl = '';
  }

  canCancel(status: OrderStatus | number): boolean {
    return status === OrderStatus.Pending || status === OrderStatus.Confirmed;
  }

  getBranchName(branchId: number): string {
    const branch = this.branches.find(b => b.id === branchId);
    return branch ? (branch.name || branch.address || `Filial #${branchId}`) : `Filial #${branchId}`;
  }

  openCancelModal(order: Order): void {
    this.orderToCancel = order;
    this.showCancelModal = true;
  }

  closeCancelModal(): void {
    this.showCancelModal = false;
    this.orderToCancel = null;
  }

  confirmCancelOrder(): void {
    if (!this.orderToCancel) return;

    this.isCancelling = true;
    const orderId = this.orderToCancel.id;

    this.orderService.cancelOrder(orderId).subscribe({
      next: () => {
        this.isCancelling = false;
        this.notification.success(`#${orderId}-buyurtma muvaffaqiyatli bekor qilindi.`);
        const found = this.orders.find(o => o.id === orderId);
        if (found) {
          found.status = OrderStatus.Cancelled;
        }
        this.closeCancelModal();
      },
      error: () => {
        this.isCancelling = false;
      }
    });
  }

  updateStatus(order: Order, newStatus: OrderStatus): void {
    this.orderService.updateStatus(order.id, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
        this.notification.success(`#${order.id}-buyurtma holati yangilandi.`);
      },
      error: () => {}
    });
  }
}
