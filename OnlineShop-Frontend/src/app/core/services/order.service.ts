import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order, OrderStatus } from '../models/order.model';
import { ResponseModel, unwrapResult } from '../models/response.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Order`;

  createOrder(
    customerId: number,
    branchId: number,
    deliveryAddress?: string,
    deliveryLatitude?: number,
    deliveryLongitude?: number
  ): Observable<boolean> {
    // Query params
    let url = `${this.apiUrl}/Create?userId=${customerId}&branchId=${branchId}`;

    // Yetkazish ma'lumotlari (agar mavjud bo'lsa)
    if (deliveryAddress) {
      url += `&deliveryAddress=${encodeURIComponent(deliveryAddress)}`;
    }
    if (deliveryLatitude !== undefined) {
      url += `&deliveryLatitude=${deliveryLatitude}`;
    }
    if (deliveryLongitude !== undefined) {
      url += `&deliveryLongitude=${deliveryLongitude}`;
    }

    return this.http.post<ResponseModel<boolean> | boolean>(url, null).pipe(
      map(res => {
        if (res && typeof res === 'object') {
          const resp = res as any;
          if (resp.statusCode && resp.statusCode >= 400) {
            throw new Error(resp.message || "Buyurtma berishda xatolik yuz berdi.");
          }
        }
        const unwrapped = unwrapResult(res);
        return unwrapped === true || unwrapped === null;
      })
    );
  }

  private normalizeOrder(o: any): Order {
    if (!o) return o;
    return {
      ...o,
      customerId: o.customerId || o.userId || 0,
      userId: o.userId || o.customerId || 0
    };
  }

  private normalizeOrders(orders: any): Order[] {
    const list = Array.isArray(orders) ? orders : [];
    return list.map(o => this.normalizeOrder(o));
  }

  getById(id: number): Observable<Order> {
    return this.http.get<ResponseModel<Order> | Order>(`${this.apiUrl}/GetById?id=${id}`).pipe(
      map(res => this.normalizeOrder(unwrapResult(res)))
    );
  }

  getAll(): Observable<Order[]> {
    return this.http.get<ResponseModel<Order[]> | Order[]>(`${this.apiUrl}/GetAll`).pipe(
      map(res => this.normalizeOrders(unwrapResult(res)))
    );
  }

  getByCustomerId(customerId: number): Observable<Order[]> {
    return this.http.get<ResponseModel<Order[]> | Order[]>(`${this.apiUrl}/GetOrderByCustomerId?customerId=${customerId}`).pipe(
      map(res => this.normalizeOrders(unwrapResult(res)))
    );
  }

  updateStatus(orderId: number, status: OrderStatus): Observable<boolean> {
    return this.http.put<ResponseModel<boolean> | boolean>(
      `${this.apiUrl}/UpdateStatus?orderId=${orderId}&status=${status}`,
      null
    ).pipe(
      map(res => unwrapResult(res) === true)
    );
  }

  cancelOrder(orderId: number): Observable<boolean> {
    return this.http.put<ResponseModel<boolean> | boolean>(
      `${this.apiUrl}/Cancel?orderId=${orderId}`,
      null
    ).pipe(
      map(res => unwrapResult(res) === true)
    );
  }

  getOrdersByBranchId(branchId: number): Observable<Order[]> {
    return this.http.get<ResponseModel<Order[]> | Order[]>(`${this.apiUrl}/GetOrdersByBranchId?branchId=${branchId}`).pipe(
      map(res => this.normalizeOrders(unwrapResult(res)))
    );
  }

  getOrdersByStatus(status: OrderStatus): Observable<Order[]> {
    return this.http.get<ResponseModel<Order[]> | Order[]>(`${this.apiUrl}/GetOrdersByStatus?status=${status}`).pipe(
      map(res => this.normalizeOrders(unwrapResult(res)))
    );
  }
}
