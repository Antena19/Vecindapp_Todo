import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AutenticacionService } from './autenticacion.service';

@Injectable()
export class TokenInterceptorService implements HttpInterceptor {

  constructor(
    private autenticacionService: AutenticacionService,
    private router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.autenticacionService.tokenValue;

    // Si existe un token, lo añadimos al header
    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    // Continuamos con la petición y manejamos errores
    return next.handle(request).pipe(
      catchError((error: any) => {
        if (error instanceof HttpErrorResponse) {
          // Si recibimos un error 401 (no autorizado), cerramos sesión
          if (error.status === 401) {
            this.autenticacionService.cerrarSesion();
            this.router.navigate(['/login']);
          }
        }
        return throwError(() => error);
      })
    );
  }
}