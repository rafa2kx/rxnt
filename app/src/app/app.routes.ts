import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'patients', loadComponent: () => import('./pages/patients/patients.component').then(m => m.PatientsComponent) },
  { path: 'doctors', loadComponent: () => import('./pages/doctors/doctors.component').then(m => m.DoctorsComponent) },
  { path: 'appointments', loadComponent: () => import('./pages/appointments/appointments.component').then(m => m.AppointmentsComponent) },
  { path: 'invoices', loadComponent: () => import('./pages/invoices/invoices.component').then(m => m.InvoicesComponent) },
  { path: 'booking', loadComponent: () => import('./pages/booking/booking.component').then(m => m.BookingComponent) }
];
