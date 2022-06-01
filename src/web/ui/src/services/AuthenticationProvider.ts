import { Injectable, OnDestroy } from "@angular/core";
import jwt_decode from "jwt-decode";
import { Subscription, timer } from "rxjs";

@Injectable()
export class AuthenticationProvider implements OnDestroy {

    private _accessTokenStorageKey: string = "AccessToken";
    private _refreshTokenStorageKey: string = "RefreshToken";
    private _userIdStorageKey: string = "UserId";
    private _userNameStorageKey: string = "Name";
    private _tokenExpireStorageKey: string = "TokenExpire";
    private _timer: Subscription | null = null;
    private _isTokenExpired: boolean = true;

    ngOnDestroy(): void {
        this._timer?.unsubscribe();
    }

    getAccessToken(): string | null {
        if (this._isTokenExpired) {
            return null;
        }

        return localStorage.getItem(this._accessTokenStorageKey);
    }


    storeLoginToken(jwtToken: string): void {
        const token: any = jwt_decode(jwtToken);
        const refreshToken: string = token.refresh_token;
        const expiration: number = token.exp;
        const userId: number = token.actor;
        const userName: string = token.user_name;

        localStorage.setItem(this._accessTokenStorageKey, jwtToken);
        localStorage.setItem(this._refreshTokenStorageKey, refreshToken);
        localStorage.setItem(this._userIdStorageKey, `${userId}`);
        localStorage.setItem(this._userNameStorageKey, userName);
        localStorage.setItem(this._tokenExpireStorageKey, `${expiration}`);

        this.startRefreshTimer();
    }

    private getTokenRemainingTime(): number {
        const tokenExpiration: string | null = localStorage.getItem(this._tokenExpireStorageKey);
        if (tokenExpiration === null) {
            return 0;
        }

        const expires = new Date(+tokenExpiration * 1000);
        return expires.getTime() - Date.now();
    }

    private startRefreshTimer(): void {
        const timeout: number = this.getTokenRemainingTime();
        this._timer = timer(timeout).subscribe(val => {
            this._isTokenExpired = true;
        });
    }
}
