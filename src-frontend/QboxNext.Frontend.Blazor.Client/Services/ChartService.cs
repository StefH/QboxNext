using System.Linq;
using System.Threading.Tasks;
using Blazorise.Charts;
using QboxNext.Frontend.Blazor.Client.Constants;
using QboxNext.Server.Domain;

namespace QboxNext.Frontend.Blazor.Client.Services
{
    public class ChartService
    {
        private readonly AuthenticatedDataQueryClient _client;

        public ChartService(AuthenticatedDataQueryClient client)
        {
            _client = client;
        }

        public async Task GetElectricityBarChartAsync(QboxDataQuery query, BarChart<int> chart)
        {
            await chart.Clear();

            var data = await _client.GetCounterDataAsync(query);

            var labels = data.Items.Select(item => item.LabelText).ToArray();

            var verbruikLaag181 = new BarChartDataset<int>
            {
                Label = "Verbruik Laag (181)",
                BackgroundColor = labels.Select(_ => (string) AppColors.ChartVerbruikLaag181),
                Data = data.Items.Select(item => item.Delta0181).ToList()
            };

            var verbruikHoog182 = new BarChartDataset<int>
            {
                Label = "Verbruik Hoog (182)",
                BackgroundColor = labels.Select(_ => (string)AppColors.ChartVerbruikHoog182),
                Data = data.Items.Select(item => item.Delta0182).ToList()
            };

            var opwekLaag281 = new BarChartDataset<int>
            {
                Label = "Opwek Laag (281)",
                BackgroundColor = labels.Select(_ => (string)AppColors.ChartOpwekLaag281),
                Data = data.Items.Select(item => item.Delta0281).ToList()
            };

            var opwekHoog282 = new BarChartDataset<int>
            {
                Label = "Opwek Hoog (282)",
                BackgroundColor = labels.Select(_ => (string)AppColors.ChartOpwekHoog282),
                Data = data.Items.Select(item => item.Delta0282).ToList()
            };

            var netto = new BarChartDataset<int>
            {
                Label = "Netto",
                BackgroundColor = labels.Select(_ => (string)AppColors.ChartNetto),
                Data = data.Items.Select(item => item.Delta0181 + item.Delta0182 + item.Delta0281 + item.Delta0282).ToList()
            };

            await chart.AddLabelsDatasetsAndUpdate(labels, verbruikLaag181, verbruikHoog182, opwekLaag281, opwekHoog282, netto);

            //var overleden = new BarChartDataset<int>
            //{
            //    Label = Resources.Label_Overleden,
            //    BackgroundColor = age.LabelsLeeftijdsverdeling.Select(x => (string)AppColors.ChartLightGray),
            //    Data = age.Overleden
            //};

            //var ic = new BarChartDataset<int>
            //{
            //    Label = Resources.Label_IC,
            //    BackgroundColor = age.LabelsLeeftijdsverdeling.Select(x => (string)AppColors.ChartYellow),
            //    Data = age.NogOpgenomen
            //};

            //var verpleegafdeling = new BarChartDataset<int>
            //{
            //    Label = Resources.Label_Verpleegafdeling,
            //    BackgroundColor = age.LabelsLeeftijdsverdeling.Select(x => (string)AppColors.ChartBlue),
            //    Data = age.ICVerlatenNogOpVerpleegafdeling
            //};

            //var gezond = new BarChartDataset<int>
            //{
            //    Label = Resources.Label_Gezond,
            //    BackgroundColor = age.LabelsLeeftijdsverdeling.Select(x => (string)AppColors.ChartGreen),
            //    Data = age.ICVerlaten
            //};

            //await chart.AddLabelsDatasetsAndUpdate(age.LabelsLeeftijdsverdeling.ToArray(), overleden, ic, verpleegafdeling, gezond);
        }
    }
}
