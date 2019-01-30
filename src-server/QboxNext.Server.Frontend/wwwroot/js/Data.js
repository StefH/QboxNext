$(function () {

    var request = {
        "SerialNumber": "15-46-001-243",
        "From": "2018-10-05T00:00:00",
        "To": "2018-10-06T00:00:00",
        "Resolution": "Hour"
    };

    var request2 = {
        "SerialNumber": "15-46-001-243",
        "From": "2018-10-01T00:00:00",
        "To": "2018-11-01T00:00:00",
        "Resolution": "Day"
    };

    $.ajax({
        type: "POST",
        url: "/data",
        data: JSON.stringify(request),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {

            var dataSource = result.items.map(function (item, index) {
                return {
                    label: item.label,
                    delta0181: item.delta0181,
                    delta0182: item.delta0182,
                    delta0281: item.delta0281,
                    delta0282: item.delta0282,
                    net: item.delta0181 + item.delta0182 + item.delta0281 + item.delta0282
                };
            });

            $("#chart").dxChart({
                dataSource: dataSource,
                title: "Energy " + request.From + " to " + request.To + " (" + result.count + ")",
                tooltip: {
                    enabled: true,
                    shared: true,
                    customizeTooltip: function (info) {
                        return {
                            html:
                                "<div class='tooltip-header'>" + info.argumentText + "</div>" +
                                "  <div class='tooltip-body'>" +
                                "    <div class='value-text'>" + info.points[0].valueText + "</div>" +
                                "    <div class='value-text'>" + info.points[1].valueText + "</div>" +
                                "    <div class='value-text'>" + info.points[2].valueText + "</div>" +
                                "    <div class='value-text'>" + info.points[3].valueText + "</div>" +
                                "  </div>" +
                                "</div>"
                        };
                    }
                },
                valueAxis: [{
                    position: "left",
                    label: {
                        customizeText: function (info) {
                            return info.valueText + "W";
                        }
                    }
                }],
                commonSeriesSettings: {
                    argumentField: "label",
                    type: "bar",
                    hoverMode: "allArgumentPoints",
                    selectionMode: "allArgumentPoints"
                },
                series: [
                    {
                        valueField: "delta0181",
                        name: "Verbruik Laag (181)",
                        color: "#FFDD00"
                    },
                    {
                        valueField: "delta0182",
                        name: "Verbruik Hoog (182)",
                        color: "#FF8800"
                    },
                    {
                        valueField: "delta0281",
                        name: "Opwek Laag (281)",
                        color: "#00DDDD"
                    },
                    {
                        valueField: "delta0282",
                        name: "Opwek Hoog (282)",
                        color: "#00DD00"
                    },
                    {
                        valueField: "net",
                        name: "Netto",
                        color: "#DDDDDD"
                    }
                ],
                legend: {
                    verticalAlignment: "top",
                    horizontalAlignment: "center"
                }
            });
        },
        failure: function (errMsg) {
            alert(errMsg);
        }
    });
});