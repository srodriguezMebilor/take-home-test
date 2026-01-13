import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LoanDialogComponent } from './loan-dialog.component';
import { LoanService } from './loan.service';
import { Loan } from './loan.model';
import { NotificationService } from './notification.service';
import { PaidMakerDialogComponent } from './paid-maker-dialog.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, 
    MatTableModule, 
    MatProgressSpinnerModule, 
    MatButtonModule, 
    MatIconModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTooltipModule
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  private loanService = inject(LoanService); 
  private dialog = inject(MatDialog);        

  displayedColumns: string[] = ['loanAmount', 'currentBalance', 'applicant', 'status', 'actions'];
  loans: Loan[] = [];
  isLoading = true;

  paymentFormVisible = false;
  payment = { loanId: 0, amount: 0 };

  constructor(
    private notification: NotificationService
  ) {}

  ngOnInit() {
    this.fetchLoans();
  }

  fetchLoans() {
    this.isLoading = true;
    this.loanService.getLoans().subscribe({
      next: (data) => {
        this.loans = data;
        this.isLoading = false;
      },
      error: (err) => { 
        console.error('Error fetching loans:', err);
        this.notification.showError('Could not load loans.');
        this.isLoading = false; 
      }
    });
  }

  createLoan() {
    const dialogRef = this.dialog.open(LoanDialogComponent, {
      width: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Sending to backend:', result);
        this.saveLoanToBackend(result);
      }
    });
  }

  saveLoanToBackend(newLoan: Loan) {
    this.isLoading = true;
    newLoan.currentBalance = newLoan.amount;
    
    this.loanService.createLoan(newLoan).subscribe({
      next: (response) => {
        console.log('Loan created:', response);
        this.notification.showSuccess('Loan created successfully!');
        this.fetchLoans(); 
      },
      error: (err) => {
        console.error('Error creating loan:', err);
        this.notification.showError('Failed to create loan.');
        this.isLoading = false;
      }
    });
  }

  openPayDialog(loan: Loan) {
    const dialogRef = this.dialog.open(PaidMakerDialogComponent, {
      width: '450px',
      data: loan,
      disableClose: true 
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        console.log('Payment confirmed. Reloading table...');
        this.fetchLoans(); 
      }
    });
  }

  closePaymentForm(): void {
    this.paymentFormVisible = false;
  }
}