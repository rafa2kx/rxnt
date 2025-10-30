import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppointmentService } from '../../services/appointment.service';
import { PatientService } from '../../services/patient.service';
import { DoctorService } from '../../services/doctor.service';
import { Appointment } from '../../models/appointment.model';
import { Patient } from '../../models/patient.model';
import { Doctor } from '../../models/doctor.model';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './appointments.component.html',
  styleUrls: ['./appointments.component.css']
})
export class AppointmentsComponent implements OnInit {
  appointments: Appointment[] = [];
  patients: Patient[] = [];
  doctors: Doctor[] = [];
  showForm = false;
  editingAppointment: Appointment | null = null;
  appointment: Appointment = {
    patientId: 0,
    doctorId: 0,
    appointmentDate: '',
    appointmentTime: '',
    reason: '',
    notes: '',
    status: 'Scheduled',
    visitFee: 0
  };

  constructor(
    private appointmentService: AppointmentService,
    private patientService: PatientService,
    private doctorService: DoctorService
  ) {}

  ngOnInit(): void {
    this.loadAppointments();
    this.loadPatients();
    this.loadDoctors();
  }

  loadAppointments(): void {
    this.appointmentService.getAppointments().subscribe(data => {
      this.appointments = data;
    });
  }

  loadPatients(): void {
    this.patientService.getPatients().subscribe(data => {
      this.patients = data;
    });
  }

  loadDoctors(): void {
    this.doctorService.getDoctors().subscribe(data => {
      this.doctors = data;
    });
  }

  addAppointment(): void {
    this.showForm = true;
    this.editingAppointment = null;
    this.resetForm();
  }

  editAppointment(appointment: Appointment): void {
    this.editingAppointment = appointment;
    this.appointment = { ...appointment };
    this.showForm = true;
  }

  saveAppointment(): void {
    // Convert date string to proper format for backend
    const appointmentToSave = {
      ...this.appointment,
      appointmentDate: this.appointment.appointmentDate ? new Date(this.appointment.appointmentDate).toISOString() : ''
    };

    if (this.editingAppointment) {
      this.appointmentService.updateAppointment(this.editingAppointment.id!, appointmentToSave)
        .subscribe(() => {
          this.loadAppointments();
          this.cancelForm();
        });
    } else {
      this.appointmentService.createAppointment(appointmentToSave)
        .subscribe(() => {
          this.loadAppointments();
          this.cancelForm();
        });
    }
  }

  deleteAppointment(id: number): void {
    if (confirm('Are you sure you want to delete this appointment?')) {
      this.appointmentService.deleteAppointment(id)
        .subscribe(() => {
          this.loadAppointments();
        });
    }
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingAppointment = null;
    this.resetForm();
  }

  resetForm(): void {
    this.appointment = {
      patientId: 0,
      doctorId: 0,
      appointmentDate: '',
      appointmentTime: '',
      reason: '',
      notes: '',
      status: 'Scheduled',
      visitFee: 0
    };
  }

  getPatientName(patientId: number): string {
    const patient = this.patients.find(p => p.id === patientId);
    return patient ? `${patient.firstName} ${patient.lastName}` : 'Unknown';
  }

  getDoctorName(doctorId: number): string {
    const doctor = this.doctors.find(d => d.id === doctorId);
    return doctor ? `${doctor.firstName} ${doctor.lastName}` : 'Unknown';
  }
}
