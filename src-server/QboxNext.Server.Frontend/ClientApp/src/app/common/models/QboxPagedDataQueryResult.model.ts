import { PagedResult } from './pagedresult.model';

export class QboxPagedDataQueryResult<T> extends PagedResult<T> {

    public constructor(init?: Partial<QboxPagedDataQueryResult<T>>) {
        super(init);
        Object.assign(this, init);
    }
}
