import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { InvoiceService } from '../../../services/invoice.service';
import { AppointmentService } from '../../../services/appointment.service';
import { Invoice } from '../../../models/invoice.model';
import { Appointment } from '../../../models/appointment.model';

@Component({
  selector: 'app-invoice-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './invoice-create.component.html',
  styleUrls: ['./invoice-create.component.css']
})
export class InvoiceCreateComponent implements OnInit {
  appointments: Appointment[] = [];
  invoice: Partial<Invoice> = {
    appointmentId: 0,
    invoiceNumber: '',
    invoiceDate: new Date(),
    subTotal: 0,
    taxAmount: 0,
    totalAmount: 0,
    status: 'Pending',
    notes: ''
  };

  // Additional fields for post-visit updates
  diagnosis: string = '';
  medicalOrders: string = '';
  additionalCharges: number = 0;
  medicationCost: number = 0;
  procedureCost: number = 0;

  constructor(
    private invoiceService: InvoiceService,
    private appointmentService: AppointmentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAppointments();
    this.generateInvoiceNumber();
  }

  loadAppointments(): void {
    this.appointmentService.getAppointments().subscribe(data => {
      this.appointments = data;
    });
  }

  generateInvoiceNumber(): void {
    const now = new Date();
    const timestamp = now.getTime().toString().slice(-6);
    this.invoice.invoiceNumber = `INV-${timestamp}`;
  }

  onAppointmentChange(): void {
    const selectedAppointment = this.appointments.find(a => a.id === this.invoice.appointmentId);
    if (selectedAppointment) {
      this.invoice.subTotal = selectedAppointment.visitFee || 0;
      this.calculateTotals();
    }
  }

  addPostVisitCharges(): void {
    const additionalTotal = this.additionalCharges + this.medicationCost + this.procedureCost;
    this.invoice.subTotal = (this.invoice.subTotal || 0) + additionalTotal;
    this.calculateTotals();
    
    // Add diagnosis and medical orders to notes
    const notes = [
      this.invoice.notes || '',
      `Diagnosis: ${this.diagnosis}`,
      `Medical Orders: ${this.medicalOrders}`,
      `Additional Charges: $${additionalTotal.toFixed(2)}`
    ].filter(note => note.trim()).join('\n');
    
    this.invoice.notes = notes;
  }

  calculateTotals(): void {
    this.invoice.taxAmount = (this.invoice.subTotal || 0) * 0.08; // 8% tax
    this.invoice.totalAmount = (this.invoice.subTotal || 0) + (this.invoice.taxAmount || 0);
  }

  saveInvoice(): void {
    if (this.invoice.appointmentId && this.invoice.invoiceNumber) {
      this.invoiceService.createInvoice(this.invoice as Invoice)
        .subscribe(() => {
          this.router.navigate(['/invoices']);
        });
    }
  }

  cancel(): void {
    this.router.navigate(['/invoices']);
  }

  getAppointmentInfo(appointmentId: number): string {
    const appointment = this.appointments.find(a => a.id === appointmentId);
    if (appointment) {
      return `Appointment #${appointment.id} - ${appointment.appointmentDate}`;
    }
    return 'Unknown Appointment';
  }
}
