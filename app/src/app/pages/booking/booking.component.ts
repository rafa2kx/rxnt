import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientService } from '../../services/patient.service';
import { DoctorService } from '../../services/doctor.service';
import { AppointmentService } from '../../services/appointment.service';
import { InvoiceService } from '../../services/invoice.service';
import { Patient } from '../../models/patient.model';
import { Doctor } from '../../models/doctor.model';
import { Invoice } from '../../models/invoice.model';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
  
  // Step 3: Payment & Invoice
  invoice: Invoice | null = null;
  paymentMethod: string = 'Credit Card';
  
  currentStep: number = 1;
  
  constructor(
    private patientService: PatientService,
    private doctorService: DoctorService,
    private appointmentService: AppointmentService,
    private invoiceService: InvoiceService
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
      this.appointmentService.scheduleWithInvoice({
        patientId: this.selectedPatient.id!,
        doctorId: this.selectedDoctor.id,
        appointmentDate: this.appointmentDate,
        appointmentTime: this.appointmentTime,
        reason: this.reason,
        visitFee: this.visitFee
      }).subscribe((result: any) => {
        this.invoice = result.invoice;
        this.currentStep = 3;
      });
    }
  }
  
  payInvoice() {
    if (this.invoice) {
      this.invoiceService.markAsPaid(this.invoice.id, this.paymentMethod).subscribe(() => {
        alert('Payment successful!');
        this.reset();
      });
    }
  }
  
  reset() {
    this.currentStep = 1;
    this.selectedPatient = null;
    this.selectedDoctor = null;
    this.appointmentDate = '';
    this.appointmentTime = '';
    this.reason = '';
    this.invoice = null;
    this.paymentMethod = 'Credit Card';
  }
}
