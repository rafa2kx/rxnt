import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { InvoiceService } from '../../../services/invoice.service';
import { Invoice } from '../../../models/invoice.model';

@Component({
  selector: 'app-invoice-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './invoice-payment.component.html',
  styleUrls: ['./invoice-payment.component.css']
})
export class InvoicePaymentComponent implements OnInit {
  invoice: Invoice | null = null;
  paymentMethod: string = 'Credit Card';
  paymentAmount: number = 0;
  paymentNotes: string = '';
  isProcessing: boolean = false;

  constructor(
    private invoiceService: InvoiceService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const invoiceId = this.route.snapshot.paramMap.get('id');
    if (invoiceId) {
      this.loadInvoice(parseInt(invoiceId));
    }
  }

  loadInvoice(id: number): void {
    this.invoiceService.getInvoiceById(id).subscribe(invoice => {
      this.invoice = invoice;
      this.paymentAmount = invoice.totalAmount;
    });
  }

  processPayment(): void {
    if (this.invoice && this.paymentAmount > 0) {
      this.isProcessing = true;
      
      // Simulate payment processing
      setTimeout(() => {
        this.invoiceService.markAsPaid(this.invoice!.id!, this.paymentMethod)
          .subscribe(() => {
            this.isProcessing = false;
            alert('Payment processed successfully!');
            this.router.navigate(['/invoices']);
          }, error => {
            this.isProcessing = false;
            alert('Payment failed. Please try again.');
          });
      }, 2000);
    }
  }

  cancel(): void {
    this.router.navigate(['/invoices']);
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
