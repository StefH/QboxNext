import { Component, OnInit, ViewChild } from '@angular/core';
import { DataService, PagedResult, CounterDataValue, BaseComponent, DataLoadStatus, HttpStatusCodes, GasValueFormatter, TimeRangeHelper } from '../common';

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

  public resultFromServer: PagedResult<CounterDataValue> = new PagedResult<CounterDataValue>();
  public result: PagedResult<CounterDataValue> = new PagedResult<CounterDataValue>();

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
    const template = [
      `<div><div class=\'tooltip-header\'>${info.argumentText}</div>`,
      '<div class=\'tooltip-body\'>'
    ];

    for (let index = 0; index < info.points.length; index++) {
      const valueAsString = new GasValueFormatter().format(info.points[index].value);
      template.push(`<div class=\'series-name\'>${info.points[index].seriesName}</div><div class=\'value-text\'>${valueAsString}</div>`);
    }
    template.push('</div></div>');

    return {
      html: template.join('\r\n')
    };
  }

  public customizeLabelText = (info: any) => {
    return this.formatter.format(info.value);
  }

  private updateChartSeries(): void {
    this.chart.series = [];
    if (this.checkgas) {
      this.chart.series.push({ valueField: 'delta2421', name: 'Gas (2421)', color: '#FFDD00' });
    }
  }

  public getTitle(): string {
    const start = moment(this.selectedFromDate).format('D MMMM YYYY');
    const end = moment(this.selectedToDate).format('D MMMM YYYY');

    return this.selectedResolution === 'Hour' ? `Gas (${start})` : `Gas (${start} tot ${end})`;
  }

  private filter(): void {
    this.result = new PagedResult<CounterDataValue>({ count: this.resultFromServer.count });
    this.resultFromServer.items.forEach(i => {

      const newItem = new CounterDataValue({
        label: i.label,
        delta2421: i.delta2421
      });

      this.result.items.push(newItem);
    });
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
    this.result = new PagedResult<CounterDataValue>();

    this.selectedToDate = this.timeRangeHelper.getToDate(this.selectedResolution, this.selectedFromDate);

    this.subscription.add(this.service.getData(this.selectedResolution, this.selectedFromDate, this.selectedToDate)
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
