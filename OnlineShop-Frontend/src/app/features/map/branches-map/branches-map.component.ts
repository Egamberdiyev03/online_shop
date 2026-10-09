import { Component, OnInit, inject, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import * as L from 'leaflet';
import { CompanyService } from '../../../core/services/company.service';
import { MapService, MapLocation } from '../../../core/services/map.service';
import { CompanyBranch } from '../../../core/models/company-branch.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-branches-map',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, LoadingSpinnerComponent],
  template: `
    <div class="map-page">
      <!-- Header -->
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">Filiallar Xaritasi</h1>
          <p class="page-subtitle">Barcha filiallarimizni xaritada ko'ring</p>
        </div>
      </div>

      <app-loading-spinner *ngIf="isLoading" message="Filiallar yuklanmoqda..."></app-loading-spinner>

      <div class="map-layout" *ngIf="!isLoading">
        <!-- Sidebar - Filiallar ro'yxati -->
        <div class="branches-sidebar">
          <div class="sidebar-header">
            <h3>Filiallar</h3>
            <span class="branch-count">{{ branches.length }} ta</span>
          </div>

          <!-- Info: koordinatali filiallar soni -->
          <div class="coords-info" *ngIf="branches.length > 0">
            <span class="coords-badge" [class.has-coords]="branchesWithCoords.length > 0">
              🗺️ {{ branchesWithCoords.length }}/{{ branches.length }} xaritada
            </span>
          </div>

          <!-- Qidiruv -->
          <div class="search-box">
            <span class="search-icon">🔍</span>
            <input
              type="text"
              [(ngModel)]="searchQuery"
              placeholder="Filial qidirish..."
              class="search-input"
            />
          </div>

          <!-- Filiallar ro'yxati -->
          <div class="branches-list">
            <div
              *ngFor="let branch of filteredAllBranches"
              class="branch-card"
              [class.active]="selectedBranch?.id === branch.id"
              [class.no-coords]="!hasCoords(branch)"
              (click)="selectBranch(branch)"
            >
              <div class="branch-icon">{{ hasCoords(branch) ? '📍' : '🏬' }}</div>
              <div class="branch-info">
                <h4 class="branch-name">{{ branch.name }}</h4>
                <p class="branch-address">{{ branch.address }}</p>
                <span class="branch-distance" *ngIf="branch.distance">
                  📍 {{ branch.distance }} km
                </span>
                <span class="no-coords-badge" *ngIf="!hasCoords(branch)">
                  Koordinata yo'q
                </span>
              </div>
              <a
                [routerLink]="['/branches', branch.id]"
                class="view-btn"
                title="Batafsil ko'rish"
                (click)="$event.stopPropagation()"
              >
                &rarr;
              </a>
            </div>

            <div class="empty-state" *ngIf="filteredAllBranches.length === 0">
              <span class="empty-icon">🔍</span>
              <p>Filial topilmadi</p>
            </div>
          </div>
        </div>

        <!-- Xarita -->
        <div class="map-container">
          <div id="branches-map" class="main-map"></div>

          <!-- Koordinata yo'q xabar -->
          <div class="no-coords-overlay" *ngIf="branches.length > 0 && branchesWithCoords.length === 0">
            <div class="no-coords-content">
              <span class="no-coords-icon">🗺️</span>
              <h4>Xaritada ko'rsatish uchun koordinata yo'q</h4>
              <p>Filiallar koordinatalarini kiritish uchun kompaniya sahifasiga o'ting va "Tahrirlash" tugmasini bosing.</p>
            </div>
          </div>

          <!-- Tanlangan filial info (xarita ustida) -->
          <div class="selected-branch-card" *ngIf="selectedBranch">
            <button class="close-btn" (click)="selectedBranch = null">&times;</button>
            <h4>{{ selectedBranch.name }}</h4>
            <p class="address">📍 {{ selectedBranch.address }}</p>
            <p class="phone" *ngIf="selectedBranch.phoneNumber">📞 {{ selectedBranch.phoneNumber }}</p>
            <a [routerLink]="['/branches', selectedBranch.id]" class="detail-link">
              Batafsil ko'rish &rarr;
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .map-page {
      min-height: calc(100vh - 140px);
      background: #f8fafc;
    }

    .page-header {
      background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
      padding: 2rem 1.5rem;
      color: #ffffff;
    }

    .header-content {
      max-width: 1280px;
      margin: 0 auto;
    }

    .page-title {
      font-size: 2rem;
      font-weight: 800;
      margin: 0 0 0.5rem;
    }

    .page-subtitle {
      font-size: 1rem;
      opacity: 0.9;
      margin: 0;
    }

    .map-layout {
      display: flex;
      max-width: 1400px;
      margin: 0 auto;
      height: calc(100vh - 220px);
      padding: 1.5rem;
      gap: 1.5rem;
    }

    /* Sidebar */
    .branches-sidebar {
      width: 360px;
      background: #ffffff;
      border-radius: 16px;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .sidebar-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid #f1f5f9;
    }

    .sidebar-header h3 {
      font-size: 1.1rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0;
    }

    .branch-count {
      background: #eef2ff;
      color: #4f46e5;
      padding: 0.25rem 0.65rem;
      border-radius: 20px;
      font-size: 0.8rem;
      font-weight: 700;
    }

    .coords-info {
      padding: 0 1.5rem;
      margin-bottom: 0.5rem;
    }
    .coords-badge {
      display: inline-block;
      font-size: 0.75rem;
      padding: 0.25rem 0.6rem;
      border-radius: 6px;
      background: #fef3c7;
      color: #92400e;
      font-weight: 600;
    }
    .coords-badge.has-coords {
      background: #dcfce7;
      color: #166534;
    }

    .no-coords-badge {
      display: block;
      font-size: 0.7rem;
      color: #dc2626;
      font-weight: 600;
      margin-top: 0.25rem;
    }
    .branch-card.no-coords {
      opacity: 0.7;
    }
    .branch-card.no-coords .branch-icon {
      background: #fef2f2;
    }

    .search-box {
      padding: 1rem 1.5rem;
      position: relative;
    }

    .search-icon {
      position: absolute;
      left: 2rem;
      top: 50%;
      transform: translateY(-50%);
    }

    .search-input {
      width: 100%;
      padding: 0.65rem 1rem 0.65rem 2.5rem;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      font-size: 0.9rem;
    }

    .search-input:focus {
      outline: none;
      border-color: #4f46e5;
    }

    .branches-list {
      flex: 1;
      overflow-y: auto;
      padding: 0.5rem 1rem 1rem;
    }

    .branch-card {
      display: flex;
      align-items: center;
      gap: 0.875rem;
      padding: 1rem;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.2s;
      border: 1px solid transparent;
      margin-bottom: 0.5rem;
    }

    .branch-card:hover {
      background: #f8fafc;
    }

    .branch-card.active {
      background: #eef2ff;
      border-color: #c7d2fe;
    }

    .branch-icon {
      font-size: 1.75rem;
      width: 48px;
      height: 48px;
      background: #f1f5f9;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .branch-info {
      flex: 1;
      min-width: 0;
    }

    .branch-name {
      font-size: 0.95rem;
      font-weight: 600;
      color: #0f172a;
      margin: 0 0 0.25rem;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .branch-address {
      font-size: 0.8rem;
      color: #64748b;
      margin: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .branch-distance {
      font-size: 0.75rem;
      color: #4f46e5;
      font-weight: 600;
    }

    .view-btn {
      width: 32px;
      height: 32px;
      background: #f1f5f9;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      text-decoration: none;
      color: #64748b;
      font-size: 1rem;
      transition: all 0.2s;
    }

    .view-btn:hover {
      background: #4f46e5;
      color: #ffffff;
    }

    .empty-state {
      text-align: center;
      padding: 2rem;
      color: #64748b;
    }

    .empty-icon {
      font-size: 2.5rem;
      display: block;
      margin-bottom: 0.5rem;
    }

    /* Map Container */
    .map-container {
      flex: 1;
      position: relative;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
    }

    .main-map {
      width: 100%;
      height: 100%;
    }

    .no-coords-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.9);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 500;
    }
    .no-coords-content {
      text-align: center;
      padding: 2rem;
      max-width: 400px;
    }
    .no-coords-icon {
      font-size: 4rem;
      display: block;
      margin-bottom: 1rem;
    }
    .no-coords-content h4 {
      font-size: 1.25rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0 0 0.75rem;
    }
    .no-coords-content p {
      font-size: 0.9rem;
      color: #64748b;
      line-height: 1.5;
      margin: 0;
    }

    .selected-branch-card {
      position: absolute;
      bottom: 24px;
      left: 24px;
      background: #ffffff;
      border-radius: 12px;
      padding: 1.25rem;
      box-shadow: 0 8px 30px rgba(0, 0, 0, 0.15);
      max-width: 300px;
      z-index: 1000;
    }

    .selected-branch-card .close-btn {
      position: absolute;
      top: 8px;
      right: 8px;
      width: 24px;
      height: 24px;
      border: none;
      background: #f1f5f9;
      border-radius: 50%;
      cursor: pointer;
      font-size: 1rem;
      color: #64748b;
    }

    .selected-branch-card h4 {
      font-size: 1rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0 0 0.5rem;
      padding-right: 20px;
    }

    .selected-branch-card .address,
    .selected-branch-card .phone {
      font-size: 0.85rem;
      color: #64748b;
      margin: 0 0 0.35rem;
    }

    .detail-link {
      display: inline-block;
      margin-top: 0.75rem;
      color: #4f46e5;
      font-size: 0.85rem;
      font-weight: 600;
      text-decoration: none;
    }

    .detail-link:hover {
      text-decoration: underline;
    }

    @media (max-width: 900px) {
      .map-layout {
        flex-direction: column;
        height: auto;
      }

      .branches-sidebar {
        width: 100%;
        max-height: 300px;
      }

      .map-container {
        height: 400px;
      }
    }
  `]
})
export class BranchesMapComponent implements OnInit, AfterViewInit, OnDestroy {
  private companyService = inject(CompanyService);
  private mapService = inject(MapService);
  private router = inject(Router);

