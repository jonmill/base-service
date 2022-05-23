import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AuthenticationInterceptor } from "../services/AuthenticationInterceptor";
import { AuthenticationProvider } from "../services/AuthenticationProvider";

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [
      {
          provide: HTTP_INTERCEPTORS,
          useClass: AuthenticationInterceptor,
          multi: true,
      },
      AuthenticationProvider
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
