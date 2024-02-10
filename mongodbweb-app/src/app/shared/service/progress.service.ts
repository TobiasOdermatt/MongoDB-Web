import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
@Injectable({
  providedIn: 'root'
})
export class ProgressService {
  private hubConnection: signalR.HubConnection;

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/api/ws/progressHub')
      .build();

    this.hubConnection.onreconnected(connectionId => console.log(`Connection reestablished. Connection ID: ${connectionId}`));
    this.hubConnection.onclose(error => console.log(`Connection closed.`, error));
  }

  public async startConnection(): Promise<void> {
    return await this.hubConnection.start()
      .then(() => console.log("Connection started successfully."))
      .catch(err => console.error("Error while starting connection: ", err));
  }

  public stopConnection(): Promise<void> {
    return this.hubConnection.stop();
  }

  public listenForDatabaseProgress(callback: (totalCollections: number, processedCollections: number, progress: number, guid: string, messageType: string) => void): void {
    this.hubConnection.on('ReceiveProgressDatabase', callback);
  }
}
