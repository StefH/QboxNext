import { Component, OnInit, ViewChild } from '@angular/core';

import { DxChartComponent } from 'devextreme-angular';
import * as moment from 'moment';
import { nameof } from 'ts-simple-nameof';

import { DataComponent } from '../common/components';
import { HttpStatusCodes } from '../common/constants';
import { DataLoadStatus } from '../common/enums';
import { ElectricityValueFormatter } from '../common/formatters';
import { ApplicationData, QboxCounterData, QboxPagedDataQueryResult } from '../common/models';
import { DataService, TimeRangeHelper } from '../common/services';
import { SessionStorageService } from '../common/services';

@Component({
  selector: 'app-show-data',
  providers: [DataService, ElectricityValueFormatter, TimeRangeHelper],
  templateUrl: './electricity.component.html',
  styleUrls: ['./electricity.component.css'],
  preserveWhitespaces: true
})
export class ElectricityComponent extends DataComponent implements OnInit {
  public resolutions = [{ id: 'QuarterOfHour', text: 'Kwartier' }, { id: 'Hour', text: 'Uur' }, { id: 'Day', text: 'Dag' }, { id: 'Month', text: 'Maand' }];

  public check181: boolean;
  public check182: boolean;
  public check281: boolean;
  public check282: boolean;
  public checknet: boolean;
  public checkall: boolean;

  constructor(private service: DataService, private formatter: ElectricityValueFormatter, private timeRangeHelper: TimeRangeHelper, private sessionStorageService: SessionStorageService) {
    super('Electriciteit');
  }

  public ngOnInit(): void {
    this.readAppData();

    this.updateChartSeries();

    this.refreshChart(true);
  }

  public get isLoadIndicatorVisible(): boolean {
    return this.loadingStatus === DataLoadStatus.Started;
  }

  public customizeTooltip(info: any): any {
    const points: any[] = [];
    info.points.forEach(point => {
      const valueAsString = new ElectricityValueFormatter().format(point.value);
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

          this.saveAppData();
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

  private readAppData(): void {
    this.appData = this.sessionStorageService.get<ApplicationData>(nameof(ApplicationData)) || new ApplicationData();
    this.selectedFromDate = this.appData.electricitySelectedFromDate || new Date('2018-10-01');
    this.selectedToDate = this.appData.electricitySelectedToDate || new Date('2018-11-01');
    this.selectedResolutionId = this.appData.electricitySelectedResolutionId || this.resolutions[1].id;
    this.check181 = this.appData.check181 || true;
    this.check182 = this.appData.check182 || true;
    this.check281 = this.appData.check281 || true;
    this.check282 = this.appData.check282 || true;
    this.checknet = this.appData.checknet || true;
    this.checkall = this.appData.checkall || true;
  }

  private saveAppData(): void {
    this.appData.electricitySelectedFromDate = this.selectedFromDate;
    this.appData.electricitySelectedToDate = this.selectedToDate;
    this.appData.electricitySelectedResolutionId = this.selectedResolutionId;
    this.appData.check181 = this.check181;
    this.appData.check182 = this.check182;
    this.appData.check281 = this.check281;
    this.appData.check282 = this.check282;
    this.appData.checknet = this.checknet;
    this.appData.checkall = this.checkall;
    this.sessionStorageService.set(nameof(ApplicationData), this.appData);
  }
}
