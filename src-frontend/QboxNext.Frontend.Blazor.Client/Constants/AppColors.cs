using Blazorise.Charts;

namespace QboxNext.Frontend.Blazor.Client.Constants
{
    public class AppColors
    {
        public static string VerbruikLaag181 = "#FFDD00";

        public static string VerbruikHoog182 = "#FF8800";

        public static string OpwekLaag281 = "#00DDDD";

        public static string OpwekHoog282 = "#00DD00";

        public static string Netto = "#AAAAAA";

        public static ChartColor ChartVerbruikLaag181;

        public static ChartColor ChartVerbruikHoog182;

        public static ChartColor ChartOpwekLaag281;

        public static ChartColor ChartOpwekHoog282;

        public static ChartColor ChartNetto;

        static AppColors()
        {
            ChartVerbruikLaag181 = ChartColor.FromHtmlColorCode(VerbruikLaag181);

            ChartVerbruikHoog182 = ChartColor.FromHtmlColorCode(VerbruikHoog182);

            ChartOpwekLaag281 = ChartColor.FromHtmlColorCode(OpwekLaag281);

            ChartOpwekHoog282 = ChartColor.FromHtmlColorCode(OpwekHoog282);

            ChartNetto = ChartColor.FromHtmlColorCode(Netto);
        }
    }
}
