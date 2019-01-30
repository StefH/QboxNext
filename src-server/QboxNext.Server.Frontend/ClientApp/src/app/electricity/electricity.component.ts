import { Component, OnInit } from '@angular/core';
import { DataService } from './data.service';

@Component({
  selector: 'app-show-data',
  providers: [DataService],
  templateUrl: './electricity.component.html',
  styleUrls: ['./electricity.component.css'],
  preserveWhitespaces: true
})
export class ElectricityComponent implements OnInit {

  dataSource: any[];

  constructor(service: DataService) {
    this.dataSource = service.getData();
  }

  customizeTooltip = (info: any) => {
    return {
      htmlOK:
        "<div class='tooltip-header'>" + info.argumentText + "</div>" +
          "  <div class='tooltip-body'>" +
          "    <div class='value-text'>" + info.points[0].valueText + "</div>" +
          "    <div class='value-text'>" + info.points[1].valueText + "</div>" +
          "    <div class='value-text'>" + info.points[2].valueText + "</div>" +
          "    <div class='value-text'>" + info.points[3].valueText + "</div>" +
          "  </div>" +
        "</div>",

      html:
        "<div><div class='tooltip-header'>" + info.argumentText + "</div>" +
        "  <div class='tooltip-body'>" +
        "    <div class='series-name'>" + info.points[0].seriesName + ": </div>" + "<div class='value-text'>" + info.points[0].valueText + "W </div>" +
        "    <div class='series-name'>" + info.points[1].seriesName + ": </div>" + "<div class='value-text'>" + info.points[1].valueText + "W </div>" +
        "    <div class='series-name'>" + info.points[2].seriesName + ": </div>" + "<div class='value-text'>" + info.points[2].valueText + "W </div>" +
        "    <div class='series-name'>" + info.points[3].seriesName + ": </div>" + "<div class='value-text'>" + info.points[3].valueText + "W </div>" +
        "    <div class='series-name'>" + info.points[4].seriesName + ": </div>" + "<div class='value-text'>" + info.points[4].valueText + "W </div>" +
        "  </div>" +
        "</div>"
    };
  }

  customizeLabelText = (info: any) => {
    return info.valueText + "W";
  }

  ngOnInit() {
    console.log("!");
  }

}
