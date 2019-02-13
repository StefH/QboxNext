using System;

namespace QboxNext.Qbiz.Dto
{
	[Flags]
	public enum EnergyType
	{
		None = 0x00,
		Electricity = 0x01,
		Gas = 0x02,
		ElectricityAndGas = Electricity | Gas
	}
}
