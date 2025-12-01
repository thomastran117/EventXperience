import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AccessTokenInterceptor } from './interceptors/access.interceptor';
import { RefreshTokenInterceptor } from './interceptors/refresh.interceptor';

@NgModule({
  imports: [HttpClientModule],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AccessTokenInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: RefreshTokenInterceptor, multi: true },
  ],
})
export class ApiModule {}
