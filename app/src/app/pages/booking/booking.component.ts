import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { PatientService } from '../../services/patient.service';
import { DoctorService } from '../../services/doctor.service';
import { AppointmentService } from '../../services/appointment.service';
import { Patient } from '../../models/patient.model';
import { Doctor } from '../../models/doctor.model';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.css']
})
export class BookingComponent implements OnInit {
  // Step 1: Create Patient
  newPatient: Partial<Patient> = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    dateOfBirth: '',
    gender: 'Male',
    address: ''
  };
  
  // Step 2: Book Appointment
  selectedPatient: Patient | null = null;
  selectedDoctor: Doctor | null = null;
  patients: Patient[] = [];
  doctors: Doctor[] = [];
  
  appointmentDate: string = '';
  appointmentTime: string = '';
  reason: string = '';
  visitFee: number = 150;
  
  currentStep: number = 1;
  appointmentCreated: boolean = false;
  createdAppointment: any = null;
  
  constructor(
    private patientService: PatientService,
    private doctorService: DoctorService,
    private appointmentService: AppointmentService,
    private router: Router
  ) {}
  
  ngOnInit() {
    this.loadPatients();
    this.loadDoctors();
  }
  
  loadPatients() {
    this.patientService.getPatients().subscribe(patients => {
      this.patients = patients;
    });
  }
  
  loadDoctors() {
    this.doctorService.getDoctors().subscribe(doctors => {
      this.doctors = doctors;
    });
  }
  
  createPatient() {
    if (this.newPatient.firstName && this.newPatient.lastName) {
      this.patientService.createPatient(this.newPatient as Patient).subscribe(patient => {
        this.selectedPatient = patient;
        this.currentStep = 2;
        this.loadPatients();
      });
    }
  }
  
  selectPatient(patient: Patient) {
    this.selectedPatient = patient;
    this.currentStep = 2;
  }
  
  selectDoctor(doctor: Doctor) {
    this.selectedDoctor = doctor;
  }
  
  bookAppointment() {
    if (this.selectedPatient && this.selectedDoctor && this.appointmentDate && this.appointmentTime) {
      this.appointmentService.createAppointment({
        patientId: this.selectedPatient.id!,
        doctorId: this.selectedDoctor.id,
        appointmentDate: this.appointmentDate,
        appointmentTime: this.appointmentTime,
        reason: this.reason,
        visitFee: this.visitFee,
        notes: '',
        status: ''
      }).subscribe((appointment: any) => {
        this.createdAppointment = appointment;
        this.appointmentCreated = true;
        this.currentStep = 3;
      });
    }
  }
  
  goToInvoices() {
    this.router.navigate(['/invoices']);
  }
  
  bookAnother() {
    this.reset();
  }
  
  reset() {
    this.currentStep = 1;
    this.selectedPatient = null;
    this.selectedDoctor = null;
    this.appointmentDate = '';
    this.appointmentTime = '';
    this.reason = '';
    this.appointmentCreated = false;
    this.createdAppointment = null;
    this.newPatient = {
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      dateOfBirth: '',
      gender: 'Male',
      address: ''
    };
  }
}
