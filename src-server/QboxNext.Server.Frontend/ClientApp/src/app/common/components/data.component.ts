import { ViewChild } from '@angular/core';

import { DxChartComponent } from 'devextreme-angular';
import * as moment from 'moment';

import { ApplicationData, QboxCounterData, QboxPagedDataQueryResult } from '../models';
import { BaseComponent } from './base.component';

export abstract class DataComponent extends BaseComponent {
  public result = new QboxPagedDataQueryResult<QboxCounterData>();
  public selectedFromDate: Date;
  public selectedToDate: Date;
  public selectedResolutionId: string;

  protected resultFromServer = new QboxPagedDataQueryResult<QboxCounterData>();
  protected appData: ApplicationData;

  @ViewChild(DxChartComponent)
  protected chart: DxChartComponent;

  constructor(private title: string) {
    super();
  }

  public getTitle(): string {
    switch (this.selectedResolutionId) {
      case 'QuarterOfHour':
      case 'Hour':
        return `${this.title} (${moment(this.selectedFromDate).format('D MMMM YYYY')})`;
      case 'Day':
        return `${this.title} (${moment(this.selectedFromDate).format('MMMM YYYY')})`;
    }

    return `${this.title} (${moment(this.selectedFromDate).format('YYYY')})`;
  }
}
