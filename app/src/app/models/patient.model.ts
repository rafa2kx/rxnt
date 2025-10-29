export interface Patient {
  id?: number;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  phone: string;
  address: string;
  gender: string;
  createdDate?: string;
  updatedDate?: string;
  isActive: boolean;
}
