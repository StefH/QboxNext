using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace QboxNext.Qserver.Core.Interfaces
{
    public interface IStatistics
    {
		IDictionary<string, string> Views(string inDb);

        DataView Data(string inDb, string key);
    }
}
