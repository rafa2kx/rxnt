export interface Doctor {
  id: number;
  firstName: string;
  lastName: string;
  specialty: string;
  email: string;
  phone: string;
  licenseNumber: string;
  isActive: boolean;
  createdDate: Date;
  updatedDate?: Date;
}
