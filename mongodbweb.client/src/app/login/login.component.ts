import { Component } from '@angular/core';
import { ConnectService } from './service/connect.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  username: string = '';
  password: string = '';
  alertSuccessVisible: boolean = false;
  alertDangerVisible: boolean = false;

  constructor(private connectService: ConnectService) { }


  sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  async validate(): Promise<void> {

    try {
      const successResult = await this.connectService.createOTP(this.username, this.password).toPromise();

      if (successResult && successResult.hasOwnProperty('uuid') && successResult.hasOwnProperty('token')) {
        this.setCookie('Token',successResult['token'] , 10);
        this.setCookie('UUID', successResult['uuid'].toString(), 10);
        this.alertSuccessVisible = true;
        this.alertDangerVisible = false;
        await this.sleep(200);
        window.location.href = window.location.protocol + '//' + window.location.host + '/dashboard';
      } else {
        this.alertDangerVisible = true;
        this.alertSuccessVisible = false;
      }
    } catch (error) {
      this.alertDangerVisible = true;
      this.alertSuccessVisible = false;
    }
  }
  private setCookie(name: string, value: string, days: number) {
    const expires = new Date();
    expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);
    document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/`;
  }
}
