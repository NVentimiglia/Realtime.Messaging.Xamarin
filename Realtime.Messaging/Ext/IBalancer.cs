using System;
using System.Threading.Tasks;

namespace Realtime.Messaging
{
	public interface IBalancer
	{
		Task<Tuple<bool,String>> ResolveClusterUrlAsync(String clusterUrl);
	}
}

