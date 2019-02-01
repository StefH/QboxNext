import { Component, OnInit, Input } from '@angular/core';
import { DataService, PagedResult, CounterDataValue, BaseComponent, DataLoadStatus, HttpStatusCodes } from '../common';
import * as moment from 'moment';

@Component({
  selector: 'app-show-data',
  providers: [DataService],
  templateUrl: './electricity.component.html',
  styleUrls: ['./electricity.component.css'],
  preserveWhitespaces: true
})
export class ElectricityComponent extends BaseComponent implements OnInit {

  public resolutions = [{ id: 'QuarterOfHour', text: 'Kwartier' }, { id: 'Hour', text: 'Uur' }, { id: 'Day', text: 'Dag' }, { id: 'Month', text: 'Maand' }];
  public resultFromServer: PagedResult<CounterDataValue>;
  public result: PagedResult<CounterDataValue>;

  public selectedFromDate = new Date('2018-10-01');
  public selectedToDate = new Date('2018-11-01');
  public selectedResolution = this.resolutions[1].id;
  public check181 = true;
  public check182 = true;
  public check281 = true;
  public check282 = true;
  public checknet = true;

  get isLoadIndicatorVisible(): boolean {
    return this.loadingStatus === DataLoadStatus.Started;
  }

  constructor(private service: DataService) {
    super();
  }

  public ngOnInit(): void {
    this.refreshChart(true);
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
    this.result = new PagedResult<CounterDataValue>({ count: this.resultFromServer.count });
    this.resultFromServer.items.forEach(i => {

      const newItem = new CounterDataValue({
        label: i.label,
        delta0181: i.delta0181,
        delta0182: i.delta0182,
        delta0281: i.delta0281,
        delta0282: i.delta0282,
        net: (this.check181 ? i.delta0181 : 0) + (this.check182 ? i.delta0182 : 0) + (this.check281 ? i.delta0281 : 0) + (this.check282 ? i.delta0282 : 0)
      });

      this.result.items.push(newItem);
    });
  }

  public refreshChart(serverSide: boolean): void {

    if (!serverSide) {
      // just filter
      return;
    }

    this.loadingStatus = DataLoadStatus.Started;
    this.resultFromServer = new PagedResult<CounterDataValue>();

    const fromMoment = moment(this.selectedFromDate);

    switch (this.selectedResolution) {
      case 'QuarterOfHour':
      case 'Hour': this.selectedToDate = fromMoment.add(1, 'day').toDate();
        break;
      case 'Day': this.selectedToDate = fromMoment.add(1, 'month').toDate();
        break;
      case 'Month': this.selectedToDate = fromMoment.add(1, 'year').toDate();
        break;
      case 'Year': this.selectedToDate = fromMoment.add(1, 'year').toDate();
        break;
    }

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

  public customizeTooltip(info: any): any {
    return {
      html:
        `<div><div class=\'tooltip-header\'>${info.argumentText}</div>` +
        '  <div class=\'tooltip-body\'>' +
        `    <div class=\'series-name\'>${info.points[0].seriesName}</div><div class=\'value-text\'>${info.points[0].valueText}W </div>` +
        `    <div class=\'series-name\'>${info.points[1].seriesName}</div><div class=\'value-text\'>${info.points[1].valueText}W </div>` +
        `    <div class=\'series-name\'>${info.points[2].seriesName}</div><div class=\'value-text\'>${info.points[2].valueText}W </div>` +
        `    <div class=\'series-name\'>${info.points[3].seriesName}</div><div class=\'value-text\'>${info.points[3].valueText}W </div>` +
        `    <div class=\'series-name\'>${info.points[4].seriesName}</div><div class=\'value-text\'>${info.points[4].valueText}W </div>` +
        '  </div>' +
        '</div>'
    };
  }

  public customizeLabelText = (info: any) => {
    return `${Math.abs(info.value) >= 10000 ? info.valueText : info.value}W`;
  }
}
