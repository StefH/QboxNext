import { Component, OnInit, ViewChild } from '@angular/core';

import { BaseComponent } from '../common/components';
import { HttpStatusCodes } from '../common/constants';
import { DataLoadStatus } from '../common/enums';
import { QboxVersionInfo } from '../common/models';
import { AboutService } from './about.service';

@Component({
    selector: 'app-show-about',
    templateUrl: './about.component.html',
    providers: [AboutService]
})
export class AboutComponent extends BaseComponent implements OnInit {

    public versionInfo: QboxVersionInfo;

    constructor(private service: AboutService) {
        super();
    }

    public ngOnInit(): void {
        this.subscription.add(this.service.getVersion()
            .subscribe(
                data => {
                    this.loadingStatus = DataLoadStatus.Finished;
                    this.versionInfo = data;
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
}
