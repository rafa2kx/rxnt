import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DoctorService } from '../../services/doctor.service';
import { Doctor } from '../../models/doctor.model';
import { Appointment } from '../../models/appointment.model';

@Component({
  selector: 'app-doctor-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './doctor-schedule.component.html',
  styleUrls: ['./doctor-schedule.component.css']
})
export class DoctorScheduleComponent implements OnInit {
  doctors: Doctor[] = [];
  selectedDoctorId: number | null = null;
  schedule: Appointment[] = [];
  loading = false;

  constructor(
    private doctorService: DoctorService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDoctors();
  }

  loadDoctors(): void {
    this.doctorService.getDoctors().subscribe(data => {
      this.doctors = data.filter(d => d.isActive);
    });
  }

  onDoctorChange(): void {
    if (this.selectedDoctorId) {
      this.loadSchedule(this.selectedDoctorId);
    } else {
      this.schedule = [];
    }
  }

  loadSchedule(doctorId: number): void {
    this.loading = true;
    this.doctorService.getDoctorSchedule(doctorId).subscribe({
      next: (data) => {
        this.schedule = data.sort((a, b) => {
          const dateA = new Date(`${a.appointmentDate}T${a.appointmentTime}`);
          const dateB = new Date(`${b.appointmentDate}T${b.appointmentTime}`);
          return dateA.getTime() - dateB.getTime();
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading schedule:', error);
        this.loading = false;
        this.schedule = [];
      }
    });
  }

  getSelectedDoctorName(): string {
    if (!this.selectedDoctorId) return '';
    const doctor = this.doctors.find(d => d.id === this.selectedDoctorId);
    return doctor ? `${doctor.firstName} ${doctor.lastName}` : '';
  }

  getPatientName(appointment: Appointment): string {
    if (appointment.patient) {
      return `${appointment.patient.firstName} ${appointment.patient.lastName}`;
    }
    return 'Unknown Patient';
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'scheduled':
        return 'bg-blue-100 text-blue-800';
      case 'completed':
        return 'bg-green-100 text-green-800';
      case 'cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  navigateToDoctors(): void {
    this.router.navigate(['/doctors']);
  }
}

