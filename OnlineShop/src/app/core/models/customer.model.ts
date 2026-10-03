export interface User {
  id: number;
  name: string;
  email: string;
  address: string;
  phoneNumber: string;
  location: string;
  createdAt?: string;
}

export interface CreateUserDto {
  name: string;
  email: string;
  address: string;
  phoneNumber: string;
  location: string;
}

export interface UpdateUserDto {
  id: number;
  name: string;
  email: string;
  address: string;
  phoneNumber: string;
  location: string;
}
