import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CustomerService } from '../../../core/services/customer.service';
import { ProductService } from '../../../core/services/product.service';
import { OrderService } from '../../../core/services/order.service';
import { CompanyService } from '../../../core/services/company.service';
import { AuthStateService } from '../../../core/services/auth-state.service';
import { Order, OrderStatus } from '../../../core/models/order.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgeComponent],
  template: `
    <div class="dashboard-wrapper">
      <!-- Header Section -->
      <div class="dash-header">
        <div class="header-left">
          <div class="header-badge">
            <span class="live-dot"></span>
            <span>{{ dashboardBadgeText }} &middot; {{ authState.userRole()?.toUpperCase() || 'ADMIN' }}</span>
          </div>
          <h1 class="dash-title">Boshqaruv Paneli</h1>
          <p class="dash-subtitle">{{ dashboardSubtitle }}</p>
        </div>

        <div class="header-right">
          <div class="admin-profile-pill">
            <span class="avatar-icon">👑</span>
            <div class="admin-meta">
              <span class="admin-name">{{ authState.userName() || 'Administrator' }}</span>
              <span class="admin-role">{{ authState.userRole() || 'SuperAdmin' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Key Metrics Stats Grid -->
      <div class="stats-grid">
        <!-- 1. Users / Filiallar / Filial Info -->
        <div class="stat-card">
          <div class="stat-icon-box users">
            <span class="stat-emoji">{{ statsFirstCardIcon }}</span>
          </div>
          <div class="stat-details">
            <span class="stat-label">{{ statsFirstCardLabel }}</span>
            <div class="stat-number-row">
              <span class="stat-value">{{ statsFirstCardValue }}</span>
              <span class="stat-tag info">{{ statsFirstCardTag }}</span>
            </div>
          </div>
        </div>

        <!-- 2. Products -->
        <div class="stat-card">
          <div class="stat-icon-box products">
            <span class="stat-emoji">📦</span>
          </div>
          <div class="stat-details">
            <span class="stat-label">{{ productsCardLabel }}</span>
            <div class="stat-number-row">
              <span class="stat-value">{{ totalProducts }}</span>
              <span class="stat-tag warning">Omborda</span>
            </div>
          </div>
        </div>

        <!-- 3. Companies (faqat SuperAdmin uchun) / Buyurtmalar (CompanyAdmin/BranchManager uchun) -->
        <div class="stat-card" *ngIf="authState.isSuperAdmin()">
          <div class="stat-icon-box companies">
            <span class="stat-emoji">🏢</span>
          </div>
          <div class="stat-details">
            <span class="stat-label">Kompaniyalar</span>
            <div class="stat-number-row">
              <span class="stat-value company-val">{{ totalCompanies }}</span>
              <span class="stat-tag purple">Faol</span>
            </div>
          </div>
        </div>

        <div class="stat-card" *ngIf="!authState.isSuperAdmin()">
          <div class="stat-icon-box companies">
            <span class="stat-emoji">📋</span>
          </div>
          <div class="stat-details">
            <span class="stat-label">{{ ordersStatLabel }}</span>
            <div class="stat-number-row">
              <span class="stat-value company-val">{{ totalOrders }}</span>
              <span class="stat-tag purple">Jami</span>
            </div>
          </div>
        </div>

        <!-- 4. Total Revenue -->
        <div class="stat-card">
          <div class="stat-icon-box revenue">
            <span class="stat-emoji">💰</span>
          </div>
          <div class="stat-details">
            <span class="stat-label">{{ revenueCardLabel }}</span>
            <div class="stat-number-row">
              <span class="stat-value revenue-val">{{ totalRevenue | number:'1.0-0' }}</span>
              <span class="currency-tag">so'm</span>
            </div>
          </div>
        </div>
      </div>

      <div class="dashboard-grid">
        <!-- Recent Orders Table -->
        <div class="grid-card recent-orders-card">
          <div class="card-header">
            <h3 class="card-title">Oxirgi Buyurtmalar</h3>
            <a routerLink="/orders" class="view-all-link">Barchasi &rarr;</a>
          </div>
          <div class="table-responsive">
            <table class="dash-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Mijoz</th>
                  <th>Summa</th>
                  <th>Holat</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let order of recentOrders">
                  <td class="fw-bold">#{{ order.id }}</td>
                  <td>Mijoz #{{ order.customerId }}</td>
                  <td class="fw-bold">{{ order.totalPrice | number:'1.0-0' }}</td>
                  <td>
                    <app-status-badge [status]="order.status" [showIcon]="false"></app-status-badge>
                  </td>
                </tr>
                <tr *ngIf="recentOrders.length === 0">
                  <td colspan="4" class="text-center py-4 text-muted">Buyurtmalar mavjud emas</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Sales by Status Chart (CSS-based) -->
        <div class="grid-card status-chart-card">
          <div class="card-header">
            <h3 class="card-title">Buyurtmalar Holati</h3>
          </div>
          <div class="chart-container">
            <div class="status-bars">
              <div class="status-bar-item" *ngFor="let item of statusStats">
                <div class="bar-info">
                  <span class="bar-label">{{ item.label }}</span>
                  <span class="bar-count">{{ item.count }} ta</span>
                </div>
                <div class="bar-track">
                  <div class="bar-fill" [style.width.%]="item.percent" [style.background-color]="item.color"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Action Navigation Hub -->
      <div class="quick-actions-section">
        <div class="section-title-row">
          <h2 class="section-title">⚡ Tezkor O'tish va Boshqaruv</h2>
        </div>

        <div class="actions-grid">
          <!-- Kompaniyalar & Filiallar - faqat SuperAdmin uchun -->
          <a *ngIf="authState.isSuperAdmin()" routerLink="/companies" class="action-card">
            <div class="action-icon-wrap comp-icon">🏢</div>
            <div class="action-content">
              <div class="action-title-row">
                <span class="action-title">Kompaniyalar & Filiallar</span>
                <span class="action-arrow">&rarr;</span>
              </div>
              <p class="action-desc">Kompaniyalar ro'yxati, filiallar ochish va boshqarish</p>
            </div>
          </a>

          <!-- CompanyAdmin uchun o'z kompaniyasi filiallari -->
          <a *ngIf="authState.isCompanyAdmin() && authState.userCompanyId()" [routerLink]="['/companies', authState.userCompanyId()]" class="action-card">
            <div class="action-icon-wrap comp-icon">🏢</div>
            <div class="action-content">
              <div class="action-title-row">
                <span class="action-title">Filiallar</span>
                <span class="action-arrow">&rarr;</span>
              </div>
              <p class="action-desc">O'z kompaniyangizning filiallari</p>
            </div>
          </a>

          <!-- BranchManager uchun o'z filiali -->
          <a *ngIf="authState.isBranchManager() && authState.userBranchId()" [routerLink]="['/branches', authState.userBranchId()]" class="action-card">
            <div class="action-icon-wrap comp-icon">🏪</div>
            <div class="action-content">
              <div class="action-title-row">
                <span class="action-title">Mening Filialim</span>
                <span class="action-arrow">&rarr;</span>
              </div>
              <p class="action-desc">Filial sozlamalari va ma'lumotlari</p>
            </div>
          </a>

          <!-- Kategoriyalar - faqat SuperAdmin uchun -->
          <a *ngIf="authState.isSuperAdmin()" routerLink="/categories" class="action-card">
            <div class="action-icon-wrap cat-icon">🏷️</div>
            <div class="action-content">
              <div class="action-title-row">
                <span class="action-title">Kategoriyalar</span>
                <span class="action-arrow">&rarr;</span>
              </div>
              <p class="action-desc">Mahsulot toifalari va katalog tuzilmasi</p>
            </div>
          </a>

          <!-- Mijozlar & Xodimlar - faqat SuperAdmin uchun -->
          <a *ngIf="authState.isSuperAdmin()" routerLink="/customers" class="action-card">
            <div class="action-icon-wrap user-icon">👥</div>
            <div class="action-content">
              <div class="action-title-row">
                <span class="action-title">Mijozlar & Xodimlar</span>
                <span class="action-arrow">&rarr;</span>
              </div>
              <p class="action-desc">Foydalanuvchilar ro'yxati va huquqlarni boshqarish</p>
            </div>
          </a>

          <!-- Buyurtmalar - barcha adminlar uchun -->
          <a routerLink="/orders" class="action-card">
            <div class="action-icon-wrap order-icon">📋</div>
            <div class="action-content">
              <div class="action-title-row">
                <span class="action-title">{{ ordersCardTitle }}</span>
                <span class="action-arrow">&rarr;</span>
              </div>
              <p class="action-desc">{{ ordersCardDesc }}</p>
            </div>
          </a>

          <!-- Mahsulotlar - barcha adminlar uchun -->
          <a routerLink="/products" class="action-card">
            <div class="action-icon-wrap prod-icon">🛍️</div>
            <div class="action-content">
              <div class="action-title-row">
                <span class="action-title">{{ productsCardTitle }}</span>
                <span class="action-arrow">&rarr;</span>
              </div>
              <p class="action-desc">{{ productsCardDesc }}</p>
            </div>
          </a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-wrapper {
      padding: 2.5rem 1.5rem 3.5rem;
      max-width: 1280px;
      margin: 0 auto;
    }

    /* Header */
    .dash-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 2.5rem;
      flex-wrap: wrap;
      gap: 1.5rem;
    }
    .header-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.3rem 0.85rem;
      background: #eef2ff;
      color: #4f46e5;
      font-size: 0.775rem;
      font-weight: 700;
      border-radius: 9999px;
      margin-bottom: 0.75rem;
      border: 1px solid #c7d2fe;
    }
    .live-dot {
      width: 7px; height: 7px;
      border-radius: 50%;
      background: #10b981;
      box-shadow: 0 0 0 2px rgba(16, 185, 129, 0.3);
    }
    .dash-title {
      font-size: 2.25rem; font-weight: 800; color: #0f172a;
      letter-spacing: -0.03em; margin: 0 0 0.5rem; line-height: 1.2;
    }
    .dash-subtitle { color: #64748b; font-size: 0.95rem; margin: 0; max-width: 600px; line-height: 1.5; }

    /* Stats Grid */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2.5rem;
    }
    .stat-card {
      background: #ffffff;
      border-radius: 16px;
      padding: 1.5rem;
      display: flex;
      align-items: center;
      gap: 1.25rem;
      box-shadow: 0 4px 12px rgba(15, 23, 42, 0.03);
      border: 1px solid #e2e8f0;
      transition: all 0.25s;
    }
    .stat-card:hover { transform: translateY(-4px); box-shadow: 0 12px 24px rgba(15, 23, 42, 0.08); }
    .stat-icon-box { width: 54px; height: 54px; border-radius: 12px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .stat-emoji { font-size: 1.6rem; }
    .stat-icon-box.users { background: #dbeafe; border: 1px solid #bfdbfe; }
    .stat-icon-box.products { background: #fef3c7; border: 1px solid #fde68a; }
    .stat-icon-box.companies { background: #e0e7ff; border: 1px solid #c7d2fe; }
    .stat-icon-box.revenue { background: #dcfce7; border: 1px solid #bbf7d0; }
    .stat-details { flex: 1; }
    .stat-label { font-size: 0.75rem; color: #64748b; font-weight: 700; text-transform: uppercase; margin-bottom: 0.25rem; }
    .stat-value { font-size: 1.6rem; font-weight: 800; color: #0f172a; line-height: 1; }
    .revenue-val { color: #10b981; }
    .stat-tag { font-size: 0.65rem; font-weight: 700; padding: 0.15rem 0.5rem; border-radius: 9999px; margin-left: 0.5rem; }
    .stat-tag.info { background: #e0f2fe; color: #0284c7; }
    .stat-tag.warning { background: #fef3c7; color: #b45309; }
    .stat-tag.purple { background: #ede9fe; color: #6d28d9; }

    /* Dashboard Grid (Charts & Tables) */
    .dashboard-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 1.5rem;
      margin-bottom: 3rem;
    }
    @media (max-width: 992px) { .dashboard-grid { grid-template-columns: 1fr; } }

    .grid-card {
      background: #ffffff;
      border-radius: 20px;
      padding: 1.5rem;
      border: 1px solid #e2e8f0;
      box-shadow: 0 4px 12px rgba(15, 23, 42, 0.03);
    }
    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.25rem;
    }
    .card-title { font-size: 1.1rem; font-weight: 800; color: #0f172a; margin: 0; }
    .view-all-link { font-size: 0.85rem; font-weight: 700; color: #4f46e5; text-decoration: none; }

    .dash-table { width: 100%; border-collapse: collapse; }
    .dash-table th { text-align: left; font-size: 0.75rem; color: #64748b; font-weight: 700; padding: 0.75rem 0; border-bottom: 1px solid #f1f5f9; }
    .dash-table td { padding: 0.85rem 0; font-size: 0.875rem; border-bottom: 1px dashed #f1f5f9; }
    .fw-bold { font-weight: 700; }

    /* Status Chart Bars */
    .status-bars { display: flex; flex-direction: column; gap: 1.25rem; }
    .status-bar-item { display: flex; flex-direction: column; gap: 0.5rem; }
    .bar-info { display: flex; justify-content: space-between; font-size: 0.85rem; font-weight: 700; }
    .bar-label { color: #334155; }
    .bar-count { color: #64748b; }
    .bar-track { height: 8px; background: #f1f5f9; border-radius: 4px; overflow: hidden; }
    .bar-fill { height: 100%; border-radius: 4px; transition: width 0.8s ease-in-out; }

    /* Quick Actions */
    .quick-actions-section { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 20px; padding: 2rem; }
    .section-title { font-size: 1.25rem; font-weight: 800; color: #0f172a; margin-bottom: 1.5rem; }
    .actions-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.25rem; }
    .action-card { display: flex; align-items: flex-start; gap: 1rem; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 14px; padding: 1.25rem; text-decoration: none; transition: all 0.2s; }
    .action-card:hover { border-color: #4f46e5; transform: translateY(-3px); box-shadow: 0 10px 20px rgba(79, 70, 229, 0.08); }
    .action-icon-wrap { width: 44px; height: 44px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.35rem; background: #f1f5f9; }
    .action-title { font-size: 0.95rem; font-weight: 700; color: #0f172a; }
    .action-arrow { color: #94a3b8; transition: transform 0.2s; }
    .action-card:hover .action-arrow { transform: translateX(4px); color: #4f46e5; }
    .action-desc { font-size: 0.8rem; color: #64748b; margin-top: 0.35rem; line-height: 1.4; }

    .admin-profile-pill { display: flex; align-items: center; gap: 0.75rem; background: white; border: 1px solid #e2e8f0; padding: 0.5rem 1rem; border-radius: 12px; }
    .avatar-icon { font-size: 1.4rem; background: #f1f5f9; width: 38px; height: 38px; border-radius: 50%; display: flex; align-items: center; justify-content: center; }
    .admin-meta { display: flex; flex-direction: column; }
    .admin-name { font-size: 0.875rem; font-weight: 700; color: #0f172a; }
    .admin-role { font-size: 0.7rem; color: #ef4444; font-weight: 700; }

    /* ========================================================
       DARK THEME STYLES (TUNGI REJIM)
       ======================================================== */
    :host-context(body.dark-theme) .dash-title,
    :host-context(.dark-theme) .dash-title {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .dash-subtitle,
    :host-context(.dark-theme) .dash-subtitle {
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .header-badge,
    :host-context(.dark-theme) .header-badge {
      background: #1e1b4b;
      color: #a5b4fc;
      border-color: #3730a3;
    }
    :host-context(body.dark-theme) .admin-profile-pill,
    :host-context(.dark-theme) .admin-profile-pill {
      background: #151e32;
      border-color: #22304d;
    }
    :host-context(body.dark-theme) .avatar-icon,
    :host-context(.dark-theme) .avatar-icon {
      background: #1e293b;
    }
    :host-context(body.dark-theme) .admin-name,
    :host-context(.dark-theme) .admin-name {
      color: #f8fafc;
    }

    /* Dark Mode - Stat Cards */
    :host-context(body.dark-theme) .stat-card,
    :host-context(.dark-theme) .stat-card {
      background: #151e32;
      border-color: #22304d;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
    }
    :host-context(body.dark-theme) .stat-card:hover,
    :host-context(.dark-theme) .stat-card:hover {
      border-color: #3b82f6;
      box-shadow: 0 12px 24px rgba(0, 0, 0, 0.6);
    }
    :host-context(body.dark-theme) .stat-label,
    :host-context(.dark-theme) .stat-label {
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .stat-value,
    :host-context(.dark-theme) .stat-value {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .revenue-val,
    :host-context(.dark-theme) .revenue-val {
      color: #34d399;
    }
    :host-context(body.dark-theme) .currency-tag,
    :host-context(.dark-theme) .currency-tag {
      color: #94a3b8;
    }

    /* Dark Mode - Stat Icon Boxes */
    :host-context(body.dark-theme) .stat-icon-box.users,
    :host-context(.dark-theme) .stat-icon-box.users {
      background: #1e3a8a;
      border-color: #2563eb;
    }
    :host-context(body.dark-theme) .stat-icon-box.products,
    :host-context(.dark-theme) .stat-icon-box.products {
      background: #78350f;
      border-color: #d97706;
    }
    :host-context(body.dark-theme) .stat-icon-box.companies,
    :host-context(.dark-theme) .stat-icon-box.companies {
      background: #312e81;
      border-color: #4f46e5;
    }
    :host-context(body.dark-theme) .stat-icon-box.revenue,
    :host-context(.dark-theme) .stat-icon-box.revenue {
      background: #064e3b;
      border-color: #10b981;
    }

    /* Dark Mode - Dashboard Grid (Oxirgi Buyurtmalar & Buyurtmalar Holati) */
    :host-context(body.dark-theme) .grid-card,
    :host-context(.dark-theme) .grid-card {
      background: #151e32;
      border-color: #22304d;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
    }
    :host-context(body.dark-theme) .card-title,
    :host-context(.dark-theme) .card-title {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .view-all-link,
    :host-context(.dark-theme) .view-all-link {
      color: #818cf8;
    }
    :host-context(body.dark-theme) .dash-table th,
    :host-context(.dark-theme) .dash-table th {
      color: #94a3b8;
      border-bottom-color: #22304d;
    }
    :host-context(body.dark-theme) .dash-table td,
    :host-context(.dark-theme) .dash-table td {
      color: #cbd5e1;
      border-bottom-color: #1e293b;
    }
    :host-context(body.dark-theme) .dash-table td.fw-bold,
    :host-context(.dark-theme) .dash-table td.fw-bold {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .bar-label,
    :host-context(.dark-theme) .bar-label {
      color: #e2e8f0;
    }
    :host-context(body.dark-theme) .bar-count,
    :host-context(.dark-theme) .bar-count {
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .bar-track,
    :host-context(.dark-theme) .bar-track {
      background: #0b0f19;
    }

    /* Dark Mode - Quick Actions Section */
    :host-context(body.dark-theme) .quick-actions-section,
    :host-context(.dark-theme) .quick-actions-section {
      background: #0f172a;
      border-color: #1e293b;
    }
    :host-context(body.dark-theme) .section-title,
    :host-context(.dark-theme) .section-title {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .action-card,
    :host-context(.dark-theme) .action-card {
      background: #151e32;
      border-color: #22304d;
    }
    :host-context(body.dark-theme) .action-card:hover,
    :host-context(.dark-theme) .action-card:hover {
      background: #1a2540;
      border-color: #6366f1;
      box-shadow: 0 10px 20px rgba(0, 0, 0, 0.4);
    }
    :host-context(body.dark-theme) .action-icon-wrap,
    :host-context(.dark-theme) .action-icon-wrap {
      background: #0f172a;
      border-color: #22304d;
    }
    :host-context(body.dark-theme) .action-title,
    :host-context(.dark-theme) .action-title {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .action-card:hover .action-title,
    :host-context(.dark-theme) .action-card:hover .action-title {
      color: #a5b4fc;
    }
    :host-context(body.dark-theme) .action-desc,
    :host-context(.dark-theme) .action-desc {
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .action-arrow,
    :host-context(.dark-theme) .action-arrow {
      color: #64748b;
    }
    :host-context(body.dark-theme) .action-card:hover .action-arrow,
    :host-context(.dark-theme) .action-card:hover .action-arrow {
      color: #a5b4fc;
    }
  `]
})
export class AdminDashboardComponent implements OnInit {
  totalUsers = 0;
  totalProducts = 0;
  totalCompanies = 0;
  totalRevenue = 0;
  totalBranches = 0;
  totalOrders = 0;
  branchName = ''; // BranchManager uchun filial nomi

