﻿using System;

using AppKit;
using Foundation;
using ResilienceClasses;
using System.Collections.Generic;

namespace UpdateAcquisition
{
    public partial class ViewController : NSViewController
    {
        clsLoan loanToUpdate;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
            List<string> addressList = clsProperty.AddressList();
            this.AddressComboBox.RemoveAll();
            foreach (string address in addressList) { this.AddressComboBox.Add((NSString)address); }

            this.HOILabel.StringValue = "0";
            this.PriceLabel.StringValue = "0";
            this.AcqTaxLabel.StringValue = "0";
            this.RecordingLabel.StringValue = "0";
            this.ConcessionLabel.StringValue = "0";
            this.ProcessingLabel.StringValue = "0";
            this.ClosingDateLabel.StringValue = "--/--/--";
            this.InitialDrawLabel.StringValue = "0";
            this.PropertyTaxLabel.StringValue = "0";
            this.TitlePolicyLabel.StringValue = "0";

            this.HOIField.StringValue = this.HOILabel.StringValue;
            this.PriceField.StringValue = this.PriceLabel.StringValue;
            this.AcqTaxField.StringValue = this.AcqTaxLabel.StringValue;
            this.RecordingField.StringValue = this.RecordingLabel.StringValue;
            this.ConcessionField.StringValue = this.ConcessionLabel.StringValue;
            this.ProcessingField.StringValue = this.ProcessingLabel.StringValue;
            this.ClosingDatePicker.DateValue = (NSDate)System.DateTime.Today;
            this.InitialDrawField.StringValue = this.InitialDrawLabel.StringValue;
            this.PropertyTaxField.StringValue = this.PropertyTaxLabel.StringValue;
            this.TitlePolicyField.StringValue = this.TitlePolicyLabel.StringValue;
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        partial void PropertyChosen(AppKit.NSComboBox sender)
        {
            if (this.AddressComboBox.StringValue != "")
            {
                this.loanToUpdate = new clsLoan(clsLoan.LoanID(this.AddressComboBox.StringValue));
                DateTime OGDate = loanToUpdate.OriginationDate();
                this.ClosingDatePicker.DateValue = (NSDate)DateTime.SpecifyKind(OGDate, DateTimeKind.Utc);
                foreach (clsCashflow cf in this.loanToUpdate.Cashflows())
                {
                    if (cf.DeleteDate() > System.DateTime.Today.AddYears(100))
                    {
                        string amount = (Math.Abs(cf.Amount())).ToString("#,##0.00");
                        switch (cf.TypeID())
                        {
                            case clsCashflow.Type.AcquisitionConcession:
                                this.ConcessionLabel.StringValue = amount;
                                break;
                            case clsCashflow.Type.AcquisitionPrice:
                                this.PriceLabel.StringValue = amount;
                                break;
                            case clsCashflow.Type.AcquisitionProcessing:
                                this.ProcessingLabel.StringValue = amount;
                                break;
                            case clsCashflow.Type.AcquisitionRecording:
                                this.RecordingLabel.StringValue = amount;
                                break;
                            case clsCashflow.Type.AcquisitionTaxes:
                                this.AcqTaxLabel.StringValue = amount;
                                break;
                            case clsCashflow.Type.TitlePolicy:
                                this.TitlePolicyLabel.StringValue = amount;
                                break;
                            case clsCashflow.Type.HomeownersInsurance:
                                this.HOILabel.StringValue = amount;
                                break;
                            case clsCashflow.Type.InitialExpenseDraw:
                                this.InitialDrawLabel.StringValue = amount;
                                break;
                            case clsCashflow.Type.PropertyTax:
                                this.PropertyTaxLabel.StringValue = amount;
                                break;
                            default:
                                break;
                        }
                    }
                }
                this.HOIField.StringValue = this.HOILabel.StringValue;
                this.PriceField.StringValue = this.PriceLabel.StringValue;
                this.AcqTaxField.StringValue = this.AcqTaxLabel.StringValue;
                this.RecordingField.StringValue = this.RecordingLabel.StringValue;
                this.ConcessionField.StringValue = this.ConcessionLabel.StringValue;
                this.ProcessingField.StringValue = this.ProcessingLabel.StringValue;
                this.ClosingDatePicker.DateValue = (NSDate)DateTime.SpecifyKind(this.loanToUpdate.OriginationDate(), DateTimeKind.Utc);
                this.InitialDrawField.StringValue = this.InitialDrawLabel.StringValue;
                this.PropertyTaxField.StringValue = this.PropertyTaxLabel.StringValue;
                this.TitlePolicyField.StringValue = this.TitlePolicyLabel.StringValue;
            }
        }

