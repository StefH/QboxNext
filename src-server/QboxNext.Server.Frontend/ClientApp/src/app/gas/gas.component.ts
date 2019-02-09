import { Component, OnInit, ViewChild } from '@angular/core';

import { DxChartComponent } from 'devextreme-angular';
import * as moment from 'moment';
import { nameof } from 'ts-simple-nameof';

import { BaseComponent, DataComponent } from '../common/components';
import { HttpStatusCodes } from '../common/constants';
import { DataLoadStatus } from '../common/enums';
import { GasValueFormatter } from '../common/formatters';
import { ApplicationData, QboxCounterData, QboxPagedDataQueryResult } from '../common/models';
import { DataService, TimeRangeHelper } from '../common/services';
import { SessionStorageService } from '../common/services';

@Component({
  selector: 'app-show-gas',
  providers: [DataService, GasValueFormatter, TimeRangeHelper],
  templateUrl: './gas.component.html',
  styleUrls: ['./gas.component.css'],
  preserveWhitespaces: true
})
export class GasComponent extends DataComponent implements OnInit {
  public resolutions = [{ id: 'Hour', text: 'Uur' }, { id: 'Day', text: 'Dag' }, { id: 'Month', text: 'Maand' }];

  public checkgas = true;

  constructor(private service: DataService, private formatter: GasValueFormatter, private timeRangeHelper: TimeRangeHelper, private sessionStorageService: SessionStorageService) {
    super('Gas');
  }

  public ngOnInit(): void {
    this.appData = this.sessionStorageService.get<ApplicationData>(nameof(ApplicationData)) || new ApplicationData();
    this.selectedFromDate = this.appData.gasSelectedFromDate || new Date('2018-10-01');
    this.selectedToDate = this.appData.gasSelectedToDate || new Date('2018-11-01');
    this.selectedResolutionId = this.appData.gasSelectedResolutionId || this.resolutions[0].id;

    this.updateChartSeries();

    this.refreshChart(true);
  }

  public get isLoadIndicatorVisible(): boolean {
    return this.loadingStatus === DataLoadStatus.Started;
  }

  public customizeTooltip(info: any): any {
    const points: any[] = [];
    info.points.forEach(point => {
      const valueAsString = new GasValueFormatter().format(point.value);
      points.push(`<div class=\'series-name\'>${point.seriesName}</div><div class=\'value-text\'>${valueAsString}</div>`);
    });

    return {
      html: `<div><div class=\'tooltip-header\'>${info.argumentText}</div><div class=\'tooltip-body\'>${points.join('\r\n')}</div></div>`
    };
  }

  public getOverview(): string {
    const info: any = {
      argumentText: 'Totaal',
      length: 1,
      points: []
    };

    this.chart.series.forEach(serie => {
      info.points.push({ seriesName: serie.name, value: this.result.overview ? this.result.overview[serie.valueField] : '' });
    });

    return this.customizeTooltip(info).html;
  }

  public customizeLabelText = (info: any) => this.formatter.format(info.value);

  private updateChartSeries(): void {
    this.chart.series = [];
    if (this.checkgas) {
      this.chart.series.push({ valueField: 'delta2421', name: 'Gas (2421)', color: '#FFDD00' });
    }
  }

  private filter(): void {
    const clientResult = new QboxPagedDataQueryResult<QboxCounterData>({
      count: this.resultFromServer.count,
      overview: this.resultFromServer.overview,
      items: this.resultFromServer.items.map(i => new QboxCounterData({
        labelText: i.labelText,
        labelValue: i.labelValue,
        delta2421: i.delta2421
      }))
    });

    this.result = clientResult;
  }

  public checkClicked(event: Event) {
    // Only update chart if event is from a user-click
    if (event) {
      this.refreshChart(false);
    }
  }

  public refreshChart(serverSide: boolean): void {
    if (!serverSide) {

      // just filter
      this.filter();

      // and update series
      this.updateChartSeries();

      return;
    }

    this.loadingStatus = DataLoadStatus.Started;
    this.result = new QboxPagedDataQueryResult<QboxCounterData>();

    const dates = this.timeRangeHelper.getToDate(this.selectedResolutionId, this.selectedFromDate);
    this.selectedToDate = dates.toDate.toDate();

    this.subscription.add(this.service.getData(this.selectedResolutionId, dates.fromDate.toDate(), this.selectedToDate)
      .subscribe(
        data => {
          this.resultFromServer = data;
          this.loadingStatus = DataLoadStatus.Finished;

          this.filter();
          this.updateChartSeries();

          // Save settings
          this.appData.gasSelectedFromDate = this.selectedFromDate;
          this.appData.gasSelectedToDate = this.selectedToDate;
          this.appData.gasSelectedResolutionId = this.selectedResolutionId;
          this.sessionStorageService.set(nameof(ApplicationData), this.appData);
        }, error => {
          switch (error.statusCode) {
            case HttpStatusCodes.NOT_FOUND:
              this.loadingStatus = DataLoadStatus.Finished;
              break;

            case HttpStatusCodes.FORBIDDEN:
              this.loadingStatus = DataLoadStatus.Forbidden;
              break;

            default:
              this.error(error);
          }
        }));
  }
}