  recentOrders: Order[] = [];
  statusStats: { label: string, count: number, percent: number, color: string }[] = [];

  authState = inject(AuthStateService);

  // Getter for dashboard badge text
  get dashboardBadgeText(): string {
    if (this.authState.isSuperAdmin()) return 'Tizim Boshqaruvi';
    if (this.authState.isCompanyAdmin()) return 'Kompaniya Boshqaruvi';
    if (this.authState.isBranchManager()) return 'Filial Boshqaruvi';
    return 'Boshqaruv';
  }

  // Getter for dashboard subtitle
  get dashboardSubtitle(): string {
    if (this.authState.isSuperAdmin()) {
      return "Platformaning umumiy statistikasi, moliyaviy ko'rsatkichlari va tezkor boshqaruv menyusi";
    }
    if (this.authState.isCompanyAdmin()) {
      return "Kompaniyangizning statistikasi, moliyaviy ko'rsatkichlari va tezkor boshqaruv menyusi";
    }
    return "Filialingizning statistikasi, moliyaviy ko'rsatkichlari va tezkor boshqaruv menyusi";
  }

  // Stats first card getters (Users/Filiallar/Filial nomi)
  get statsFirstCardIcon(): string {
    if (this.authState.isSuperAdmin()) return '👥';
    if (this.authState.isCompanyAdmin()) return '🏪';
    return '🏬'; // BranchManager
  }

