import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { Observable } from 'rxjs';

import { QboxVersionInfo } from '../common/models';
import { BaseService } from '../common/services/base.service';

@Injectable()
export class AboutService extends BaseService<QboxVersionInfo> {
    private baseUrl = '/api/version';

    constructor(http: HttpClient, appInsightsService: AppInsightsService) {
        super(http, appInsightsService);
    }

    public getVersion(): Observable<QboxVersionInfo> {
        return this.get(this.baseUrl, QboxVersionInfo);
    }
}
