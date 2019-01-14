using System.Collections.Generic;
using System.Threading;
using NLog;
using QboxNext.Core;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Factories;
using QboxNext.Qserver.Core.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Qboxes.Classes;
using System.Xml;
using QboxNext.Qserver.Core.Interfaces;
using System.Text;
using QboxNext.Qboxes.Parsing.Factories;
using QboxNext.Qboxes.Parsing.Protocols;


namespace Qboxes
{
    public static class Mini
    {
        public static string WriteMeterType(DeviceMeterType deviceMeterType)
        {
            var result = new BaseParseResult();
            result.Write((byte)3);
            var meterType = (byte)deviceMeterType;
            result.Write(meterType);

            //Rolf 25-4-2013: In overleg met Ron verwijderd voor dit moment
            ////todo: manufacturer
            //if (deviceMeterType == DeviceMeterType.Ferraris_Black_Toothed)
            //{
            //    result.Write((byte)5);
            //    const short manufacturer = 0x00000101;
            //    result.Write(manufacturer);
            //}

            return result.GetMessage();
        }
    }
}