        partial void MarkActualPressed(NSButton sender)
        {
            this.SummaryMessageField.StringValue = "";
            if (this.loanToUpdate == null)
            {
                this.SummaryMessageField.StringValue = "No loan selected.  No updates made.";
            }
            else
            {
                double dTotalCost = 0D;
                foreach (clsCashflow cf in this.loanToUpdate.Cashflows())
                {
                    if ((cf.DeleteDate() > System.DateTime.Today.AddYears(100)) && (cf.PayDate() <= System.DateTime.Today))
                    {
                        if (cf.MarkActual(System.DateTime.Today))
                        {
                            this.SummaryMessageField.StringValue += "Marked actual " + cf.TypeID().ToString() +
                                " (" + cf.TransactionID().ToString("#") + ")";
                            dTotalCost += cf.Amount();
                        }
                        else
                            this.SummaryMessageField.StringValue += "FAILED to mark actual " + cf.TypeID().ToString() +
                                " (" + cf.TransactionID().ToString("#") + ")";
                    }
                }
                this.SummaryMessageField.StringValue += "TOTAL Marked Actual = " + dTotalCost.ToString("#,##0.00");
                this.SummaryMessageField.StringValue += "Loan Save to Files " + this.loanToUpdate.Save().ToString().ToUpper();
            }
        }

        partial void UpdateButtonPushed(AppKit.NSButton sender)
        {
            this.SummaryMessageField.StringValue = "";
            if (this.loanToUpdate == null)
                this.SummaryMessageField.StringValue = "No loan selected.  No updates made.";
            else
            {
                // only validaton is:  are any cashflows ACTUAL
                bool bAnyActuals = false;
                double oldAcqCost = this.loanToUpdate.AcquisitionCost(false);
                foreach (clsCashflow cf in this.loanToUpdate.Cashflows())
                {
                    if (cf.Actual()) bAnyActuals = true;
                }
                if (bAnyActuals)
                {
                    this.SummaryMessageField.StringValue = "Can't update - some cashflows are marked Actual";
                }
                else
                {
                    int delayDays = ((DateTime)this.ClosingDatePicker.DateValue - this.loanToUpdate.OriginationDate()).Days;
                    List<clsCashflow> newCashflows = new List<clsCashflow>();
                    // delete all existing scheduled cashflows
                    foreach (clsCashflow cf in this.loanToUpdate.Cashflows())
                    {
                        if (cf.DeleteDate() > System.DateTime.Today.AddYears(50))
                        {
                            if ((cf.TypeID() == clsCashflow.Type.NetDispositionProj) ||
                                (cf.TypeID() == clsCashflow.Type.RehabDraw))
                            {
                                newCashflows.Add(new clsCashflow(cf.PayDate().AddDays(delayDays),
                                                 System.DateTime.Today, System.DateTime.MaxValue,
                                                 this.loanToUpdate.ID(), cf.Amount(), false, cf.TypeID()));
                            }
                            cf.Delete(System.DateTime.Today);
                        }
                    }
                    // create all the new cashflows
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), -this.PriceField.DoubleValue,
                                                                  false, clsCashflow.Type.AcquisitionPrice));
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), this.ConcessionField.DoubleValue,
                                                                  false, clsCashflow.Type.AcquisitionConcession));
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), -this.HOIField.DoubleValue,
                                                                  false, clsCashflow.Type.HomeownersInsurance));
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), -this.AcqTaxField.DoubleValue,
                                                                  false, clsCashflow.Type.AcquisitionTaxes));
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), -this.RecordingField.DoubleValue,
                                                                  false, clsCashflow.Type.AcquisitionRecording));
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), -this.ProcessingField.DoubleValue,
                                                                  false, clsCashflow.Type.AcquisitionProcessing));
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), -this.TitlePolicyField.DoubleValue,
                                                                  false, clsCashflow.Type.TitlePolicy));
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), -this.InitialDrawField.DoubleValue,
                                                                  false, clsCashflow.Type.InitialExpenseDraw));
                    this.loanToUpdate.AddCashflow(new clsCashflow((DateTime)this.ClosingDatePicker.DateValue,
                                                                  System.DateTime.Now, System.DateTime.MaxValue,
                                                                  this.loanToUpdate.ID(), -this.PropertyTaxField.DoubleValue,
                                                                  false, clsCashflow.Type.PropertyTax));
                    foreach (clsCashflow cf in newCashflows) { this.loanToUpdate.AddCashflow(cf); }
                    // Update origination Date and Save
                    this.loanToUpdate.SetNewOriginationDate((DateTime)this.ClosingDatePicker.DateValue);
                    if (this.loanToUpdate.Save())
                    {
                        this.SummaryMessageField.StringValue += "\nSave successful.  Old / New Acquisition Cost = ";
                        this.SummaryMessageField.StringValue += oldAcqCost.ToString("#,##0.00") + " / ";
                        this.SummaryMessageField.StringValue += this.loanToUpdate.AcquisitionCost(false).ToString("#,##0.00");
                    }
                    else
                    {
                        this.SummaryMessageField.StringValue += "\nSave Failed.";
                    }
                }
            }
        }
    }
}
