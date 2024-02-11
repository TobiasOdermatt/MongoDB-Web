import {inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { map, catchError } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { EnvService } from './env.service';

export const canActivate: CanActivateFn = (
  next: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<boolean | UrlTree> => {
  const envService = inject(EnvService);
  const router = inject(Router);

  return envService.isAuthorized().pipe(
    map(isAuthorized => {
      if (!isAuthorized) {
        return router.createUrlTree(['/login']);
      }
      envService.updateAuthorizationStatus(true);
      return true;
    }),
    catchError(() => of(router.createUrlTree(['/login'])))
  );
};
