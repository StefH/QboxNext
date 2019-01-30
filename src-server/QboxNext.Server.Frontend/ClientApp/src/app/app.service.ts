import { Injectable } from '@angular/core';

class Complaints {
  complaint: string;
  count: number
}

export class ComplaintsWithPercent {
  complaint: string;
  count: number;
  cumulativePercent: number;
}

let complaintsData: Complaints[] = [{
  complaint: "Pizza is cold",
  count: 780
}, {
  complaint: "Inadequate cheese quantity",
  count: 120
}, {
  complaint: "Not baked properly",
  count: 52
}, {
  complaint: "Delayed delivery",
  count: 1123
}, {
  complaint: "Damaged delivery",
  count: 321
}, {
  complaint: "Incorrect billing",
  count: 89
}, {
  complaint: "Wrong size delivered",
  count: 222
}];

@Injectable()
export class Service {
  getComplaintsData(): ComplaintsWithPercent[] {
    var data = complaintsData.sort(function (a, b) {
        return b.count - a.count;
      }),
      totalCount = data.reduce(function (prevValue, item) {
        return prevValue + item.count;
      }, 0),
      cumulativeCount = 0;
    return data.map(function (item, index) {
      cumulativeCount += item.count;
      return {
        complaint: item.complaint,
        count: item.count,
        cumulativePercent: Math.round(cumulativeCount * 100 / totalCount)
      };
    });
  }
}
