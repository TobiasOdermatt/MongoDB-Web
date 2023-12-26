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

  getInputData(): string {
    return this.username + '@' + this.password;
  }

  randomBinaryString(length: number): string {
    let result = '';
    let characters = '01';
    let charactersLength = characters.length;
    const array = new Uint32Array(length);
    window.crypto.getRandomValues(array);

    for (let i = 0; i < length; i++) {
      if (i % 8 === 0 && i !== 0) {
        result += ' ';
      }
      result += characters[array[i] % charactersLength];
    }
    return result;
  }

  async sendDataToServer(authCookieKey: string, randData: string): Promise<string | boolean> {
    const url = window.location.protocol + '//' + window.location.host + '/api/Auth/CreateOTP';
    const data = { 'AuthCookieKey': authCookieKey, 'RandData': randData };

    try {
      const res = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });

      const text = await res.text();
      const dataResult = JSON.parse(text);

      if (dataResult.hasOwnProperty('uuid')) {
        return dataResult['uuid'];
      } else {
        return false;
      }
    } catch (err) {
      return false;
    }
  }

  stringToBinary(string: string): string {
    return string.split('').map(char => {
      let tempBinaryBlock = char.charCodeAt(0).toString(2);
      return String(tempBinaryBlock).padStart(8, '0');
    }).join(' ');
  }

  xoring(a: string, b: string, n: number): string {
    let ans = '';
    for (let i = 0; i < n; i++) {
      if (a[i] === ' ' && b[i] === ' ') {
        ans += ' ';
      } else if (a[i] === b[i]) {
        ans += '0';
      } else {
        ans += '1';
      }
    }
    return ans;
  }

  async validate(): Promise<void> {
    const inputString = 'Data:' + this.getInputData();
    const inputBinaryString = this.stringToBinary(inputString);
    const lengthOfRandomString = inputBinaryString.length - Math.floor(inputBinaryString.length / 9);

    const randomDataBinaryString = this.randomBinaryString(lengthOfRandomString);
    const authCookieBinaryKey = this.xoring(inputBinaryString, randomDataBinaryString, inputBinaryString.length);

    try {
      const successResult = await this.connectService.createOTP(authCookieBinaryKey, randomDataBinaryString).toPromise();

      if (successResult && successResult.hasOwnProperty('uuid')) {
        this.setCookie('Token', btoa(authCookieBinaryKey), 10);
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
