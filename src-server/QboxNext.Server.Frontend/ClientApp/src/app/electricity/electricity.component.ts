import { Component, OnInit, ViewChild } from '@angular/core';
import { DataService, QboxCounterData, BaseComponent, DataLoadStatus, HttpStatusCodes, ElectricityValueFormatter, TimeRangeHelper, QboxPagedDataQueryResult } from '../common';

import * as moment from 'moment';
import { DxChartComponent } from 'devextreme-angular';

@Component({
  selector: 'app-show-data',
  providers: [DataService, ElectricityValueFormatter, TimeRangeHelper],
  templateUrl: './electricity.component.html',
  styleUrls: ['./electricity.component.css'],
  preserveWhitespaces: true
})
export class ElectricityComponent extends BaseComponent implements OnInit {
  public resolutions = [{ id: 'QuarterOfHour', text: 'Kwartier' }, { id: 'Hour', text: 'Uur' }, { id: 'Day', text: 'Dag' }, { id: 'Month', text: 'Maand' }];

  @ViewChild(DxChartComponent) chart: DxChartComponent;

  public resultFromServer = new QboxPagedDataQueryResult<QboxCounterData>();
  public result = new QboxPagedDataQueryResult<QboxCounterData>();

  public selectedFromDate = new Date('2018-10-01');
  public selectedToDate = new Date('2018-11-01');
  public selectedResolution = this.resolutions[1].id;
  public check181 = true;
  public check182 = true;
  public check281 = true;
  public check282 = true;
  public checknet = true;
  public checkall = true;

  constructor(private service: DataService, private formatter: ElectricityValueFormatter, private timeRangeHelper: TimeRangeHelper) {
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
      const valueAsString = new ElectricityValueFormatter().format(point.value);
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

  public customizeLabelText = (info: any) => {
    return this.formatter.format(info.value);
  }

  private updateChartSeries(): void {
    this.chart.series = [];
    if (this.check181) {
      this.chart.series.push({ valueField: 'delta0181', name: 'Verbruik Laag (181)', color: '#FFDD00' });
    }
    if (this.check182) {
      this.chart.series.push({ valueField: 'delta0182', name: 'Verbruik Hoog (182)', color: '#FF8800' });
    }
    if (this.check281) {
      this.chart.series.push({ valueField: 'delta0281', name: 'Opwek Laag (281)', color: '#00DDDD' });
    }
    if (this.check282) {
      this.chart.series.push({ valueField: 'delta0282', name: 'Opwek Hoog (282)', color: '#00DD00' });
    }
    if (this.checknet) {
      this.chart.series.push({ valueField: 'net', name: 'Netto', color: '#AAAAAA' });
    }
  }

  public getTitle(): string {
    const start = moment(this.selectedFromDate).format('D MMMM YYYY');
    const end = moment(this.selectedToDate).format('D MMMM YYYY');

    if (this.selectedResolution === 'QuarterOfHour' || this.selectedResolution === 'Hour') {
      return `Electriciteit (${start})`;
    }

    return `Electriciteit (${start} tot ${end})`;
  }

  private filter(): void {
    const mapCounterDataValue = (i: QboxCounterData) => {
      return new QboxCounterData({
        labelText: i.labelText,
        labelValue: i.labelValue,
        delta0181: i.delta0181,
        delta0182: i.delta0182,
        delta0281: i.delta0281,
        delta0282: i.delta0282,
        net: !(this.check181 && this.check182 && this.check281 && this.check282) ?
          i.delta0181 + i.delta0182 + i.delta0281 + i.delta0282 :
          (this.check181 ? i.delta0181 : 0) + (this.check182 ? i.delta0182 : 0) + (this.check281 ? i.delta0281 : 0) + (this.check282 ? i.delta0282 : 0)
      });
    };

    const clientResult = new QboxPagedDataQueryResult<QboxCounterData>({
      count: this.resultFromServer.count,
      overview: mapCounterDataValue(this.resultFromServer.overview),
      items: this.resultFromServer.items.map(mapCounterDataValue)
    });

    this.result = clientResult;
  }

  public checkClicked(event: Event) {
    // Only update chart if event is from a user-click
    if (event) {
      this.checkall = this.check181 && this.check182 && this.check281 && this.check282 && this.checknet;

      this.refreshChart(false);
    }
  }

  public checkAllClicked(event: Event): void {
    // Only update if event is from a user-click
    if (event) {
      this.check181 = this.checkall;
      this.check182 = this.checkall;
      this.check281 = this.checkall;
      this.check282 = this.checkall;
      this.checknet = this.checkall;

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
