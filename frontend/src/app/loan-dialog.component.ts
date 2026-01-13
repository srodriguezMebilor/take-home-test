import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { Loan } from './loan.model';

@Component({
  selector: 'app-loan-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule
  ],
  styles: [`
    mat-form-field {
      width: 100%;
    }
    input[type="number"] {
      text-align: right;
      padding-right: 5px; 
    }
    input[type=number]::-webkit-inner-spin-button, 
    input[type=number]::-webkit-outer-spin-button { 
      opacity: 1;
    }
  `],
  template: `
    <h2 mat-dialog-title>New Loan</h2>
    
    <mat-dialog-content>
      <div class="form-container" style="display: flex; flex-direction: column; gap: 15px; min-width: 350px; padding-top: 10px;">
        
        <mat-form-field appearance="outline">
          <mat-label>Applicant Name</mat-label>
          <input matInput 
                 [(ngModel)]="data.applicantName" 
                 required 
                 pattern=".*\\S.*"
                 #nameInput="ngModel" 
                 placeholder="Ex: John Doe">
          
          <mat-error *ngIf="nameInput.errors?.['required']">
            Name is required.
          </mat-error>
          <mat-error *ngIf="nameInput.errors?.['pattern']">
            Name cannot be just whitespace.
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Amount</mat-label>
          <input matInput type="number" min="0.01" required [(ngModel)]="data.amount" #amountInput="ngModel">
          <span matTextPrefix>$&nbsp;</span>
          <mat-error *ngIf="amountInput.invalid">Must be greater than 0</mat-error>
        </mat-form-field>

        </div>
    </mat-dialog-content>
    
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      
      <button mat-raised-button color="primary" 
              (click)="onSave()" 
              [disabled]="amountInput.invalid || nameInput.invalid">
        Save
      </button>
    </mat-dialog-actions>
  `
})
export class LoanDialogComponent {
  readonly dialogRef = inject(MatDialogRef<LoanDialogComponent>);

  data: Loan = {
    id: 0,
    applicantName: '',
    amount: 0,
    currentBalance: 0, 
    status: 'active'        
  };

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.data.amount > 0 && this.data.applicantName.trim().length > 0) {
      this.dialogRef.close(this.data);
    }
  }
}