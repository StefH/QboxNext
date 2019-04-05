import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { AppInsightsService } from '@markpieszak/ng-application-insights';
import * as moment from 'moment';
import { Observable } from 'rxjs';

import { Resolution } from '../enums';
import { QboxCounterData, QboxDataQuery, QboxPagedDataQueryResult } from '../models';
import { BaseService } from './base.service';

@Injectable()
export class DataService extends BaseService<QboxCounterData> {
    private baseUrl = '/api/data';

    constructor(http: HttpClient, appInsightsService: AppInsightsService) {
        super(http, appInsightsService);
    }

    public getData(resolution: Resolution, from: Date, to: Date): Observable<QboxPagedDataQueryResult<QboxCounterData>> {
        // console.log(from);
        // console.log(to);

        const request = new QboxDataQuery({
            from: moment(from).format('YYYY-MM-DD'),
            to: moment(to).format('YYYY-MM-DD'),
            resolution: resolution,
            adjustHours: true
        });

        // console.log(request);

        return this.post<QboxPagedDataQueryResult<QboxCounterData>>(this.baseUrl, request);
    }
}