  get statsFirstCardLabel(): string {
    if (this.authState.isSuperAdmin()) return 'Foydalanuvchilar';
    if (this.authState.isCompanyAdmin()) return 'Filiallar';
    return 'Filial';
  }

  get statsFirstCardValue(): number | string {
    if (this.authState.isSuperAdmin()) return this.totalUsers;
    if (this.authState.isCompanyAdmin()) return this.totalBranches;
    return this.branchName || 'Yuklanmoqda...';
  }

  get statsFirstCardTag(): string {
    if (this.authState.isSuperAdmin()) return 'Mijozlar';
    if (this.authState.isCompanyAdmin()) return 'Faol';
    return 'Faol';
  }

  // Products card label
  get productsCardLabel(): string {
    if (this.authState.isSuperAdmin()) return 'Mahsulotlar';
    if (this.authState.isCompanyAdmin()) return 'Kompaniya Mahsulotlari';
    return 'Filial Mahsulotlari';
  }

  // Orders stat label
  get ordersStatLabel(): string {
    if (this.authState.isCompanyAdmin()) return 'Buyurtmalar';
    return 'Filial Buyurtmalari';
  }

  // Revenue card label
  get revenueCardLabel(): string {
    if (this.authState.isSuperAdmin()) return 'Umumiy Tushum';
    if (this.authState.isCompanyAdmin()) return 'Kompaniya Tushumi';
    return 'Filial Tushumi';
  }

