import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

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
            from: from,
            to: to,
            addHours: 1,
            resolution: resolution
        };

        return this.post<QboxPagedDataQueryResult<CounterDataValue>>(this.baseUrl, request);

        // const result: Observable<QboxPagedDataQueryResult<CounterDataValue>> = this.post<QboxPagedDataQueryResult<CounterDataValue>>(this.baseUrl, request);

        // return result.pipe(map(p => {
        //     p.items.forEach(i => {
        //         i.net = i.delta0181 + i.delta0182 + i.delta0281 + i.delta0282;
        //     });
        //     return p;
        // }));
    }
}
