import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { CompanyService } from '../../../core/services/company.service';
import { CustomerService } from '../../../core/services/customer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthStateService } from '../../../core/services/auth-state.service';
import { CompanyBranch, UpdateCompanyBranchDto } from '../../../core/models/company-branch.model';
import { Product } from '../../../core/models/product.model';
import { Customer } from '../../../core/models/customer.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { MapViewComponent } from '../../../shared/components/map-view/map-view.component';

@Component({
  selector: 'app-branch-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    LoadingSpinnerComponent,
    MapViewComponent
  ],
  template: `
    <div class="branch-detail-wrapper" *ngIf="branch">
      <!-- Top Navigation Link -->
      <div class="page-top-nav">
        <!-- BranchManager uchun Admin panelga, boshqalar uchun kompaniyaga -->
        <a *ngIf="authState.isBranchManager()" routerLink="/admin" class="back-link">
          &larr; Admin Panelga qaytish
        </a>
        <a *ngIf="!authState.isBranchManager()" [routerLink]="['/companies', branch.companyId]" class="back-link">
          &larr; Kompaniyaga qaytish
        </a>
      </div>

      <!-- Header Section -->
      <div class="page-header">
        <div>
          <h1 class="page-title">{{ branch.name }}</h1>
          <p class="page-subtitle">Filial maʻlumotlari, mahsulotlar ombori va xodimlar boshqaruvi</p>
        </div>

        <!-- Tahrirlash tugmalari faqat SuperAdmin va CompanyAdmin uchun, BranchManager faqat ko'radi -->
        <div class="header-actions" *ngIf="authState.isAdminMode() && !authState.isBranchManager()">
          <button class="btn btn-outline-primary" (click)="openAssignModal()" title="Filialga mudir tayinlash">
            <span>👤</span> Mudir Tayinlash
          </button>
          <button class="btn btn-secondary" (click)="openEditModal()" title="Filial maʻlumotlarini tahrirlash">
            <span>✏️</span> Tahrirlash
          </button>
          <button class="btn btn-danger" *ngIf="authState.isSuperAdmin()" (click)="deleteBranch()" title="Filialni oʻchirish">
            <span>🗑️</span> Oʻchirish
          </button>
        </div>
      </div>

      <app-loading-spinner *ngIf="isLoading" message="Filial maʻlumotlari yuklanmoqda..."></app-loading-spinner>

      <div class="branch-content" *ngIf="!isLoading">
        <!-- Branch Header Info Card -->
        <div class="branch-header-card card">
          <div class="branch-info-main">
            <div class="branch-icon-box">🏬</div>
            <div class="branch-meta-wrap">
              <div class="branch-title-row">
                <h2 class="branch-heading">{{ branch.name }}</h2>
                <span class="branch-id-badge">#{{ branch.id }}</span>
                <span class="loc-badge" *ngIf="branch.location">🌐 {{ branch.location }}</span>
              </div>
              <div class="branch-details-row">
                <div class="detail-pill">
                  <span class="pill-icon">📍</span>
                  <span class="pill-text">{{ branch.address }}</span>
                </div>
                <div class="detail-pill" *ngIf="branch.phoneNumber">
                  <span class="pill-icon">📞</span>
                  <span class="pill-text">{{ branch.phoneNumber }}</span>
                </div>
                <div class="detail-pill prod-pill">
                  <span class="pill-icon">📦</span>
                  <span class="pill-text"><strong>{{ products.length }}</strong> ta mahsulot</span>
                </div>
                <div class="detail-pill user-pill">
                  <span class="pill-icon">👥</span>
                  <span class="pill-text"><strong>{{ branchUsers.length }}</strong> ta xodim</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Mini Xarita (faqat koordinatalar bo'lsa) -->
        <div class="branch-map-section card" *ngIf="branch.latitude && branch.longitude">
          <div class="map-section-header">
            <h3 class="map-section-title">Filial joylashuvi</h3>
            <a routerLink="/map" class="view-all-link">Barcha filiallar &rarr;</a>
          </div>
          <app-map-view
            [branches]="[branch]"
            [singleBranch]="true"
            [centerLat]="branch.latitude"
            [centerLng]="branch.longitude"
            height="250px"
            [showEmptyState]="false"
          ></app-map-view>
        </div>

        <!-- Navigation Tabs -->
        <div class="custom-tabs">
          <button 
            class="tab-btn" 
            [class.active]="activeTab === 'products'" 
            (click)="setTab('products')"
          >
            <span class="tab-icon">📦</span>
            <span>Mahsulotlar</span>
            <span class="tab-badge">{{ products.length }}</span>
          </button>

          <button 
            class="tab-btn" 
            [class.active]="activeTab === 'users'" 
            (click)="setTab('users')"
          >
            <span class="tab-icon">👥</span>
            <span>Xodimlar va Mudirlar</span>
            <span class="tab-badge">{{ branchUsers.length }}</span>
          </button>

          <button 
            class="tab-btn" 
            [class.active]="activeTab === 'comments'" 
            (click)="setTab('comments')"
          >
            <span class="tab-icon">⭐</span>
            <span>Sharhlar</span>
          </button>
        </div>

        <!-- TAB 1: Products -->
        <div class="tab-content card" *ngIf="activeTab === 'products'">
          <div class="tab-header-row">
            <div>
              <h3 class="tab-section-title">Filial Mahsulotlari (Ombor Qoldigʻi)</h3>
              <p class="tab-section-subtitle">Ushbu filialda sotuvda mavjud boʻlgan tovarlar roʻyxati</p>
            </div>

            <div class="table-search-box" *ngIf="products.length > 0">
              <span class="search-ico">🔍</span>
              <input 
                type="text" 
                [(ngModel)]="productSearch" 
                placeholder="Nomi boʻyicha qidiruv..." 
                class="search-input"
              />
              <button *ngIf="productSearch" class="clear-btn" (click)="productSearch = ''">&times;</button>
            </div>
          </div>

          <div *ngIf="isProductsLoading" class="tab-loading">Mahsulotlar yuklanmoqda...</div>

          <div class="table-responsive" *ngIf="!isProductsLoading && filteredProducts.length > 0">
            <table class="table custom-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Mahsulot Nomi</th>
                  <th>Narxi</th>
                  <th>Ombordagi Qoldiq</th>
                  <th>Sana</th>
                  <th class="text-right">Amal</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let p of filteredProducts">
                  <td class="fw-bold">#{{ p.id }}</td>
                  <td class="prod-name-cell">
                    <span class="prod-name-text">{{ p.name }}</span>
                  </td>
                  <td class="fw-bold price-cell">{{ p.price | number:'1.0-0' }} soʻm</td>
                  <td>
                    <span class="stock-badge" [class.zero-stock]="p.quantity <= 0">
                      {{ p.quantity > 0 ? (p.quantity + ' dona') : 'Tugagan (0)' }}
                    </span>
                  </td>
                  <td class="date-cell">{{ p.createdAt ? (p.createdAt | date:'dd.MM.yyyy') : '—' }}</td>
                  <td class="text-right">
                    <a [routerLink]="['/products', p.id]" class="btn btn-outline-primary btn-xs">
                      👁️ Koʻrish
                    </a>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div *ngIf="!isProductsLoading && products.length === 0" class="empty-tab-state">
            <div class="empty-icon">📦</div>
            <h4>Filialda mahsulotlar mavjud emas</h4>
            <p>Ushbu filial omboriga hozircha tovarlar biriktirilmagan.</p>
          </div>

          <div *ngIf="!isProductsLoading && products.length > 0 && filteredProducts.length === 0" class="empty-tab-state">
            <h4>Qidiruv boʻyicha mahsulot topilmadi</h4>
            <p>Boshqa qidiruv soʻzini sinab koʻring.</p>
          </div>
        </div>

        <!-- TAB 2: Users & Managers -->
        <div class="tab-content card" *ngIf="activeTab === 'users'">
          <div class="tab-header-row">
            <div>
              <h3 class="tab-section-title">Filial Mudirlari va Xodimlari</h3>
              <p class="tab-section-subtitle">Ushbu filial faoliyatini boshqaruvchi masʻul shaxslar</p>
            </div>
            <button class="btn btn-primary btn-sm" (click)="openAssignModal()" *ngIf="authState.isAdminMode()">
              <span>👤</span> Mudir Tayinlash
            </button>
          </div>

          <div *ngIf="isUsersLoading" class="tab-loading">Xodimlar yuklanmoqda...</div>

          <div class="table-responsive" *ngIf="!isUsersLoading && branchUsers.length > 0">
            <table class="table custom-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Ismi</th>
                  <th>Email</th>
                  <th>Telefon</th>
                  <th>Roli</th>
                  <th *ngIf="authState.isAdminMode()" class="text-right">Amallar</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let u of branchUsers">
                  <td class="fw-bold">#{{ u.id }}</td>
                  <td class="fw-bold user-name-cell">{{ u.name }}</td>
                  <td>{{ u.email }}</td>
                  <td>{{ u.phoneNumber || 'Kiritilmagan' }}</td>
                  <td>
                    <span class="role-pill" [class.manager]="u.role === 'BranchManager'">
                      {{ u.role === 'BranchManager' ? '🏬 Filial Mudiri' : (u.role || 'Xodim') }}
                    </span>
                  </td>
                  <td *ngIf="authState.isAdminMode()" class="text-right">
                    <button class="btn btn-danger btn-xs" (click)="removeManager(u)" title="Mudir vazifasidan ozod qilish">
                      ❌ Vazifadan olish
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div *ngIf="!isUsersLoading && branchUsers.length === 0" class="empty-tab-state">
            <div class="empty-icon">👥</div>
            <h4>Hozircha filialga mudir yoki xodim tayinlanmagan</h4>
            <p>Filial ishini yoʻlga qoʻyish uchun masʻul mudirni biriktiring.</p>
            <button class="btn btn-primary btn-sm" (click)="openAssignModal()" *ngIf="authState.isAdminMode()" style="margin-top: 0.5rem;">
              👤 Birinchi mudirni tayinlash
            </button>
          </div>
        </div>

        <!-- TAB 3: Comments -->
        <div class="tab-content card" *ngIf="activeTab === 'comments'">
          <div class="tab-header-row">
            <div>
              <h3 class="tab-section-title">Filial Mahsulotlariga Sharhlar</h3>
              <p class="tab-section-subtitle">Mijozlar tomonidan qoldirilgan fikr va baholar</p>
            </div>
          </div>
          <div class="empty-tab-state">
            <div class="empty-icon">⭐</div>
            <h4>Sharhlar mavjud emas</h4>
            <p>Ushbu filial tovarlariga hozircha sharhlar qoldirilmagan.</p>
          </div>
        </div>
      </div>

      <!-- Edit Branch Modal -->
      <div class="modal-overlay" *ngIf="showEditModal" (click)="closeEditModal()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3 class="modal-title">Filialni Tahrirlash</h3>
            <button class="modal-close" (click)="closeEditModal()">&times;</button>
          </div>

          <form [formGroup]="editForm" (ngSubmit)="submitEdit()" class="modal-body">
            <div class="form-group">
              <label class="form-label">Filial Nomi *</label>
              <input type="text" class="form-control" formControlName="name" placeholder="Masalan: Chilonzor Filiali" />
              <div class="form-error" *ngIf="editForm.get('name')?.touched && editForm.get('name')?.invalid">
                Filial nomi kamida 2 ta belgidan iborat boʻlishi kerak.
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label class="form-label">Telefon Raqami</label>
                <input type="text" class="form-control" formControlName="phoneNumber" placeholder="+998 90 123 45 67" />
              </div>
              <div class="form-group">
                <label class="form-label">Moʻljal / Joylashuv</label>
                <input type="text" class="form-control" formControlName="location" placeholder="Masalan: Metro bekati yonida" />
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Manzil *</label>
              <input type="text" class="form-control" formControlName="address" placeholder="Toshkent sh., Chilonzor tumani" />
              <div class="form-error" *ngIf="editForm.get('address')?.touched && editForm.get('address')?.invalid">
                Manzil kiritilishi shart.
              </div>
            </div>

            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="closeEditModal()" [disabled]="isSubmittingEdit">
                Bekor qilish
              </button>
              <button type="submit" class="btn btn-primary" [disabled]="editForm.invalid || isSubmittingEdit">
                {{ isSubmittingEdit ? 'Saqlanmoqda...' : 'Saqlash' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Assign Mudir Modal -->
      <div class="modal-overlay" *ngIf="showAssignModal" (click)="closeAssignModal()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3 class="modal-title">Filial Mudirini Tayinlash</h3>
            <button class="modal-close" (click)="closeAssignModal()">&times;</button>
          </div>

          <form [formGroup]="assignForm" (ngSubmit)="submitAssign()" class="modal-body">
            <p class="assign-branch-name">
              Filial: <strong>{{ branch.name }}</strong> (#{{ branch.id }})
            </p>

            <div class="form-group">
              <label class="form-label">Xodim yoki Mudirni tanlang *</label>
              <select class="form-control" formControlName="userId">
                <option [ngValue]="null" disabled>Foydalanuvchini tanlang</option>
                <option *ngFor="let c of allCustomers" [value]="c.id">
                  #{{ c.id }} - {{ c.name }} ({{ c.email }}) — {{ c.role || 'Mijoz' }}
                </option>
              </select>
              <div class="form-error" *ngIf="assignForm.get('userId')?.touched && assignForm.get('userId')?.invalid">
                Foydalanuvchi tanlanishi shart.
              </div>
            </div>

            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="closeAssignModal()" [disabled]="isSubmitting">
                Bekor qilish
              </button>
              <button type="submit" class="btn btn-primary" [disabled]="assignForm.invalid || isSubmitting">
                {{ isSubmitting ? 'Tayinlanmoqda...' : 'Mudir qilib tayinlash' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .branch-detail-wrapper {
      max-width: 1240px;
      margin: 0 auto;
      padding: 2rem 1.5rem 4rem;
    }

    /* Top Nav */
    .page-top-nav {
      margin-bottom: 1.25rem;
    }
    .back-link {
      font-size: 0.875rem;
      font-weight: 700;
      color: #6366f1;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      transition: color 0.2s;
    }
    .back-link:hover {
      color: #4f46e5;
      text-decoration: underline;
    }

    /* Header */
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 2rem;
      gap: 1.5rem;
      flex-wrap: wrap;
    }
    .page-title {
      font-size: 2.25rem;
      font-weight: 800;
      color: #0f172a;
      letter-spacing: -0.03em;
      margin: 0 0 0.5rem;
      line-height: 1.2;
    }
    .page-subtitle {
      color: #64748b;
      font-size: 0.95rem;
      margin: 0;
      line-height: 1.5;
    }
    .header-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    /* Branch Header Card */
    .branch-header-card {
      background: #ffffff;
      border: 1px solid #e2e8f0;
      border-radius: 18px;
      padding: 1.75rem 2rem;
      margin-bottom: 1.5rem;
      box-shadow: 0 4px 14px rgba(15, 23, 42, 0.04);
    }

    /* Branch Map Section */
    .branch-map-section {
      background: #ffffff;
      border: 1px solid #e2e8f0;
      border-radius: 16px;
      padding: 1.25rem;
      margin-bottom: 1.5rem;
      box-shadow: 0 4px 14px rgba(15, 23, 42, 0.04);
    }
    .map-section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    .map-section-title {
      font-size: 1rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0;
    }
    .view-all-link {
      font-size: 0.85rem;
      font-weight: 600;
      color: #4f46e5;
      text-decoration: none;
    }
    .view-all-link:hover {
      text-decoration: underline;
    }

    .branch-info-main {
      display: flex;
      align-items: center;
      gap: 1.5rem;
      flex-wrap: wrap;
    }
    .branch-icon-box {
      width: 64px;
      height: 64px;
      border-radius: 16px;
      background: #ecfdf5;
      border: 1px solid #a7f3d0;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 2rem;
      flex-shrink: 0;
    }
    .branch-meta-wrap {
      flex: 1;
      min-width: 260px;
    }
    .branch-title-row {
      display: flex;
      align-items: center;
      gap: 0.85rem;
      margin-bottom: 0.85rem;
      flex-wrap: wrap;
    }
    .branch-heading {
      font-size: 1.5rem;
      font-weight: 800;
      color: #0f172a;
      margin: 0;
      line-height: 1.2;
    }
    .branch-id-badge {
      font-size: 0.75rem;
      font-weight: 700;
      color: #64748b;
      background: #f1f5f9;
      padding: 0.2rem 0.65rem;
      border-radius: 6px;
    }
    .loc-badge {
      font-size: 0.75rem;
      font-weight: 700;
      color: #047857;
      background: #ecfdf5;
      padding: 0.2rem 0.65rem;
      border-radius: 6px;
      border: 1px solid #a7f3d0;
    }
    .branch-details-row {
      display: flex;
      gap: 0.85rem;
      flex-wrap: wrap;
    }
    .detail-pill {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      color: #475569;
      background: #f8fafc;
      padding: 0.4rem 0.85rem;
      border-radius: 10px;
      border: 1px solid #e2e8f0;
    }
    .prod-pill {
      background: #eff6ff;
      border-color: #bfdbfe;
      color: #1d4ed8;
    }
    .user-pill {
      background: #faf5ff;
      border-color: #e9d5ff;
      color: #7e22ce;
    }

    /* Tabs Navigation */
    .custom-tabs {
      display: flex;
      gap: 0.75rem;
      margin-bottom: 1.5rem;
      background: #f1f5f9;
      padding: 0.35rem;
      border-radius: 14px;
      width: fit-content;
      max-width: 100%;
      overflow-x: auto;
    }
    .tab-btn {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      background: transparent;
      border: none;
      padding: 0.65rem 1.25rem;
      font-size: 0.875rem;
      font-weight: 700;
      color: #64748b;
      cursor: pointer;
      border-radius: 10px;
      transition: all 0.2s;
      white-space: nowrap;
    }
    .tab-btn:hover {
      color: #0f172a;
    }
    .tab-btn.active {
      background: #ffffff;
      color: #4f46e5;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    }
    .tab-badge {
      font-size: 0.72rem;
      font-weight: 800;
      padding: 0.1rem 0.45rem;
      border-radius: 9999px;
      background: #e2e8f0;
      color: #475569;
    }
    .tab-btn.active .tab-badge {
      background: #e0e7ff;
      color: #4f46e5;
    }

    /* Tab Content Card */
    .tab-content {
      padding: 2rem;
      background: #ffffff;
      border: 1px solid #e2e8f0;
      border-radius: 18px;
      box-shadow: 0 4px 14px rgba(15, 23, 42, 0.04);
    }
    .tab-header-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .tab-section-title {
      font-size: 1.25rem;
      font-weight: 800;
      color: #0f172a;
      margin: 0 0 0.25rem;
    }
    .tab-section-subtitle {
      font-size: 0.875rem;
      color: #64748b;
      margin: 0;
    }
    .table-search-box {
      display: flex;
      align-items: center;
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      padding: 0.35rem 0.85rem;
      min-width: 240px;
    }
    .search-ico {
      font-size: 0.95rem;
      margin-right: 0.5rem;
      color: #94a3b8;
    }
    .search-input {
      border: none;
      outline: none;
      background: transparent;
      width: 100%;
      font-size: 0.875rem;
      color: #0f172a;
    }
    .clear-btn {
      background: none;
      border: none;
      color: #94a3b8;
      cursor: pointer;
      font-size: 1.1rem;
    }
    .tab-loading {
      padding: 3rem;
      text-align: center;
      color: #94a3b8;
      font-size: 0.95rem;
    }

    /* Table Styles */
    .table-responsive {
      overflow-x: auto;
    }
    .custom-table {
      width: 100%;
      border-collapse: collapse;
    }
    .custom-table th {
      text-align: left;
      padding: 0.85rem 1rem;
      border-bottom: 1px solid #e2e8f0;
      color: #64748b;
      font-size: 0.75rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }
    .custom-table td {
      padding: 1rem;
      border-bottom: 1px solid #f1f5f9;
      font-size: 0.875rem;
      color: #334155;
      vertical-align: middle;
    }
    .custom-table tr:hover td {
      background-color: #f8fafc;
    }
    .text-right {
      text-align: right;
    }
    .fw-bold {
      font-weight: 700;
      color: #0f172a;
    }
    .prod-name-cell {
      max-width: 280px;
    }
    .prod-name-text {
      font-weight: 600;
      color: #0f172a;
    }
    .price-cell {
      color: #059669;
    }
    .date-cell {
      color: #64748b;
      font-size: 0.8rem;
    }
    .stock-badge {
      background: #ecfdf5;
      color: #047857;
      border: 1px solid #a7f3d0;
      padding: 0.25rem 0.65rem;
      border-radius: 9999px;
      font-weight: 700;
      font-size: 0.775rem;
    }
    .stock-badge.zero-stock {
      background: #fef2f2;
      color: #b91c1c;
      border-color: #fecaca;
    }
    .role-pill {
      background: #f1f5f9;
      color: #475569;
      padding: 0.25rem 0.65rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 700;
    }
    .role-pill.manager {
      background: #dcfce7;
      color: #15803d;
      border: 1px solid #bbf7d0;
    }
    .btn-xs {
      padding: 0.35rem 0.65rem;
      font-size: 0.75rem;
      border-radius: 6px;
      font-weight: 600;
    }

    /* Empty Tab State */
    .empty-tab-state {
      text-align: center;
      padding: 3.5rem 1.5rem;
    }
    .empty-icon {
      font-size: 2.75rem;
      margin-bottom: 0.75rem;
    }
    .empty-tab-state h4 {
      font-size: 1.15rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0 0 0.5rem;
    }
    .empty-tab-state p {
      color: #64748b;
      font-size: 0.875rem;
      max-width: 420px;
      margin: 0 auto;
    }

    /* Modals */
    .modal-overlay {
      position: fixed;
      inset: 0;
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
      max-width: 520px;
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
      overflow: hidden;
    }
    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid #f1f5f9;
    }
    .modal-title {
      font-size: 1.15rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0;
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
    .assign-branch-name {
      background: #eef2ff;
      border: 1px solid #c7d2fe;
      color: #3730a3;
      padding: 0.6rem 0.85rem;
      border-radius: 8px;
      font-size: 0.875rem;
      margin-bottom: 1.25rem;
    }
    .modal-footer {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      padding: 1rem 1.5rem;
      border-top: 1px solid #f1f5f9;
      background: #f8fafc;
    }
    .form-group {
      margin-bottom: 1.25rem;
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }
    @media (max-width: 500px) {
      .form-row { grid-template-columns: 1fr; }
    }
    .form-label {
      display: block;
      font-size: 0.85rem;
      font-weight: 600;
      color: #334155;
      margin-bottom: 0.4rem;
    }
    .form-control {
      width: 100%;
      padding: 0.65rem 0.85rem;
      font-size: 0.875rem;
      border: 1px solid #cbd5e1;
      border-radius: 8px;
      outline: none;
      transition: border 0.2s;
    }
    .form-control:focus {
      border-color: #4f46e5;
      box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.15);
    }
    .form-error {
      color: #ef4444;
      font-size: 0.75rem;
      margin-top: 0.35rem;
    }

    /* ========================================================
       DARK THEME STYLES (TUNGI REJIM)
       ======================================================== */
    :host-context(body.dark-theme) .page-title,
    :host-context(.dark-theme) .page-title {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .page-subtitle,
    :host-context(.dark-theme) .page-subtitle {
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .back-link,
    :host-context(.dark-theme) .back-link {
      color: #a5b4fc;
    }
    :host-context(body.dark-theme) .branch-header-card,
    :host-context(.dark-theme) .branch-header-card {
      background: #151e32;
      border-color: #22304d;
    }
    :host-context(body.dark-theme) .branch-heading,
    :host-context(.dark-theme) .branch-heading {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .branch-icon-box,
    :host-context(.dark-theme) .branch-icon-box {
      background: #064e3b;
      border-color: #047857;
    }
    :host-context(body.dark-theme) .detail-pill,
    :host-context(.dark-theme) .detail-pill {
      background: #0f172a;
      border-color: #22304d;
      color: #cbd5e1;
    }
    :host-context(body.dark-theme) .prod-pill,
    :host-context(.dark-theme) .prod-pill {
      background: #1e1b4b;
      border-color: #3730a3;
      color: #a5b4fc;
    }
    :host-context(body.dark-theme) .user-pill,
    :host-context(.dark-theme) .user-pill {
      background: #3b0764;
      border-color: #581c87;
      color: #d8b4fe;
    }
    :host-context(body.dark-theme) .custom-tabs,
    :host-context(.dark-theme) .custom-tabs {
      background: #0f172a;
      border: 1px solid #22304d;
    }
    :host-context(body.dark-theme) .tab-btn,
    :host-context(.dark-theme) .tab-btn {
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .tab-btn:hover,
    :host-context(.dark-theme) .tab-btn:hover {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .tab-btn.active,
    :host-context(.dark-theme) .tab-btn.active {
      background: #1e293b;
      color: #818cf8;
      box-shadow: none;
    }
    :host-context(body.dark-theme) .tab-badge,
    :host-context(.dark-theme) .tab-badge {
      background: #1e293b;
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .tab-btn.active .tab-badge,
    :host-context(.dark-theme) .tab-btn.active .tab-badge {
      background: #312e81;
      color: #c7d2fe;
    }
    :host-context(body.dark-theme) .tab-content,
    :host-context(.dark-theme) .tab-content {
      background: #151e32;
      border-color: #22304d;
    }
    :host-context(body.dark-theme) .tab-section-title,
    :host-context(.dark-theme) .tab-section-title {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .tab-section-subtitle,
    :host-context(.dark-theme) .tab-section-subtitle {
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .table-search-box,
    :host-context(.dark-theme) .table-search-box {
      background: #0b0f19;
      border-color: #334155;
    }
    :host-context(body.dark-theme) .search-input,
    :host-context(.dark-theme) .search-input {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .custom-table th,
    :host-context(.dark-theme) .custom-table th {
      border-bottom-color: #22304d;
      color: #94a3b8;
    }
    :host-context(body.dark-theme) .custom-table td,
    :host-context(.dark-theme) .custom-table td {
      border-bottom-color: #1e293b;
      color: #cbd5e1;
    }
    :host-context(body.dark-theme) .custom-table tr:hover td,
    :host-context(.dark-theme) .custom-table tr:hover td {
      background-color: #1e293b;
    }
    :host-context(body.dark-theme) .fw-bold,
    :host-context(body.dark-theme) .prod-name-text,
    :host-context(.dark-theme) .fw-bold,
    :host-context(.dark-theme) .prod-name-text {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .price-cell,
    :host-context(.dark-theme) .price-cell {
      color: #34d399;
    }
    :host-context(body.dark-theme) .modal-content,
    :host-context(.dark-theme) .modal-content {
      background: #151e32;
      border-color: #22304d;
    }
    :host-context(body.dark-theme) .modal-title,
    :host-context(.dark-theme) .modal-title {
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .modal-header,
    :host-context(body.dark-theme) .modal-footer,
    :host-context(.dark-theme) .modal-header,
    :host-context(.dark-theme) .modal-footer {
      background: #0f172a;
      border-color: #22304d;
    }
    :host-context(body.dark-theme) .form-label,
    :host-context(.dark-theme) .form-label {
      color: #e2e8f0;
    }
    :host-context(body.dark-theme) .form-control,
    :host-context(.dark-theme) .form-control {
      background: #0b0f19;
      border-color: #334155;
      color: #f8fafc;
    }
    :host-context(body.dark-theme) .assign-branch-name,
    :host-context(.dark-theme) .assign-branch-name {
      background: #1e1b4b;
      border-color: #3730a3;
      color: #c7d2fe;
    }
    :host-context(body.dark-theme) .empty-tab-state h4,
    :host-context(.dark-theme) .empty-tab-state h4 {
      color: #f8fafc;
    }
  `]
})
export class BranchDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private companyService = inject(CompanyService);
  private customerService = inject(CustomerService);
  private notification = inject(NotificationService);
  private fb = inject(FormBuilder);
  authState = inject(AuthStateService);

  branchId!: number;
  branch: CompanyBranch | null = null;
  isLoading = true;

  activeTab: 'products' | 'users' | 'comments' = 'products';
  
  products: Product[] = [];
  productSearch = '';
  isProductsLoading = false;

  branchUsers: Customer[] = [];
  isUsersLoading = false;

  allCustomers: Customer[] = [];
  showAssignModal = false;
  isSubmitting = false;

  // Edit Branch Modal
  showEditModal = false;
  isSubmittingEdit = false;
  editForm!: FormGroup;

  assignForm!: FormGroup;

  get filteredProducts(): Product[] {
    if (!this.productSearch.trim()) {
      return this.products;
    }
    const q = this.productSearch.toLowerCase();
    return this.products.filter(p => 
      p.name.toLowerCase().includes(q) ||
      (p.id && p.id.toString().includes(q))
    );
  }

  ngOnInit(): void {
    this.initForms();

    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.branchId = +id;
        this.loadBranchDetails();
      }
    });
  }

  initForms(): void {
    this.assignForm = this.fb.group({
      userId: [null as number | null, Validators.required]
    });

    this.editForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      phoneNumber: [''],
      address: ['', Validators.required],
      location: ['']
    });
  }

  setTab(tab: 'products' | 'users' | 'comments'): void {
    this.activeTab = tab;
    if (tab === 'users') {
      this.loadUsers();
    }
  }

  loadBranchDetails(): void {
    this.isLoading = true;
    this.companyService.getBranchById(this.branchId).subscribe({
      next: (b) => {
        this.branch = b;
        this.isLoading = false;
        this.loadProducts();
        this.loadUsers();
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadProducts(): void {
    this.isProductsLoading = true;
    this.companyService.getBranchProducts(this.branchId).subscribe({
      next: (prods) => {
        this.products = prods;
        this.isProductsLoading = false;
      },
      error: () => {
        this.isProductsLoading = false;
      }
    });
  }

  loadUsers(): void {
    this.isUsersLoading = true;
    this.customerService.getUsersByBranchId(this.branchId).subscribe({
      next: (users) => {
        this.branchUsers = users;
        this.isUsersLoading = false;
      },
      error: () => {
        this.isUsersLoading = false;
      }
    });
  }

  // --- Edit Branch Actions ---
  openEditModal(): void {
    if (!this.branch) return;
    this.editForm.patchValue({
      name: this.branch.name,
      phoneNumber: this.branch.phoneNumber,
      address: this.branch.address,
      location: this.branch.location
    });
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
  }

  submitEdit(): void {
    if (this.editForm.invalid || !this.branch) return;

    this.isSubmittingEdit = true;
    const val = this.editForm.value;

    const updateDto: UpdateCompanyBranchDto = {
      id: this.branch.id,
      name: val.name,
      phoneNumber: val.phoneNumber || '',
      address: val.address,
      location: val.location || '',
      companyId: this.branch.companyId
    };

    this.companyService.updateBranch(updateDto).subscribe({
      next: (updated) => {
        this.isSubmittingEdit = false;
        this.showEditModal = false;
        this.branch = { ...this.branch, ...updated };
        this.notification.success("Filial maʻlumotlari muvaffaqiyatli saqlandi!");
      },
      error: (err) => {
        this.isSubmittingEdit = false;
        this.notification.error(err?.error?.message || "Filialni yangilashda xatolik yuz berdi.");
      }
    });
  }

  deleteBranch(): void {
    if (!this.branch) return;

    if (!confirm(`Haqiqatan ham "${this.branch.name}" filialini oʻchirmoqchimisiz?`)) {
      return;
    }

    const companyId = this.branch.companyId;

    this.companyService.deleteBranch(this.branch.id).subscribe({
      next: () => {
        this.notification.success("Filial muvaffaqiyatli oʻchirildi.");
        this.router.navigate(['/companies', companyId]);
      },
      error: (err) => {
        this.notification.error(err?.error?.message || "Filialni oʻchirishda xatolik yuz berdi.");
      }
    });
  }

  // --- Assign Manager Actions ---
  openAssignModal(): void {
    this.assignForm.reset();
    this.showAssignModal = true;
    this.customerService.getAll().subscribe({
      next: (list) => {
        this.allCustomers = list;
      }
    });
  }

  closeAssignModal(): void {
    this.showAssignModal = false;
  }

  submitAssign(): void {
    if (this.assignForm.invalid) return;

    this.isSubmitting = true;
    const userId = Number(this.assignForm.value.userId);

    this.customerService.assignBranchManager(userId, this.branchId).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.showAssignModal = false;
        this.notification.success("Foydalanuvchi muvaffaqiyatli filial mudiri etib tayinlandi!");
        this.loadUsers();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.notification.error(err?.error?.message || "Mudir tayinlashda xatolik yuz berdi.");
      }
    });
  }

  removeManager(user: Customer): void {
    if (!confirm(`${user.name} ni filial mudirligidan ozod qilishni xohlaysizmi?`)) return;

    this.customerService.removeBranchManager(user.id).subscribe({
      next: () => {
        this.notification.success("Mudir vazifasidan ozod qilindi.");
        this.loadUsers();
      },
      error: (err) => {
        this.notification.error(err?.error?.message || "Vazifadan olishda xatolik yuz berdi.");
      }
    });
  }
}
