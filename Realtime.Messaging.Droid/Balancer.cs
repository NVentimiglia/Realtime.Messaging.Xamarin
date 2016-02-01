using System;
using Square.OkHttp;
using System.Threading.Tasks;
using Realtime.Messaging.Ext;
using Balancer = Realtime.Messaging.Droid.Balancer;

[assembly: Xamarin.Forms.Dependency(typeof(Balancer))]

namespace Realtime.Messaging.Droid
{
	public class Balancer : IBalancer
	{
		public Balancer ()
		{
		}

		public async Task<Tuple<Boolean,String>> ResolveClusterUrlAsync (String clusterUrl)
		{
			OkHttpClient client = new OkHttpClient();
			Request request = new Request.Builder()
				.Url(clusterUrl)
				.Build();
			
			Response response = await client.NewCall(request).ExecuteAsync();
			String server = await response.Body ().StringAsync();
			return new Tuple<bool,String>(response.IsSuccessful,server);
		}
	}
}

