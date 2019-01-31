import { Component, OnInit, Input } from '@angular/core';
import { DataService } from './services/data.service';
import { PagedResult, CounterDataValue, BaseComponent, DataLoadStatus, HttpStatusCodes } from '../common';
import * as moment from 'moment';

@Component({
  selector: 'app-show-data',
  providers: [DataService],
  templateUrl: './electricity.component.html',
  styleUrls: ['./electricity.component.css'],
  preserveWhitespaces: true
})
export class ElectricityComponent extends BaseComponent implements OnInit {

  public resolutions = ['QuarterOfHour', 'Hour', 'Day', 'Month'];
  public result: PagedResult<CounterDataValue>;

  public selectedFromDate = new Date('2018-10-01');
  public selectedToDate = new Date('2018-11-01');
  public selectedResolution = 'Hour';

  constructor(private service: DataService) {
    super();
  }

  public ngOnInit(): void {
    this.refreshGraph();
  }

  public getTitle(): string {
    return `Electriciteit (${moment(this.selectedFromDate).format('YYYY-MM-DD')} to ${moment(this.selectedToDate).format('YYYY-MM-DD')})`;
  }

  public refreshGraph(): void {
    this.loadingStatus = DataLoadStatus.Started;
    this.result = new PagedResult<CounterDataValue>();

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
          this.result = data;
          this.loadingStatus = DataLoadStatus.Finished;
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
    return `${Math.abs(info.value) > 10000 ? info.valueText : info.value}W`;
  }
}
