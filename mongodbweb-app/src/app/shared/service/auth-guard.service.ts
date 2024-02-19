import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { EnvService } from './env.service';
import { v4 as uuidv4 } from 'uuid';

export const canActivate: CanActivateFn = (
  next: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<boolean | UrlTree> => {
  const envService = inject(EnvService);
  const router = inject(Router);

  return envService.useAuthorization().pipe(
    switchMap(useAuthorization => {
      if (!useAuthorization) {
        envService.updateAuthorizationStatus(true);
        if (!envService.isCookieSet()) {
          const newGuid = uuidv4();
          envService.setCookie('UUID', newGuid, 7); 
        }
        return of(true);
      }
      return envService.isAuthorized().pipe(
        map(isAuthorized => {
          if (!isAuthorized) {
            return router.createUrlTree(['/login']);
          }
          envService.updateAuthorizationStatus(true);
          return true;
        })
      );
    }),
    catchError(() => of(router.createUrlTree(['/login'])))
  );
};
