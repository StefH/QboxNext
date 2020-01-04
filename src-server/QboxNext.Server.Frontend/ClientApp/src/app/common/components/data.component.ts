import { ViewChild } from '@angular/core';

import { DxChartComponent } from 'devextreme-angular';
import * as moment from 'moment';

import { DevExpressPointClickEvent } from '../interfaces/devexpress';

import { CurrencyPipe } from '@angular/common';
import { Costs } from '../constants';
import { Resolution } from '../enums';
import { ApplicationData, QboxCounterData, QboxDataQuery, QboxPagedDataQueryResult } from '../models';
import { TimeRangeHelper } from '../services';
import { BaseComponent } from './base.component';

export abstract class DataComponent extends BaseComponent {
  public result = new QboxPagedDataQueryResult<QboxCounterData>();
  public selectedFromDate: Date;
  public selectedToDate: Date;
  public selectedResolutionId: Resolution;

  protected resultFromServer = new QboxPagedDataQueryResult<QboxCounterData>();
  protected appData: ApplicationData;

  @ViewChild(DxChartComponent)
  protected chart: DxChartComponent;

  constructor(private title: string, protected timeRangeHelper: TimeRangeHelper, protected cp: CurrencyPipe) {
    super();
  }

  public nextClick(): void {
    this.selectedFromDate = this.timeRangeHelper.getNextDate(this.selectedResolutionId, this.selectedFromDate);
  }

  public previousClick(): void {
    this.selectedFromDate = this.timeRangeHelper.getPreviousDate(this.selectedResolutionId, this.selectedFromDate);
  }

  public next2Click(): void {
    this.selectedFromDate = this.timeRangeHelper.getNextDate(this.selectedResolutionId + 1, this.selectedFromDate);
  }

  public previous2Click(): void {
    this.selectedFromDate = this.timeRangeHelper.getPreviousDate(this.selectedResolutionId + 1, this.selectedFromDate);
  }

  public pointClick(e: DevExpressPointClickEvent): void {
    const drillDownQuery: QboxDataQuery = e.target.data.drillDownQuery;

    if (drillDownQuery) {
      if (this.title === 'Gas' && drillDownQuery.resolution < Resolution.Hour) {
        return;
      }
      this.selectedResolutionId = drillDownQuery.resolution;
      this.selectedFromDate = new Date(drillDownQuery.from);
    }
  }

  public getTitle(): string {
    const fromDate = moment(this.selectedFromDate);

    switch (this.selectedResolutionId) {
      case Resolution.FiveMinutes:
      case Resolution.QuarterOfHour:
      case Resolution.Hour:
        return `${this.title} (${fromDate.format('D MMMM YYYY')})`;
      case Resolution.Day:
        if (fromDate.day() === 1) {
          return `${this.title} (${fromDate.format('MMMM YYYY')})`;
        } else {
          return `${this.title} (${fromDate.format('D MMMM YYYY')} tot ${moment(this.selectedToDate).format('D MMMM YYYY')})`;
        }
    }

    return `${this.title} (${moment(this.selectedFromDate).format('YYYY')})`;
  }

  public getEnergyCosts(): string {
    const header = 'Kosten';
    const costs = [
      `<div class=\'series-name\'>Electriciteit (kWh)</div><div class=\'value-text\'>${this.cp.transform(Costs.Electricity, 'EUR', 'symbol', '1.5-5')}</div>`,
      `<div class=\'series-name\'>Gas (m³)</div><div class=\'value-text\'>${this.cp.transform(Costs.Gas, 'EUR', 'symbol', '1.5-5')}</div>`
    ];

    return `<div><div class=\'tooltip-header\'>${header}</div><div class=\'tooltip-body\'>${costs.join('\r\n')}</div></div>`;
  }
}