  // Getters for quick action card titles and descriptions
  get ordersCardTitle(): string {
    if (this.authState.isSuperAdmin()) return 'Barcha Buyurtmalar';
    if (this.authState.isCompanyAdmin()) return 'Kompaniya Buyurtmalari';
    return 'Filial Buyurtmalari';
  }

  get ordersCardDesc(): string {
    if (this.authState.isSuperAdmin()) return "Mijozlar buyurtmalari va to'lov nazorati";
    if (this.authState.isCompanyAdmin()) return "O'z kompaniyangizning buyurtmalari";
    return "O'z filialingizning buyurtmalari";
  }

  get productsCardTitle(): string {
    if (this.authState.isSuperAdmin()) return 'Mahsulotlar Bazasi';
    if (this.authState.isCompanyAdmin()) return 'Kompaniya Mahsulotlari';
    return 'Filial Mahsulotlari';
  }

  get productsCardDesc(): string {
    if (this.authState.isSuperAdmin()) return "Tovarlar katalogi, narxlar va qoldiqlar";
    if (this.authState.isCompanyAdmin()) return "O'z kompaniyangizning mahsulotlari";
    return "O'z filialingizning mahsulotlari va ombor qoldiqlari";
  }
  private customerService = inject(CustomerService);
  private productService = inject(ProductService);
  private companyService = inject(CompanyService);
  private orderService = inject(OrderService);

