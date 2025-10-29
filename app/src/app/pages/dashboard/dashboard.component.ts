import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PatientService } from '../../services/patient.service';
import { AppointmentService } from '../../services/appointment.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  totalPatients = 0;
  totalAppointments = 0;
  upcomingAppointments = 0;

  constructor(
    private patientService: PatientService,
    private appointmentService: AppointmentService
  ) {}

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.patientService.getPatients().subscribe(patients => {
      this.totalPatients = patients.length;
    });

    this.appointmentService.getAppointments().subscribe(appointments => {
      this.totalAppointments = appointments.length;
      const today = new Date();
      this.upcomingAppointments = appointments.filter(apt => {
        const aptDate = new Date(apt.appointmentDate);
        return aptDate >= today && apt.status === 'Scheduled';
      }).length;
    });
  }
}
