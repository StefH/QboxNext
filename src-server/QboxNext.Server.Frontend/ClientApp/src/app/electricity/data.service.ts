import { Injectable } from '@angular/core';
import { Http, Headers, Response } from "@angular/http";

var items = [
  {
    "label": "00",
    "measureTime": "2018-10-06T00:00:00",
    "delta0181": 49,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 41
  },
  {
    "label": "01",
    "measureTime": "2018-10-06T01:00:00",
    "delta0181": 57,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 17
  },
  {
    "label": "02",
    "measureTime": "2018-10-06T02:00:00",
    "delta0181": 34,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "03",
    "measureTime": "2018-10-06T03:00:00",
    "delta0181": 128,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "04",
    "measureTime": "2018-10-06T04:00:00",
    "delta0181": 56,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "05",
    "measureTime": "2018-10-06T05:00:00",
    "delta0181": 32,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "06",
    "measureTime": "2018-10-06T06:00:00",
    "delta0181": 57,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "07",
    "measureTime": "2018-10-06T07:00:00",
    "delta0181": 61,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "08",
    "measureTime": "2018-10-06T08:00:00",
    "delta0181": 25,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "09",
    "measureTime": "2018-10-06T09:00:00",
    "delta0181": 93,
    "delta0182": 0,
    "delta0281": -14,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "10",
    "measureTime": "2018-10-06T10:00:00",
    "delta0181": 128,
    "delta0182": 0,
    "delta0281": -21,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "11",
    "measureTime": "2018-10-06T11:00:00",
    "delta0181": 520,
    "delta0182": 0,
    "delta0281": -75,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "12",
    "measureTime": "2018-10-06T12:00:00",
    "delta0181": 32,
    "delta0182": 0,
    "delta0281": -307,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "13",
    "measureTime": "2018-10-06T13:00:00",
    "delta0181": 26,
    "delta0182": 0,
    "delta0281": -1188,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "14",
    "measureTime": "2018-10-06T14:00:00",
    "delta0181": 0,
    "delta0182": 0,
    "delta0281": -1236,
    "delta0282": 0,
    "delta2421": 3
  },
  {
    "label": "15",
    "measureTime": "2018-10-06T15:00:00",
    "delta0181": 61,
    "delta0182": 0,
    "delta0281": -611,
    "delta0282": 0,
    "delta2421": 2
  },
  {
    "label": "16",
    "measureTime": "2018-10-06T16:00:00",
    "delta0181": 88,
    "delta0182": 0,
    "delta0281": -200,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "17",
    "measureTime": "2018-10-06T17:00:00",
    "delta0181": 39,
    "delta0182": 0,
    "delta0281": -19,
    "delta0282": 0,
    "delta2421": 0
  },
  {
    "label": "18",
    "measureTime": "2018-10-06T18:00:00",
    "delta0181": 159,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 1
  },
  {
    "label": "19",
    "measureTime": "2018-10-06T19:00:00",
    "delta0181": 429,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 14
  },
  {
    "label": "20",
    "measureTime": "2018-10-06T20:00:00",
    "delta0181": 514,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 3
  },
  {
    "label": "21",
    "measureTime": "2018-10-06T21:00:00",
    "delta0181": 515,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 1
  },
  {
    "label": "22",
    "measureTime": "2018-10-06T22:00:00",
    "delta0181": 416,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 1
  },
  {
    "label": "23",
    "measureTime": "2018-10-06T23:00:00",
    "delta0181": 381,
    "delta0182": 0,
    "delta0281": 0,
    "delta0282": 0,
    "delta2421": 1
  }
];

@Injectable()
export class DataService {

  constructor(private http: Http) { }

  getData(): any[] {
    var request = {
      SerialNumber: "15-46-001-243",
      From: "2018-10-05T00:00:00",
      To: "2018-10-06T00:00:00",
      Resolution: "Hour"
    };

    var dataSource = items.map(function (item, index) {
      return {
        label: item.label,
        delta0181: item.delta0181,
        delta0182: item.delta0182,
        delta0281: item.delta0281,
        delta0282: item.delta0282,
        net: item.delta0181 + item.delta0182 + item.delta0281 + item.delta0282
      };
    });

    return dataSource;
  }
}
