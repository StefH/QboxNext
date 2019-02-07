import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { QboxCounterData, QboxPagedDataQueryResult } from '../models';
import { BaseService } from './base.service';

@Injectable()
export class DataService extends BaseService<QboxCounterData> {
    private baseUrl = '/api/data';

    constructor(private http: HttpClient) {
        super(http);
    }

    public getData(resolution: string, from: Date, to: Date): Observable<QboxPagedDataQueryResult<QboxCounterData>> {

        const request = {
            from: from.toISOString().split('T')[0],
            to: to.toISOString().split('T')[0],
            addHours: 1,
            resolution: resolution
        };

        return this.post<QboxPagedDataQueryResult<QboxCounterData>>(this.baseUrl, request);
    }
}
