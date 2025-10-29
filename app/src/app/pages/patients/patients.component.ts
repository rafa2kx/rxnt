import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientService } from '../../services/patient.service';
import { Patient } from '../../models/patient.model';

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './patients.component.html',
  styleUrls: ['./patients.component.css']
})
export class PatientsComponent implements OnInit {
  patients: Patient[] = [];
  showForm = false;
  editingPatient: Patient | null = null;
  patient: Patient = {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    email: '',
    phone: '',
    address: '',
    gender: 'Male',
    isActive: true
  };

  constructor(private patientService: PatientService) {}

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.patientService.getPatients().subscribe(data => {
      this.patients = data;
    });
  }

  addPatient(): void {
    this.showForm = true;
    this.editingPatient = null;
    this.resetForm();
  }

  editPatient(patient: Patient): void {
    this.editingPatient = patient;
    this.patient = { ...patient };
    this.showForm = true;
  }

  savePatient(): void {
    if (this.editingPatient) {
      this.patientService.updatePatient(this.editingPatient.id!, this.patient)
        .subscribe(() => {
          this.loadPatients();
          this.cancelForm();
        });
    } else {
      this.patientService.createPatient(this.patient)
        .subscribe(() => {
          this.loadPatients();
          this.cancelForm();
        });
    }
  }

  deletePatient(id: number): void {
    if (confirm('Are you sure you want to delete this patient?')) {
      this.patientService.deletePatient(id)
        .subscribe(() => {
          this.loadPatients();
        });
    }
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingPatient = null;
    this.resetForm();
  }

  resetForm(): void {
    this.patient = {
      firstName: '',
      lastName: '',
      dateOfBirth: '',
      email: '',
      phone: '',
      address: '',
      gender: 'Male',
      isActive: true
    };
  }
}
