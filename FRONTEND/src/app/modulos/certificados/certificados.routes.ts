import { Routes } from '@angular/router';
import { SolicitudCertificadoComponent } from './components/solicitud-certificado/solicitud-certificado.component';
import { ListaCertificadosComponent } from './components/lista-certificados/lista-certificados.component';
import { DetalleCertificadoComponent } from './components/detalle-certificado/detalle-certificado.component';
import { PagoCertificadoComponent } from './components/pago-certificado/pago-certificado.component';
import { HistorialCertificadosComponent } from './components/historial-certificados/historial-certificados.component';

export const CERTIFICADOS_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'solicitar',
    pathMatch: 'full'
  },
  {
    path: '',
    children: [
      { path: 'solicitar', component: SolicitudCertificadoComponent },
      { path: 'lista', component: ListaCertificadosComponent },
      { path: 'detalle/:id', component: DetalleCertificadoComponent },
      { path: 'pago/:id', component: PagoCertificadoComponent },
      { path: 'historial', component: HistorialCertificadosComponent }
    ]
  }
]; 