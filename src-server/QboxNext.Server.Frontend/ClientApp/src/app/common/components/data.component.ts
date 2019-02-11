import { ViewChild } from '@angular/core';

import { DxChartComponent } from 'devextreme-angular';
import * as moment from 'moment';

import { DevExpressPointClickEvent } from '../interfaces/devexpress';

import { Resolution } from '../enums';
import { ApplicationData, QboxCounterData, QboxDataQuery, QboxPagedDataQueryResult } from '../models';
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

  constructor(private title: string) {
    super();
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
}
