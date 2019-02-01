import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { BaseService } from './base.service';
import { CounterDataValue, QboxPagedDataQueryResult } from '..';


@Injectable()
export class DataService extends BaseService<CounterDataValue> {
    private baseUrl = '/api/data';

    constructor(private http: HttpClient) {
        super(http);
    }

    public getData(resolution: string, from: Date, to: Date): Observable<QboxPagedDataQueryResult<CounterDataValue>> {

        const request = {
            serialNumber: '15-46-001-243',
            from: from.toISOString().split('T')[0],
            to: to.toISOString().split('T')[0],
            addHours: 1,
            resolution: resolution
        };

        return this.post<QboxPagedDataQueryResult<CounterDataValue>>(this.baseUrl, request);
    }
}
