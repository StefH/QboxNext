import { Injectable } from '@angular/core';

import { PartialParamConstructor } from '../interfaces';

export class SessionValue<T> {
    public value: T;

    public constructor(init?: Partial<SessionValue<T>>) {
        Object.assign(this, init);
    }
}

@Injectable({
    providedIn: 'root'
})
export class SessionStorageService {

    // set session item by propertyName
    public set(propertyName: string, value: any): void {
        sessionStorage.setItem(propertyName, JSON.stringify(new SessionValue({ value: value })));
    }

    // get session item by propertyName
    public get<T>(propertyName: string, ctor?: PartialParamConstructor<T>): T | null {
        const itemValue = sessionStorage.getItem(propertyName);
        if (itemValue == null) {
            return null;
        }

        const wrapper: SessionValue<T> = JSON.parse(itemValue);
        return ctor ? new ctor(wrapper.value) : wrapper.value;
    }

    // delete session item
    public delete(propertyName: string): void {
        sessionStorage.removeItem(propertyName);
    }

    // clear session item or items
    public clear(propertyName: string | null): void {
        if (propertyName) {
            sessionStorage.removeItem(propertyName);
        }

        sessionStorage.clear();
    }
}