  branches: CompanyBranch[] = [];
  branchesWithCoords: (CompanyBranch & { distance?: number })[] = [];
  selectedBranch: CompanyBranch | null = null;
  searchQuery = '';
  isLoading = true;

  map?: L.Map;
  markers: Map<number, L.Marker> = new Map();
  userLocation?: MapLocation;

  ngOnInit(): void {
    this.loadBranches();
    this.getUserLocation();
  }

  ngAfterViewInit(): void {
    // Map will be initialized after branches are loaded
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
    }
  }

  get filteredBranches(): (CompanyBranch & { distance?: number })[] {
    if (!this.searchQuery.trim()) {
      return this.branchesWithCoords;
    }
    const q = this.searchQuery.toLowerCase();
    return this.branchesWithCoords.filter(
      b => b.name.toLowerCase().includes(q) || b.address.toLowerCase().includes(q)
    );
  }

  hasCoords(branch: CompanyBranch | null | undefined): boolean {
    if (!branch) return false;
    const lat = Number(branch.latitude);
    const lng = Number(branch.longitude);
    return branch.latitude != null && branch.longitude != null && !isNaN(lat) && !isNaN(lng);
  }

  private normalizeBranch(b: CompanyBranch): CompanyBranch {
    let lat = b.latitude != null ? Number(b.latitude) : undefined;
    let lng = b.longitude != null ? Number(b.longitude) : undefined;

    // Fallback: agar latitude/longitude bo'sh bo'lib, location maydonida "lat, lng" bo'lsa
    if ((lat == null || isNaN(lat) || lng == null || isNaN(lng)) && b.location) {
      const match = b.location.match(/(-?\d+\.?\d*)\s*,\s*(-?\d+\.?\d*)/);
      if (match) {
        const parsedLat = parseFloat(match[1]);
        const parsedLng = parseFloat(match[2]);
        if (!isNaN(parsedLat) && !isNaN(parsedLng)) {
          lat = parsedLat;
          lng = parsedLng;
        }
      }
    }

    return {
      ...b,
      latitude: lat != null && !isNaN(lat) ? lat : undefined,
      longitude: lng != null && !isNaN(lng) ? lng : undefined
    };
  }

  get filteredAllBranches(): (CompanyBranch & { distance?: number })[] {
    if (!this.searchQuery.trim()) {
      // Koordinatalilari birinchi, keyin koordinatasizlar
      return [
        ...this.branchesWithCoords,
        ...this.branches.filter(b => !this.hasCoords(b))
      ];
    }
    const q = this.searchQuery.toLowerCase();
    const filtered = this.branches.filter(
      b => b.name.toLowerCase().includes(q) || b.address.toLowerCase().includes(q)
    );
    // Koordinatalilari birinchi
    return [
      ...filtered.filter(b => this.hasCoords(b)),
      ...filtered.filter(b => !this.hasCoords(b))
    ];
  }

  private loadBranches(): void {
    this.companyService.getAllBranches().subscribe({
      next: (branches) => {
        console.log('[BranchesMap] Backend dan kelgan filiallar:', branches);
        const normalized = (branches || []).map(b => this.normalizeBranch(b));
        this.branches = normalized;
        this.branchesWithCoords = normalized.filter(b => this.hasCoords(b));
        console.log('[BranchesMap] Koordinatali filiallar:', this.branchesWithCoords);
        this.isLoading = false;

        // Xaritani yuklash
        setTimeout(() => this.initMap(), 100);
      },
      error: (err) => {
        console.error('[BranchesMap] Xatolik:', err);
        this.isLoading = false;
      }
    });
  }

  private async getUserLocation(): Promise<void> {
    try {
      this.userLocation = await this.mapService.getCurrentLocation();
      this.calculateDistances();
    } catch (error) {
      console.warn('User location not available');
    }
  }

  private calculateDistances(): void {
    if (!this.userLocation) return;

    this.branchesWithCoords = this.branchesWithCoords.map(branch => ({
      ...branch,
      distance: this.mapService.calculateDistance(this.userLocation!, {
        latitude: branch.latitude!,
        longitude: branch.longitude!
      })
    })).sort((a, b) => (a.distance || 999) - (b.distance || 999));
  }

  private initMap(): void {
    this.map = this.mapService.createMap('branches-map', this.mapService.defaultCenter, 11);

    // Filial markerlarini qo'shish
    this.branchesWithCoords.forEach(branch => {
      const marker = this.mapService.addBranchMarker(
        this.map!,
        { latitude: branch.latitude!, longitude: branch.longitude! },
        branch.name,
        branch.address,
        () => this.selectBranch(branch)
      );
      this.markers.set(branch.id, marker);
    });

    // Barcha markerlarni ko'rsatish
    if (this.branchesWithCoords.length > 0) {
      this.mapService.fitBounds(
        this.map,
        this.branchesWithCoords.map(b => ({
          latitude: b.latitude!,
          longitude: b.longitude!
        }))
      );
    }
  }

  selectBranch(branch: CompanyBranch): void {
    this.selectedBranch = branch;

    if (this.map && this.hasCoords(branch)) {
      this.mapService.centerMap(this.map, {
        latitude: Number(branch.latitude),
        longitude: Number(branch.longitude)
      }, 16);

      // Marker popup ochish
      const marker = this.markers.get(branch.id);
      if (marker) {
        marker.openPopup();
      }
    } else {
      console.log('Bu filialda koordinata mavjud emas');
    }
  }
}
