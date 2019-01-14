using System;
using System.Linq;
using NLog;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;

namespace QboxNext.Qserver.Core.Utils
{
	/// <summary>
	/// Class voor het detecteren van counterpiekwaarden.
	/// </summary>
	public class CounterPeakDetector
	{
		public static readonly Logger Log = LogManager.GetCurrentClassLogger();
		private readonly decimal _maxPeakValue;
		private readonly string[] _excludedQboxSerials;


		public CounterPeakDetector(decimal maxPeakValue, string excludedQboxSerialsText)
		{
			_maxPeakValue = maxPeakValue;
			_excludedQboxSerials = excludedQboxSerialsText.Split(';');
		}


		public bool IsPeak(DateTime measurementTime, ulong value, decimal formula, Record lastValue, CounterPoco counter)
		{
			if ((lastValue != null) && (lastValue.Time < measurementTime))
			{
				if (_excludedQboxSerials.Contains(counter.QboxSerial))
				{
					Log.Trace("Not detecting peak for Qbox {0}", counter.QboxSerial);
					return false;
				}

				// Gas is measured every hour (* 60) (and saved every minute)
				if (counter.IsGasCounter)
					formula = formula * 60;

				var delta = value < lastValue.Raw ? 0m : (value - lastValue.Raw) / (formula == 0 ? 1 : formula);

				var minutes = (int)(measurementTime - lastValue.Time).TotalMinutes;
				minutes = minutes <= 0 ? 1 : minutes;
				var factor = Convert.ToDecimal(minutes < 60d ? (60.0 / minutes) : 1.0);
				var consumption = delta * factor * 1000m;

				return consumption > _maxPeakValue;
			}

			return false;
		}
	}
}
