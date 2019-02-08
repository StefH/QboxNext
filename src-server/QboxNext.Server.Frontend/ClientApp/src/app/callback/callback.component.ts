import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'app-callback',
    templateUrl: './callback.component.html',
    styleUrls: ['./callback.component.css']
})
export class CallbackComponent implements OnInit {

    private sub: any;

    constructor(private route: ActivatedRoute, private router: Router) { }

    ngOnInit() {
        this.sub = this.route.params.subscribe(params => {
            // console.log('params : ');
            // console.log(params);
        });


        this.sub = this.route.fragment.subscribe(fragment => {
            // console.log('fragment : ');
            // console.log(fragment);
        });
    }

}
