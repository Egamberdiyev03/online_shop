import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Payment, PaymentStatus, CreatePaymentDto, RejectPaymentDto } from '../models/payment.model';
import { ResponseModel, unwrapResult } from '../models/response.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Payment`;

  // 1. Chek rasmini serverga yuklash
  uploadReceipt(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<ResponseModel<string> | string>(`${this.apiUrl}/upload-receipt`, formData).pipe(
      map(res => {
        const unwrapped = unwrapResult(res);
        return typeof unwrapped === 'string' ? unwrapped : (res as any)?.data || '';
      })
    );
  }

  // 2. To'lov yaratish (mijoz chek yuboradi)
  createPayment(dto: CreatePaymentDto): Observable<Payment> {
    return this.http.post<ResponseModel<Payment> | Payment>(`${this.apiUrl}/create`, dto).pipe(
      map(res => unwrapResult(res))
    );
  }

  // 3. Buyurtma bo'yicha to'lov ma'lumotini olish
  getPaymentByOrderId(orderId: number): Observable<Payment | null> {
    return this.http.get<ResponseModel<Payment> | Payment>(`${this.apiUrl}/get-by-order/${orderId}`).pipe(
      map(res => unwrapResult(res) || null)
    );
  }

  // 4. Admin uchun barcha to'lovlar
  getAllPayments(status?: PaymentStatus, branchId?: number): Observable<Payment[]> {
    let url = `${this.apiUrl}/get-all`;
    const params: string[] = [];
    if (status !== undefined && status !== null) {
      params.push(`status=${status}`);
    }
    if (branchId !== undefined && branchId !== null) {
      params.push(`branchId=${branchId}`);
    }
    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }

    return this.http.get<ResponseModel<Payment[]> | Payment[]>(url).pipe(
      map(res => unwrapResult(res) || [])
    );
  }

  // 5. Admin to'lovni tasdiqlashi
  confirmPayment(paymentId: number, adminId: number): Observable<Payment> {
    return this.http.post<ResponseModel<Payment> | Payment>(
      `${this.apiUrl}/confirm/${paymentId}?adminId=${adminId}`,
      null
    ).pipe(
      map(res => unwrapResult(res))
    );
  }

  // 6. Admin to'lovni rad etishi
  rejectPayment(dto: RejectPaymentDto, adminId: number): Observable<Payment> {
    return this.http.post<ResponseModel<Payment> | Payment>(
      `${this.apiUrl}/reject?adminId=${adminId}`,
      dto
    ).pipe(
      map(res => unwrapResult(res))
    );
  }

  // Serverdagi rasm URL-ini to'liq formatga keltirish
  getFullImageUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }
    // API host (masalan http://localhost:5000 yoki https://onlineshopuz.runasp.net)
    const base = environment.apiUrl.replace(/\/api\/?$/, '');
    return `${base}${path.startsWith('/') ? '' : '/'}${path}`;
  }
}
