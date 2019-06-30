using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Connexionz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.WebClients
{
    public interface ITransitClient
    {
        BusSystemData LoadTransitData();
        Task<ConnexionzPlatformET?> GetEta(int platformTag);
        Task<List<ServiceAlert>> GetServiceAlerts();
    }
}
