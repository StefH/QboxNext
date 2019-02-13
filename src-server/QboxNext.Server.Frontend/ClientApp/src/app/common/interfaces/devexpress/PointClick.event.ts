import DevExpress from 'devextreme/bundles/dx.all';

export interface DevExpressPointClickEvent {
  component: any;
  element: DevExpress.core.dxElement;
  model: any;
  jQueryEvent?: JQueryEventObject;
  event: DevExpress.dxEvent;
  target: DevExpress.viz.basePointObject;
}
