import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebpayService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  crearTransaccion(monto: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/api/webpay/crear`, {
      BuyOrder: 'ORD' + Date.now(),
      SessionId: 'session_' + Date.now(),
      Amount: monto,
      ReturnUrl: `${window.location.origin}/payment/return`
    });
  }

  confirmarTransaccion(token: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/api/certificados/pago/confirmar`, {
      token
    });
  }
} 