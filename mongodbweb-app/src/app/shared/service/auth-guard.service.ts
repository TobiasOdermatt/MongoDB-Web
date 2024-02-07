import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { EnvService } from './env.service';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private envService: EnvService, private router: Router) { }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    return this.envService.isAuthorized().pipe(
      map(isAuthorized => {
        if (!isAuthorized) {
          this.router.navigate(['/login']);
          return false;
        }
        this.envService.updateAuthorizationStatus(true);
        return true;
      })
    );
  }
}