  ngOnInit() {
    if (this.authState.isSuperAdmin()) {
      this.loadSuperAdminStats();
    } else if (this.authState.isCompanyAdmin()) {
      this.loadCompanyAdminStats();
    } else if (this.authState.isBranchManager()) {
      this.loadBranchManagerStats();
    }
  }

  private loadSuperAdminStats(): void {
    // SuperAdmin uchun barcha statistikalar
    this.customerService.getAll().subscribe({
      next: (d) => this.totalUsers = d?.length || 0,
      error: () => {}
    });

    this.productService.getAll().subscribe({
      next: (d) => this.totalProducts = d?.length || 0,
      error: () => {}
    });

    this.companyService.getAllCompanies().subscribe({
      next: (d) => this.totalCompanies = d?.length || 0,
      error: () => {}
    });

    this.orderService.getAll().subscribe({
      next: (orders) => this.processOrders(orders),
      error: () => {}
    });
  }

  private loadCompanyAdminStats(): void {
    const companyId = this.authState.userCompanyId();
    if (!companyId) return;

    // CompanyAdmin uchun faqat o'z kompaniyasi statistikalari
    this.companyService.getBranchesByCompanyId(companyId).subscribe({
      next: (branches) => {
        this.totalBranches = branches?.length || 0;

        // Filiallar orqali mahsulotlarni hisoblash
        if (branches && branches.length > 0) {
          const branchIds = branches.map(b => b.id);
          this.productService.getAll().subscribe({
            next: (products) => {
              // Faqat kompaniya filiallariga tegishli mahsulotlar
              const companyProducts = products?.filter(p => p.companyBranchId && branchIds.includes(p.companyBranchId)) || [];
              this.totalProducts = companyProducts.length;
            },
            error: () => {}
          });
        }
      },
      error: () => {}
    });

    // Buyurtmalarni kompaniya bo'yicha filterlash
    this.orderService.getAll().subscribe({
      next: (orders) => {
        if (orders && Array.isArray(orders)) {
          // Buyurtmalar kompaniya filiallari orqali filterlanadi
          this.companyService.getBranchesByCompanyId(companyId!).subscribe({
            next: (branches) => {
              const branchIds = branches?.map(b => b.id) || [];
              const companyOrders = orders.filter(o => branchIds.includes(o.companyBranchId));
              this.processOrders(companyOrders);
              this.totalOrders = companyOrders.length;
            },
            error: () => this.processOrders(orders)
          });
        }
      },
      error: () => {}
    });
  }

