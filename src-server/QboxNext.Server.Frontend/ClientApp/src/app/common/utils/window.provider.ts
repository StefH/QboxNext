// code based on https://stackoverflow.com/questions/36222845/how-to-get-domain-name-for-service-in-angular2

import { InjectionToken, FactoryProvider } from '@angular/core';

export const WINDOW = new InjectionToken<Window>('window');

export const WindowProvider: FactoryProvider = {
    provide: WINDOW,
    useFactory: () => window
};
