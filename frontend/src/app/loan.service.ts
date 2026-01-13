import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Loan } from './loan.model';

@Injectable({
  providedIn: 'root'
})
export class LoanService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5050/Loan'; 

  getLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(this.apiUrl);
  }

  createLoan(loan: Loan): Observable<Loan> {
    return this.http.post<Loan>(this.apiUrl, loan);
  }

  makePayment(id: number, amount: number): Observable<Loan> {
    return this.http.post<Loan>(`${this.apiUrl}/${id}/payment`, amount)
  }

}