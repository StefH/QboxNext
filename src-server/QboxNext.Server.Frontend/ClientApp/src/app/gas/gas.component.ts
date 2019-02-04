import { Component, OnInit, ViewChild } from '@angular/core';
import { DataService, QboxCounterData, BaseComponent, DataLoadStatus, HttpStatusCodes, GasValueFormatter, TimeRangeHelper, QboxPagedDataQueryResult } from '../common';

import * as moment from 'moment';
import { DxChartComponent } from 'devextreme-angular';

@Component({
  selector: 'app-show-gas',
  providers: [DataService, GasValueFormatter, TimeRangeHelper],
  templateUrl: './gas.component.html',
  styleUrls: ['./gas.component.css'],
  preserveWhitespaces: true
})
export class GasComponent extends BaseComponent implements OnInit {
  public resolutions = [{ id: 'Hour', text: 'Uur' }, { id: 'Day', text: 'Dag' }, { id: 'Month', text: 'Maand' }];

  @ViewChild(DxChartComponent) chart: DxChartComponent;

  private resultFromServer = new QboxPagedDataQueryResult<QboxCounterData>();
  public result = new QboxPagedDataQueryResult<QboxCounterData>();

  public selectedFromDate = new Date('2018-10-01');
  public selectedToDate = new Date('2018-11-01');
  public selectedResolution = this.resolutions[0].id;
  public checkgas = true;

  constructor(private service: DataService, private formatter: GasValueFormatter, private timeRangeHelper: TimeRangeHelper) {
    super();
  }

  public ngOnInit(): void {
    this.updateChartSeries();

    this.refreshChart(true);
  }

  public get isLoadIndicatorVisible(): boolean {
    return this.loadingStatus === DataLoadStatus.Started;
  }

  public customizeTooltip(info: any): any {
    const points = [];
    info.points.forEach(point => {
      const valueAsString = new GasValueFormatter().format(point.value);
      points.push(`<div class=\'series-name\'>${point.seriesName}</div><div class=\'value-text\'>${valueAsString}</div>`);
    });

    return {
      html: `<div><div class=\'tooltip-header\'>${info.argumentText}</div><div class=\'tooltip-body\'>${points.join('\r\n')}</div></div>`
    };
  }

  public getOverview(): string {
    const info = {
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

  public getTitle(): string {
    const start = moment(this.selectedFromDate).format('D MMMM YYYY');
    const end = moment(this.selectedToDate).format('D MMMM YYYY');

    if (this.selectedResolution === 'QuarterOfHour' || this.selectedResolution === 'Hour') {
      return `Gas (${start})`;
    }

    return `Gas (${start} tot ${end})`;
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

  private refreshChart(serverSide: boolean): void {
    if (!serverSide) {

      // just filter
      this.filter();

      // and update series
      this.updateChartSeries();

      return;
    }

    this.loadingStatus = DataLoadStatus.Started;
    this.result = new QboxPagedDataQueryResult<QboxCounterData>();

    const dates = this.timeRangeHelper.getToDate(this.selectedResolution, this.selectedFromDate);
    this.selectedToDate = dates.toDate.toDate();

    this.subscription.add(this.service.getData(this.selectedResolution, dates.fromDate.toDate(), this.selectedToDate)
      .subscribe(
        data => {
          this.resultFromServer = data;
          this.loadingStatus = DataLoadStatus.Finished;

          this.filter();
          this.updateChartSeries();
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
