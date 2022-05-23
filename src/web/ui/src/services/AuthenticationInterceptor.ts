import { Injectable } from "@angular/core";
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Observable } from "rxjs";
import { AuthenticationProvider } from "./AuthenticationProvider";

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {

    constructor(private _authProvider: AuthenticationProvider) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        // If we have an access token, add it to the HTTP request and pass it to the next interceptor; if not, pass the original request
        const accessToken: string | null = this._authProvider.getAccessToken();
        if (accessToken !== null) {
            const newReq: HttpRequest<any> = req.clone({
                headers: req.headers.set("Authorization", `Bearer ${accessToken}`)
            });

            return next.handle(newReq);
        }
        else
        {
            return next.handle(req);
        }
    }

}
