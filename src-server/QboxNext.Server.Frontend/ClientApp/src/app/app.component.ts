import { Component, OnInit } from "@angular/core";
import { Http, Headers, Response } from "@angular/http";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"]
})
export class AppComponent implements OnInit {
  title = "QboxNext.Server.Frontend";

  constructor(private http: Http) {}



  ngOnInit() {
    

    //this.http
    //  .get("https://localhost:44379/api/Claim", { headers: headers })
    //  .subscribe(data => console.log(data));

    console.log("----------------------------------------------------");

    //this.http
    //  .get("https://graph.microsoft.com/beta/me/", { headers: headers })
    //  .subscribe(data => console.log(data));
  }
}
