import { Component, OnInit } from '@angular/core';

import * as moment from 'moment';

import { CurrencyPipe } from '@angular/common';
import { DataComponent } from '../common/components';
import { HttpStatusCodes } from '../common/constants';
import { DataLoadStatus, Resolution } from '../common/enums';
import { GasValueFormatter } from '../common/formatters';
import { ApplicationData, QboxCounterData, QboxPagedDataQueryResult } from '../common/models';
import { DataService, PriceService, SessionStorageService, TimeRangeHelper } from '../common/services';

@Component({
  selector: 'app-show-gas',
  providers: [DataService, GasValueFormatter, TimeRangeHelper, PriceService],
  templateUrl: './gas.component.html',
  styleUrls: ['./gas.component.css'],
  preserveWhitespaces: true
})
export class GasComponent extends DataComponent implements OnInit {
  public resolutions = [{ id: Resolution.Hour, text: 'Uur' }, { id: Resolution.Day, text: 'Dag' }, { id: Resolution.Month, text: 'Maand' }];

  public checkgas = true;

  constructor(timeRangeHelper: TimeRangeHelper, cp: CurrencyPipe, priceService: PriceService,
    private service: DataService, private formatter: GasValueFormatter, private sessionStorageService: SessionStorageService) {
    super('Gas', timeRangeHelper, cp, priceService);
  }

  public ngOnInit(): void {
    this.appData = this.sessionStorageService.get<ApplicationData>('ApplicationData') || ApplicationData.Default;
    this.selectedFromDate = this.appData.gasSelectedFromDate || moment();
    this.selectedToDate = this.appData.gasSelectedToDate || moment().add(1, 'day');
    this.selectedResolutionId = this.appData.gasSelectedResolutionId;

    this.updateChartSeries();

    this.refreshChart(true);
  }

  public refreshClicked() {
    this.refreshChart(true);
  }

  public get isLoadIndicatorVisible(): boolean {
    return this.loadingStatus === DataLoadStatus.Started;
  }

  public customizeTooltip(info: any): any {
    const points: any[] = [];
    info.points.forEach(point => {
      const valueAsString = point.seriesName === 'Kosten' ?
        this.cp.transform(point.value || 0, 'EUR', 'symbol', '1.2-2') : new GasValueFormatter().format(point.value);
      points.push(`<div class=\'series-name\'>${point.seriesName}</div><div class=\'value-text\'>${valueAsString}</div>`);
    });

    return {
      html: `<div><div class=\'tooltip-header\'>${info.argumentText}</div><div class=\'tooltip-body\'>${points.join('\r\n')}</div></div>`
    };
  }

  public customizeLabelText = (info: any) => this.formatter.format(info.value);

  private updateChartSeries(): void {
    this.chart.series = [];
    if (this.checkgas) {
      this.chart.series.push({ valueField: 'delta2421', name: 'Gas (2421)', color: '#FFDD00' });
    }
  }

  private filter(): void {
    const price = this.priceService.getGasPrice(moment(this.selectedFromDate).year());

    const mapCounterDataValue = (i: QboxCounterData) => {
      return new QboxCounterData({
        labelText: i.labelText,
        labelValue: i.labelValue,
        drillDownQuery: i.drillDownQuery,
        delta2421: i.delta2421,
        costs: i.delta2421 * price / 1000
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

    this.updateEnergyCosts();

    this.subscription.add(this.service.getData(this.selectedResolutionId, dates.fromDate.toDate(), this.selectedToDate)
      .subscribe(
        data => {
          this.resultFromServer = data;
          this.loadingStatus = DataLoadStatus.Finished;

          this.filter();
          this.updateChartSeries();
          this.updateOverview();

          // Save settings
          this.appData.gasSelectedFromDate = this.selectedFromDate;
          this.appData.gasSelectedToDate = this.selectedToDate;
          this.appData.gasSelectedResolutionId = this.selectedResolutionId;
          this.sessionStorageService.set('ApplicationData', this.appData);
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

          this.updateOverview();
        }));
  }
}
