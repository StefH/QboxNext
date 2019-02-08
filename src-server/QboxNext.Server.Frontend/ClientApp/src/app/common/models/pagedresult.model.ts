
export abstract class PagedResult<T>  {

    public count = 0;

    public items: T[] = [];

    public constructor(init?: Partial<PagedResult<T>>) {
        Object.assign(this, init);
    }
}
