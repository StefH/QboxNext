import { Injectable } from '@angular/core';
import { PartialParamConstructor } from '../interfaces';

@Injectable({
    providedIn: 'root'
})
export class SessionStorageService {

    // set session item
    public set(prop: string, value: any): void {
        sessionStorage.setItem(prop, JSON.stringify(value));
    }

    // get session item
    public get<T>(prop: string, ctor?: PartialParamConstructor<T>): T | null {
        const itemValue = sessionStorage.getItem(prop);
        if (itemValue == null) {
            return null;
        }

        const value: T = JSON.parse(itemValue);
        return ctor ? new ctor(value) : value;
    }

    // delete session item
    public delete(prop: string): void {
        sessionStorage.removeItem(prop);
    }

    // clear session item or items
    public clear(prop: string | null): void {
        if (prop) {
            sessionStorage.removeItem(prop);
        }

        sessionStorage.clear();
    }
}
