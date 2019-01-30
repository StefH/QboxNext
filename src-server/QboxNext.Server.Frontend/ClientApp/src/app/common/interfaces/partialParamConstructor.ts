/**
* Interface to create a new generic type `T` based on anoptional Partial object.
*/
export interface PartialParamConstructor<T> {

    /**
    * Constructor function which takes an optional Partial object.
    *
    * @param init the partial object [optional]
    * @return constructed type `T`.
    */
    new(init?: Partial<T>): T;
}
