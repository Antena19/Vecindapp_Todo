import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/api/payment`;

  constructor(private http: HttpClient) { }

  createTransaction(amount: number, buyOrder: string, sessionId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, {
      amount,
      buyOrder,
      sessionId
    });
  }

  commitTransaction(token: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/commit`, { token });
  }

  getTransactionStatus(token: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/status/${token}`);
  }
} 