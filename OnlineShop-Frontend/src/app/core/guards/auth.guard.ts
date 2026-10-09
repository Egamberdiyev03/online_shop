import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from '../services/auth-state.service';
import { NotificationService } from '../services/notification.service';

/**
 * Faqat tizimga kirgan foydalanuvchilar uchun ruxsat beruvchi guard
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const notification = inject(NotificationService);

  if (authState.isAuthenticated()) {
    return true;
  }

  notification.warning("Bu sahifani ko'rish uchun avval tizimga kiring.");
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/**
 * Faqat mehmonlar (tizimga kirmaganlar) uchun ruxsat beruvchi guard (Login, Register uchun)
 */
export const guestGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  if (authState.isAuthenticated()) {
    return router.createUrlTree(['/products']);
  }

  return true;
};

/**
 * Faqat Admin yoki Menejerlar uchun ruxsat beruvchi guard
 */
export const adminGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const notification = inject(NotificationService);

  const role = authState.userRole();
  // SuperAdmin, CompanyAdmin va BranchManager admin paneliga kirishi mumkin
  const isAdmin = role === 'SuperAdmin' || role === 'CompanyAdmin' || role === 'BranchManager';

  if (isAdmin) {
    return true;
  }

  notification.error("Ushbu bo'limga faqat administratorlar kirishi mumkin.");
  return router.createUrlTree(['/products']);
};

/**
 * Faqat SuperAdmin uchun ruxsat beruvchi guard (Kategoriyalar, barcha Kompaniyalar boshqaruvi)
 */
export const superAdminGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const notification = inject(NotificationService);

  if (authState.isSuperAdmin()) {
    return true;
  }

  notification.error("Ushbu bo'limga faqat SuperAdmin kirishi mumkin.");
  return router.createUrlTree(['/products']);
};

/**
 * Faqat Customer (oddiy mijoz) uchun ruxsat beruvchi guard (Savat, Buyurtma berish)
 * CompanyAdmin, BranchAdmin, SuperAdmin bu sahifaga kira olmaydi
 */
export const customerGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const notification = inject(NotificationService);

  // Agar foydalanuvchi autentifikatsiya qilinmagan bo'lsa - login sahifasiga yo'naltirish
  if (!authState.isAuthenticated()) {
    notification.warning("Bu sahifani ko'rish uchun avval tizimga kiring.");
    return router.createUrlTree(['/login']);
  }

  // Agar foydalanuvchi admin roli bo'lsa - ruxsat bermaslik
  if (authState.isAdminRole()) {
    notification.warning("Savat faqat mijozlar uchun mavjud. Admin sifatida savatdan foydalana olmaysiz.");
    return router.createUrlTree(['/products']);
  }

  return true;
};
