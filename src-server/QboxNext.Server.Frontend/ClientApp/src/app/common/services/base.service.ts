import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { PartialParamConstructor } from '../interfaces';
import { ErrorMessage, PagedResult } from '../models';

export abstract class BaseService<T> {
    constructor(private httpClient: HttpClient) {
    }

    /**
    * Construct a POST request which returns a `TResponse` object.
    *
    * @param url the url
    * @param model the model
    * @param ctor a constructor which creates a new typed `TResponse` object.
    * @return an `Observable` of the body as type `TResponse`.
    */
    protected post<TResponse>(url: string, model: any, ctor?: PartialParamConstructor<TResponse>): Observable<TResponse> | Observable<any> {
        return this.httpClient.post(url, model, { observe: 'response', responseType: 'text' })
            .pipe(
                map(response => this.mapResponse(response, ctor)),
                catchError(error => this.handleError(error))
            );
    }

    /**
    * Construct a GET request which returns a `TResponse` object.
    *
    * @param url the url
    * @param ctor a constructor which creates a new typed `TResponse` object based on the data.
    * @return an `Observable` of the body as type `TResponse`.
    */
    protected get(url: string, ctor?: PartialParamConstructor<T>): Observable<T> | Observable<any> {
        return this.httpClient.get(url, { observe: 'response', responseType: 'text' })
            .pipe(
                map(response => this.mapResponse(response, ctor)),
                catchError(error => this.handleError(error))
            );
    }

    /**
     * Construct a GET request which returns a `TResponse[]` object.
     *
     * @param url the url
     * @param ctor a constructor which creates a new typed `TResponse` object based on the data.
     * @return an `Observable` of the body as type `TResponse[]`.
     */
    protected getListNOTUSEDYET(url: string, ctor?: PartialParamConstructor<T>): Observable<T[]> | Observable<any> {
        return this.httpClient.get(url, { observe: 'response', responseType: 'text' })
            .pipe(
                map(response => this.mapListResponse(response, ctor)),
                catchError(error => this.handleError(error))
            );
    }

    /**
     * Construct a GET request which returns a `PagedResult<T>` object.
     *
     * @param url the url
     * @param ctor a constructor which creates a new typed `TResponse` object based on the data.
     * @return an `Observable` of the body as type `TResponse[]`.
     */
    protected getPagedResult(url: string, ctor?: PartialParamConstructor<T>): Observable<PagedResult<T>> | Observable<any> {
        return this.httpClient.get(url, { observe: 'response', responseType: 'text' })
            .pipe(
                map(response => this.mapResponse(response, ctor)),
                catchError(error => this.handleError(error))
            );
    }

    private handleError(errorResponse: HttpErrorResponse | any): Observable<ErrorMessage> {
        const errorMessage = new ErrorMessage({ statusCode: errorResponse.status });

        if (errorResponse instanceof HttpErrorResponse) {
            const error = errorResponse.error;

            if (error) {
                const parsedError = new ErrorMessage(JSON.parse(error));
                errorMessage.message = parsedError.message;
                errorMessage.exceptionType = parsedError.exceptionType;
            } else {
                errorMessage.message = errorResponse.message ? errorResponse.message : errorResponse.toString();
            }
        } else {
            errorMessage.message = errorResponse.toString();
        }

        return throwError(errorMessage);
    }

    protected mapResponse<TResponse>(response: HttpResponse<string>, ctor?: PartialParamConstructor<TResponse>): TResponse {
        const body: TResponse = response.body ? JSON.parse(response.body) : {};
        return ctor ? new ctor(body) : body;
    }

    protected mapListResponse(response: HttpResponse<string>, ctor?: PartialParamConstructor<T>): T[] {
        const array: T[] = response.body ? JSON.parse(response.body) : [];
        return array.map(element => {
            return ctor ? new ctor(element) : element;
        });
    }
}
