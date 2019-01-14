using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Qboxes.Classes;
using QboxNext.Core.Encryption;
using QboxNext.Core.Log;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;

namespace QboxNext.Qserver.Classes
{
    public class QboxDataDumpContextFactory : IQboxDataDumpContextFactory
    {
        private static readonly Logger Log = QboxNextLogFactory.GetLogger("QboxDataDumpContextFactory");

        /// <summary>
        /// Overide to allow the creation of the Qbox Data Dump context object.
        /// It retrieves the url and external ip from the request and adds the request bytes to the context.
        /// After decrypting the content (if found to be encrypted) the QboxDataDump object is constructed.
        /// </summary>
        /// <param name="context">The controller context holding associations and objects in relation to the controller</param>
        /// <param name="pn">Product number</param>
        /// <param name="sn">Serial number</param>
        /// <returns>An object model that is requested in the bindingcontext</returns>
        public QboxDataDumpContext CreateContext(ControllerContext context, string pn, string sn)
        {
            try
            {
                int length;
                string lastSeenAtUrl;
                string externalIp;

                var bytes = GetRequestVariables(context, out length, out lastSeenAtUrl, out externalIp);

                var mini = Mini(sn);

                if (mini != null)
                {
                    mini.SetStorageProvider();
                    var message = QboxMessageDecrypter.DecryptPlainOrEncryptedMessage(bytes);
                    return new QboxDataDumpContext(message, length, lastSeenAtUrl, externalIp, mini, error: null);
                }

                return null;
            }
            catch (Exception e)
            {
                var s = String.Format("Serialnumber: {0} - orginal error message: {2} | {1}", sn, e.Message, pn);
                Log.Error(e, s);
                return new QboxDataDumpContext("N/A", 0, "N/A", "N/A", null, error: e.Message + " - " + s);//refactor: beter oplossen wordt nu gecontroleerd in de controller en die gooit een exception
            }
            finally
            {
                Log.Trace("Return");
            }

        }

        /// <summary>
		/// Returns a mini poco by serial number. First tries the Redis cache repository and if not found
		/// checks if it can find the box in Eco.
		/// Upon connection exception it will also fall back to ECO.
		/// </summary>
		/// <param name="sn">Serialnumber of the Mini</param>
		/// <returns>MiniPoco object holding the Mini data</returns>
        private MiniPoco Mini(string sn)
        {
            try
            {
                //SAM: previously the Qbox metadata was read from Redis. For now we take a huge shortcut and
                // only support smart meters EG with S0.
                //TODO: make the datastorepath configurable.
                var counterSensorMappingsSmartMeter = new CounterSensorMappingPoco
                {
                    PeriodeBegin = new DateTime(2000, 1, 1),
                    Formule = 1000
                };

                var mini = new MiniPoco()
                {
                    SerialNumber = sn,
                    DataStorePath = Environment.OSVersion.Platform == PlatformID.Win32NT ? @"D:\QboxNextData" : "/var/qboxnextdata",
                    Counters = new List<CounterPoco>()
                    {
                        new CounterPoco
                        {
                            CounterId = 181,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 182,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 281,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 282,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 2421,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 1,
                            // This is not correct, since the Eltako's have different formula's. Keep it simple for now.
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        }
                    },
                    Clients = new List<ClientQboxPoco>()
                    {
                        new ClientQboxPoco
                        {
                            ClientId = 0,
                            MeterType = DeviceMeterType.Smart_Meter_EG,     // Main meter type for second Qbox of Duo.
                            SecondaryMeterType = DeviceMeterType.SO_Pulse   // Should be DeviceMeterType.SO_Pulse for Mono with Qbox Solar.
                        }
                    },
                    Precision = Precision.mWh,
                    MeterType = DeviceMeterType.NO_Meter,       // This should contain the actual meter type for Mono.
                    SecondaryMeterType = DeviceMeterType.None,  // This should be DeviceMeterType.SO_Pulse for Mono with Qbox Solar.
                    AutoAnswer = true
                };
                if (mini != null)
                    mini.PrepareCounters();
                return mini;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
            throw new ArgumentOutOfRangeException("sn");
        }


        /// <summary>
		/// Finds the values from the request and returnes them in out params for later use in the process.
		/// </summary>
		/// <param name="controllerContext">The Controller Context holds the information for the request and actual controller action</param>
		/// <param name="length">The length of the message in the request is returned using this out param</param>
		/// <param name="lastSeenAtUrl">The current request url is returned using this out param </param>
		/// <param name="externalIp">The external ip the mini is sending the request from</param>
		/// <returns></returns>
        private byte[] GetRequestVariables(ControllerContext controllerContext, out int length,
                                                  out string lastSeenAtUrl,
                                                  out string externalIp)
        {

            lastSeenAtUrl = controllerContext.HttpContext.Request.Host.Value;
            externalIp = controllerContext.HttpContext.Connection.RemoteIpAddress.ToString();

            using (var memoryStream = new MemoryStream())
            {
                controllerContext.HttpContext.Request.Body.CopyTo(memoryStream);

                length = (int) memoryStream.Length;

                return memoryStream.ToArray();
            }
        }

        /// <summary>
		/// Reads data from a stream until the end is reached. The
		/// data is returned as a byte array. An IOException is
		/// thrown if any of the underlying IO calls fail.
		/// </summary>
		/// <param name="stream">The stream to read data from</param>
		/// <param name="initialLength">The initial buffer length</param>
        public byte[] ReadRequestInputStreamFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            var buffer = new byte[initialLength];
            long read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, (int)read, (int)(buffer.Length - read))) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read != buffer.Length) continue;

                var nextByte = stream.ReadByte();

                // End of stream? If so, we're done
                if (nextByte == -1)
                {
                    return buffer;
                }

                // Nope. Resize the buffer, put in the byte we've just
                // read, and continue
                var newBuffer = new byte[buffer.Length * 2];
                Array.Copy(buffer, newBuffer, buffer.Length);
                newBuffer[read] = (byte)nextByte;
                buffer = newBuffer;
                read++;
            }
            // Buffer is now too big. Shrink it.
            var ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}