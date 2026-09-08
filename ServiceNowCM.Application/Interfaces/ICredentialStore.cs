using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowCM.Application.Interfaces
{
    public interface ICredentialStore
    {
        Task<string> StoreAsync(
            string key,
            string secret);

        Task<string?> GetAsync(
            string reference);

        Task DeleteAsync(
            string reference);
    }
}
