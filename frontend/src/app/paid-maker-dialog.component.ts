import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon'; // <--- 1. IMPORTANTE: Agregado
import { HttpClient } from '@angular/common/http';
import { Loan } from './loan.model';
import { LoanService } from './loan.service';
import { NotificationService } from './notification.service';

@Component({
  selector: 'app-paid-maker-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatIconModule // <--- 2. IMPORTANTE: Agregado a los imports
  ],
  styles: [`
    mat-form-field { width: 100%; }
    input[type="number"] { text-align: right; padding-right: 5px; }
    input[type=number]::-webkit-inner-spin-button, 
    input[type=number]::-webkit-outer-spin-button { opacity: 1; }
    
    .payment-section { 
      margin-top: 20px; 
      padding-top: 15px; 
      border-top: 1px dashed #ccc; 
    }

    .success-msg {
      color: #2e7d32; 
      font-weight: bold; 
      text-align: center; 
      margin: 15px 0;
      background-color: #e8f5e9;
      padding: 10px;
      border-radius: 4px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
  `],
  template: `
    <h2 mat-dialog-title>Register Payment</h2>
    
    <mat-dialog-content>
      <div *ngIf="loading && !isSaving" style="text-align: center; padding: 30px; color: #666;">
        <em>Loading loan details...</em>
      </div>

      <div [style.display]="(loading && !isSaving) ? 'none' : 'flex'" 
           class="form-container" 
           style="flex-direction: column; gap: 10px; min-width: 350px; padding-top: 10px;">
        
        <mat-form-field appearance="outline">
          <mat-label>Applicant Name</mat-label>
          <input matInput [disabled]="true" [value]="data.applicantName">
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Original Amount</mat-label>
          <input matInput [disabled]="true" [value]="data.amount | currency">
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Current Balance</mat-label>
          <input matInput [disabled]="true" [value]="data.currentBalance | currency">
        </mat-form-field>

        <div *ngIf="data.currentBalance <= 0" class="success-msg">
          <mat-icon style="margin-right: 5px;">check_circle</mat-icon>
          This loan is fully paid.
        </div>

        <div class="payment-section" *ngIf="data.currentBalance > 0">
          <h3>Payment Details</h3>
          <mat-form-field appearance="outline" color="primary">
            <mat-label>Payment Amount</mat-label>
            <input matInput 
                   type="number" 
                   min="0.01" 
                   [max]="data.currentBalance"
                   required 
                   [(ngModel)]="paymentAmount" 
                   #payInput="ngModel"
                   [disabled]="isSaving">
            <span matTextPrefix>$&nbsp;</span>
            
            <mat-error *ngIf="payInput.errors?.['required']">Amount is required.</mat-error>
            <mat-error *ngIf="payInput.errors?.['max']">Amount exceeds current balance.</mat-error>
          </mat-form-field>
        </div>

      </div>
    </mat-dialog-content>
    
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()" [disabled]="isSaving">Cancel</button>
      
      <button mat-raised-button 
              color="primary" 
              (click)="onSave()" 
              [disabled]="loading || isSaving || data.currentBalance <= 0 || !paymentAmount || paymentAmount > data.currentBalance">
        {{ isSaving ? 'Processing...' : 'Confirm Payment' }}
      </button>
    </mat-dialog-actions>
  `
})
export class PaidMakerDialogComponent implements OnInit {
  readonly dialogRef = inject(MatDialogRef<PaidMakerDialogComponent>);
  private http = inject(HttpClient);
  private loanService = inject(LoanService);
  private notification = inject(NotificationService);
  
  private passedData = inject<Loan>(MAT_DIALOG_DATA);
  data: Loan = { ...this.passedData }; 
  
  paymentAmount: number = 0;
  
  loading: boolean = true;
  isSaving: boolean = false;

  private apiUrl = 'http://localhost:5050/Loan'; 

  ngOnInit() {
    this.fetchLoanDetails(this.passedData.id);
  }

  fetchLoanDetails(id: number) {
    this.loading = true;
    this.http.get<Loan>(`${this.apiUrl}/${id}`).subscribe({
      next: (freshData) => {
        this.data = freshData; 
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching details:', err);
        this.loading = false;
      }
    });
  }

  onCancel(): void {
    if (!this.isSaving) {
      this.dialogRef.close(false);
    }
  }

  onSave(): void {
    if (this.paymentAmount > 0 && this.paymentAmount <= this.data.currentBalance) {
      
      this.isSaving = true;

      this.loanService.makePayment(this.data.id, this.paymentAmount).subscribe({
        next: (response) => {
          console.log('Payment successful:', response);
          this.isSaving = false;
          // this.notification.showSuccess('Payment registered successfully');
          this.dialogRef.close(true); 
        },
        error: (err) => {
          console.error('Payment error:', err);
          this.isSaving = false;
          this.notification.showError('An error occurred while processing the payment.');
        }
      });
    }
  }
}