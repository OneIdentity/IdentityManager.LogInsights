using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInsights.LogReader
{
    public class DataPreloader<T>
    {
        public DataPreloader(Func<Task<T>> factory)
        {
            // check the parameters
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            m_Factory = factory;

            m_ActiveTask = m_Factory();
        }

        public async Task<T> GetNextAsync()
        {
            T result = await m_ActiveTask.ConfigureAwait(false);
            
            m_ActiveTask = m_Factory();

            return result;
        }

    
		private readonly Func<Task<T>> m_Factory;
        private Task<T> m_ActiveTask;

    }
}
