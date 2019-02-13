import DevExpress from 'devextreme/bundles/dx.all';

export interface DevExpressChartEvent {
  component?: DevExpress.viz.dxChart;
  element?: DevExpress.core.dxElement;
  model?: any;
  jQueryEvent?: JQueryEventObject;
  event?: DevExpress.event;
  target?: DevExpress.viz.chartSeriesObject;
}
