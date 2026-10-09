import { Injectable } from '@angular/core';
import * as L from 'leaflet';

// Leaflet marker icon fix (CDN orqali)
const iconRetinaUrl = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png';
const iconUrl = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png';
const shadowUrl = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png';

L.Marker.prototype.options.icon = L.icon({
  iconRetinaUrl,
  iconUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  tooltipAnchor: [16, -28],
  shadowSize: [41, 41]
});

export interface MapLocation {
  latitude: number;
  longitude: number;
  address?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MapService {
  // Toshkent markazi (default)
  readonly defaultCenter: MapLocation = {
    latitude: 41.2995,
    longitude: 69.2401,
    address: 'Toshkent, O\'zbekiston'
  };

  readonly defaultZoom = 12;
  readonly markerZoom = 15;

  /**
   * Yangi xarita yaratish
   */
  createMap(elementId: string, center?: MapLocation, zoom?: number): L.Map {
    const loc = center || this.defaultCenter;
    const z = zoom || this.defaultZoom;

    const map = L.map(elementId).setView([loc.latitude, loc.longitude], z);

    // OpenStreetMap tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    return map;
  }

  /**
   * Marker qo'shish
   */
  addMarker(map: L.Map, location: MapLocation, popupContent?: string): L.Marker {
    const marker = L.marker([location.latitude, location.longitude]).addTo(map);

    if (popupContent) {
      marker.bindPopup(popupContent);
    }

    return marker;
  }

  /**
   * Filial uchun custom marker
   */
  addBranchMarker(
    map: L.Map,
    location: MapLocation,
    name: string,
    address: string,
    onClick?: () => void
  ): L.Marker {
    const popupContent = `
      <div style="min-width: 180px;">
        <h4 style="margin: 0 0 8px; font-size: 14px; color: #1e293b;">${name}</h4>
        <p style="margin: 0; font-size: 12px; color: #64748b;">📍 ${address}</p>
      </div>
    `;

    const marker = this.addMarker(map, location, popupContent);

    if (onClick) {
      marker.on('click', onClick);
    }

    return marker;
  }

  /**
   * Yetkazish joyi uchun maxsus marker (qizil)
   */
  createDeliveryIcon(): L.Icon {
    return L.icon({
      iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
      shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
      iconSize: [25, 41],
      iconAnchor: [12, 41],
      popupAnchor: [1, -34],
      shadowSize: [41, 41]
    });
  }

  /**
   * Xaritani markazlashtirish
   */
  centerMap(map: L.Map, location: MapLocation, zoom?: number): void {
    map.setView([location.latitude, location.longitude], zoom || this.markerZoom);
  }

  /**
   * Foydalanuvchi joylashuvini olish (Geolocation API)
   */
  getCurrentLocation(): Promise<MapLocation> {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        reject(new Error('Geolocation qo\'llab-quvvatlanmaydi'));
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude
          });
        },
        (error) => {
          console.warn('Geolocation xatosi:', error.message);
          // Xato bo'lsa default lokatsiya qaytarish
          resolve(this.defaultCenter);
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 300000 // 5 daqiqa cache
        }
      );
    });
  }

  /**
   * Ikki nuqta orasidagi masofani hisoblash (km)
   */
  calculateDistance(loc1: MapLocation, loc2: MapLocation): number {
    const R = 6371; // Yer radiusi (km)
    const dLat = this.toRad(loc2.latitude - loc1.latitude);
    const dLon = this.toRad(loc2.longitude - loc1.longitude);
    const a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(this.toRad(loc1.latitude)) *
        Math.cos(this.toRad(loc2.latitude)) *
        Math.sin(dLon / 2) *
        Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return Math.round(R * c * 10) / 10; // 1 raqam aniqlikda
  }

  private toRad(deg: number): number {
    return deg * (Math.PI / 180);
  }

  /**
   * Barcha markerlarni ko'rsatadigan bounds
   */
  fitBounds(map: L.Map, locations: MapLocation[]): void {
    if (locations.length === 0) return;

    const bounds = L.latLngBounds(
      locations.map(loc => [loc.latitude, loc.longitude] as [number, number])
    );
    map.fitBounds(bounds, { padding: [50, 50] });
  }
}