  private loadBranchManagerStats(): void {
    const branchId = this.authState.userBranchId();
    if (!branchId) return;

    // Filial nomini olish
    this.companyService.getBranchById(branchId).subscribe({
      next: (branch) => {
        this.branchName = branch?.name || `Filial #${branchId}`;
      },
      error: () => {
        this.branchName = `Filial #${branchId}`;
      }
    });

    // Faqat shu filial mahsulotlari
    this.productService.getAll().subscribe({
      next: (products) => {
        const branchProducts = products?.filter(p => p.companyBranchId === branchId) || [];
        this.totalProducts = branchProducts.length;
      },
      error: () => {}
    });

    // Faqat shu filial buyurtmalari
    this.orderService.getAll().subscribe({
      next: (orders) => {
        if (orders && Array.isArray(orders)) {
          const branchOrders = orders.filter(o => o.companyBranchId === branchId);
          this.processOrders(branchOrders);
          this.totalOrders = branchOrders.length;
        }
      },
      error: () => {}
    });
  }

  private processOrders(orders: Order[]): void {
    if (orders && Array.isArray(orders)) {
      this.totalRevenue = orders.reduce((sum, order) => sum + (order.totalPrice || 0), 0);

      // Oxirgi 5 ta buyurtma
      this.recentOrders = [...orders]
        .sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime())
        .slice(0, 5);

      // Statuslar bo'yicha hisob-kitob
      this.calculateStatusStats(orders);
    }
  }

  private calculateStatusStats(orders: Order[]) {
    const total = orders.length;
    if (total === 0) return;

    const stats = [
      { status: OrderStatus.Pending, label: 'Kutilmoqda', color: '#f59e0b' },
      { status: OrderStatus.Confirmed, label: 'Tasdiqlangan', color: '#3b82f6' },
      { status: OrderStatus.Shipped, label: 'Jo\'natilgan', color: '#8b5cf6' },
      { status: OrderStatus.Delivered, label: 'Yetkazilgan', color: '#10b981' },
      { status: OrderStatus.Cancelled, label: 'Bekor qilingan', color: '#ef4444' }
    ];

    this.statusStats = stats.map(s => {
      const count = orders.filter(o => o.status === s.status).length;
      return {
        ...s,
        count,
        percent: (count / total) * 100
      };
    });
  }
}

