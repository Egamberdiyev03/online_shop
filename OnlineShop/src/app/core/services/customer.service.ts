// import { Injectable, inject } from '@angular/core';
// import { HttpClient } from '@angular/common/http';
// import { Observable, map } from 'rxjs';
// import { environment } from '../../../environments/environment';
// import { User, CreateUserDto, UpdateUserDto } from '../models/customer.model';
// import { ResponseModel, unwrapResult } from '../models/response.model';

// @Injectable({
//   providedIn: 'root'
// })
// export class UserService {
//   private http = inject(HttpClient);
//   private apiUrl = `${environment.apiUrl}/User`;

//   getAll(): Observable<User[]> {
//     return this.http.get<User[] | ResponseModel<User[]>>(`${this.apiUrl}/GetAll`).pipe(
//       map(res => unwrapResult(res) || [])
//     );
//   }

//   getById(id: number): Observable<User> {
//     return this.http.get<ResponseModel<User> | User>(`${this.apiUrl}/GetById?id=${id}`).pipe(
//       map(res => unwrapResult(res))
//     );
//   }

//   create(dto: CreateUserDto): Observable<User> {
//     return this.http.post<User | ResponseModel<User>>(`${this.apiUrl}/Create`, dto).pipe(
//       map(res => unwrapResult(res))
//     );
//   }

//   update(dto: UpdateUserDto): Observable<User> {
//     return this.http.put<ResponseModel<User> | User>(`${this.apiUrl}/Update`, dto).pipe(
//       map(res => unwrapResult(res))
//     );
//   }

//   delete(id: number): Observable<boolean> {
//     return this.http.delete<ResponseModel<boolean> | boolean>(`${this.apiUrl}/Delete?id=${id}`).pipe(
//       map(res => unwrapResult(res) === true)
//     );
//   }
// }
