import * as signalR from '@microsoft/signalr';
import { MessageData } from '../types';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isConnected = false;

  async start(): Promise<void> {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7267/chathub', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .build();

    this.connection.onreconnecting(() => {
      this.isConnected = false;
      this.onConnectionStateChanged?.(false);
    });

    this.connection.onreconnected(() => {
      this.isConnected = true;
      this.onConnectionStateChanged?.(true);
    });

    this.connection.onclose(() => {
      this.isConnected = false;
      this.onConnectionStateChanged?.(false);
    });

    try {
      await this.connection.start();
      this.isConnected = true;
      this.onConnectionStateChanged?.(true);
      console.log('SignalR Connected');
    } catch (error) {
      console.error('SignalR Connection Error:', error);
      throw error;
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.isConnected = false;
    }
  }

  async joinUser(userId: number, username: string): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('JoinUser', userId, username);
    }
  }

  async joinChat(chatId: number): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('JoinChat', chatId);
    }
  }

  async leaveChat(chatId: number): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('LeaveChat', chatId);
    }
  }

  async sendMessageToChat(chatId: number, message: string): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('SendMessageToChat', chatId, message);
    }
  }

  async sendTyping(chatId: number, isTyping: boolean): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('SendTyping', chatId, isTyping);
    }
  }

  async markMessagesAsRead(chatId: number, lastReadMessageId: number): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('MarkMessagesAsRead', chatId, lastReadMessageId);
    }
  }

  // Event handlers
  onReceiveMessage?: (messageData: MessageData) => void;
  onUserOnline?: (userId: number, username: string) => void;
  onUserOffline?: (userId: number, username: string) => void;
  onUserTyping?: (userId: number, username: string, isTyping: boolean, chatId?: number) => void;
  onConnectionStateChanged?: (isConnected: boolean) => void;

  constructor() {
    // Set up event listeners when connection is established
    this.setupEventListeners();
  }

  private setupEventListeners(): void {
    // We'll set up listeners after connection is established
    setTimeout(() => {
      if (this.connection) {
        this.connection.on('ReceiveMessage', (messageData: MessageData) => {
          this.onReceiveMessage?.(messageData);
        });

        this.connection.on('UserOnline', (userId: number, username: string) => {
          this.onUserOnline?.(userId, username);
        });

        this.connection.on('UserOffline', (userId: number, username: string) => {
          this.onUserOffline?.(userId, username);
        });

        this.connection.on('UserTyping', (userId: number, username: string, isTyping: boolean) => {
          this.onUserTyping?.(userId, username, isTyping);
        });
      }
    }, 100);
  }
}

export const signalRService = new SignalRService();