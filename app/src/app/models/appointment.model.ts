import { Patient } from './patient.model';
import { Doctor } from './doctor.model';

export interface Appointment {
  
  id?: number;
  patientId: number;
  patient?: Patient;
  doctorId: number;
  doctor?: Doctor;
  appointmentDate: string;
  appointmentTime: string;
  reason: string;
  notes: string;
  status: string;
  createdDate?: string;
  updatedDate?: string;
  visitFee: number;
}
