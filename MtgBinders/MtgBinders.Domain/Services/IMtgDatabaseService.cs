using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.Services
{
    public interface IMtgDatabaseService
    {
        void Initialize();

        void UpdateDatabase(bool forceUpdate);
    }
}