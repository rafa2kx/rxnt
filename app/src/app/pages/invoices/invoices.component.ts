import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { InvoiceService } from '../../services/invoice.service';
import { AppointmentService } from '../../services/appointment.service';
import { Invoice } from '../../models/invoice.model';
import { Appointment } from '../../models/appointment.model';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './invoices.component.html',
  styleUrls: ['./invoices.component.css']
})
export class InvoicesComponent implements OnInit {
  invoices: Invoice[] = [];
  appointments: Appointment[] = [];
  showForm = false;
  editingInvoice: Invoice | null = null;
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
    this.loadInvoices();
    this.loadAppointments();
  }

  loadInvoices(): void {
    this.invoiceService.getInvoices().subscribe(data => {
      this.invoices = data;
    });
  }

  loadAppointments(): void {
    this.appointmentService.getAppointments().subscribe(data => {
      this.appointments = data;
    });
  }

  addInvoice(): void {
    this.router.navigate(['/invoices/create']);
  }

  editInvoice(invoice: Invoice): void {
    this.editingInvoice = invoice;
    this.invoice = { ...invoice };
    this.showForm = true;
  }

  saveInvoice(): void {
    if (this.editingInvoice) {
      // Update existing invoice
      this.invoiceService.updateInvoice(this.editingInvoice.id!, this.invoice as Invoice)
        .subscribe(() => {
          this.loadInvoices();
          this.cancelForm();
        });
    } else {
      // Create new invoice
      this.invoiceService.createInvoice(this.invoice as Invoice)
        .subscribe(() => {
          this.loadInvoices();
          this.cancelForm();
        });
    }
  }

  addPostVisitCharges(): void {
    if (this.invoice) {
      const additionalTotal = this.additionalCharges + this.medicationCost + this.procedureCost;
      this.invoice.subTotal = (this.invoice.subTotal || 0) + additionalTotal;
      this.invoice.taxAmount = this.invoice.subTotal * 0.08; // 8% tax
      this.invoice.totalAmount = this.invoice.subTotal + this.invoice.taxAmount;
      
      // Add diagnosis and medical orders to notes
      const notes = [
        this.invoice.notes || '',
        `Diagnosis: ${this.diagnosis}`,
        `Medical Orders: ${this.medicalOrders}`,
        `Additional Charges: $${additionalTotal.toFixed(2)}`
      ].filter(note => note.trim()).join('\n');
      
      this.invoice.notes = notes;
    }
  }

  processPayment(invoice: Invoice): void {
    this.router.navigate(['/invoices/payment', invoice.id]);
  }

  deleteInvoice(id: number): void {
    if (confirm('Are you sure you want to delete this invoice?')) {
      this.invoiceService.deleteInvoice(id)
        .subscribe(() => {
          this.loadInvoices();
        });
    }
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingInvoice = null;
    this.resetForm();
  }

  resetForm(): void {
    this.invoice = {
      appointmentId: 0,
      invoiceNumber: '',
      invoiceDate: new Date(),
      subTotal: 0,
      taxAmount: 0,
      totalAmount: 0,
      status: 'Pending',
      notes: ''
    };
    this.diagnosis = '';
    this.medicalOrders = '';
    this.additionalCharges = 0;
    this.medicationCost = 0;
    this.procedureCost = 0;
  }

  getAppointmentInfo(appointmentId: number): string {
    const appointment = this.appointments.find(a => a.id === appointmentId);
    if (appointment) {
      return `Appointment #${appointment.id} - ${appointment.appointmentDate}`;
    }
    return 'Unknown Appointment';
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'bg-yellow-100 text-yellow-800';
      case 'paid': return 'bg-green-100 text-green-800';
      case 'cancelled': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
