import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DoctorService } from '../../services/doctor.service';
import { Doctor } from '../../models/doctor.model';

@Component({
  selector: 'app-doctors',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './doctors.component.html',
  styleUrls: ['./doctors.component.css']
})
export class DoctorsComponent implements OnInit {
  doctors: Doctor[] = [];
  showForm = false;
  editingDoctor: Doctor | null = null;
  doctor: Partial<Doctor> = {
    firstName: '',
    lastName: '',
    specialty: '',
    email: '',
    phone: '',
    licenseNumber: '',
    isActive: true
  };

  constructor(private doctorService: DoctorService) {}

  ngOnInit(): void {
    this.loadDoctors();
  }

  loadDoctors(): void {
    this.doctorService.getDoctors().subscribe(data => {
      this.doctors = data;
    });
  }

  addDoctor(): void {
    this.showForm = true;
    this.editingDoctor = null;
    this.resetForm();
  }

  editDoctor(doctor: Doctor): void {
    this.editingDoctor = doctor;
    this.doctor = { ...doctor };
    this.showForm = true;
  }

  saveDoctor(): void {
    if (this.editingDoctor) {
      this.doctorService.updateDoctor(this.editingDoctor.id!, this.doctor as Doctor)
        .subscribe(() => {
          this.loadDoctors();
          this.cancelForm();
        });
    } else {
      this.doctorService.createDoctor(this.doctor as Doctor)
        .subscribe(() => {
          this.loadDoctors();
          this.cancelForm();
        });
    }
  }

  deleteDoctor(id: number): void {
    if (confirm('Are you sure you want to delete this doctor?')) {
      this.doctorService.deleteDoctor(id)
        .subscribe(() => {
          this.loadDoctors();
        });
    }
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingDoctor = null;
    this.resetForm();
  }

  resetForm(): void {
    this.doctor = {
      firstName: '',
      lastName: '',
      specialty: '',
      email: '',
      phone: '',
      licenseNumber: '',
      isActive: true
    };
  }
}
