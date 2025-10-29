export interface Invoice {
  id: number;
  appointmentId: number;
  invoiceNumber: string;
  invoiceDate: Date;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  status: string;
  paymentMethod?: string;
  notes?: string;
  paidDate?: Date;
  createdDate: Date;
  updatedDate?: Date;
}
